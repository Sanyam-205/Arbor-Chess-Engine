using System;
using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Text.RegularExpressions;
using static Board;

public class Search
{
    public long nodeCount, leafCount, qNodes;
    //alpha is the highest score the AI is guaranteed to achieve. Good for AI
    //beta is the lowest score the opponent can have. Good for human
    /*
                depth 0
           (A)  /     \  (B)
               3       1
              / \     / \
            -1   3  -5   1          
    White to move. +ve value= = better for white. -ve value = better for black. This is standard mimimax.
    
    In this example, for branch A, alpha is 3 which is the best score for white and beta is -1, the best score for black.
    For branch B, alpha is 1, the best score for white and beta is -5, the best score for black. We always assume that the opponent makes the best move for them leading to highest beta value.
    Beta is the maximum score the opponent will allow us to get
    Beta can also be explained as the absolute best score we can have for that particular branch. For example, if -1 and -5 both continued downwards, these two scores would be the absolute best we would be looking to get if we went down those paths.
    Since at depth 0, the function must return the move order leading to best possible evaluation for you meaning the branch with best Alpha value. 
    
    */

    const int MaxPly = 256; //ply represents half move. ! full move = 2 ply.

    ushort[,] killerMoves = new ushort[MaxPly, 2]; //stores quiet moves that caused beta cutoff at that specific ply in a search tree.

    /*
        Killer moves are used to score quiet moves that caused a beta cutoff in the game so that our move ordering logic can order these quiet moves.
        Killer moves store the 2 best moves that caused beta cutoff at each ply and is updated at each ply.
        In killer moves, a move that caused a beta cutoff at a specific ply is scored. Imagine MaxPly is 8, meaning the engine will search at 8 depth. Killer move store the encoded move struct, for example at depth 1, e2e4 caused a beta cutoff so killerMoves[1, 1] will store encoded e2e4 ushort value and ScoreMove will give it a score (90 for first move, 80 for second). 
        When e2e4 is scored, the move ordering logic orders it higher than other quiet moves which have a score of 0.
        Now, at depth 2, the move d2d4 caused beta cutoff. So now, killerMoves[2,1] will store d2d4 as e2e4 was shifted one place down to [2,2]. Same move ordering and scoring logic applies.
        If the same move causes beta cutoff, then it is discarded.
        Now, suppose at depth 7, somehow Nc6 causes beta cutoff, so killerMoves[7,1] will store Nb1c6, killerMoves[7,2] will store d2d4 and e2e4 is replaced.    
    
    */
    int[,] historyMoves = new int[12,64]; //12 = number of piecesm 64 = board squares.
    /*
        History heuristics are also used to apply a score to quiet moves that caused beta cutoff so our move ordering logic can order these moves higher than standard quiet moves (with score of 0). 
        But unlike killer moves that store 2 best moves that caused beta cutoff at specific ply, history heuristics, store moves that caused beta cutoff throughout the game. 
        The idea is that if a quiet move caused a beta cutoff at one part of the game, then that move is likely good enough to be assigned a higher search priority.
        If a move e2e4, causes beta cutoff multiple times, then that move's score is higher and it will be searched first.
        Imagine the move Nb1c3 causes beta cutoff once, historyMoves, store [1,18] -> white knight = 1, c3 = 18 and assigns it a score x. That same move, causes beta cutoff once again in a different search tree, so now the previous score historyMoves[1,18] will be added to its new value, so historyMoves[1,18] will store x+y
        Because we are tracking the entire game, moves that cause beta cutoff frequently should be assigned higher scores than moves that cause beta cutoff rarely.

        The general formula for historyMoves is historyMoves[piece, square] += depth * depth. But we can't use this directly because 1. it will cause integer overflow pretty quickly. 2. We still need mvv-lva captures and quiet moves to be scored higher, so we apply bounds to this.

        There is also an idea where moves that don't cause beta cutoff are given a penalty. We will see if we implement this or not....
    
    */

    const int maxHistory = 1 << 14;
    //We use maxHistory to limit the hitoryMove score from becoming too high. 
    //Why 1<<14? Because we want to use a nice round number like 10,000 but if we use intergers, we will need to use / operator to limit the score, but with 1 << 14, which is 2^14 = 16,384, we get the closest to 10,000.

    Move[,] pvTable = new Move[MaxPly, MaxPly]; //fixed 2d array to store the move and depth score.
    int[] pvLength = new int[MaxPly]; //need to track how long the move sequence is at each depth so it can be copied downward at the next depth level

