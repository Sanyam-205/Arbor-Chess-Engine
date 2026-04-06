using System;
using System.Runtime.InteropServices;
using System.Numerics;
using static Board;

public class MoveGenerator
{
    ///Move Generation is a 2 step process or so I understand it.
    /// 1. You get the pseudo legal move for a piece from their attack table
    /// 2. You check if that piece can move to that square from attack table. It can only move to that square if
    ///     2.a.  A friendly piece does not occupy that square
    ///     2.b.  Moving to that square does not result in our king being in check
    /// To check for condition 2.a, we will use a loop
        
    // Move[] moveList = new Move[256];
    // int moveCount = 0;
    public void GenerateKnightMoves(Board board, Move[] moveList, ref int moveCount)
    {

        ulong friendlyPieces = (board.colorToMove == 0) ? board.colorBitboard[(int)PieceTeam.WhitePieces] : board.colorBitboard[(int)PieceTeam.BlackPieces];

        /// int knightIndex = (board.colorToMove == 0) ? 1 : 7;
        /// We could have used an if statement to determine which knight to move but to squeeze out that tiny but of performance we use the same logic we did in our promotion method in Board.cs
        /// Since white and black pieces are placed exactly 6 indexes apart, and colorToMove is 0 for white and 1 for black, we can add (colorToMove * 6) to our WhiteKnight index. IF color to move is 0 (white's turn), it will remain the same. If colorToMove is 1 (black's turn), then it will add 6 to the index of WhiteKnights giving us BlackKnights.
        /// ---------------------------------------------------------------------------------------------------------------------------------------
        /// Clever little workaround that only works because of our Piece enum definition. Changing it will break the logic.
        /// ---------------------------------------------------------------------------------------------------------------------------------------
        ulong knights = board.pieceBitboards[(int)Piece.WhiteKnights + (board.colorToMove * 6)]; //make a copy of knights bitboard for operations
        
        while (knights != 0)
        {
            /// TrailingZeroCount gives us the address of the LSB or the 1st bit in the ulong
            /// If our ulong knights = 0000000000100001;
            /// in first iteration it will give 10. On second iteration of the loop it will give 14
            int square = BitOperations.TrailingZeroCount(knights);
            
            /// ~friendlyPieces inverts the bits of friendly pieces giving us all the squares not occupied by a friendly piece.
            /// &~ will give us all the squares a knight go to by using knightAttacks and inverse of friendlyPieces.
            /// Essentially, all squares a knight on 'square' index can go to minus the squares already occupied by friendly pieces.
            ulong pseudoLegalMoves = AttackTables.knightAttacks[square] & ~friendlyPieces;
            
            // We now have a bitboard (pseudoLegalMoves) of all valid target squares!
            
            while(pseudoLegalMoves != 0)
            {
                // Stores the integer for each pseudo legal move.
                int targetSquare = BitOperations.TrailingZeroCount(pseudoLegalMoves);   

                Move newMove = new Move(square ,targetSquare);
                
                moveList[moveCount++] = newMove;
                // Directly implementing moveCount++ inside the array is a cleaner way of incrementing it after filling it out.

                // Removes the bit we just worked on from ulong,
                pseudoLegalMoves &= (pseudoLegalMoves -1); 
            }
            
            // 3. This removes the knight from the knights ulong by removing 1 from the LSB. So we can move to a different bit.
            knights &= (knights - 1);
        }
    }
    
