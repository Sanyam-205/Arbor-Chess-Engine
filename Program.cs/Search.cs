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

    const int MaxPly = 256;
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
        return NegaMax(board, moveGenerator, evaluation, depth, alpha, beta, ply);
    }

    public long ttProbes, ttHits, ttCutoffs;
    public long ttMoveFirst;
    public long ttMoveBest;


    public int NegaMax(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth, int alpha, int beta, int ply) 
    {
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
        int moveCount = 0;
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


            // // Make sure the move isn't a blank move before rewarding it 1,000,000 points!
            if (newMove.Value == bestMoveThisNode.Value && bestMoveThisNode.Value != 0) 
            {
                moveScore[i] = 15000; 
            }

            else
            {
                moveScore[i] = ScoreMove(newMove, board);
            }

            //========================================================================================================
            //                                               TT
            //========================================================================================================

            
            // moveScore[i] = ScoreMove(newMove, board);

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
                /*Beta cutoff in alpha beta pruning and TT are somewhat different. In alpha beta pruning, the beta cutoff is telling us that if a line is too good for us and the opponent already has another line that whose score is less than the score of this line then the opponent will never let us enter that path.
                That path is 'too good' because up in the tree, the opponent has another line that leads to a better evaluation for them
                A better, guaranteed evaluation for the opponent is the beta score..
                
                To understand TT, TT does not care if the opponent will ever let us enter than line. That is managed by alpha beta pruning. TT only cares to return the evaluation that we were going to get if that line were to happen.
                TT tells us that if that line were to happen, then you could get atleast this much score. So this becomes our lowerbound. 

                Imagine alpha = 20, beta = 50, score = 90.
                alpha being 20 means that out current best score is alpha. Beta being 50 means that the opponent already has a line that forces our score to be 50. Score being 90 means that if we were to go down that line than we could get an evaluation of 90.
                In alpha beta pruning, this is too good so opponent will never allow it.
                In TT however, if we were to ever go down that line then 90 is the score we are guaranteed to have so that 90 becomes our lowerbound flag which is hashBeta.
                
                */


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


        }


        // if (bestMoveThisNode.Value != 0)
        // {
        //     bool found = false;

        //     for(int i = 0; i < moveCount; i++)
        //     {
        //         if(moveList[i].Value == bestMoveThisNode.Value)
        //         {
        //             found = true;
        //             break;
        //         }
        //     }

        //     if(!found)
        //     {
        //         Console.WriteLine("TT move not found!");
        //         Console.Out.Flush();
        //         Environment.Exit(1);
        //     }
        // }

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



    //Quiescence function
    // public int quiescence(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int alpha, int beta, int ply)
    // {
    //     pvLength[ply] = 0;

    //     int kingSquare1 = board.GetKingSquare(board.colorToMove);

    //     bool isCheck = board.IsSquareAttacked(kingSquare1, board.colorToMove);

    //     if(isCheck)
    //     {
    //         //do something
    //         // if the king is in check we need to discard the standPat score. If a king in check should be treated as a noisy move even though it is quiet. The problem without thst is if a king is in check, quiescence only generates noisy moves (captures, promotions) and it completely disregards checks.

    //         Move[] moveList = new Move[256];
    //         int moveCount = 0;
    //         moveGenerator.GenerateAllPseudoLegalMoves(board, moveList, ref moveCount);
    //         int legalMoves = 0;

    //         for(int i = 0; i<moveCount; i++)
    //         {
    //             Move move = moveList[i];

    //             board.MakeMove(move);
                
    //             if((kingSquare1!= -1) && board.IsSquareAttacked(kingSquare1, board.colorToMove ^ 1))
    //             {
    //                 board.UnmakeMove(move);
    //                 continue;
    //             }
    //             legalMoves++;
    //         }

    //         if(legalMoves == 0) return -100000;
    //     }



    //     else //king is not in check.
    //     {
    //         int standPat = evaluation.EvaluatePosition(board);
    //         if(standPat >= beta) return beta;

    //         //alpha = max of alpha, standPat
    //         alpha = (alpha > standPat) ? alpha : standPat;

    //         Move[] moveList = new Move[256];
    //         int moveCount = 0;
    //         moveGenerator.GenerateAllPseudoLegalCaptures(board, moveList, ref moveCount);

    //         int legalMovesPlayed = 0;

    //         int[] moveScore = new int[moveCount];
    //         for(int i = 0; i<moveCount; i++)
    //         {
    //             Move newMove = moveList[i];
    //             moveScore[i] = ScoreMove(newMove, board);
    //         }

    //         for(int i = 0; i<moveCount; i++)
    //         {
    //             int bestMoveIndex = i;

    //             for(int j = i; j < moveCount; j++)
    //             {
    //                 if(moveScore[j] > moveScore[bestMoveIndex])
    //                 {
    //                     bestMoveIndex = j;
    //                 }
    //             }

    //             Move tempMove = moveList[i];
    //             moveList[i] = moveList[bestMoveIndex];
    //             moveList[bestMoveIndex] = tempMove;

    //             int temp = moveScore[i];
    //             moveScore[i] = moveScore[bestMoveIndex];
    //             moveScore[bestMoveIndex] = temp;

    //             Move move = moveList[i];
    //             board.MakeMove(move);
    //             int colorThatJustMoved = board.colorToMove ^ 1;
    //             int kingSquare = board.GetKingSquare(colorThatJustMoved);

    //             if((kingSquare!= -1) && board.IsSquareAttacked(kingSquare, colorThatJustMoved))
    //             {
    //                 board.UnmakeMove(move);
    //                 continue;
    //             }

    //             board.UnmakeMove(move);
    //             legalMovesPlayed++;

    //             int score = -quiescence(board, moveGenerator, evaluation, -beta, -alpha, ply+1);
            
    //             board.UnmakeMove(move);

    //             if(score>=beta)
    //             {
    //                 return score; //cutoff
    //             }

    //             if(score > alpha)
    //             {
    //                 alpha = score;

    //                 pvTable[ply, 0] = move;

    //                  // 2. Copy the sequence of moves from the deeper ply
    //                 for (int j = 0; j < pvLength[ply + 1]; j++)
    //                 {
    //                     pvTable[ply, j + 1] = pvTable[ply + 1, j];
    //                 }

    //                 // 3. Update the length of the sequence for this ply
    //                 pvLength[ply] = pvLength[ply + 1] + 1;


    //             }
    //         }
    //     }

    //     return alpha;
    // }


    public int Quiescence (Board board, MoveGenerator moveGenerator, Evaluation evaluation, int alpha, int beta, int ply)
    {
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
            moveScore[i] = ScoreMove(moveList[i], board); //assign scores to moves and store those in moveScore array
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
#endregion




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

#endregion
        if(inCheck && legalMovesPlayed == 0)
        {
            return -100000 + ply;
            
        }


        return bestScore;

        // return alpha;
    }





    //Helper function
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



    public int ScoreMove(Move move, Board board)
    {

        // int[] pieceValues = {5, 3, 3, 9, 10000, 1, 5, 3, 3, 9, 10000, 1};
        int[] pieceTypeMap = {0, 1, 2, 3, 4, 5, 0, 1, 2, 3, 4, 5}; //rook, knight, bishop, queen, king, pawn

        if((move.Flag >= (int)Move.MoveFlag.promoteToQueen) && (move.Flag <= (int)Move.MoveFlag.promoteToBishop))
        {
            return 500; //Will fine tune this value later.
        }
        if((move.Flag >= (int)Move.MoveFlag.whiteKingSideCastle) && (move.Flag <= (int)Move.MoveFlag.blackQueenSideCastle))
        {
            return 50; //will fine tune this value later.
        }

        int movingPiece = board.pieceOnSquare[move.StartSquare];
        int capturedPieceType = board.pieceOnSquare[move.TargetSquare];
        //pieceOnSquare stores the piece type for every square. If there is a white queen on index 12 that is e2, then the 12th element in this array would be 3 since 3 is the WhiteQueen value on piece enum.

        if(move.Flag == (int)Move.MoveFlag.enPassantCapture)
        {
            capturedPieceType = (int)Board.Piece.WhitePawns + (board.colorToMove^1) * 6;
        }

        if(capturedPieceType != -1)
        {
            int capturedPieceIndex = pieceTypeMap[capturedPieceType];
            int movingPieceIndex = pieceTypeMap[movingPiece];

            // int capturedPieceValue = pieceValues[capturedPieceType];
            // int movingPieceValue = pieceValues[movingPiece];
            
            // //MVV - LVA. Most valuable capture, least valuable attacker.
            // //1000 is a base value
            // // return 1000 + (10 * capturedPieceValue) - movingPieceValue;

            return Evaluation.mvvLva[movingPieceIndex, capturedPieceIndex];
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

    //     return bestMove;
    // }


    public Move GetBestMove(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth)
    {
        nodeCount = 0;
        leafCount = 0;
        qNodes = 0;
        ttProbes = 0;
        ttHits = 0;
        ttCutoffs = 0;
        ttMoveFirst = 0;
        ttMoveBest = 0;

        int infinity = 500000; 
        Move bestMove = new Move(0);
        
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        // Iterative Deepening Loop
        for (int currentDepth = 1; currentDepth <= depth; currentDepth++)
        {
            int alpha = -infinity;
            int beta = infinity;
            
            // Search the current depth. The TT will pass move ordering from the previous depth!
            int score = NegaMax(board, moveGenerator, evaluation, currentDepth, alpha, beta, 0);
            
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

            //Print info to the UCI GUI 
            Console.WriteLine($"info string TT Probes: {ttProbes} | TT Hits: {ttHits} | TT Cutoffs: {ttCutoffs} | Hit Rate: {hitRate:F2}%");
            // FIX: Print 'currentDepth' instead of 'depth' so the GUI sees the progression
            Console.WriteLine($"info depth {currentDepth} score cp {score} time {timeMs} nodes {totalNodes} nps {nps} pv {pvString.TrimEnd()}");


            //==============================Logging code===================================
            //DISABLE IT IN BENCHMARK RUNS
            if (currentDepth == 8)
            {
                string engineFolder = AppDomain.CurrentDomain.BaseDirectory;
                
                // 1. Get the unique Operating System Process ID for this running instance
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                
                // 2. Embed the PID directly into the filename so instances never collide
                string filePath = Path.Combine(engineFolder, $"node_counts_depth8_pid_{pid}.csv");

                // 3. Standard write check
                if (!System.IO.File.Exists(filePath))
                {
                    System.IO.File.WriteAllText(filePath, "Depth,Nodes,TimeMs\n");
                }

                System.IO.File.AppendAllText(filePath, $"8,{totalNodes},{timeMs}\n");
            }
            //DISABLE IT IN BENCHMARK RUNS
            //==============================Logging code===================================
            


        }

        return bestMove;
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