    /* A principal variation table is used to store the sequence of moves that will result in the best move for that depth. At depth 6, the alpha value (score) will show the evaluation after the board has reached that particular stage. The root node is the final move at that particular depth. We need to store the sequence of moves. Here's how we do it.
    A 2d array in which, one element referes to the depth value as depth decreases in each recursive call of the loop and another sotring the best move found.
    Lets say, the best sequence of moves for a position is this -> e2e4, e7e5, d2d4, d7d5, g1f3, b8c6. The pvTable will look like this -
    depth    move sequence
      6      .e2e4
      5      .e2e4 .e7e5
      4      .e2e4 .e7e5 .d2d4 
      3      .e2e4 .e7e5 .d2d4 .d7d5 
      2      .e2e4 .e7e5 .d2d4 .d7d5 .g1f3
      1      .e2e4 .e7e5 .d2d4 .d7d5 .g1f3 .b8c6
    *If we were only storing the root node, it will just give is b8c6*

    To copy the content of the previous depth onto the next depth, we need another pvLength table.
    */

    //debug code
    public int StartSearch(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth, int alpha, int beta, int ply)
    {
        nodeCount = 0; // Clear the board for the new search
        leafCount = 0;
        qNodes = 0;
        ttMoveFirst = 0;
        ttMoveBest = 0;
        killerMovesHit = 0;
        killerMovesProbed = 0;
        gameKillerMovesHit = 0;
        gameKillerMovesProbed = 0;
        historyHit = 0;
        historyProbed = 0;

        Array.Clear(killerMoves, 0, killerMoves.Length);
        Array.Clear(historyMoves, 0, historyMoves.Length);
        return NegaMax(board, moveGenerator, evaluation, depth, alpha, beta, ply);
    }

    public long ttProbes, ttHits, ttCutoffs;
    public long ttMoveFirst;
    public long ttMoveBest;
    public long killerMovesHit, killerMovesProbed;
    public long gameKillerMovesHit, gameKillerMovesProbed;
    public long historyHit, historyProbed;
    
    public bool abortSearch = false;
    public int allocatedTimeMs = -1;
    public System.Diagnostics.Stopwatch? sw;


