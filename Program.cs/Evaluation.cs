using System;
using System.Numerics;
using static Board;
public class Evaluation
{
    public int EvaluatePosition(Board board)
    {
        int score = 
        340 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteBishops]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackBishops])) + 
        320 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteKnights]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackKnights])) + 
        900 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteQueens]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackQueens])) + 
        500 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteRooks]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackRooks])) + 
        100 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhitePawns]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackPawns]));


        return score;
    }
}