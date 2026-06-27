using System;
using System.Drawing;
using System.IO.Pipelines;
using System.Net;
using System.Numerics;
using static Board;
public class Evaluation
{
    public static readonly int[,] ReductionTable = new int[128,256]; // a table to store logarithms for reduction value.


    static readonly int[] ColorMultiplier = {1, -1};
    static readonly ulong[] fileMask= 
    {
        /*File A*/ 0x0101010101010101UL, 
        /*File B*/ 0x0202020202020202UL, 
        /*File C*/ 0x0404040404040404UL, 
        /*File D*/ 0x0808080808080808UL,
        /*File E*/ 0x1010101010101010UL,
        /*File F*/ 0x2020202020202020UL,
        /*File G*/ 0x4040404040404040UL,
        /*File H*/ 0x8080808080808080UL 
    };

    static readonly ulong[] rankMask =
    {
      /*Rank 1*/  0x00000000000000FFUL,
      /*Rank 2*/  0x000000000000FF00UL,
      /*Rank 3*/  0x0000000000FF0000UL,
      /*Rank 4*/  0x00000000FF000000UL,
      /*Rank 5*/  0x000000FF00000000UL,
      /*Rank 6*/  0x0000FF0000000000UL,
      /*Rank 7*/  0x00FF000000000000UL,
      /*Rank 8*/  0xFF00000000000000UL 
    };

    static readonly ulong[] passedPawnFileMask = new ulong[8];
    static readonly ulong[] whitePassedPawnMask = new ulong[64];
    static readonly ulong[] blackPassedPawnMask = new ulong[64];

    static readonly ulong[] isolatedPawnMask = new ulong[8];


    static Evaluation()
    {
        InitializePassedPawnMasks();
        InitializeIsolatedPawnMasks();
        InitializeReductionTable();
    }

    public int EvaluatePosition(Board board)
    {
        
        // int score = 340 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteBishops]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackBishops])) + 
        // 320 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteKnights]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackKnights])) + 
        // 900 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteQueens]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackQueens])) + 
        // 500 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhiteRooks]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackRooks])) + 
        // 100 * (BitOperations.PopCount(board.pieceBitboards[(int)Piece.WhitePawns]) - BitOperations.PopCount(board.pieceBitboards[(int)Piece.BlackPawns]));

#region
     
        int whiteMiddlegameScore = 0;
        int whiteEndgameSCore = 0;
        int blackMiddlegameScore = 0;
        int blackEndgameScore = 0;

#region knight
        ulong whiteKnights = board.pieceBitboards[(int)Piece.WhiteKnights];
        while(whiteKnights!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteKnights);

            ulong knightAttacks = AttackTables.knightAttacks[squareIndex] & ~ (board.colorBitboard[(int)PieceTeam.WhitePieces] | board.pieceBitboards[(int)Piece.BlackKing]);

            int attackSquares = BitOperations.PopCount(knightAttacks);
    

            //Adding positional scores based on PST
            whiteMiddlegameScore += PST_Knight[squareIndex] + knightMobility_middlegame[attackSquares] + 330;
            whiteEndgameSCore += PST_Knight[squareIndex] +    knightMobility_endgame[attackSquares] + 320;

            whiteKnights &= whiteKnights -1;
        }
        
        ulong blackKnights = board.pieceBitboards[(int)Piece.BlackKnights];
        while(blackKnights!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackKnights);

            ulong knightAttacks = AttackTables.knightAttacks[squareIndex] & ~ (board.colorBitboard[(int)PieceTeam.BlackPieces] | board.pieceBitboards[(int)Piece.WhiteKing]);

            int attackSquares = BitOperations.PopCount(knightAttacks);

            //Adding positional scores based on PST
            blackMiddlegameScore += PST_Knight[squareIndex] + knightMobility_middlegame[attackSquares] + 330;
            blackEndgameScore += PST_Knight[squareIndex] + knightMobility_endgame[attackSquares] + 320;

            blackKnights &= blackKnights -1;
        }
#endregion knight

#region bishop
        ulong whiteBishops = board.pieceBitboards[(int)Piece.WhiteBishops];
        while(whiteBishops!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteBishops);

            ulong rawAttacks = AttackTables.GetBishopAttacks(squareIndex, board.occupiedMask);
            //we or the white piece bitboard with black king because a bishop can't actually step onto the enemy king's square so it might skew the mobility bonus.
            ulong bishopAttacks = rawAttacks & ~(board.colorBitboard[(int)PieceTeam.WhitePieces] | board.pieceBitboards[(int)Piece.BlackKing]); 

            int attackSquares = BitOperations.PopCount(bishopAttacks);

            //Adding positional scores based on PST + mobility
            whiteMiddlegameScore += PST_Bishop[squareIndex] + bishopMobility_middlegame[attackSquares] + 340;
            whiteEndgameSCore += PST_Bishop[squareIndex] + bishopMobility_endgame[attackSquares] + 340;

            whiteBishops &= whiteBishops -1;
        }

        ulong blackBishops = board.pieceBitboards[(int)Piece.BlackBishops];
        while(blackBishops!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackBishops);

            ulong rawAttacks = AttackTables.GetBishopAttacks(squareIndex, board.occupiedMask);
            //we or the white piece bitboard with black king because a bishop can't actually step onto the enemy king's square so it might skew the mobility bonus.
            ulong bishopAttacks = rawAttacks & ~(board.colorBitboard[(int)PieceTeam.BlackPieces] | board.pieceBitboards[(int)Piece.WhiteKing]); 

            int attackSquares = BitOperations.PopCount(bishopAttacks);


            //Adding positional scores based on PST
            blackMiddlegameScore += PST_Bishop[squareIndex] + bishopMobility_middlegame[attackSquares] +340;
            blackEndgameScore += PST_Bishop[squareIndex] + bishopMobility_endgame[attackSquares] +340;

            blackBishops &= blackBishops -1;
        }