    public int NegaMax(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth, int alpha, int beta, int ply) 
    {
        // Periodically check if time ran out (every 2048 nodes)
        if (allocatedTimeMs != -1 && ((nodeCount + qNodes) & 2047) == 0)
        {
            if (sw != null && sw.ElapsedMilliseconds >= allocatedTimeMs)
            {
                abortSearch = true;
            }
        }
        if (abortSearch) return 0;

        nodeCount++;

        if (ply >= MaxPly)
        {
            //THE PLY COUNT HAS EXCEEDED MAX SET LIMIT. DO SOMETHING IDK
            return evaluation.EvaluatePosition(board);
        }

        pvLength[ply] = 0; //set it to 0 for each recursive call to prevent PV pollution

        //TT
        int originalAlpha = alpha;
        int bestScore = -500000;
        Move bestMoveThisNode = new Move(0); // Track the best move found to store in the TT
        
        ttProbes++;



        //probing logic for TT.
        //Probing means, before the search algorithm spend crucial resources searching through a position, does that position already exist in the TT? i.e. have we searched through this exact position before?
        int index = (int)(board.currentHash % (ulong)TT.table.Length); //since one particulat hash will be stored at one particular index, we can straight up jump to that index without needing to loop through the TT.
        if(TT.table[index].zobristKey == board.currentHash)
        {
            ttHits++;


            bestMoveThisNode = TT.table[index].bestMove;
        
            if(TT.table[index].depth >= depth && ply > 0)
            {
                byte flag_temp = TT.table[index].flagType;
                int storedScore = TT.table[index].score;

                // Adjust mate scores retrieved from TT (clamp to avoid catching +/- infinity)
                if (storedScore > 90000 && storedScore < 400000) storedScore -= ply;
                else if (storedScore < -90000 && storedScore > -400000) storedScore += ply;

                // 1. Exact Match Check
                if (flag_temp == (byte)TT.Flags.exactScore) 
                {
                    ttCutoffs++;
                    return storedScore;
                }
                
                // 2. Lower Bound (Beta Cutoff) Check
                if (flag_temp == (byte)TT.Flags.hashBeta && storedScore >= beta) 
                {
                    ttCutoffs++;
                    return storedScore;
                }
                
                // 3. Upper Bound (Alpha Cutoff) Check
                if (flag_temp == (byte)TT.Flags.hashAlpha && storedScore <= alpha) 
                {
                    ttCutoffs++;
                    return storedScore;
                }
            }
        }
        //TT part end

        Move ttMove = bestMoveThisNode;
        bool hadTTMove = ttMove.Value != 0;

        if (depth == 0) 
        {
            leafCount++;
            // pvLength[ply] = 0; //signifies the search function finished searching
            // return evaluation.EvaluatePosition(board);
            return Quiescence(board, moveGenerator, evaluation, alpha, beta, ply);
        }

        //Populate moveList with pseudolegal moves
        Move[] moveList = new Move[256];
        Move[] quietMoveList = new Move[256];
        int moveCount = 0;
        int quietMoveCount = 0;
        moveGenerator.GenerateAllPseudoLegalMoves(board, moveList, ref moveCount);

        int legalMovesPlayed = 0;

        //Populate scores for each move in the moveList array in the same order.

        int[] moveScore = new int[moveCount];
        for(int i = 0; i<moveCount; i++)
        {
            Move newMove = moveList[i];

            //========================================================================================================
            //                                               TT
            //========================================================================================================

            if (newMove.Value == bestMoveThisNode.Value && bestMoveThisNode.Value != 0) 
            {
                moveScore[i] = 15000; 
            }

            else
            {
                moveScore[i] = ScoreMove(newMove, board, ply);
            }

            //========================================================================================================
            //                                               TT
            //========================================================================================================

            
            // moveScore[i] = ScoreMove(newMove, board, ply);

        }


        //Apply selection sort to the moveScore array 

        //run moves in moveList through the legality check
        for (int i = 0; i < moveCount; i++)
        {
#region move ordering
            //Selection sort           
            
            int bestMoveIndex = i;

            for(int j = i; j < moveCount; j++)
            {
                
                if(moveScore[j] > moveScore[bestMoveIndex])
                {
                    bestMoveIndex = j;
                }  
            }

            //swap the best move in moveList
            Move tempMove = moveList[i];
            moveList[i] = moveList[bestMoveIndex];
            moveList[bestMoveIndex] = tempMove;

            //swap their scores in moveScore.
            int temp = moveScore[i];
            moveScore[i] = moveScore[bestMoveIndex];
            moveScore[bestMoveIndex] = temp;
        
#endregion


            if (i == 0 && ttMove.Value != 0 && moveList[0].Value == ttMove.Value)
            {
                ttMoveFirst++;
            }


            // continuation of search
            Move move = moveList[i];

            bool isQuietMove = board.pieceOnSquare[move.TargetSquare] == -1 && move.Flag != (int)Move.MoveFlag.enPassantCapture;
            bool isKiller = (move.Value != 0) && (move.Value == killerMoves[ply, 0] || move.Value == killerMoves[ply, 1]);
            bool isHistory = isQuietMove && !isKiller;

            if (isKiller)
            {
                killerMovesProbed++;
                gameKillerMovesProbed++;
            }
            else if (isHistory)
            {
                historyProbed++;
            }


#region debug
//
//     int startSq = move.StartSquare;
//     int targetSq = move.TargetSquare;
//     int originalStartPiece = board.pieceOnSquare[startSq];
//     int originalTargetPiece = board.pieceOnSquare[targetSq];

// if((startSq == 0)  && (targetSq == 0))
// {
//     Console.WriteLine("=================================");
//     Console.WriteLine("BAD MOVE LIST");
//     Console.WriteLine("=================================");

//     // Console.WriteLine($"Previous move - {moveList[i-1].StartSquare}, {moveList[i-1].TargetSquare}");
//     Console.WriteLine($"Current move - {moveList[i].StartSquare}, {moveList[i].TargetSquare}");
//     Console.WriteLine($"Next move - {moveList[i+1].StartSquare}, {moveList[i+1].TargetSquare}");

//     Console.WriteLine("=================================");
//     Console.Out.Flush();
//     Environment.Exit(1);

// }



// if (board.pieceOnSquare[move.StartSquare] == -1)
// {
//     Console.WriteLine("=================================");
//     Console.WriteLine("FATAL DESYNC CAUGHT BEFORE MAKEMOVE!");
//     Console.WriteLine($"MoveGen generated a move for square {move.StartSquare}, {move.TargetSquare}, but pieceOnSquare says it is empty.");
    
//     if (i > 0) 
//     {
//         Console.WriteLine($"The culprit is the PREVIOUS move in this loop: Iteration {i-1}");
//         Console.WriteLine($"Previous move - {moveList[i-1].StartSquare}, {moveList[i-1].TargetSquare}");
//         // If your Move struct has a way to print the square/value, print moveList[i-1] here to see what it was.
//     }
//     else 
//     {
//         Console.WriteLine("The corruption happened in a deeper recursive call before this loop started in negamax");
//         for (int z = 0; z < 64; z++)
//         {
//             if(board.pieceOnSquare[z]== 0)
//             {
//             Console.Write(board.pieceOnSquare[z] + "\t");

//             if ((z + 1) % 8 == 0)
//             {
//                 Console.WriteLine();
//             }}
//         }
//     }
    
//     Console.WriteLine("=================================");
//     Console.Out.Flush();
//     Environment.Exit(1);
// }

#endregion


            board.MakeMove(move);

            int colorThatJustMoved = board.colorToMove ^ 1;
            int kingSquare = board.GetKingSquare(colorThatJustMoved);

            // Legality check
            if (kingSquare != -1 && board.IsSquareAttacked(kingSquare, colorThatJustMoved))
            {
                board.UnmakeMove(move);


                continue; 
            }

            

            legalMovesPlayed++;

            //Recursive call for lower depth.
            int score = -NegaMax(board, moveGenerator, evaluation, depth - 1, -beta, -alpha, ply+1);
        
            board.UnmakeMove(move);

            //fail soft
            if (score > bestScore)
            {
                bestScore = score;
                bestMoveThisNode = move; // Track the best move for TT
            }


            //Alpha-Beta pruning
            if (score >= beta) 
            {
                if (isKiller)
                {
                    killerMovesHit++;
                    gameKillerMovesHit++;
                }
                else if (isHistory)
                {
                    historyHit++;
                }
                
                /*Beta cutoff in alpha beta pruning and TT are somewhat different. 
                In alpha beta pruning, the beta cutoff is telling us that if a line is too good for us and the opponent already has another line that whose score is less than the score of this line then the opponent will never let us enter that path.
                That path is 'too good' because up in the tree, the opponent has another line that leads to a better evaluation for them
                A better, guaranteed evaluation for the opponent is the beta score..
                
                To understand TT, TT does not care if the opponent will ever let us enter than line. That is managed by alpha beta pruning. TT only cares to return the evaluation that we were going to get if that line were to happen.
                TT tells us that if that line were to happen, then you could get atleast this much score. So this becomes our lowerbound. 

                Imagine alpha = 20, beta = 50, score = 90.
                alpha being 20 means that out current best score is alpha. Beta being 50 means that the opponent already has a line that forces our score to be 50. Score being 90 means that if we were to go down that line than we could get an evaluation of 90.
                In alpha beta pruning, this is too good so opponent will never allow it.
                In TT however, if we were to ever go down that line then 90 is the score we are guaranteed to have so that 90 becomes our lowerbound flag which is hashBeta.
                
                */


                /*If a quiet move causes a beta cutoff then that move is likely good. So we store that move in a killer move array. 
                A killer move is a move that is non capture. As beta cutoof only happens when choosing this branch will lead to us getting a better evaluation, this means that a quiet move raised our eval.*/

                //=========================killer move + history heuristics==================================

                //quiet move check
                if(board.pieceOnSquare[move.TargetSquare] == -1 && move.Flag != (int)Move.MoveFlag.enPassantCapture)
                {
                    //killer move
                    if(move.Value != killerMoves[ply,0])
                    {
                        killerMoves[ply, 1] = killerMoves[ply, 0]; //shift the prevoius killer move
                    
                        killerMoves[ply, 0] = move.Value; //store the move.value to killermove[ply,0]
                    }

                    int bonus = depth * depth;

                    historyMoves[board.pieceOnSquare[move.StartSquare], move.TargetSquare] += bonus; //assign an internal history score which will be given a move ordering score in ScoreMove

                    if(historyMoves[board.pieceOnSquare[move.StartSquare], move.TargetSquare ] >= maxHistory)
                    {
                        AgeHistoryTable(); //divide the entire history table by 2 so that the raw score never exceeds maxHistory
                    }
                }

                // Penalize quiet moves that failed to cause a cutoff regardless of the cutoff move type
                for (int k = 0; k < quietMoveCount; k++)
                {
                    Move penalizedMove = quietMoveList[k];
                
                    int piece = board.pieceOnSquare[penalizedMove.StartSquare];
                    int square = penalizedMove.TargetSquare;

                    int tempScore = historyMoves[piece, square] - depth * depth;

                    historyMoves[piece, square] = (tempScore < 0) ? 0 : tempScore;
                }

                //=========================killer move + history heuristics end==================================

                int ttScore = score;
                if (ttScore > 90000 && ttScore < 400000) ttScore += ply;
                else if (ttScore < -90000 && ttScore > -400000) ttScore -= ply;


                if (hadTTMove && bestMoveThisNode.Value == ttMove.Value)
                {
                    ttMoveBest++;
                }

                TT.Store(board.currentHash, (byte)depth, ttScore, (byte)TT.Flags.hashBeta, move); //hashBeta is the lowerbound score.



                // TT.Store(board.currentHash, (byte)depth, score, (byte)TT.Flags.hashBeta, move); //hashBeta is the lowerbound score.
                return score; //If this branch leads to a worse outcome, do not consider it. Return beta and get out of the path.
            }
            if (score > alpha) 
            {
                //normal alpha beta pruning logic. We found a better move, update score.
                alpha = score; //We found a better move for ourselves
             
                pvTable[ply, 0] = move; //stores the best move found at this depth.                
                // 2. Copy the sequence of moves from the deeper ply
                for (int j = 0; j < pvLength[ply + 1]; j++)
                {
                    pvTable[ply, j + 1] = pvTable[ply + 1, j];
                }

                // 3. Update the length of the sequence for this ply
                pvLength[ply] = pvLength[ply + 1] + 1;
            }

            if (isQuietMove) 
            {
                quietMoveList[quietMoveCount] = move;
                quietMoveCount++;
            }


        }


        //STALEMATE, CHECKMATE CHECK.
        if(legalMovesPlayed == 0)
        {
            int colorToMove = board.colorToMove;
            int currentKingSquare = board.GetKingSquare(colorToMove);

            if((currentKingSquare != -1) && board.IsSquareAttacked(currentKingSquare, colorToMove))
            {
                return -100000 + ply; //checkmate
            }
            else
            {
                return 0; //stalemate
            }
        }

        /*Suppose a node at n depth enters the negamax with alpha score of 30. We store this to original alpha. 
        Alpha score of 30 means 30 is the besst possible evaluation for the moving side before it searches any deeper. Now after the search, the best possible score or alpha we got is 20.
        Since alpha < originalAlpha meaning 30 is the upper bound, every other score we can get from this branch would be less than 30. Thats why it becomes our upperbound. 

        Imagine in that some node at depth n, we entered the search with 30 alpha, 70 beta and 50 score, this becomes our exact score since this fits inside the upperbound(calculated right here) and lowerbound(calculated above.)
        */
        // byte flag = (alpha <= originalAlpha) ? (byte)TT.Flags.hashAlpha : (byte)TT.Flags.exactScore;


        byte flag = (bestScore <= originalAlpha) ? (byte)TT.Flags.hashAlpha : (byte)TT.Flags.exactScore;


        int ttScoreToStore = bestScore;
        if (ttScoreToStore > 90000 && ttScoreToStore < 400000) ttScoreToStore += ply;
        else if (ttScoreToStore < -90000 && ttScoreToStore > -400000) ttScoreToStore -= ply;

        if (hadTTMove && bestMoveThisNode.Value == ttMove.Value)
        {
            ttMoveBest++;
        }
        
        TT.Store(board.currentHash, (byte)depth, ttScoreToStore, flag, bestMoveThisNode);




        // TT.Store(board.currentHash, (byte)depth, alpha, flag, bestMoveThisNode);
        return bestScore;
    }