    public void GenerateRookMoves(Board board, Move[] moveList, ref int moveCount)
    {
        ulong friendlyPieces = (board.colorToMove == 0) ? board.colorBitboard[(int)PieceTeam.WhitePieces] : board.colorBitboard[(int)PieceTeam.BlackPieces];

        ulong rooks = board.pieceBitboards[(int)Piece.WhiteRooks + board.colorToMove * 6];

        

        while(rooks!=0)
        {
            int square = BitOperations.TrailingZeroCount(rooks);
       
            ulong rawAttacks = AttackTables.GetRookAttacks(square, board.AllPieces);

            ulong pseudoLegalMoves = rawAttacks & ~friendlyPieces;
        
            while(pseudoLegalMoves != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(pseudoLegalMoves);

                Move newMove = new Move(square, targetSquare);
                moveList[moveCount++] = newMove;

                pseudoLegalMoves &= (pseudoLegalMoves-1);
            }
            
            rooks &= rooks-1;
        }
        
        
    }

    public void GenerateBishopMoves(Board board, Move[] moveList, ref int moveCount)
    {
        ulong friendlyPieces = (board.colorToMove == 0) ? board.colorBitboard[(int)PieceTeam.WhitePieces] : board.colorBitboard[(int)PieceTeam.BlackPieces];

        ulong bishops = board.pieceBitboards[(int)Piece.WhiteBishops + board.colorToMove * 6];

        while(bishops != 0)
        {
            int square = BitOperations.TrailingZeroCount(bishops);

            ulong rawAttacks = AttackTables.GetBishopAttacks(square, board.AllPieces);

            ulong pseudoLegalMoves = rawAttacks & ~friendlyPieces;

            while(pseudoLegalMoves != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(pseudoLegalMoves);

                Move newMove = new Move(square, targetSquare);
                moveList[moveCount++] = newMove;

                pseudoLegalMoves &= (pseudoLegalMoves-1); 
            }

            bishops &= bishops-1;
        }
    }


    public void GenerateQueenMoves(Board board, Move[] moveList, ref int moveCount)
    {
        ulong friendlyPieces = (board.colorToMove == 0) ? board.colorBitboard[(int)PieceTeam.WhitePieces] : board.colorBitboard[(int)PieceTeam.BlackPieces];

        ulong queens = board.pieceBitboards[(int)Piece.WhiteQueens + board.colorToMove * 6];

        

        while(queens!=0)
        {
            int square = BitOperations.TrailingZeroCount(queens);
            ulong rawAttacks = AttackTables.GetQueenAttacks(square, board.AllPieces);
            ulong pseudoLegalMoves = rawAttacks & ~friendlyPieces;
            

            while(pseudoLegalMoves!= 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(pseudoLegalMoves);
                
                Move newMove = new Move(square, targetSquare);
                moveList[moveCount++] = newMove;

                pseudoLegalMoves &= pseudoLegalMoves - 1;
            }

            queens &= queens - 1;
        }

    }
    public void GenerateKingMoves(Board board, Move[] moveList, ref int moveCount)
    {

        ulong friendlyPieces = (board.colorToMove == 0) ? board.colorBitboard[(int)PieceTeam.WhitePieces] : board.colorBitboard[(int)PieceTeam.BlackPieces];

        /// int kingIndex = (board.colorToMove == 0) ? 4 : 10;
        /// We could have used an if statement to determine which knight to move but to squeeze out that tiny but of performance we use the same logic we did in our promotion method in Board.cs
        /// Since white and black pieces are placed exactly 6 indexes apart, and colorToMove is 0 for white and 1 for black, we can add (colorToMove * 6) to our WhiteKing index. IF color to move is 0 (white's turn), it will remain the same. If colorToMove is 1 (black's turn), then it will add 6 to the index of WhiteKing giving us BlackKing.
        /// ---------------------------------------------------------------------------------------------------------------------------------------
        /// Clever little workaround that only works because of our Piece enum definition. Changing it will break the logic.
        /// ---------------------------------------------------------------------------------------------------------------------------------------
        ulong king = board.pieceBitboards[(int)Piece.WhiteKing + (board.colorToMove * 6)]; //make a copy of king bitboard for operations
        
        while (king != 0)
        {
            /// TrailingZeroCount gives us the address of the LSB or the 1st bit in the ulong
            /// If our ulong knights = 0000000000100001;
            /// in first iteration it will give 10. On second iteration of the loop it will give 14
            int square = BitOperations.TrailingZeroCount(king);
            
            /// ~friendlyPieces inverts the bits of friendly pieces giving us all the squares not occupied by a friendly piece.
            /// &~ will give us all the squares a king go to by using kingAttacks and inverse of friendlyPieces.
            /// Essentially, all squares a king on 'square' index can go to minus the squares already occupied by friendly pieces.
            ulong pseudoLegalMoves = AttackTables.kingAttacks[square] & ~friendlyPieces;
            
            // We now have a bitboard (pseudoLegalMoves) of all valid target squares!
            
            while(pseudoLegalMoves != 0)
            {
                // Stores the integer for each pseudo legal move.
                int targetSquare = BitOperations.TrailingZeroCount(pseudoLegalMoves);   

                Move newMove = new Move(square ,targetSquare);
                moveList[moveCount++] = newMove;
                

                // Removes the bit we just worked on from ulong,
                pseudoLegalMoves &= (pseudoLegalMoves -1); 
            }
            
            // 3. This removes the king from the king ulong by removing 1 from the LSB. So we can move to a different bit.
            king &= (king - 1);
        }
    }



