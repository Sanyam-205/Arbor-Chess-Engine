using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Numerics;
using System.Collections;

public struct BoardState
{
    public int capturedPieceType;
    public ulong enPassantSquare;
    public int castlingRights;
    public ulong currentHash;
}

public class Board
{
    static Board()
    {
        Innit();
    }


    public BoardState[] history = new BoardState[1024];
    public int plyCount = 0, phaseScore;

    //combined 64 bit integers for black and white pieces
    public ulong occupiedMask;

    public int colorToMove, castlingRights;
    public ulong enPassantSquare;
    public ulong currentHash;


    public static int[] castlingRightsUpdate = new int[64]; //stores bord information for the specific squares for castling.
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


    public int[] pieceOnSquare = new int[64];

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

    public int[] PieceValue = {500, 330, 340, 900, 1000, 100, 500, 330, 340, 900, 1000, 100}; //map the piece value to piece type


    public static readonly int[] PiecePhaseWeights = 
    {
        2,1,1,4,0,0,2,1,1,4,0,0
    };

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

        // Console.WriteLine($"MakeMove Called: Start = {move.StartSquare}, Target = {move.TargetSquare}, Flag = {move.Flag}");
        //Generate Masks for starting and ending square
        ulong startMask = 1UL << move.StartSquare;
        ulong targetMask = 1UL << move.TargetSquare;

        ulong moveMask = (1UL << move.StartSquare) | (1UL << move.TargetSquare);


#region debug
// if (move.StartSquare < 0 || move.StartSquare > 63)
// {
//     Console.WriteLine("=================================");
//     Console.WriteLine("FATAL: GARBAGE MOVE DETECTED!");
//     Console.WriteLine($"StartSquare value: {move.StartSquare}");
    
//     // If your Move struct has a raw integer value (like move.Value or move.moveValue), 
//     // uncomment the next line and put the correct variable name:
//     // Console.WriteLine($"Raw Move Integer: {move.Value}"); 
    
//     Console.WriteLine("=================================");
//     Console.Out.Flush();
//     Environment.Exit(1); // Kills the engine safely so you can read the terminal
// }
#endregion

        

        int movingPiece = pieceOnSquare[move.StartSquare];
        int capturedPiece = pieceOnSquare[move.TargetSquare];

#region debug
        // if (movingPiece < 0 || movingPiece >= pieceBitboards.Length)
        // {
        //     Console.WriteLine("=================================");
        //     Console.WriteLine("FATAL: GARBAGE MOVE CAUGHT!");
        //     Console.WriteLine($"StartSquare was: {move.StartSquare}");
        //     Console.WriteLine($"movingPiece evaluated to: {movingPiece} (Out of Bounds!)");
        //     Console.WriteLine($"Raw Move Integer: {move.StartSquare}, {move.TargetSquare}, {move.Flag}");
        //     // BoardUtility.PrintUlongBitboard(pieceBitboards[(int)Piece.WhiteKnights]);
        //     // BoardPrinter.PrintBitboard(board);
        //     Console.WriteLine("=================================");
        //     Console.Out.Flush();
        //     Environment.Exit(1);
        // }
    #endregion

        //Take snapshot of the current board state
        history[plyCount].capturedPieceType = capturedPiece;
        history[plyCount].enPassantSquare = enPassantSquare; 
        history[plyCount].castlingRights = castlingRights;
        history[plyCount].currentHash = currentHash;   
        plyCount++;
        
        //hash key removal for en passant
        //If the enPassantSquare currently holds the value, then wipe that value clean from the hash key.
        if(enPassantSquare != 0)
        {
            int enPassantFile = (BitOperations.TrailingZeroCount(enPassantSquare)) % 8;
            currentHash ^= Zobrist.enPassantKeys[enPassantFile];
        }

        enPassantSquare = 0; // set enPassantSquare back to 0 at start of new move