    public int Quiescence (Board board, MoveGenerator moveGenerator, Evaluation evaluation, int alpha, int beta, int ply)
    {
        // Periodically check if time ran out (every 2048 nodes)
        if (allocatedTimeMs != -1 && ((nodeCount + qNodes) & 2047) == 0)
        {
            if (sw != null && sw.ElapsedMilliseconds >= allocatedTimeMs)
            {
                abortSearch = true;
            }
        }
        if (abortSearch) return 0;

        qNodes++;
        
        
        if (ply >= MaxPly)
        {
            //THE PLY COUNT HAS EXCEEDED MAX SET LIMIT. DO SOMETHING IDK
            return evaluation.EvaluatePosition(board);
        }

        pvLength[ply] = 0;
        
        int currentKingSquare = board.GetKingSquare(board.colorToMove);

        // If the King is missing (captured in the previous ply via pseudo-legal generation),
        // immediately evaluate this branch as a loss (checkmate).
        if (currentKingSquare == -1)
        {
            return -100000 + ply;
        }

        bool inCheck = board.IsSquareAttacked(currentKingSquare, board.colorToMove);

        // Console.WriteLine($"Quiescence called! Side to move: {board.colorToMove}. In Check? {inCheck}");

        // if(inCheck) Console.WriteLine("in check");
        Move[] moveList = new Move[256];
        int moveCount = 0;



        int bestScore = -500000;




        if(inCheck)
        {
            moveGenerator.GenerateAllPseudoLegalMoves(board, moveList, ref moveCount);
        }

        else
        {
            int standPat = evaluation.EvaluatePosition(board);
            
            // if(standPat >= beta) return beta;
            




            bestScore = standPat;

            if(standPat >= beta) return standPat;
            
            
            
            
            
            if(standPat > alpha) alpha = standPat;

            moveGenerator.GenerateAllPseudoLegalCaptures(board, moveList, ref moveCount);
        }

        int legalMovesPlayed = 0;

        //Move ordering logic
        int[] moveScore = new int[moveCount];
        for(int i = 0; i<moveCount; i++)
        {
            moveScore[i] = ScoreMove(moveList[i], board, ply); //assign scores to moves and store those in moveScore array
        }


        for(int i = 0; i<moveCount; i++)
        {
            int bestMoveIndex = i;

            for(int j = i; j < moveCount; j++)
            {
                if(moveScore[j] > moveScore[bestMoveIndex])
                {
                    bestMoveIndex = j;
                }
            }

            Move tempMove = moveList[i];
            moveList[i] = moveList[bestMoveIndex];
            moveList[bestMoveIndex] = tempMove;

            int temp = moveScore[i];
            moveScore[i] = moveScore[bestMoveIndex];
            moveScore[bestMoveIndex] = temp;

            Move move = moveList[i];

#region debug


// if (board.pieceOnSquare[move.StartSquare] == -1)
// {
//     Console.WriteLine("=================================");
//     Console.WriteLine("FATAL DESYNC CAUGHT BEFORE MAKEMOVE!");
//     Console.WriteLine($"MoveGen generated a move for square {move.StartSquare}, {move.TargetSquare}, but pieceOnSquare says it is empty.");
    
//     if (i > 0) 
//     {
//         Console.WriteLine($"The culprit is the PREVIOUS move in this loop: Iteration {i-1}");
//         // If your Move struct has a way to print the square/value, print moveList[i-1] here to see what it was.
//     }
//     else 
//     {
//         Console.WriteLine("The corruption happened in a deeper recursive call before this loop started in quiescence");
//         for (int z = 0; z < 64; z++)
//         {
//             Console.Write(board.pieceOnSquare[z] + "\t");

//             if ((z + 1) % 8 == 0)
//             {
//                 Console.WriteLine();
//             }
//         }
//     }
    
//     Console.WriteLine("=================================");
//     Console.Out.Flush();
//     Environment.Exit(1);
// }
#endregion debug




            board.MakeMove(move);
            int colorThatJustMoved = board.colorToMove ^ 1;
            int kingSquareAfterMove = board.GetKingSquare(colorThatJustMoved);

            if ((kingSquareAfterMove != -1) && board.IsSquareAttacked(kingSquareAfterMove, colorThatJustMoved))
            {
                board.UnmakeMove(move);


                continue; //Move is illegal, skip that index and continue the for loop from next index (move)
            }
            legalMovesPlayed++;

            int score = -Quiescence(board, moveGenerator, evaluation, -beta, -alpha, ply + 1);

            board.UnmakeMove(move);


            //fail soft
            if (score > bestScore)
            {
                bestScore = score;
            }




            //alpha beta pruning
            if(score >= beta)
            {
                return score; //beta cutoff
            }

            if(score > alpha)
            {
                alpha = score; //found a better guaranteed path/move. Update alpha
            

                pvTable[ply, 0] = move;

                // 2. Copy the sequence of moves from the deeper ply
                for (int j = 0; j < pvLength[ply + 1]; j++)
                {
                    pvTable[ply, j + 1] = pvTable[ply + 1, j];
                }

                // 3. Update the length of the sequence for this ply
                pvLength[ply] = pvLength[ply + 1] + 1;


            }
        }

#region debug
        // if (board.colorToMove == 1) 
        // {
        //     if (inCheck && legalMovesPlayed == 0) 
        //     {
        //         Console.WriteLine("MATE CAUGHT: But I missed the return statement!");
        //     } 
        //     else if (inCheck && legalMovesPlayed > 0) 
        //     {
        //         Console.WriteLine($"GHOST EVASION: I am in check, but I think I found {legalMovesPlayed} legal moves!");
        //     } 
        //     else if (!inCheck) 
        //     {
        //         Console.WriteLine("BLIND: I am supposed to be in check from RxP, but inCheck is FALSE!");
        //     }
        // }

#endregion debug
        if(inCheck && legalMovesPlayed == 0)
        {
            return -100000 + ply;
            
        }


        return bestScore;

        // return alpha;
    }