#endregion bishop


#region rook
        ulong whiteRooks = board.pieceBitboards[(int)Piece.WhiteRooks];
        while(whiteRooks!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteRooks);

            ulong rawAttacks = AttackTables.GetRookAttacks(squareIndex, board.occupiedMask);
            ulong rookAttacks = rawAttacks & ~(board.colorBitboard[(int)PieceTeam.WhitePieces] | board.pieceBitboards[(int) Piece.BlackKing]);

            int attackSquares = BitOperations.PopCount(rookAttacks);

            whiteMiddlegameScore+= PST_Rook[squareIndex] + rookMobility_middlegame[attackSquares] +500;
            whiteEndgameSCore += PST_Rook[squareIndex] +   rookMobility_endgame[attackSquares] + 500;

            whiteRooks &= whiteRooks - 1;
        }

        ulong blackRooks = board.pieceBitboards[(int)Piece.BlackRooks];
        while(blackRooks!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackRooks);

            ulong rawAttacks = AttackTables.GetRookAttacks(squareIndex, board.occupiedMask);
            ulong rookAttacks = rawAttacks & ~(board.colorBitboard[(int)PieceTeam.BlackPieces] | board.pieceBitboards[(int) Piece.WhiteKing]);

            int attackSquares = BitOperations.PopCount(rookAttacks);

            blackMiddlegameScore += PST_Rook [squareIndex] + rookMobility_middlegame[attackSquares] +  500;
            blackEndgameScore += PST_Rook [squareIndex] + rookMobility_endgame[attackSquares] + 500;

            blackRooks &= blackRooks -1;
        }
#endregion rook


#region  queen
        ulong whiteQueens = board.pieceBitboards[(int)Piece.WhiteQueens];
        while(whiteQueens != 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteQueens);

            ulong rawAttacks = AttackTables.GetQueenAttacks(squareIndex, board.occupiedMask);
            ulong queenAttacks = rawAttacks & ~(board.colorBitboard[(int) PieceTeam.WhitePieces] | board.pieceBitboards[(int) Piece.BlackKing]);

            int attackSquares = BitOperations.PopCount(queenAttacks);

            whiteMiddlegameScore += PST_Queen[squareIndex] + queenMobility_middlegame[attackSquares] +900;
            whiteEndgameSCore += PST_Queen[squareIndex] +    queenMobility_endgame[attackSquares] + 900;

            whiteQueens &= whiteQueens - 1;

        }

        ulong blackQueens = board.pieceBitboards[(int)Piece.BlackQueens];
        while(blackQueens!= 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackQueens);

            ulong rawAttacks = AttackTables.GetQueenAttacks(squareIndex, board.occupiedMask);
            ulong queenAttacks = rawAttacks & ~(board.colorBitboard[(int) PieceTeam.BlackPieces] | board.pieceBitboards[(int) Piece.WhiteKing]);

            int attackSquares = BitOperations.PopCount(queenAttacks);

            blackMiddlegameScore += PST_Queen[squareIndex] + queenMobility_middlegame[attackSquares] + 900;
            blackEndgameScore += PST_Queen[squareIndex] + queenMobility_endgame[attackSquares] + 900;


            blackQueens &= blackQueens - 1;    
        }
#endregion queen