        //Capture
        if(capturedPiece != -1) 
        {
            phaseScore -= PiecePhaseWeights[capturedPiece];
            occupiedMask &= ~targetMask; // Erase the piece from global occupancy

            /*
                If a piece moves from square 12 to 20 and captures a piece on square 20, we have to remove the piece at square 20 from global occupancy. Since we do AllPieces ^= moveMask, to update global occupancy, it will work in normal moves (no capture), but in case of captures, it wont work. Why?
                Assume a rook on a1(0) captures a piece at a8(56). moveMask will have 0 and 56th index turned on. When we do the standard XOR toggle, it will remove the rook from a1, but instead of it being on a8, it will remove the rook as well as the piece on a8. To fix this, before removing the piece from a8, we do AllPieces &= ~targetMask. It will turn the exact bit where a piece lands while capturing off on AllPieces.   
            */

            ///////////////////////////////////////////////////////////////////

            pieceBitboards[capturedPiece] &= ~targetMask; //Remove the captured piece 

            colorBitboard[colorToMove ^ 1] &= ~targetMask;
            
            // Remove the captured piece from the hash
            currentHash ^= Zobrist.pieceHash[move.TargetSquare][capturedPiece];
        }
        

        //teleport the rook if castled
        if(move.Flag >= (int)Move.MoveFlag.whiteKingSideCastle && move.Flag <= (int)Move.MoveFlag.blackQueenSideCastle)
        {

            int rookIndex = (int)Piece.WhiteRooks + (colorToMove * 6);
            pieceBitboards[rookIndex] ^= rookCastleMasks[move.Flag - 7];
            
            
            occupiedMask ^= rookCastleMasks[move.Flag - 7]; // update occupancy for rook manually   
            colorBitboard[colorToMove] ^= rookCastleMasks[move.Flag - 7];     

            if (move.Flag == (int)Move.MoveFlag.whiteKingSideCastle) 
            { 
                pieceOnSquare[7] = -1; pieceOnSquare[5] = rookIndex; 
                currentHash ^= Zobrist.pieceHash[7][rookIndex];
                currentHash ^= Zobrist.pieceHash[5][rookIndex];
            }
            else if (move.Flag == (int)Move.MoveFlag.whiteQueenSideCastle) 
            {   
                pieceOnSquare[0] = -1; pieceOnSquare[3] = rookIndex; 
                currentHash ^= Zobrist.pieceHash[0][rookIndex];
                currentHash ^= Zobrist.pieceHash[3][rookIndex];
            }
            else if (move.Flag == (int)Move.MoveFlag.blackKingSideCastle) 
            {
                pieceOnSquare[63] = -1; pieceOnSquare[61] = rookIndex; 
                currentHash ^= Zobrist.pieceHash[63][rookIndex];
                currentHash ^= Zobrist.pieceHash[61][rookIndex];
            }
            else if (move.Flag == (int)Move.MoveFlag.blackQueenSideCastle) 
            { 
                pieceOnSquare[56] = -1; pieceOnSquare[59] = rookIndex; 
                currentHash ^= Zobrist.pieceHash[56][rookIndex];
                currentHash ^= Zobrist.pieceHash[59][rookIndex];
            }
        


        }



        //Promotion
        if(move.Flag >= (int)Move.MoveFlag.promoteToQueen && move.Flag <= (int)Move.MoveFlag.promoteToBishop)
        {
            //Piece already teleported.

            
            /*pieceBitboards[movingPiece] ^= targetMask; //Remove the piece from its bitboard (pawn)

            

            int baseIndex = Move.flagToBaseIndex[move.Flag];
            int finalPieceIndex = baseIndex + (colorToMove * 6);
            pieceOnSquare[move.TargetSquare] = baseIndex + (colorToMove * 6);


            //Add it to promoted piece bitboard depending upon flag
            pieceBitboards[finalPieceIndex] |= targetMask;*/
        
        
            ulong startMask1 = (1UL << move.StartSquare);
            ulong targetMask1 = (1UL << move.TargetSquare);
            ulong moveMask1 = startMask | targetMask;

            //Remove the pawn hash key from starting square
            currentHash ^= Zobrist.pieceHash[move.StartSquare][movingPiece];


            // 1. Completely remove the Pawn from the Start Square
            pieceBitboards[movingPiece] ^= startMask1;

            // 2. Add the Promoted Piece to the Target Square
            int baseIndex = Move.flagToBaseIndex[move.Flag];
            int finalPieceIndex = baseIndex + (colorToMove * 6);
            pieceBitboards[finalPieceIndex] |= targetMask1;

            //update phase score
            phaseScore += PiecePhaseWeights[finalPieceIndex];

            // 3. Update global occupancies
            occupiedMask ^= moveMask;
            colorBitboard[colorToMove] ^= moveMask1;

            // 4. Update the pieceOnSquare array correctly
            pieceOnSquare[move.StartSquare] = -1; // Clear the start square
            pieceOnSquare[move.TargetSquare] = finalPieceIndex;

            //Update hash key for promoted piece
            currentHash ^= Zobrist.pieceHash[move.TargetSquare][finalPieceIndex]; 
        
        
        }


