//pure c# script. No unity element. 
using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml;

public struct BoardState
{
    public int capturedPieceType;
    public ulong enPassantSquare;
    public int castlingRights;
}

public class Board
{
    
    public BoardState[] history = new BoardState[1024];
    public int plyCount = 0;

    //combined 64 bit integers for black and white pieces
    public ulong AllPieces;

    public int colorToMove, castlingRights;
    public ulong enPassantSquare;


    public static int[] castlingRightsUpdate = new int[64]; //stores bord information for the specific squares for en passant.
    static readonly ulong[] rookCastleMasks =
    {
        0b0000000000000000000000000000000000000000000000000000000010100000, // white kingside       Flag = 7     
        0b0000000000000000000000000000000000000000000000000000000000001001, // white queenside      Flag = 8 

        0b1010000000000000000000000000000000000000000000000000000000000000, // black kingside       Flag = 9
        0b0000100100000000000000000000000000000000000000000000000000000000 // black queenside       Flag = 10
    }; 

    

    //list containing 12 ulong integers. It is used to store separate ulong values for each peiece type. We store them in a list because it is much faster to get to a specific index in a list compared to 12 different if, else if branches.
    public ulong[] pieceBitboards = new ulong[12];
    
    //We create a enum Piece because in our list, each index, stores a value for a piece. Since remembering index 0 stores white rook values, 1 stores white knights values isn't exactly human friendly, we create a enum and call the list using (int)Piece.WhiteRooks.

    //-------------------------------------------------------------------------------------------------------------------------------------------//
    /// White rook, knight, bishop and queen being exactly 6 places apart from their black piece counterparts is what allows the promotion code 
    // (int finalPieceIndex = baseIndex + (colorToMove * 6);) to work. It's fragile but it works. SO DONT CHANGE
    //-------------------------------------------------------------------------------------------------------------------------------------------//

    public ulong[] colorBitboard = new ulong[2];

    public enum PieceTeam
    {
        WhitePieces, BlackPieces
    }

    public enum Piece
    {
        WhiteRooks, WhiteKnights, WhiteBishops, WhiteQueens, WhiteKing, WhitePawns, BlackRooks, BlackKnights, BlackBishops, BlackQueens, BlackKing, BlackPawns
    }

