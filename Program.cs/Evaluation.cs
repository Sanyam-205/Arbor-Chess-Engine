using System;
using System.Net;
using System.Numerics;
using static Board;
public class Evaluation
{

    static readonly int[] ColorMultiplier = {1, -1};
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

        ulong whiteKnights = board.pieceBitboards[(int)Piece.WhiteKnights];
        while(whiteKnights!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteKnights);

            //Adding positional scores based on PST
            whiteMiddlegameScore += PST_Knight[squareIndex] + 330;
            whiteEndgameSCore += PST_Knight[squareIndex] + 310;

            whiteKnights &= whiteKnights -1;
        }
        
        ulong blackKnights = board.pieceBitboards[(int)Piece.BlackKnights];
        while(blackKnights!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackKnights);

            //Adding positional scores based on PST
            blackMiddlegameScore += PST_Knight[squareIndex] + 330;
            blackEndgameScore += PST_Knight[squareIndex] + 310;

            blackKnights &= blackKnights -1;
        }
        
        ulong whiteBishops = board.pieceBitboards[(int)Piece.WhiteBishops];
        while(whiteBishops!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteBishops);

            //Adding positional scores based on PST
            whiteMiddlegameScore += PST_Bishop[squareIndex] + 340;
            whiteEndgameSCore += PST_Bishop[squareIndex] + 340;

            whiteBishops &= whiteBishops -1;
        }

        ulong blackBishops = board.pieceBitboards[(int)Piece.BlackBishops];
        while(blackBishops!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackBishops);

            //Adding positional scores based on PST
            blackMiddlegameScore += PST_Bishop[squareIndex] + 340;
            blackEndgameScore += PST_Bishop[squareIndex] + 340;

            blackBishops &= blackBishops -1;
        }

        ulong whiteRooks = board.pieceBitboards[(int)Piece.WhiteRooks];
        while(whiteRooks!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteRooks);

            whiteMiddlegameScore+= PST_Rook[squareIndex] + 500;
            whiteEndgameSCore += PST_Rook[squareIndex] + 500;

            whiteRooks &= whiteRooks - 1;
        }

        ulong blackRooks = board.pieceBitboards[(int)Piece.BlackRooks];
        while(blackRooks!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackRooks);

            blackEndgameScore += PST_Rook [squareIndex] + 500;
            blackMiddlegameScore += PST_Rook [squareIndex] + 500;

            blackRooks &= blackRooks -1;
        }

        ulong whiteQueens = board.pieceBitboards[(int)Piece.WhiteQueens];
        while(whiteQueens != 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteQueens);

            whiteMiddlegameScore += PST_Queen[squareIndex] + 900;
            whiteEndgameSCore += PST_Queen[squareIndex] + 900;

            whiteQueens &= whiteQueens - 1;

        }

        ulong blackQueens = board.pieceBitboards[(int)Piece.BlackQueens];
        while(blackQueens!= 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackQueens);

            blackMiddlegameScore += PST_Queen[squareIndex] + 900;
            blackEndgameScore += PST_Queen[squareIndex] + 900;


            blackQueens &= blackQueens - 1;    
        }

        ulong whitePawns = board.pieceBitboards[(int)Piece.WhitePawns];
        while(whitePawns!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whitePawns);

            whiteMiddlegameScore += PST_WhitePawn_Starting[squareIndex] + 100;
            whiteEndgameSCore += PST_WhitePawn_EndGame[squareIndex] + 100;

            whitePawns &= whitePawns -1;
        }

        ulong blackPawns = board.pieceBitboards[(int)Piece.BlackPawns];
        while(blackPawns!=0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackPawns);

            blackMiddlegameScore += PST_BlackPawn_Starting[squareIndex] + 100;
            blackEndgameScore += PST_BlackPawn_EndGame[squareIndex] + 100;

            blackPawns &= blackPawns -1;
        }

        ulong whiteKing = board.pieceBitboards[(int)Piece.WhiteKing];
        while(whiteKing != 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(whiteKing);

            whiteMiddlegameScore += PST_WhiteKing_Starting[squareIndex]; //king is assigned base material evaluation of 0;
            whiteEndgameSCore += PST_King_EndGame[squareIndex];

            whiteKing &= whiteKing-1;
        }

        ulong blackKing = board.pieceBitboards[(int)Piece.BlackKing];
        while(blackKing!= 0)
        {
            int squareIndex = BitOperations.TrailingZeroCount(blackKing);

            blackMiddlegameScore += PST_BlackKing_Starting[squareIndex];
            blackEndgameScore += PST_King_EndGame[squareIndex];

            blackKing &= blackKing-1;
        }

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