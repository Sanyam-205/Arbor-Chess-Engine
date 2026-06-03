using System;
using System.Numerics;
using System.Text.RegularExpressions;
using static Board;

public class Search
{
    public int nodeCount, leafCount;
    //alpha is the highest score the AI is guaranteed to achieve. Good for AI
    //beta is the lowest score the opponent can have. Good for human
    /*
                depth 0
           (A)  /     \  (B)
               3       1
              / \     / \
            -1   3  -5   1          
    White to move. +ve value= = better for white. -ve value = better for black. This is standard mimimax. Negamax is slightly different which will be explained later.
    
    In this example, for branch A, alpha is 3 which is the best score for white and beta is -1, the best score for black.
    For branch B, alpha is 1, the best score for white and beta is -5, the best score for black. We always assume that the opponent makes the best move for them leading to highest beta value.
    Beta can also be explained as the absolute best score we can have for that particular branch. For example, if -1 and -5 both continued downwards, these two scores would be the absolute best we would be looking to get if we went down those paths.
    Since at depth 0, the function must return the move order leading to best possible evaluation for you meaning the branch with best Alpha value. 
    
    */

    //debug code
    public int StartSearch(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth, int alpha, int beta)
    {
        nodeCount = 0; // Clear the board for the new search
        leafCount = 0;
        return NegaMax(board, moveGenerator, evaluation, depth, alpha, beta);
    }

    public int NegaMax(Board board, MoveGenerator moveGenerator, Evaluation evaluation, int depth, int alpha, int beta) 
    {
        nodeCount++;//debug code

        if (depth == 0) 
        {
            leafCount++;
            return evaluation.EvaluatePosition(board);
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
            moveScore[i] = ScoreMove(newMove, board);
        }


        //Apply selection sort to the moveScore array 

        //run moves in moveList through the legality check
        for (int i = 0; i < moveCount; i++)
        {
            //Selection sort
        #region selection sort    
            
            
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
            
            // continuation of search
            Move move = moveList[i];
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
            int score = -NegaMax(board, moveGenerator, evaluation, depth - 1, -beta, -alpha);
        
            board.UnmakeMove(move);

            //Alpha-Beta pruning
        #region alpha beta pruning

            if (score >= beta) 
            {
                return score; //If this branch leads to a worse outcome, do not consider it. Return beta and get out of the path.
                //There is some confusion as to returning beta or score. I don't understand it yet but returning score is better because it gives more information somehow. I don't know.
            }
            if (score > alpha) 
            {
                alpha = score; //We found a better move for ourselves
            }

        #endregion    

        }

        //STALEMATE, CHECKMATE CHECK.
        if(legalMovesPlayed == 0)
        {
            int colorToMove = board.colorToMove;
            int currentKingSquare = board.GetKingSquare(colorToMove);

            if((currentKingSquare != -1) && board.IsSquareAttacked(currentKingSquare, colorToMove))
            {
                return -9999; //checkmate
            }
            else
            {
                return 0; //stalemate
            }
        }

        return alpha;
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