    public void GeneratePawnMoves(Board board, Move[] moveList, ref int moveCount)
    {
        ulong enemyPieces = (board.colorToMove == 0) ? board.colorBitboard[(int)PieceTeam.BlackPieces] : board.colorBitboard[(int)PieceTeam.WhitePieces];


    /* Debugging code

        // 1. Where does the computer think the White Pawn is?
        int pawnIndex = BitOperations.TrailingZeroCount(board.pieceBitboards[(int)Piece.WhitePawns]);
        Console.WriteLine("Pawn Index: " + pawnIndex);

        // 2. Where does the computer think the blocking piece is?
        // (We mask out the pawn so we only find the index of the blocking piece)
        ulong blockingPiece = board.AllPieces & ~board.pieceBitboards[(int)Piece.WhitePawns];
        int blockIndex = BitOperations.TrailingZeroCount(blockingPiece);
        Console.WriteLine("Blocker Index: " + blockIndex);

    */

        //used for calculating pawn promotions
        const ulong rank7Mask = 0x00FF000000000000UL;
        const ulong rank2Mask = 0x000000000000FF00UL;
        
        //used for calulating double pawn push
        const ulong rank6Mask = 0x0000FF0000000000UL; //280375465082880
        const ulong rank3Mask = 0x00FF0000UL; //16711680  0x0000000000FF0000UL

        //used for separating standard pawns from promoting pawns
        const ulong notRank7Mask = 0xFF00FFFFFFFFFFFFUL;
        const ulong notRank2Mask = 0xFFFFFFFFFFFF00FFUL;
        
        // const ulong fileAMask  = 0x0101010101010101UL;
        // const ulong fileHMask  = 0x8080808080808080UL;

        const ulong notHFile = 0x7F7F7F7F7F7F7F7FUL;
        const ulong notAFile = 0xFEFEFEFEFEFEFEFEUL;

        
        /// Pawn moves are inheritly different from every other type. They capture diagonally and move in a straight line. They can also push 2 places but only on their first turn, then they move 1 square. Not to mention En Passant which we haven't even touched yet.
        /// We can generate a separate table to precalculate all pawn moves but there exist an even faster strategy known as parallel processing. In paraller processing rather than looping through bitbaord and generating moves for every piece like we did with knight and king, we generate moves for all the pawns at once.
    
        /// In pawn's case we need to back track to find the starting square because we moved all 8 pawns at once unlike the knight moves where only one knight was moved at once. Because of this, the program does not know which pawn is to be pushed. That's why we use while(whitePawnPush != 0) and (blackPawnPush != 0)
         
        /// To understand the logic behind while (xyz != 0) in out context, we need to understand bit shifting. In case of king and knight where we make copy of the king and knight bitboard, the while loop allows the code to only run on bits where king or knight = 1. Since in a standard position, WhiteKing will only have 1 bit turned on, it will only run once. 
        /// But in case of pawns, we aren't making a simple copy, but shifting the bits simeltaneously. So we use the while loop for bits that are turned on (shifted), then we shift those bits again in the opp direction to get the starting square.
        
        if(board.colorToMove == 0)
        {

            //Pawn promotion
            // we create a separate ulong for non promoting pawn so the move generator never generates the move pawn from 7th rank to 8th as normal pawn push.
            ulong whiteStandardPawn = board.pieceBitboards[(int)Piece.WhitePawns] & notRank7Mask;
            ulong whitePromotingPawn = board.pieceBitboards[(int)Piece.WhitePawns] & rank7Mask;

            ulong whiteSinglePush = (whiteStandardPawn << 8) & ~board.AllPieces;
            ulong whiteDoublePush = ((whiteSinglePush & rank3Mask) << 8) & ~board.AllPieces;
            ulong whitePromotionPush = (whitePromotingPawn << 8) & ~board.AllPieces;

            


            /// For calculating white double push, we shift the previously calculated single push by 8. We do not directly shift pawns by 16 because that would result in a pawn on starting rank jumping 2 squares even if its path is blocked.
            /// 16711680 is 3rd rank. Same technique we used in generating attack tables. 
            

            while(whiteSinglePush != 0) //single push
            {
                // Trace the starting square

                int targetSquare = BitOperations.TrailingZeroCount(whiteSinglePush); //Gets the LSB of the 64 bit int
                int startSquare = targetSquare -8;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 0);

                whiteSinglePush &= (whiteSinglePush - 1);
            }

            while(whiteDoublePush != 0) //double push
            {
                int targetSquare = BitOperations.TrailingZeroCount(whiteDoublePush);
                int startSquare = targetSquare -16;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 6); // flag for double push is 6
                

                whiteDoublePush &= (whiteDoublePush-1);   
            }

