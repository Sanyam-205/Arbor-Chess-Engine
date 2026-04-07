using System;
using static Board;
public class FenUtility
{
    
    //Default fen string for starting pieces = rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
    //Inside of it, rnbqkbnr represent backrank pieces of black (lowercase)
    // / represent next line
    // p (lowercase) represent black pawns
    // 8/ represent an empty rank (horizontal)
    // P (uppercase) represent white pawns
    // RNBQKBNR represent backrank pieces of white (uppercase)
    // w - represent white's turn. It will be b for black's turn
    // K (uppercase) represent White's castling rights on kingside
    // Q (uppercase) represent White's castling rights on queenside    
    // k (lowercase) represent black's castling rights on kingside
    // q (lowercase) represent black's castling rights on queenside    
    // - represent en passant available pieces. If no piece can be captured through en passant, it stays at -
    // 0 represent moves made since last piece capture or pawn move. Used to track 50 move rule
    // 1 represent total legal moves made since the start of the match



    // parsing a Fen 
    // encounters / then decrease the rank, set file to 0
    // encounters a number, increment the file by that number



    // string boardFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public static void LoadFromFen(string boardFen, Board board)
    {
        //initializing castling rights to 0
        board.castlingRights = 0;

        for (int i = 0; i < 64; i++)
        {
            board.pieceOnSquare[i] = -1;
        }
        
        //splitting fen string in parts whenever we encounter a space, it would result in 6 different parts each representing a separate parameter.
        string[] fenParts = boardFen.Split(' ');
        
        //fen part 0 which is actual position of pieces
        int rank = 7;
        int file = 0;
        foreach (char c in fenParts[0])
        {
            if(c == '/') //next line check
            {
                rank--;
                file = 0;
            }
            
            
            //change char.IsDigit(c) to check for ASCII value rather than calling isDigit function for efficiency
            else if(char.IsDigit(c)) //empty squares check
            {
                file += c - '0';
                // using ASCII values since calling a function like char.GetNumericValue would be expensive.
                // ASCII codes for 0 through 9 are 48 to 57. So subtracting ASCII value of '0' which is 48 would instantly convert c to its numeric value
            }
            
            
            
            else //actual piece information
            {
                int square = rank*8 + file;

                switch (c)
                {
                    case 'p': board.pieceBitboards[(int)Piece.BlackPawns] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.BlackPawns;
                    break;

                    case 'P': board.pieceBitboards[(int)Piece.WhitePawns] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.WhitePawns;
                    break;

                    case 'r' : board.pieceBitboards[(int)Piece.BlackRooks] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.BlackRooks;
                    break;
                    
                    case 'R' : board.pieceBitboards[(int)Piece.WhiteRooks] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.WhiteRooks;
                    break;
                    
                    case 'b' : board.pieceBitboards[(int)Piece.BlackBishops] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.BlackBishops;
                    break;
                    
                    case 'B' : board.pieceBitboards[(int)Piece.WhiteBishops] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.WhiteBishops;
                    break;

                    case 'n' : board.pieceBitboards[(int)Piece.BlackKnights] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.BlackKnights;
                    break;
                    
                    case 'N' : board.pieceBitboards[(int)Piece.WhiteKnights] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.WhiteKnights;
                    break;

                    case 'q' : board.pieceBitboards[(int)Piece.BlackQueens] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.BlackQueens;
                    break;
                    
                    case 'Q' : board.pieceBitboards[(int)Piece.WhiteQueens] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.WhiteQueens;
                    break;

                    case 'k' : board.pieceBitboards[(int)Piece.BlackKing] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.BlackKing;
                    break;

                    case 'K' : board.pieceBitboards[(int)Piece.WhiteKing] |= 1UL << square;
                    board.pieceOnSquare[square] = (int)Piece.WhiteKing;
                    break;
                }

                file++;
            }
        }

        //fen part 1, which stores turn 
        board.colorToMove = (fenParts[1] == "w") ? 0 : 1;

        //fen part 2, which stores castling rights
        foreach (char c in fenParts[2])
        {
            switch (c)
            {
                case 'K': board.castlingRights |= 1;
                break;
                case 'Q': board.castlingRights |= 2;
                break;
                case 'k': board.castlingRights |= 4;
                break;
                case 'q': board.castlingRights |= 8;
                break;


                // If White king can castle kingside, then castlingRight will be 0001
                // If White king can castle queenside, then castlingRight will be 0010
                // If Black king can castle kingside, then castlingRight will be 0100
                // If Black king can castle queenside, then castlingRight will be 1000
            }
        }
    

        //fen part 3, en passant active square check
        if(fenParts[3]!= "-")
        {
            int enPassantFile = fenParts[3][0] - 'a';
            int enPassantRank = fenParts[3][1] - '1';

            int square = 8*enPassantRank + enPassantFile; 
            board.enPassantSquare |= 1UL << square;
        }
      

        //combining all bitboards 
        //we do it outside of the foreach loop in fenParts[0] for efficiency.
        board.colorBitboard[(int)PieceTeam.WhitePieces] = board.pieceBitboards[(int)Piece.WhitePawns] | board.pieceBitboards[(int)Piece.WhiteKnights] | board.pieceBitboards[(int)Piece.WhiteBishops] | board.pieceBitboards[(int)Piece.WhiteRooks] | board.pieceBitboards[(int)Piece.WhiteQueens] | board.pieceBitboards[(int)Piece.WhiteKing];
        board.colorBitboard[(int)PieceTeam.BlackPieces] = board.pieceBitboards[(int)Piece.BlackPawns] | board.pieceBitboards[(int)Piece.BlackKnights] | board.pieceBitboards[(int)Piece.BlackBishops] | board.pieceBitboards[(int)Piece.BlackRooks] | board.pieceBitboards[(int)Piece.BlackQueens] | board.pieceBitboards[(int)Piece.BlackKing];
        board.AllPieces = board.colorBitboard[(int)PieceTeam.WhitePieces] | board.colorBitboard[(int)PieceTeam.BlackPieces];



    }










}