    public void PrintPrincipalVariation()
    {
        Console.Write("Best Line: ");
        for (int i = 0; i < pvLength[0]; i++)
        { 
            // Console.Write((pvTable[0,i].StartSquare) + "," + (pvTable[0,i].TargetSquare));
            Console.Write(BoardUtility.MoveToUci(pvTable[0,i]) + " ");
        }
        Console.WriteLine();
    }



    public int ScoreMove(Move move, Board board, int ply)
    {

        // int[] pieceValues = {5, 3, 3, 9, 10000, 1, 5, 3, 3, 9, 10000, 1};
        int[] pieceTypeMap = {0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5}; //rook, knight, bishop, queen, king, pawn

        int movingPiece = board.pieceOnSquare[move.StartSquare];
        int capturedPieceType = board.pieceOnSquare[move.TargetSquare];
        //pieceOnSquare stores the piece type for every square. If there is a white queen on index 12 that is e2, then the 12th element in this array would be 3 since 3 is the WhiteQueen value on piece enum.

        if(move.Flag == (int)Move.MoveFlag.enPassantCapture)
        {
            capturedPieceType = (int)Board.Piece.WhitePawns + (board.colorToMove^1) * 6;
        }

        if((move.Flag >= (int)Move.MoveFlag.promoteToQueen) && (move.Flag <= (int)Move.MoveFlag.promoteToBishop))
        {
            int promotionScore = 500;
            if(capturedPieceType != -1)
            {
                promotionScore += Evaluation.mvvLva[pieceTypeMap[movingPiece], pieceTypeMap[capturedPieceType]];
            }
            return promotionScore; 
        }

        if(capturedPieceType != -1)
        {
            int capturedPieceIndex = pieceTypeMap[capturedPieceType];
            int movingPieceIndex = pieceTypeMap[movingPiece];

            // int capturedPieceValue = pieceValues[capturedPieceType];
            // int movingPieceValue = pieceValues[movingPiece];
            
            // //MVV - LVA. Most valuable capture, least valuable attacker

            return Evaluation.mvvLva[movingPieceIndex, capturedPieceIndex];
        }

        if (capturedPieceType == -1) //add scores to quiet moves
        {
            if (move.Value == killerMoves[ply, 0])
            {
                return 90;
            }
            else if (move.Value == killerMoves[ply, 1])
            {
                return 80;
            }
            
            if((move.Flag >= (int)Move.MoveFlag.whiteKingSideCastle) && (move.Flag <= (int)Move.MoveFlag.blackQueenSideCastle))
            {
                return 79; 
            }
            
            return (historyMoves[board.pieceOnSquare[move.StartSquare], move.TargetSquare] * 78) >> 14;
        }
        
        return 0;
    }