        //En Passant
        if(move.Flag == (int)Move.MoveFlag.enPassantCapture)
        {
            int enemyPawn = (colorToMove == 0) ? (int)Piece.BlackPawns : (int)Piece.WhitePawns;
            ulong enPassantVictimMask = (colorToMove == 0) ? targetMask >> 8 : targetMask << 8;
            pieceBitboards[enemyPawn] ^= enPassantVictimMask;
            occupiedMask &= ~enPassantVictimMask; // Erase the pawn from global occupancy. Same logic as normal capture toggle, but since in en passant, the enemy pawn isn't at targetMask but at enPassantVictimMask, we use this.

            colorBitboard[colorToMove ^ 1] &= ~enPassantVictimMask;

            int epVictimSquare = (colorToMove == 0) ? move.TargetSquare - 8 : move.TargetSquare + 8;

            pieceOnSquare[epVictimSquare] = -1;

            // Correctly remove the enemy pawn that was on the victim square
            currentHash ^= Zobrist.pieceHash[epVictimSquare][enemyPawn];

        }

        //double pawn push
        if(((movingPiece == (int)Piece.WhitePawns) || (movingPiece == (int)Piece.BlackPawns)) && (move.StartSquare ^ move.TargetSquare) == 16)
        {
            int skippedIndex = (colorToMove == 0) ? move.StartSquare + 8 : move.StartSquare - 8;
            enPassantSquare = 1UL << skippedIndex;

            //update zobrist key if double push   
            int enPassantFile = BitOperations.TrailingZeroCount(enPassantSquare) % 8;
            currentHash ^= Zobrist.enPassantKeys[enPassantFile];
        }




        currentHash ^= Zobrist.castlingKeys[castlingRights];//remove old hash keys for castling
        
        //Castling
        castlingRights &= castlingRightsUpdate[move.StartSquare];//update physical rights for castling
        castlingRights &= castlingRightsUpdate[move.TargetSquare];
    
        currentHash ^= Zobrist.castlingKeys[castlingRights];//add new hash keys for caslting


        //teleport the piece / NORMAL MOVE
        if(move.Flag == 0 || move.Flag > (int)Move.MoveFlag.promoteToBishop)
        {
            currentHash ^= Zobrist.pieceHash[move.StartSquare][movingPiece]; //remove the moving piece from start square
            currentHash ^= Zobrist.pieceHash[move.TargetSquare][movingPiece]; //add the moving piece to target square

            pieceBitboards[movingPiece] ^= moveMask; // teleport the piece
        
            occupiedMask ^= moveMask; // updates global occupancy

            colorBitboard[colorToMove] ^= moveMask; // update white or black piece bitboard 
        }

        // pieceBitboards[movingPiece] ^= moveMask; // teleport the piece

        // AllPieces ^= moveMask; // updates global occupancy

        // colorBitboard[colorToMove] ^= moveMask; // update white or black piece bitboard 
        

        if(move.Flag == 0 || move.Flag > (int)Move.MoveFlag.promoteToBishop)
        {
            pieceOnSquare[move.TargetSquare] = movingPiece;
            pieceOnSquare[move.StartSquare] = -1;
        }
       