#region pawns
        ulong whitePawns = board.pieceBitboards[(int)Piece.WhitePawns];
        while(whitePawns!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whitePawns);
            int file = squareIndex & 7;
            int rank = squareIndex >> 3;

            int middlegamePassedPawnBonus = 0;
            int endgamePassedPawnBonus = 0;

            //================================================Passed Pawns=======================================================

            ulong pawnMask = whitePassedPawnMask[squareIndex];

            if((pawnMask & board.pieceBitboards[(int)Piece.BlackPawns]) == 0)
            {
                middlegamePassedPawnBonus += whitePassedPawn_Middlegame[rank];
                endgamePassedPawnBonus += whitePassedPawn_Endgame[rank];
                
                bool defended = (AttackTables.blackPawnAttacks[squareIndex] & board.pieceBitboards[(int)Piece.WhitePawns]) != 0;
                if(defended)//defended passed pawn
                {
                    middlegamePassedPawnBonus += whiteDefendedPassedPawn_Middlegame[rank];
                    endgamePassedPawnBonus += whiteDefendedPassedPawn_Endgame[rank];
                }
            }

            //==========================================Passed Pawns End==========================================================

            int isolatedPenalty = 0;

            //==========================================Isolated Pawns==========================================================

            bool isolated = (isolatedPawnMask[file] & board.pieceBitboards[(int)Piece.WhitePawns]) == 0;

            if(isolated) isolatedPenalty = -15;

            //==========================================Isolated Pawns End==========================================================

            int doubledPawnsPenalty = 0;

            //==========================================Doubled Pawns==========================================================

            ulong pawnsOnFile = board.pieceBitboards[(int)Piece.WhitePawns] & fileMask[file];
            if(BitOperations.PopCount(pawnsOnFile) > 1) doubledPawnsPenalty = -10;

            //==========================================Doubled Pawns End==========================================================

            whiteMiddlegameScore += PST_WhitePawn_Starting[squareIndex] + middlegamePassedPawnBonus + isolatedPenalty + doubledPawnsPenalty + 100;
            whiteEndgameSCore += PST_WhitePawn_EndGame[squareIndex] + endgamePassedPawnBonus + isolatedPenalty + doubledPawnsPenalty + 100;

            whitePawns &= whitePawns -1;
        }

        ulong blackPawns = board.pieceBitboards[(int)Piece.BlackPawns];
        while(blackPawns!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackPawns);
            int file = squareIndex & 7;
            int rank = squareIndex >> 3;

            int middlegamePassedPawnBonus = 0;
            int endgamePassedPawnBonus = 0;

            //================================================Passed Pawns=======================================================

            ulong pawnMask = blackPassedPawnMask[squareIndex];

            if((pawnMask & board.pieceBitboards[(int)Piece.WhitePawns]) == 0)
            {
                middlegamePassedPawnBonus += blackPassedPawn_Middlegame[rank];
                endgamePassedPawnBonus += blackPassedPawn_Endgame[rank];

                bool defended = (AttackTables.whitePawnAttacks[squareIndex] & board.pieceBitboards[(int)Piece.BlackPawns]) != 0;
                if(defended)//defended passed pawn
                {
                    middlegamePassedPawnBonus += blackDefendedPassedPawn_Middlegame[rank];
                    endgamePassedPawnBonus += blackDefendedPassedPawn_Endgame[rank];
                }
            }

            //==========================================Passed Pawns End==========================================================


            int isolatedPenalty = 0;


            //==========================================Isolated Pawns==========================================================

            bool isolated = (isolatedPawnMask[file] & board.pieceBitboards[(int)Piece.BlackPawns]) == 0;

            if(isolated) isolatedPenalty = -15;

            //==========================================Isolated Pawns End==========================================================

            int doubledPawnsPenalty = 0;

            //==========================================Doubled Pawns==========================================================

            ulong pawnsOnFile = board.pieceBitboards[(int)Piece.BlackPawns] & fileMask[file];
            if(BitOperations.PopCount(pawnsOnFile) > 1) doubledPawnsPenalty = -10;

            //==========================================Doubled Pawns End==========================================================

            blackMiddlegameScore += PST_BlackPawn_Starting[squareIndex] + middlegamePassedPawnBonus + isolatedPenalty + doubledPawnsPenalty + 100;
            blackEndgameScore += PST_BlackPawn_EndGame[squareIndex] + endgamePassedPawnBonus + isolatedPenalty + doubledPawnsPenalty + 100;

            blackPawns &= blackPawns -1;
        }
#endregion pawns


#region king
        ulong whiteKing = board.pieceBitboards[(int)Piece.WhiteKing];
        while(whiteKing != 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteKing);

            //====================================castling====================================
            //check the square the king is on and check for the pawn shield.
           
            int pawnPenalty = ExposedKing(board);
            whiteMiddlegameScore += PST_WhiteKing_Starting[squareIndex] + pawnPenalty; //king is assigned base material evaluation of 0;
            whiteEndgameSCore += PST_King_EndGame[squareIndex];

            whiteKing &= whiteKing-1;
        }

        ulong blackKing = board.pieceBitboards[(int)Piece.BlackKing];
        while(blackKing!= 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackKing);

            int pawnPenalty = ExposedKing(board);

            blackMiddlegameScore += PST_BlackKing_Starting[squareIndex] + pawnPenalty;
            blackEndgameScore += PST_King_EndGame[squareIndex];

            blackKing &= blackKing-1;
        }
#endregion king


        int netMiddlegame = whiteMiddlegameScore - blackMiddlegameScore;
        int netEndgame = whiteEndgameSCore - blackEndgameScore;

        //int FinalScore = ((MidgameScore * CurrentPhase) + (EndgameScore * (MaxPhase - CurrentPhase))) / MaxPhase;

        int safePhase = board.phaseScore;

        //protects against overflow, if phaseScore reaches higher than 24.
        if (safePhase > 24)
        {
            safePhase = 24;
        }

        //protects against underflow. SHouldn't happen but better safe than sorry.
        if (safePhase < 0) 
        {
            safePhase = 0;
        }

        int score = ((netMiddlegame * safePhase) + (netEndgame * (24 - safePhase))) / 24; 