            while(whitePromotionPush != 0) //promotion push
            {
                int targetSquare = BitOperations.TrailingZeroCount(whitePromotionPush);
                int startSquare = targetSquare - 8;
                
                moveList[moveCount++] = new Move (startSquare, targetSquare, 1); //queen
                moveList[moveCount++] = new Move (startSquare, targetSquare, 2); //knight
                moveList[moveCount++] = new Move (startSquare, targetSquare, 3); //rook
                moveList[moveCount++] = new Move (startSquare, targetSquare, 4); //bishop

                whitePromotionPush &= (whitePromotionPush - 1);
            }
            
            
            //Pawn Captures
            /// For pawn captures we refrain from using Pawn attack table generated before. This is because bit shifting for all 8 bits will happen in one CPU cycle which is faster than running 8 while loops to check for the attack table. We will still use pawn attack tables for check detection and whatnot but generating all pawn moves on the fly is generally faster.
            
            ulong whiteRightCapture = (( whiteStandardPawn & notHFile) << 9) & enemyPieces;
            ulong whiteLeftCapture = (( whiteStandardPawn& notAFile) << 7) & enemyPieces;

            //Have to create separate ulongs for promotion capture since a pawn reaching last rank would always result in promotion no matter if it captures or not, so we can't treat capture at the last rank as a normal capture

            ulong whiteRightPromotingCapture = ((whitePromotingPawn & notHFile) << 9) & enemyPieces;
            ulong whiteLeftPromotingCapture = ((whitePromotingPawn & notAFile) << 7) & enemyPieces;

            while(whiteRightCapture != 0) //right side standard capture
            {
                int targetSquare = BitOperations.TrailingZeroCount(whiteRightCapture);
                int startSquare = targetSquare - 9;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 0);