    // public Move GetBestMove(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth)
    // {
    //     // 1. Reset any global search statistics
    //     nodeCount = 0;
    //     qNodes = 0;
    //     leafCount = 0;
    //     ttMoveFirst = 0;
    //     ttMoveBest = 0;

    //     killerMovesHit = 0;
    //     killerMovesProbed = 0;
    //     Array.Clear(killerMoves, 0, killerMoves.Length);


    //     // 2. Set initial Alpha and Beta bounds to +/- infinity
    //     // Make sure these match or exceed the highest possible scores (like your +/- 100000 for mate)
    //     int infinity = 500000; 
    //     int alpha = -infinity;
    //     int beta = infinity;

    //     // 3. Initiate the search from the root (ply = 0)
    //     int score = NegaMax(board, moveGenerator, evaluation, depth, alpha, beta, 0);

    //     // 4. Retrieve the best move from the root of the PV table
    //     Move bestMove = pvTable[0, 0];

    //     string pvString = "";
    //     for (int i = 0; i < pvLength[0]; i++)
    //     {
    //         pvString += BoardUtility.MoveToUci(pvTable[0, i]) + " ";
    //     }

    //     //Print info to the UCI GUI 
    //     Console.WriteLine($"info depth {depth} score cp {score} nodes {nodeCount + qNodes} pv {pvString.TrimEnd()}");
        