    /*
        0   WhiteRooks, 
        1   WhiteKnights, 
        2   WhiteBishops, 
        3   WhiteQueens, 
        4   WhiteKing, 
        5   WhitePawns, 
        6   BlackRooks, 
        7   BlackKnights, 
        8   BlackBishops, 
        9   BlackQueens, 
        10  BlackKing, 
        11  BlackPawns
    */

    
    public void MakeMove(Move move)
    {
        //Generate Masks for starting and ending square
        ulong startMask = 1UL << move.StartSquare;
        ulong targetMask = 1UL << move.TargetSquare;

        ulong moveMask = (1UL << move.StartSquare) | (1UL << move.TargetSquare);

        int movingPiece = -1;
        int capturedPiece = -1;

        for (int i = 0; i < 12; i++)
        {
            if ((startMask &  (pieceBitboards[i])) != 0) movingPiece = i;
            if ((targetMask & (pieceBitboards[i])) != 0) capturedPiece = i;
        }



        //Take snapshot of the current board state
        history[plyCount].capturedPieceType = capturedPiece;
        history[plyCount].enPassantSquare = enPassantSquare; 
        history[plyCount].castlingRights = castlingRights;   
        plyCount++;
        
        enPassantSquare = 0; // set enPassantSquare back to 0 at start of new move

        //Capture
        if(capturedPiece != -1) //Remove the captured piece
        {
            pieceBitboards[capturedPiece] &= ~targetMask; 
        }
        

        //teleport the piece (normal move)
        pieceBitboards[movingPiece] ^= moveMask; // teleport the piece

        //teleport the rook if castled
        if(move.Flag >= (int)Move.MoveFlag.whiteKingSideCastle && move.Flag <= (int)Move.MoveFlag.blackQueenSideCastle)
        {
            // if(move.Flag == (int)Move.MoveFlag.whiteKingSideCastle)
            // {
            //     ulong rookMask = 0000000000000000000000000000000000000000000000000000000000000100; // white kingside
            //     pieceBitboards[(int)Piece.WhiteRooks] ^= 0000000000000000000000000000000000000000000000000000000000000001;
            //     pieceBitboards[(int)Piece.WhiteRooks] |= rookMask;
            
            // }

            int rookIndex = (int)Piece.WhiteRooks + (colorToMove * 6);
            pieceBitboards[rookIndex] ^= rookCastleMasks[move.Flag - 7];

        }



        //Promotion
        if(move.Flag >= (int)Move.MoveFlag.promoteToQueen && move.Flag <= (int)Move.MoveFlag.promoteToBishop)
        {
            //Piece already teleported.
            //Remove the piece from its bitboard (pawn)
            pieceBitboards[movingPiece] ^= targetMask;


            int baseIndex = Move.flagToBaseIndex[move.Flag];
            int finalPieceIndex = baseIndex + (colorToMove * 6);


            //Add it to promoted piece bitboard depending upon flag
            pieceBitboards[finalPieceIndex] |= targetMask;
        }


        //En Passant
        int enemyPawn = (colorToMove == 0) ? (int)Piece.BlackPawns : (int)Piece.WhitePawns;
        ulong enPassantVictimMask = (colorToMove == 0) ? targetMask >> 8 : targetMask << 8;
        pieceBitboards[enemyPawn] ^= enPassantVictimMask;

        //double pawn push
        if(((movingPiece == (int)Piece.WhitePawns) || (movingPiece == (int)Piece.BlackPawns)) && (move.StartSquare ^ move.TargetSquare) == 16)
        {
            int skippedIndex = (colorToMove == 0) ? move.StartSquare + 8 : move.StartSquare - 8;
            enPassantSquare = 1UL << skippedIndex;
        }





        //Castling
        castlingRights &= castlingRightsUpdate[move.StartSquare];
        castlingRights &= castlingRightsUpdate[move.TargetSquare];



        //Turn switch
        colorToMove ^= colorToMove;


    }