#endregion
 
        score *= ColorMultiplier[board.colorToMove];


        return score;
    }

    static int ExposedKing(Board board)
    {
        int color = board.colorToMove; 
        
        ulong pawnShield = board.pieceBitboards[(int)Piece.WhitePawns + (color * 6)];
        ulong king = board.pieceBitboards[(int)Piece.WhiteKing + (color * 6)];
        int kingSquare = BitOperations.TrailingZeroCount(king);

        int penalty = 0;

        if (color == 0) 
        {
            // Queenside (a1-c1 area)
            if (kingSquare <= 2)
            {
                ulong APawn = pawnShield & fileMask[0];
                ulong BPawn = pawnShield & fileMask[1];
                ulong CPawn = pawnShield & fileMask[2];

                int aSq = BitOperations.TrailingZeroCount(APawn);
                int bSq = BitOperations.TrailingZeroCount(BPawn);
                int cSq = BitOperations.TrailingZeroCount(CPawn);

                if(aSq == 64)
                {
                    penalty += -25;
                    if((fileMask[0] & board.pieceBitboards[(int)Piece.BlackPawns]) == 0) penalty += -15;
                }
                else penalty+= whiteExposedKingPenalty[aSq];

                if(bSq == 64)
                {
                    penalty += -60;
                    if((fileMask[1] & board.pieceBitboards[(int)Piece.BlackPawns]) == 0) penalty += -20;
                }
                else penalty += whiteExposedKingPenalty[bSq];

                if(cSq == 64)
                {
                    penalty += -35;
                    if((fileMask[2] & board.pieceBitboards[(int)Piece.BlackPawns] )== 0) penalty += -15;
                }
                else penalty += whiteExposedKingPenalty[cSq];




                return penalty;
            }
            // Kingside (f1-h1 area)
            else if (kingSquare >= 5 && kingSquare <= 7)
            {
                ulong FPawn = pawnShield & fileMask[5];
                ulong GPawn = pawnShield & fileMask[6];
                ulong HPawn = pawnShield & fileMask[7];

                int fSq = BitOperations.TrailingZeroCount(FPawn);
                int gSq = BitOperations.TrailingZeroCount(GPawn);
                int hSq = BitOperations.TrailingZeroCount(HPawn);

                if(hSq == 64)
                {
                    penalty += -25;
                    if((fileMask[7] & board.pieceBitboards[(int)Piece.BlackPawns]) == 0) penalty += -15;
                }
                else penalty+= whiteExposedKingPenalty[hSq];

                if(gSq == 64)
                {
                    penalty += -60;
                    if((fileMask[6] & board.pieceBitboards[(int)Piece.BlackPawns]) == 0) penalty += -20;
                }
                else penalty += whiteExposedKingPenalty[gSq];

                if(fSq == 64)
                {
                    penalty += -35;
                    if((fileMask[5] & board.pieceBitboards[(int)Piece.BlackPawns] )== 0) penalty += -15;
                }
                else penalty += whiteExposedKingPenalty[fSq];


                return penalty;
            }
        }
        else 
        {
            // Queenside (a8-c8 area)
            if (kingSquare >= 56 && kingSquare <= 58)
            {
                ulong APawn = pawnShield & fileMask[0];
                ulong BPawn = pawnShield & fileMask[1];
                ulong CPawn = pawnShield & fileMask[2];

                // Use LeadingZeroCount for Black to find the pawn furthest down the board
                int aSq = (APawn == 0) ? 64 : 63 - BitOperations.LeadingZeroCount(APawn);
                int bSq = (BPawn == 0) ? 64 : 63 - BitOperations.LeadingZeroCount(BPawn);
                int cSq = (CPawn == 0) ? 64 : 63 - BitOperations.LeadingZeroCount(CPawn);

                
                if(aSq == 64)
                {
                    penalty += -25;
                    if((fileMask[0] & board.pieceBitboards[(int)Piece.WhitePawns]) == 0) penalty += -15;
                }
                else penalty+= blackExposedKingPenalty[aSq];

                if(bSq == 64)
                {
                    penalty += -60;
                    if((fileMask[1] & board.pieceBitboards[(int)Piece.WhitePawns]) == 0) penalty += -20;
                }
                else penalty += blackExposedKingPenalty[bSq];

                if(cSq == 64)
                {
                    penalty += -35;
                    if((fileMask[2] & board.pieceBitboards[(int)Piece.WhitePawns] )== 0) penalty += -15;
                }
                else penalty += blackExposedKingPenalty[cSq];



                return penalty;
            }
            // Kingside (f8-h8 area)
            else if (kingSquare >= 61 && kingSquare <= 63)
            {
                ulong FPawn = pawnShield & fileMask[5];
                ulong GPawn = pawnShield & fileMask[6];
                ulong HPawn = pawnShield & fileMask[7];

                int fSq = (FPawn == 0) ? 64 : 63 - BitOperations.LeadingZeroCount(FPawn);
                int gSq = (GPawn == 0) ? 64 : 63 - BitOperations.LeadingZeroCount(GPawn);
                int hSq = (HPawn == 0) ? 64 : 63 - BitOperations.LeadingZeroCount(HPawn);

                if(hSq == 64)
                {
                    penalty += -25;
                    if((fileMask[7] & board.pieceBitboards[(int)Piece.WhitePawns]) == 0) penalty += -15;
                }
                else penalty+= blackExposedKingPenalty[hSq];

                if(gSq == 64)
                {
                    penalty += -60;
                    if((fileMask[6] & board.pieceBitboards[(int)Piece.WhitePawns]) == 0) penalty += -20;
                }
                else penalty += blackExposedKingPenalty[gSq];

                if(fSq == 64)
                {
                    penalty += -35;
                    if((fileMask[5] & board.pieceBitboards[(int)Piece.WhitePawns] )== 0) penalty += -15;
                }
                else penalty += blackExposedKingPenalty[fSq];


                return penalty;
            }
        }

        return 0;    
    }

    static readonly int[] whiteExposedKingPenalty =
    {       //a1   b1   c1   d1   e1   f1   g1   h1
      /*a2*/  0,   0,   0,   0,  0,   0,   0,    0,      //missing -60 = g,b pawn
      /*a1*/  0,   0,   0,   0,  0,   0,   0,    0,      //missing -35 = f,c pawn
      /*a3*/ -2,  -5,  -3,   0,  0,  -3,  -5,   -2,      //missing -25 = h,a pawn
      /*a4*/ -5,  -15,  -8,  0,  0,  -8,  -15,  -5,
      /*a5*/ -10, -30, -15,  0,  0, -15,  -30, -10,
      /*a6*/ -20, -50, -25,  0,  0, -25,  -50, -20,       
      /*a7*/   0,   0,   0,  0,  0,   0,   0,    0,       
      /*a8*/   0,   0,   0,  0,  0,   0,   0,    0        
    };
    static readonly int[] blackExposedKingPenalty =
    {       //a1 b1 c1 d1 e1 f1 g1 h1
      /*a2*/  0,   0,   0,   0,  0,   0,   0,    0,      //missing -60 = g pawn
      /*a1*/  0,   0,   0,   0,  0,   0,   0,    0,      //missing -35 = f pawn
      /*a3*/ -20, -50, -25,  0,  0, -25,  -50,  -20,      //missing -25 = h pawn
      /*a4*/ -10, -30, -15,  0,  0, -15,  -30,  -10,
      /*a5*/  -5, -15,  -8,  0,  0,  -8,  -15,  -5,
      /*a6*/  -2,  -5,  -3,  0,  0,  -3,  -5,   -2,       
      /*a7*/   0,   0,   0,  0,  0,   0,   0,    0,       
      /*a8*/   0,   0,   0,  0,  0,   0,   0,    0        
    };


    static readonly int[] whitePassedPawn_Middlegame =
    {
        0, 0, 5, 10, 20, 35, 60, 0
    };

    static readonly int[] whitePassedPawn_Endgame =
    {
        0, 0, 10, 20, 40, 70, 120, 0
    };

    static readonly int[] whiteDefendedPassedPawn_Middlegame =
    {
        0, 0, 5, 5, 10, 15, 20, 0
    };

    static readonly int[] whiteDefendedPassedPawn_Endgame =
    {
        0, 0, 5, 10, 15, 25, 40, 0
    };

    static readonly int[] blackPassedPawn_Middlegame =
    {
        0, 60, 35, 20, 10, 5, 0, 0
    };

    static readonly int[] blackPassedPawn_Endgame =
    {
        0, 120, 70, 40, 20, 10, 0, 0  
    };

    static readonly int[] blackDefendedPassedPawn_Middlegame =
    {
        0, 20, 15, 10, 5, 5, 0, 0   
    };
    static readonly int[] blackDefendedPassedPawn_Endgame =
    {
        0, 40, 25, 15, 10, 5, 0, 0  
    };
    static readonly int[] bishopMobility_middlegame =
    {
      // bonus or penalties for number of squares a bishop attacks
      -25, -11, -4, 0, 3, 7, 11, 15, 18, 21, 23, 25, 26, 27
    };
    static readonly int[] bishopMobility_endgame =
    {
      -30, -14, -4, 2, 7, 13, 19, 24, 29, 33, 36, 38, 40, 41
    };
    
    static readonly int[] rookMobility_middlegame =
    {
        -4, -2, 0, 1, 3, 5, 7, 9, 11, 12, 13, 14, 14, 14, 14
    };
    
    static readonly int[] rookMobility_endgame =
    {
        -15, -10, -5, -1, 2, 5, 8, 11, 14, 17, 19, 21, 22, 23, 24
    };

    static readonly int[] queenMobility_middlegame = 
    { 
        -30, -20, -10, -5, -1, 1, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 
    };

    static readonly int[] queenMobility_endgame = 
    { 
        -45, -30, -15, -4, 4, 12, 18, 23, 27, 30, 33, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51 
    };

    static readonly int[] knightMobility_middlegame = 
    { 
        -20, -10, -5, 0, 3, 6, 9, 11, 12 
    };

    static readonly int[] knightMobility_endgame = 
    { 
        -25, -12, -6, 0, 4, 8, 12, 15, 17 
    };
    


    public static readonly int[,] mvvLva = new int[6,6]
    {
        //victim
        // ROOK  BISHOP  KNIGHT  QUEEN  KING  PAWN
                                                        //attacker 
           {500,  380,    260,    620,     0,    140},     // ROOK
           {520,  400,    280,    640,     0,    160},     // BISHOP
           {540,  420,    300,    660,     0,    180},     // KNIGHT
           {480,  360,    240,    600,     0,    120},     // QUEEN
           {460,  340,    220,    580,     0,    100},     // KING
           {560,  440,    320,    680,     0,    200}      // PAWN
    };


    public static readonly int[] PST_WhitePawn_Starting =
    {
       0,   0,   0,   0,   0,   0,   0,  0,						
       5,  10,  10, -20, -20,  10,  10,  5,
       5,  -5, -10,   0,   0, -10,  -5,  5,
       0,   0,   0,  20,  20,   0,   0,  0,
       5,   5,  10,  25,  25,  10,   5,  5,
      10,  10,  20,  30,  30,  20,  10, 10,
      50,  50,  50,  50,  50,  50,  50, 50,
       0,   0,   0,   0,   0,   0,   0,  0
    };

    public static readonly int[] PST_WhitePawn_EndGame = 
    { 
         0,   0,   0,   0,   0,   0,   0,   0, 
       -30, -30, -30, -30, -30, -30, -30, -30,
       -20, -20, -20, -20, -20, -20, -20, -20,
       -10, -10, -10, -10, -10, -10, -10, -10,
        20,  30,  30,  30,  30,  30,  30,  20,
        50,  60,  60,  60,  60,  60,  60,  50,
        70,  80,  80,  80,  80,  80,  80,  70,
       100, 100, 100, 100, 100, 100, 100, 100
    }; 


    public static readonly int[] PST_BlackPawn_EndGame =
    {
        100, 100, 100, 100, 100, 100, 100, 100,
         70,  80,  80,  80,  80,  80,  80,  70,
         50,  60,  60,  60,  60,  60,  60,  50,
         20,  30,  30,  30,  30,  30,  30,  20,
        -10, -10, -10, -10, -10, -10, -10, -10,
        -20, -20, -20, -20, -20, -20, -20, -20,
        -30, -30, -30, -30, -30, -30, -30, -30,
          0,   0,   0,   0,   0,   0,   0,   0 
        
    };

    // public static readonly int[] PST_BlackPawn_Starting =
    // {
    //    /*a1*/ 100, 100, 100, 100, 100, 100, 100, 100,
    //    /*a2*/ -30, -30, -30, -30, -30, -30, -30, -30,
    //    /*a3*/ -25, -25, -25, -25, -25, -25, -25, -25, 
    //    /*a4*/ -10,  -5,   5,   5,   5,   5,  -5, -10,
    //    /*a5*/   0,   5,  15,  20,  20,  15,   5,   0,
    //    /*a6*/   5,   5,   5,  15,  15,   5,   5,   5,
    //    /*a7*/   0,   0,   0,   0,   0,   0,   0,   0, 
    //    /*a8*/   0,   0,   0,   0,   0,   0,   0,   0   
    // };

    public static readonly int[] PST_BlackPawn_Starting =
    {
       0,   0,   0,   0,   0,   0,   0,  0,   
      50,  50,  50,  50,  50,  50,  50, 50,   
      10,  10,  20,  30,  30,  20,  10, 10,   
       5,   5,  10,  25,  25,  10,   5,  5,  
       0,   0,   0,  20,  20,   0,   0,  0,  
       5,  -5, -10,   0,   0, -10,  -5,  5,  
       5,  10,  10, -20, -20,  10,  10,  5,  
       0,   0,   0,   0,   0,   0,   0,  0  
    };
    
    
    public static readonly int[] PST_Knight =
    {
       /*a1*/ -40, -20, -20, -20, -20, -20, -20, -40, 
       /*a2*/ -30, -10,   0,   0,   0,   0, -10, -30,
       /*a3*/ -15,   5,  20,  20,  20,  20,   5, -15, 
       /*a4*/ -15,   5,  20,  25,  25,  20,   5, -15,
       /*a5*/ -15,   5,  20,  25,  25,  20,   5, -15,
       /*a6*/ -15,   5,  20,  20,  20,  20,   5, -15,
       /*a7*/ -30, -10,   0,   0,   0,   0, -10, -30,
       /*a8*/ -40, -20, -20, -20, -20, -20, -20, -40
    };

    public static readonly int[] PST_Bishop =
    {
      -30, -10, -10, -10, -10, -10, -10, -30,
      -10,  15,   0,   0,   0,   0,  15, -10,
      -10,   0,   0,   0,   0,   0,   0, -10,  
      -10,   0,   5,  10,  10,   5,   0, -10,  
      -10,   0,   5,  10,  10,   5,   0, -10,
      -10,   0,   0,   0,   0,   0,   0, -10,  
      -10,  15,   0,   0,   0,   0,  15, -10,
      -30, -10, -10, -10, -10, -10, -10, -30
    };

    public static readonly int[] PST_Rook =
    {
       0,   0,   5,  10,  10,   5,   0,   0, 
       0,   5,   5,  10,  10,   5,   5,   0, 
       0,   0,   0,   0,   0,   0,   0,   0, 
       0,   0,   0,   0,   0,   0,   0,   0, 
       0,   0,   0,   0,   0,   0,   0,   0, 
       0,   0,   0,   0,   0,   0,   0,   0, 
       0,   5,   5,  10,  10,   5,   5,   0, 
       0,   0,   5,  10,  10,   5,   0,   0    
    };

    public static readonly int[] PST_Queen =
    {
      -20, -5, -5, -5, -5, -5, -5, -20,
       -5,  0,  0,  0,  0,  0,  0,  -5,
       -5,  0,  5,  5,  5,  5,  0,  -5,  
       -5,  0,  5,  5,  5,  5,  0,  -5,  
       -5,  0,  5,  5,  5,  5,  0,  -5,  
       -5,  0,  5,  5,  5,  5,  0,  -5,  
       -5,  0,  0,  0,  0,  0,  0,  -5,
      -20, -5, -5, -5, -5, -5, -5, -20 
    };

    public static readonly int[] PST_WhiteKing_Starting =
    {
      20,  30,  30,   0,   0,   0,  30,  20,
      10,  10,   0,   0,   0,   0,  10,  10,
     -20, -20, -20, -20, -20, -20, -20, -20,
     -20, -30, -30, -40, -40, -30, -30, -20,     
     -30, -40, -40, -50, -50, -40, -40, -30,
     -30, -40, -40, -50, -50, -40, -40, -30,
     -30, -40, -40, -50, -50, -40, -40, -30,
     -30, -40, -40, -50, -50, -40, -40, -30

    };

    public static readonly int[] PST_BlackKing_Starting =
    {
 
     -30, -40, -40, -50, -50, -40, -40, -30,
     -30, -40, -40, -50, -50, -40, -40, -30,
     -30, -40, -40, -50, -50, -40, -40, -30,
     -30, -40, -40, -50, -50, -40, -40, -30,
     -20, -30, -30, -40, -40, -30, -30, -20,
     -20, -20, -20, -20, -20, -20, -20, -20, 
      10,  10,   0,   0,   0,   0,  10,  10,
      20,  30,  30,   0,   0,   0,  30,  20

    };

    public static readonly int[] PST_King_EndGame =
    {
      -40, -30, -30, -30, -30, -30, -30, -40, 
      -30, -20, -10,   0,   0, -10, -20, -30,
      -30, -10,  10,  15,  15,  10, -10, -30,
      -30, -10,  15,  20,  20,  15, -10, -30,
      -30, -10,  15,  20,  20,  15, -10, -30,
      -30, -10,  10,  15,  15,  10, -10, -30,
      -30, -20, -10,   0,   0, -10, -20, -30,
      -40, -30, -30, -30, -30, -30, -30, -40
    };


    static void InitializePassedPawnMasks()
    {
        // A-file (0): Files A and B
        passedPawnFileMask[0] = fileMask[0] | fileMask[1];
        
        // B-file (1) through G-file (6): Files (file-1), file, and (file+1)
        for (int i = 1; i <= 6; i++)
        {
            passedPawnFileMask[i] = fileMask[i - 1] | fileMask[i] | fileMask[i + 1];
        }
        
        // H-file (7): Files G and H
        passedPawnFileMask[7] = fileMask[6] | fileMask[7];

        InitializeMasks();
    }

    static void InitializeMasks()
    {
        for (int square = 0; square < 64; square++)
        {
            int rank = square / 8;
            int file = square % 8;

            ulong forwardRanks = 0;
            
            // 1. Combine all ranks ahead of the pawn using OR
            for (int r = rank + 1; r <= 7; r++)
            {
                forwardRanks |= rankMask[r];
            }

            // 2. Intersect the files and the forward ranks using AND
            whitePassedPawnMask[square] = passedPawnFileMask[file] & forwardRanks;

            ulong backwardRanks = 0;

            for(int r = rank - 1; r >= 0; r--)
            {
                backwardRanks |= rankMask[r];
            }
            blackPassedPawnMask[square] = passedPawnFileMask[file] & backwardRanks;
        }
    }

    static void InitializeIsolatedPawnMasks()
    {
        // A-file (0): Only check B-file
        isolatedPawnMask[0] = fileMask[1];
        
        // B-file to G-file (1-6): Check adjacent files
        for (int i = 1; i <= 6; i++)
        {
            isolatedPawnMask[i] = fileMask[i - 1] | fileMask[i + 1];
        }
        
        // H-file (7): Only check G-file
        isolatedPawnMask[7] = fileMask[6];
    }


    //======================================================Static Exchange Evaluation======================================================

    // Static Exchange Evaluation is an algorithm that is used to calculate what the outcome of an exchange will be. In all fairness, this can be done using makemove and recursice calls but that is too slow. So we use a iterative approach.
    // In iterative approach, we have an array 'gains[]' that store the pure material eval after each exchange. Based on moving piece and captured piece, the array gains[] stores the score and if after all sequences of captures and recaptures are done, and the score is bad, then that capture is also bad so we never go in the full quiescence loop of searching through all captures.


    public int CalculateSEE(Board board, Move move)
    {
        int startSquare = move.StartSquare;
        int targetSquare = move.TargetSquare;

        int movingPiece = board.pieceOnSquare[startSquare];
        int capturedPiece = board.pieceOnSquare[targetSquare];

        if(capturedPiece == -1) return 0; //if quiet move then no use for SEE, return 0.

        //we set the max value for gain to 32 so it will store 32 plys of captures at one square at most.
        int[] gain = new int[32];
        int d = 0; //counter for the gain array

        gain[d] = board.PieceValue[capturedPiece]; //the first gain is the value of the piece we are initially capturing.

        // simulate the piece moving internally for the SEE
        int currentAttackerValue = board.PieceValue[movingPiece]; //store the value of moving piece
        int colorToMove = board.colorToMove ^ 1; //we don't change the actual board state but store the changed turn in another int.

        ulong occupiedPiecesMask = board.occupiedMask;
        occupiedPiecesMask &= ~(1UL << startSquare); //we remove the piece from the startsquare in the temporary occupancy bitboard. Since this code block will only trigger for captures, the captured square will stil store the 1, so no need to update it.

        while(true)
        {
            d++;

            if(d >= 32) break;

            int nextAttackerSquare = GetLeastValuableAttacker(board, targetSquare, colorToMove, occupiedPiecesMask);
            if(nextAttackerSquare == -1) break; //no more pieces attacking the square

            int nextAttackerPiecce = board.pieceOnSquare[nextAttackerSquare];

            // gain for d index is current attacker value minus the gain for previous iteration
            gain[d] = currentAttackerValue - gain[d-1];

            // If the King is captured, we instantly break.
            if (Math.Max(gain[d], gain[d - 1]) >= 10000) break;

            //for next iteration, current attacker value becomes next attacker piece's value
            currentAttackerValue = board.PieceValue[nextAttackerPiecce];
            colorToMove ^= 1;

            //remove the attacker piece from the temporary board
            occupiedPiecesMask &= ~(1UL << nextAttackerSquare);
        
        }

        while (--d > 0)
        {
            gain[d - 1] = -Math.Max(-gain[d - 1], gain[d]);
        }
        

        return gain[0];
        
    }

    public int GetLeastValuableAttacker(Board board, int targetSquare, int colorToMove, ulong occupiedMask)
    {

        ulong pawnAttack = (colorToMove == 0) ? AttackTables.blackPawnAttacks[targetSquare] : AttackTables.whitePawnAttacks[targetSquare];
        ulong pawns = pawnAttack & board.pieceBitboards[(int)Piece.WhitePawns + (colorToMove * 6)] & occupiedMask;
        if(pawns != 0) return BitOperations.TrailingZeroCount(pawns);


        ulong knightAttack = AttackTables.knightAttacks[targetSquare];
        ulong knights = knightAttack & board.pieceBitboards[(int)Piece.WhiteKnights + (colorToMove * 6)] & occupiedMask;
        if(knights != 0) return BitOperations.TrailingZeroCount(knights);

        ulong rawBishopAttacks = AttackTables.GetBishopAttacks(targetSquare, occupiedMask);
        ulong bishops = rawBishopAttacks & board.pieceBitboards[(int)Piece.WhiteBishops + (colorToMove * 6)] & occupiedMask;
        if(bishops != 0) return BitOperations.TrailingZeroCount(bishops);

        ulong rawRookAttacks = AttackTables.GetRookAttacks(targetSquare, occupiedMask);
        ulong rooks = rawRookAttacks & board.pieceBitboards[(int)Piece.WhiteRooks + (colorToMove * 6)] & occupiedMask;
        if(rooks != 0) return BitOperations.TrailingZeroCount(rooks);

        ulong rawQueenAttacks = AttackTables.GetQueenAttacks(targetSquare, occupiedMask);
        ulong queens = rawQueenAttacks & board.pieceBitboards[(int)Piece.WhiteQueens + (colorToMove * 6)] & occupiedMask;
        if(queens != 0) return BitOperations.TrailingZeroCount(queens);

        ulong kingAttacks = AttackTables.kingAttacks[targetSquare];
        ulong kings = kingAttacks & board.pieceBitboards[(int)Piece.WhiteKing + (colorToMove * 6)] & occupiedMask;
        if(kings!= 0) return BitOperations.TrailingZeroCount(kings);
        


        return -1;//no attacker
    }



    static double baseReduction = 0.75;
    static double divisor = 2.25; 

    static void InitializeReductionTable()
    {
        for (int d = 0; d < 128; d++) 
        {
            for (int m = 0; m < 256; m++) 
            {
                // Avoid math errors with log(0)
                if (d > 0 && m > 0) 
                {
                    ReductionTable[d,m] = (int)(baseReduction + Math.Log(d) * Math.Log(m) / divisor);
                } 
                else 
                {
                    ReductionTable[d,m] = 0;
                }
            }
        }

    }





}