    //     double killerHitRate = killerMovesProbed > 0 ? (100.0 * killerMovesHit / killerMovesProbed) : 0;

        
    //      Console.WriteLine($"info string Killer Probes: {killerMovesProbed} | Killer Hits: {killerMovesHit} | Killer Hit Rate: {killerHitRate:F2}%");

    //     return bestMove;
    // }


    public Move GetBestMove(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth, int timeLimitMs = -1)
    {
        nodeCount = 0;
        leafCount = 0;
        qNodes = 0;
        ttProbes = 0;
        ttHits = 0;
        ttCutoffs = 0;
        ttMoveFirst = 0;
        ttMoveBest = 0;
        killerMovesHit = 0;
        killerMovesProbed = 0;
        historyHit = 0;
        historyProbed = 0;
        Array.Clear(killerMoves, 0, killerMoves.Length);
        Array.Clear(historyMoves, 0, historyMoves.Length);

        abortSearch = false;
        allocatedTimeMs = timeLimitMs;



        int infinity = 500000; 
        Move bestMove = new Move(0);
        
        sw = System.Diagnostics.Stopwatch.StartNew();

        // Iterative Deepening Loop
        for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
        {
            int alpha = -infinity;
            int beta = infinity;
            
            // Search the current depth. The TT will pass move ordering from the previous depth!
            int score = NegaMax(board, moveGenerator, evaluation, currentDepth, alpha, beta, 0);
            
            //time logic
            //if time ran out, break out of the loop. Do not update the bestMove and return the bestMove found at previous depth.
            if(abortSearch) break;



            // Grab the best move found at this depth
            bestMove = pvTable[0, 0];

            string pvString = "";
            for (int i = 0; i < pvLength[0]; i++)
            {
                pvString += BoardUtility.MoveToUci(pvTable[0, i]) + " ";
            }

            long totalNodes = nodeCount + qNodes;
            long timeMs = Math.Max(1, sw.ElapsedMilliseconds); // Use Math.Max to avoid division by zero
            long nps = (totalNodes * 1000) / timeMs;

            double hitRate = ttProbes > 0 ? (100.0 * ttHits / ttProbes) : 0;
            double killerHitRate = killerMovesProbed > 0 ? (100.0 * killerMovesHit / killerMovesProbed) : 0;
            double gameKillerHitRate = gameKillerMovesProbed > 0 ? (100.0 * gameKillerMovesHit / gameKillerMovesProbed) : 0;
            double historyHitRate = historyProbed > 0 ? (100.0 * historyHit / historyProbed) : 0;

            //Print info to the UCI GUI 
            // Console.WriteLine($"info string TT Probes: {ttProbes} | TT Hits: {ttHits} | TT Cutoffs: {ttCutoffs} | Hit Rate: {hitRate:F2}%");
            // Console.WriteLine($"info string Killer Probes: {killerMovesProbed} | Killer Hits: {killerMovesHit} | Killer Hit Rate: {killerHitRate:F2}%");
            Console.WriteLine($"info depth {currentDepth} score cp {score} time {timeMs} nodes {totalNodes} nps {nps} pv {pvString.TrimEnd()}");


            //==============================Logging code===================================
            //DISABLE IT IN BENCHMARK RUNS
            
            // if (currentDepth == 8)
            // {
            //     string engineFolder = AppDomain.CurrentDomain.BaseDirectory;
                
            //     // 1. Get the unique Operating System Process ID for this running instance
            //     int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                
            //     // 2. Embed the PID directly into the filename so instances never collide
            //     string filePath = Path.Combine(engineFolder, $"node_counts_depth8_pid_{pid}.csv");

            //     // 3. Standard write check
            //     if (!System.IO.File.Exists(filePath))
            //     {
            //         System.IO.File.WriteAllText(filePath, "Depth,Nodes,TimeMs,HistoryHitRate\n");
            //     }

            //     System.IO.File.AppendAllText(filePath, $"8,{totalNodes},{timeMs},{historyHitRate:F2}\n");
            // }

            // DISABLE IT IN BENCHMARK RUNS
            //==============================Logging code===================================
            


        }

        return bestMove;
    }

    
    void AgeHistoryTable()
    {
        int numPieces = historyMoves.GetLength(0);
        int numSquares = historyMoves.GetLength(1);

        for (int p = 0; p < numPieces; p++)
        {
            for (int sq = 0; sq < numSquares; sq++)
            {
                // Divide the score by 2
                historyMoves[p, sq] >>= 1; 
            }
        }
    }

    public void ClearHistory()
    {
        Array.Clear(historyMoves, 0, historyMoves.Length);

        Array.Clear(killerMoves, 0, killerMoves.Length);
    }


    /*
    MOVE ORDERING

            Attacker - 
    Victim               Rook  Knight  Bishop  Queen  King
                Rook
                
                Knight

                Bishop

                Queen

                King

    */


}