        //Turn switch
        currentHash ^= Zobrist.sideToMoveKeys[colorToMove]; //remove the current turn's zorbist key
        colorToMove ^= 1; //switch turn
        currentHash ^= Zobrist.sideToMoveKeys[colorToMove]; //add the new turn's zorbist key

    }

    public void MakeNullMove()
    {
        //store current state
        history[plyCount].currentHash = currentHash;
        history[plyCount].enPassantSquare = enPassantSquare;
        plyCount++;

        if(enPassantSquare != 0)
        {
            int enPassantFile = (BitOperations.TrailingZeroCount(enPassantSquare)) % 8;
            currentHash ^= Zobrist.enPassantKeys[enPassantFile];
        }
        enPassantSquare = 0;

        currentHash ^= Zobrist.sideToMoveKeys[colorToMove]; //remove the current turn's zorbist key
        colorToMove ^= 1; //switch turn
        currentHash ^= Zobrist.sideToMoveKeys[colorToMove]; //add the new turn's zorbist key
    }

    public void UnmakeNullMove ()
    {
        // 1. Switch the turn back to the original color
        colorToMove ^= 1;

        // 2. Decrement the ply count to point back to the saved state
        plyCount--;

        // 3. Restore the exact Zobrist hash and En Passant square from history
        currentHash = history[plyCount].currentHash;
        enPassantSquare = history[plyCount].enPassantSquare;
    }

    public void UnmakeMove (Move move)
    {

        colorToMove ^= 1;
        plyCount--;

        int prevCapturedPiece = history[plyCount].capturedPieceType;
        castlingRights = history[plyCount].castlingRights; // Restore state
        enPassantSquare = history[plyCount].enPassantSquare; // Restore state
        currentHash = history[plyCount].currentHash;


        ulong targetMask = (1UL << move.TargetSquare);

        int movingPiece = pieceOnSquare[move.TargetSquare];


        //=======================debug====================================
        // if (move.TargetSquare == 5) // f1 is square index 5
        // {
        //     Console.WriteLine($"Unmaking f2f1q. Flag is: {move.Flag}");
        // }
        //==========================debug=================================




    
        if (move.Flag != 0)
        {
            if (move.Flag >= (int)Move.MoveFlag.promoteToQueen && move.Flag <= (int)Move.MoveFlag.promoteToBishop) //promotion
            {
                //update phase score
                phaseScore -= PiecePhaseWeights[movingPiece]; //when a piece is promoted, we add to phase score. In unmake move, we subtract from phase score.
                
                //Remove the piece from the move.TargetSquare
                pieceBitboards[movingPiece] ^= targetMask;

                //Place the pawn back on the previous square
                pieceBitboards[(int)Piece.WhitePawns + (colorToMove * 6)] |= targetMask;
                movingPiece = (int)Piece.WhitePawns + (colorToMove * 6);
            }

            else if(move.Flag >= (int)Move.MoveFlag.whiteKingSideCastle && move.Flag <= (int)Move.MoveFlag.blackQueenSideCastle) //castle
            {
                
                int rookIndex = (int)Piece.WhiteRooks + (colorToMove * 6);
                pieceBitboards[rookIndex] ^= rookCastleMasks[move.Flag - 7];
                occupiedMask ^= rookCastleMasks[move.Flag - 7]; // return the global occupancy
                colorBitboard[colorToMove] ^= rookCastleMasks[move.Flag - 7];


                //Place the rooks back at starting squares
                if (move.Flag == (int)Move.MoveFlag.whiteKingSideCastle) { pieceOnSquare[7] = (int)Piece.WhiteRooks; pieceOnSquare[5] = -1; }
                else if (move.Flag == (int)Move.MoveFlag.whiteQueenSideCastle) { pieceOnSquare[0] = (int)Piece.WhiteRooks; pieceOnSquare[3] = -1; }
                else if (move.Flag == (int)Move.MoveFlag.blackKingSideCastle) { pieceOnSquare[63] = (int)Piece.BlackRooks; pieceOnSquare[61] = -1; }
                else if (move.Flag == (int)Move.MoveFlag.blackQueenSideCastle) { pieceOnSquare[56] = (int)Piece.BlackRooks; pieceOnSquare[59] = -1; }
            }

            else if(move.Flag == (int)Move.MoveFlag.enPassantCapture) // en passant
            {
                int pawnTypeToRestore = (colorToMove == 0) ? (int)Piece.BlackPawns : (int)Piece.WhitePawns;
                ulong enPassantVictimMask = (colorToMove == 0) ? targetMask >> 8 : targetMask << 8;
                pieceBitboards[pawnTypeToRestore] ^= enPassantVictimMask;
                occupiedMask ^= enPassantVictimMask;
                colorBitboard[colorToMove ^ 1] ^= enPassantVictimMask;

                int epVictimSquare = (colorToMove == 0) ? move.TargetSquare - 8 : move.TargetSquare + 8;
                pieceOnSquare[epVictimSquare] = pawnTypeToRestore;
            }
        }


        ulong moveMask = (1UL << move.StartSquare) | (1UL << move.TargetSquare);
        
        
        // if(move.Flag == 0 || move.Flag > (int)Move.MoveFlag.promoteToBishop)
        // {
        //     pieceBitboards[movingPiece] ^= moveMask; // Remove the piece
        // }


        pieceBitboards[movingPiece] ^= moveMask; // Remove the piece

        occupiedMask ^= moveMask; // Return global occupancy
        colorBitboard[colorToMove] ^= moveMask; // return specific color occupancy

        pieceOnSquare[move.StartSquare] = movingPiece;
        pieceOnSquare[move.TargetSquare] = prevCapturedPiece;


        //Put the piece back
        if(prevCapturedPiece != -1)
        {
            phaseScore += PiecePhaseWeights[prevCapturedPiece];
            pieceBitboards[prevCapturedPiece] |= targetMask;
            occupiedMask ^= targetMask;
            colorBitboard[colorToMove ^ 1] ^= targetMask;


            pieceOnSquare[move.TargetSquare] = prevCapturedPiece; // place the piece back
        }
        

        
    }
 
    public bool IsSquareAttacked(int square, int defendingColor)
    {
        //if we have to check for white king is under attack, we use the whitePawnAttack table as pawn mask. 
        // We do so because if the white king is on square 20, it can be attacked by a black pawn on square 27 or 29. Since a white pawn on square 20 also attacks square 27 and 29, we reverse it's logic. 
        // If whitePawnAttack table for the square coincides with black king on that square, the king on that square will be attacked.

#region debug
// if(square == -1)
// {
//     Console.WriteLine("BAD SQUARE INDEX");
//     Console.WriteLine($"FATAL: Invalid square index {square} passed to IsSquareAttacked");

//     Console.Out.Flush();
//     Environment.Exit(1);   
// }
#endregion


        int enemyColor = defendingColor ^ 1;
        ulong enemyPawnBitboard = pieceBitboards[(int)Piece.WhitePawns + (enemyColor * 6)]; 
        // ulong pawnMask = AttackTables.pawnAttacks[defendingColor] [square];

        ulong pawnMask = (defendingColor == 0)? AttackTables.whitePawnAttacks[square] : AttackTables.blackPawnAttacks[square];        


        if((pawnMask & enemyPawnBitboard) != 0)
        {
            return true;
        }  

        //Check if a square is under attack by a knight
        ulong enemyKnightBitboard = pieceBitboards[(int)Piece.WhiteKnights + (enemyColor * 6)];
        ulong knightMask = AttackTables.knightAttacks[square];
        if((knightMask & enemyKnightBitboard) !=0)
        {
            return true;
        }
        
        
        // check if a square is under attack by a king. 
        ulong enemyKingBitboard = pieceBitboards[(int)Piece.WhiteKing + (enemyColor * 6)];
        ulong kingMask = AttackTables.kingAttacks[square];
        if((kingMask & enemyKingBitboard) !=0)
        {
            return true;
        }
        

        //check if a square is under attack by the sliders
        ulong diagonalMask = AttackTables.GetBishopAttacks(square, occupiedMask);
        ulong enemyBishop = pieceBitboards[(int)Piece.WhiteBishops + (enemyColor * 6)];
        ulong enemyQueen = pieceBitboards[(int)Piece.WhiteQueens + (enemyColor * 6)];
        if((diagonalMask & (enemyBishop|enemyQueen)) != 0)
        {
            return true;
        }

        ulong straightMak = AttackTables.GetRookAttacks(square, occupiedMask);
        ulong enemyRook = pieceBitboards[(int)Piece.WhiteRooks + (enemyColor * 6)];
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


        /// 1111 - No castling disabled
        
        ///  14     1110 - White kindside castling disabled
        ///  13     1101 - White queenside castling disabled
        ///  12     1100 - White castling disabled 
              
        ///  11     1011 - Black kingside castling disabled
        ///   7     0111 - Black queenside castling disabled
        ///   3     0011 - Black castling disabled
    }


    public int GetKingSquare(int colorToMove)
    {
        ulong kingBitBoard = pieceBitboards[(int)Piece.WhiteKing + (colorToMove * 6)];
        if (kingBitBoard == 0)
        {
            // This can happen in analysis if a pseudo-legal move captures the king.
            // A king bitboard should never be zero in a legal, ongoing game.
            return -1;
        }
        return BitOperations.TrailingZeroCount(kingBitBoard);
    }

    public bool HasHeavyPieces(int color)
    {
        ulong heavyPieves = (color == 0) ? (colorBitboard[(int)PieceTeam.WhitePieces] &~pieceBitboards[(int)Piece.WhiteKing]) : colorBitboard[(int)PieceTeam.BlackPieces] &~pieceBitboards[(int)Piece.BlackKing];

        if((heavyPieves ^ pieceBitboards[(int)Piece.WhitePawns + (color * 6)]) != 0) return true;
        return false;
    }



}