                whiteRightCapture &= (whiteRightCapture - 1);
            }

            while(whiteLeftCapture != 0) //left side standard capture
            {
                int targetSquare = BitOperations.TrailingZeroCount(whiteLeftCapture);
                int startSquare = targetSquare - 7;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 0);

                whiteLeftCapture &= (whiteLeftCapture - 1);
                
            }

            while(whiteRightPromotingCapture != 0) // right side promoting capture
            {
                int targetSquare = BitOperations.TrailingZeroCount(whiteRightPromotingCapture);
                int startSquare  = targetSquare - 9;

                // First instinct was to use a standard for loop to achive this, but discovered a technique called loop unrooling, which is implemented below.
                // for (int i = 1; i <= 4; i++)
                // {
                //     moveList [moveCount] = new Move (startSquare, targetSquare, i);
                //     moveCount++;
                // }

                //loop unrooling
                moveList[moveCount++] = new Move (startSquare, targetSquare, 1); //1 is the flag for queen promotion
                moveList[moveCount++] = new Move (startSquare, targetSquare, 2); //1 is the flag for knight promotion
                moveList[moveCount++] = new Move (startSquare, targetSquare, 3); //1 is the flag for rook promotion
                moveList[moveCount++] = new Move (startSquare, targetSquare, 4); //1 is the flag for bishop promotion

                
                
                whiteRightPromotingCapture &= (whiteRightPromotingCapture - 1);
            }

            while(whiteLeftPromotingCapture != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(whiteLeftPromotingCapture);
                int startSquare = targetSquare - 7;

                moveList[moveCount++] = new Move (startSquare, targetSquare, 1); //queen
                moveList[moveCount++] = new Move (startSquare, targetSquare, 2); //knight
                moveList[moveCount++] = new Move (startSquare, targetSquare, 3); //rook
                moveList[moveCount++] = new Move (startSquare, targetSquare, 4); //bishop

                whiteLeftPromotingCapture &= (whiteLeftPromotingCapture - 1);
            }

            if (board.enPassantSquare != 0)
            {
                ulong whiteRightEPCapture = ((whiteStandardPawn & notHFile) << 9) & board.enPassantSquare;
                ulong whiteLeftEPCapture = ((whiteStandardPawn & notAFile) << 7) & board.enPassantSquare;

                while (whiteRightEPCapture != 0)
                {
                    int targetSquare = BitOperations.TrailingZeroCount(whiteRightEPCapture);
                    int startSquare = targetSquare - 9;

                    moveList[moveCount++] = new Move(startSquare, targetSquare, 5); // 5 is the flag for en passant
                    whiteRightEPCapture &= (whiteRightEPCapture - 1);
                }

                while (whiteLeftEPCapture != 0)
                {
                    int targetSquare = BitOperations.TrailingZeroCount(whiteLeftEPCapture);
                    int startSquare = targetSquare - 7;

                    moveList[moveCount++] = new Move(startSquare, targetSquare, 5);
                    whiteLeftEPCapture &= (whiteLeftEPCapture - 1);
                }
            }
        }


        else
        { 

            ulong blackStandardPawns = board.pieceBitboards[(int)Piece.BlackPawns] & notRank2Mask;
            ulong blackPromotingPawns = board.pieceBitboards[(int)Piece.BlackPawns] & rank2Mask;
        
            ulong blackSinglePush = (blackStandardPawns >> 8) & ~board.AllPieces;
            ulong blackDoublePush = ((blackSinglePush & rank6Mask) >> 8) & ~board.AllPieces;

            ulong blackPromotionPush = (blackPromotingPawns >> 8) & ~board.AllPieces;

            while(blackSinglePush != 0) //black single push
            {
                // Trace the starting square

                int targetSquare = BitOperations.TrailingZeroCount(blackSinglePush);

                int startSquare = targetSquare + 8;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 0);

                blackSinglePush &= (blackSinglePush-1);
            }

            while(blackDoublePush != 0) //black double push
            {
                int targetSquare = BitOperations.TrailingZeroCount(blackDoublePush);
                int startSquare = targetSquare + 16;

                moveList[moveCount++] = new Move (startSquare, targetSquare, 6); // flag for double push is 6
                

                blackDoublePush &= (blackDoublePush-1);
            }
       
            while(blackPromotionPush != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(blackPromotionPush);
                int startSquare = targetSquare + 8;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 1); //queen
                moveList[moveCount++] = new Move(startSquare, targetSquare, 2); //knight
                moveList[moveCount++] = new Move(startSquare, targetSquare, 3); //rook
                moveList[moveCount++] = new Move(startSquare, targetSquare, 4); //bishop
                
                blackPromotionPush &= (blackPromotionPush - 1);
                
            }

            ulong blackRightCapture = ((blackStandardPawns & notHFile) >> 7) & enemyPieces;
            ulong blackLeftCapture =  ((blackStandardPawns & notAFile) >> 9) & enemyPieces;

            ulong blackRightPromotingCapture = ((blackPromotingPawns & notHFile) >> 7) & enemyPieces;
            ulong blackLeftPromotingCapture = ((blackPromotingPawns & notAFile) >> 9) & enemyPieces;
                
            while(blackRightCapture != 0) //standard right capture for black
            {
                int targetSquare = BitOperations.TrailingZeroCount(blackRightCapture);
                int startSquare = targetSquare + 7;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 0);

                blackRightCapture &= (blackRightCapture - 1);
            }        

            while(blackLeftCapture != 0) //standard left capture for black
            {
                int targetSquare = BitOperations.TrailingZeroCount(blackLeftCapture);
                int startSquare = targetSquare + 9;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 0);
                

                blackLeftCapture &= (blackLeftCapture - 1);
            }

            while(blackRightPromotingCapture != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(blackRightPromotingCapture);
                int startSquare = targetSquare + 7;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 1); //queen
                moveList[moveCount++] = new Move(startSquare, targetSquare, 2); //knight
                moveList[moveCount++] = new Move(startSquare, targetSquare, 3); //rook
                moveList[moveCount++] = new Move(startSquare, targetSquare, 4); //bishop
                

                blackRightPromotingCapture &= (blackRightPromotingCapture - 1);
            }
            
            while(blackLeftPromotingCapture != 0)
            {
                int targetSquare = BitOperations.TrailingZeroCount(blackLeftPromotingCapture);
                int startSquare = targetSquare + 9;

                moveList[moveCount++] = new Move(startSquare, targetSquare, 1); //queen
                moveList[moveCount++] = new Move(startSquare, targetSquare, 2); //knight
                moveList[moveCount++] = new Move(startSquare, targetSquare, 3); //rook
                moveList[moveCount++] = new Move(startSquare, targetSquare, 4); //bishop
                

                blackLeftPromotingCapture &= (blackLeftPromotingCapture - 1);
            }

            if (board.enPassantSquare != 0)
            {
                ulong blackRightEPCapture = ((blackStandardPawns & notHFile) >> 7) & board.enPassantSquare;
                ulong blackLeftEPCapture = ((blackStandardPawns & notAFile) >> 9) & board.enPassantSquare;

                while (blackRightEPCapture != 0)
                {
                    int targetSquare = BitOperations.TrailingZeroCount(blackRightEPCapture);
                    int startSquare = targetSquare + 7;

                    moveList[moveCount++] = new Move(startSquare, targetSquare, 5); // 5 is the flag for en passant
                    blackRightEPCapture &= (blackRightEPCapture - 1);
                }

                while (blackLeftEPCapture != 0)
                {
                    int targetSquare = BitOperations.TrailingZeroCount(blackLeftEPCapture);
                    int startSquare = targetSquare + 9;

                    moveList[moveCount++] = new Move(startSquare, targetSquare, 5);
                    blackLeftEPCapture &= (blackLeftEPCapture - 1);
                }
            }


        }
        

    }



}