/*
    mvvLva(3,5) -> Queen capturing pawn. 3-> attacker, 5-> victim. x = attacker. y = victim.
    knight capturing rook should be lower than rook capturing queen
    lowest score for free queen is 375 which is queen capturuing queen. Capturing any other free piece should always be lower than that of queen capturing queen.

    queen - 400, 430, 450, 375, 600 ----- 375 - 600 ----- 580 - 680
    rook - 340, 350, 360, 320, 380 ------ 320 - 380 ----- 460 - 560
    bishop - 280, 300, 310, 265, 310 ----- 265 - 310 ----- 340 - 440
    knight - 275, 280, 305, 260, 320 ----- 260 - 320 ----- 220 - 320
    pawn - 160, 170, 175, 150, 200 ----- 150 - 200 ----- 100-200


    public static readonly int[,] mvvLva = new int[6,6]
    {
        //victim
        // ROOK  BISHOP  KNIGHT  QUEEN  KING  PAWN
                                                        //attacker 
           {500,  380,    260,    620,     0,    140},     // ROOK
           {520,  400,    280,    640,     0,    160},     // BISHOP
           {540,  420,    300,    660,     0,    180},     // KNIGHT
           {480,  360,    240,    600,     0,    120},     // QUEEN
           {460,  340,    220,    580,     0,    100},     // KING
           {560,  440,    320,    680,     0,    200}      // PAWN
    };



*/