using System;
using static Board;

public class BoardPrinter
{
    
    public static void PrintBitboard(Board board)
    {
        string output = ""; //Take an empty string which can be replaced with bitboard data

        //loop backwards for rank because computer prints from the top. If we looped in increment, then the first rank would be printed at the top of the output.
        for (int rank = 7; rank >= 0; rank--) 
        {
            for(int file = 0; file <= 7; file++ )
            {
                int square = rank*8 + file;
                output += GetPieceAtSquare(square, board) + " ";
            }
            output += "\n";

        }

        Console.WriteLine(output);

    }


    static char GetPieceAtSquare(int square, Board board)
    {
        // ulong mask = 1UL << square; 
        // if ((board.pieceBitboards[(int)Piece.WhitePawns] & mask) != 0) return 'P';
        // if ((board.pieceBitboards[(int)Piece.WhiteRooks] & mask) != 0) return 'R';
        // if ((board.pieceBitboards[(int)Piece.WhiteBishops] & mask) != 0) return 'B';
        // if ((board.pieceBitboards[(int)Piece.WhiteKnights] & mask) != 0) return 'N';
        // if ((board.pieceBitboards[(int)Piece.WhiteQueens] & mask) != 0) return 'Q';
        // if ((board.pieceBitboards[(int)Piece.WhiteKing] & mask) != 0) return 'K';
        // if ((board.pieceBitboards[(int)Piece.BlackPawns] & mask) != 0) return 'p';
        // if ((board.pieceBitboards[(int)Piece.BlackRooks] & mask) != 0) return 'r';
        // if ((board.pieceBitboards[(int)Piece.BlackBishops] & mask) != 0) return 'b';
        // if ((board.pieceBitboards[(int)Piece.BlackKnights] & mask) != 0) return 'n';
        // if ((board.pieceBitboards[(int)Piece.BlackQueens] & mask) != 0) return 'q';
        // if ((board.pieceBitboards[(int)Piece.BlackKing] & mask) != 0) return 'k';

        // return '.';

        int piece = board.pieceOnSquare[square];
        if (piece == -1) return '.';
        
        char[] pieceChars = { 'R', 'N', 'B', 'Q', 'K', 'P', 'r', 'n', 'b', 'q', 'k', 'p' };
        return pieceChars[piece];

    }







}