    public void UnMakeMove (Move move)
    {

        colorToMove ^= colorToMove;
        plyCount--;

        int prevCapturedPiece = history[plyCount].capturedPieceType;
        castlingRights = history[plyCount].castlingRights; // Restore state
        enPassantSquare = history[plyCount].enPassantSquare; // Restore state

        int movingPiece = -1;

        ulong targetMask = (1UL << move.TargetSquare);
        
        for(int i = 0; i<12; i++)
        {

            movingPiece = i;


            if((targetMask & (pieceBitboards[i])) != 0)
            {

                if (move.Flag >= (int)Move.MoveFlag.promoteToQueen && move.Flag <= (int)Move.MoveFlag.promoteToBishop) //promotion
                {
                    //Remove the piece from the move.TargetSquare
                    pieceBitboards[i] ^= targetMask;

                    //Place the pawn back on the previous square
                    pieceBitboards[(int)Piece.WhitePawns + (colorToMove * 6)] |= targetMask;
                    movingPiece = (int)Piece.WhitePawns + (colorToMove * 6);

                      
                }
                
                else if(move.Flag >= (int)Move.MoveFlag.whiteKingSideCastle && move.Flag <= (int)Move.MoveFlag.blackQueenSideCastle) //castle
                {
                    
                    int rookIndex = (int)Piece.WhiteRooks + (colorToMove * 6);
                    pieceBitboards[rookIndex] ^= rookCastleMasks[move.Flag - 7];
                }

                else if(move.Flag == (int)Move.MoveFlag.enPassantCapture) // en passant
                {
                    int pawnTypeToRestore = (colorToMove == 0) ? (int)Piece.BlackPawns : (int)Piece.WhitePawns;
                    ulong enPassantVictimMask = (colorToMove == 0) ? targetMask >> 8 : targetMask << 8;
                    pieceBitboards[pawnTypeToRestore] ^= enPassantVictimMask;
                }


                break;
                
            }
            
        }

        ulong moveMask = (1UL << move.StartSquare) | (1UL << move.TargetSquare);
        pieceBitboards[movingPiece] ^= moveMask; // Remove the piece

        //Put the piece back
        if(prevCapturedPiece != -1)
        {
            pieceBitboards[prevCapturedPiece] |= targetMask;
        }
    }




     
    public bool IsSquareAttacked(int square, int defendingColor)
    {
        //if we have to check for white king is under attack, we use the whitePawnAttack table as pawn mask. 
        // We do so because if the white king is on square 20, it can be attacked by a black pawn on square 27 or 29. Since a white pawn on square 20 also attacks square 27 and 29, we reverse it's logic. 
        // If whitePawnAttack table for the square coincides with black pawn on that square, the king on that square will be attacked.
        ulong pawnMask = (defendingColor == 0)? AttackTables.whitePawnAttacks[square] : AttackTables.blackPawnAttacks[square];
        ulong enemyPawnBitboard = (defendingColor == 0) ? pieceBitboards[(int)Piece.BlackPawns] : pieceBitboards[(int)Piece.WhitePawns]; 
        if((pawnMask & enemyPawnBitboard) != 0)
        {
            return true;
        }  

        //Check if a square is under attack by a knight
        ulong enemyKnightBitboard = (defendingColor == 0) ? pieceBitboards[(int)Piece.BlackKnights] : pieceBitboards[(int)Piece.WhiteKnights];
        ulong knightMask = AttackTables.knightAttacks[square];
        if((knightMask & enemyKnightBitboard) !=0)
        {
            return true;
        }
        
        
        // check if a square is under attack by a king. 
        ulong enemyKingBitboard = (defendingColor == 0) ? pieceBitboards[(int)Piece.BlackKing] : pieceBitboards[(int)Piece.WhiteKing];
        ulong kingMask = AttackTables.kingAttacks[square];
        if((kingMask & enemyKingBitboard) !=0)
        {
            return true;
        }
        

        //check if a square is under attack by the sliders
        ulong diagonalMask = AttackTables.GetBishopAttacks(square, AllPieces);
        ulong enemyBishop = (defendingColor == 0) ? pieceBitboards[(int)Piece.BlackBishops] : pieceBitboards[(int)Piece.WhiteBishops];
        ulong enemyQueen = (defendingColor == 0) ? pieceBitboards[(int)Piece.BlackQueens] : pieceBitboards[(int)Piece.WhiteQueens];
        if((diagonalMask & (enemyBishop|enemyQueen)) != 0)
        {
            return true;
        }

        ulong straightMak = AttackTables.GetRookAttacks(square, AllPieces);
        ulong enemyRook = (defendingColor == 0) ? pieceBitboards[(int)Piece.BlackRooks] : pieceBitboards[(int)Piece.WhiteRooks];
        if((straightMak &(enemyRook|enemyQueen))!=0)
        {
            return true;
        }
        





        return false;
    }


    public static void Innit()
    {
        for (int i = 0; i < 64; i++)
        {
            castlingRightsUpdate[i] = 15;
        }

        castlingRightsUpdate[4] = 12; // white king 
        castlingRightsUpdate[7] = 14; // rook h1
        castlingRightsUpdate[0] = 13; // rook a1
        
        castlingRightsUpdate[60] = 3; // black king
        castlingRightsUpdate[63] = 11; // rook h8
        castlingRightsUpdate[56] = 7; // rook a8


        /// 1111 - No castling desabled
        
        ///  14     1110 - White kindside castling disabled
        ///  13     1101 - White queenside castling disabled
        ///  12     1100 - White castling disabled 
              
        ///  11     1011 - Black kingside castling disabled
        ///   7     0111 - Black queenside castling disabled
        ///   3     0011 - Black castling disabled
    }


}
