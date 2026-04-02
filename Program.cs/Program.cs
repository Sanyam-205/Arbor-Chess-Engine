using System;
using System.Numerics;
class Program
{
    static void Main()
    {
        Board board = new Board();

        string fen0 = "r3r1k1/p3bppp/1q2pn2/3p4/3p1B2/1PN2Q2/P1P2PPP/3R1RK1 w - - 0 2";
        // string fen1 = "r3r1k1/1bpp1p1p/p2b1q2/1p1npnN1/B2PP3/1PN1B3/P1PQ1PPP/R3R1K1 b KQkq - 0 1";
        // string fen2 = "r1bqkbnr/pppp1ppp/2n5/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 3 4"; //scholar's mate
        // string fen3 = "rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2"; //scillian
        // string fen4 = "rnbqkbnr/ppppp1pp/8/4Pp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 1"; //enPassant
        // string fen5 = "r1bqkb1r/pppppppp/2n2n2/3p4/1q2p1p1/P4N2/1PPPPPPP/RNBQKB1R b KQkq - 0 1"; //custom testing fen
        // string fen6 = "r3rkq1/p2qbp1q/1q3n2/3p3q/5Bq1/1PN2Q2/P1P1pPPP/3R1RK1 b - - 0 2";
        // string fen7 = "1R6/5pk1/6pp/1P2p3/1r1p4/3K3P/6P1/8 w - - 0 2";
        // string fen8 = "8/8/8/8/8/r7/P7/8 w - - 0 1";
        FenUtility.LoadFromFen(fen0, board);

        // PrintUlongBitboard(board.colorBitboard[(int)Board.PieceTeam.WhitePieces], "White Pieces Bitboard");
        // PrintUlongBitboard(board.colorBitboard[(int)Board.PieceTeam.BlackPieces], "Black Pieces Bitboard");
        // PrintUlongBitboard(board.AllPieces, "All Pieces Bitboard");


        MoveGenerator moveGenerator = new MoveGenerator(); //create instance of MoveGenerator

        // 2. Prepare the move list and counter
        Move[] moveList = new Move[256];
        int moveCount = 0;


        // BoardPrinter.PrintBitboard(board);

        // Move myMove = new Move(12,53);

        // // Console.WriteLine("Enter Move\n");
        // // string move = Console.ReadLine();

        // board.MovePiece(MoveUtility.MoveFromName("e2e8q"));
        
        // Console.WriteLine("After move -");

        // BoardPrinter.PrintBitboard(board);
        
        // void MakeMove(Board board)
        // {

        //     // moveGenerator.GenerateKnightMoves(board, moveList, ref moveCount);
        //     // moveGenerator.GenerateKingMoves(board, moveList, ref moveCount);
        //     // moveGenerator.GeneratePawnMoves(board, moveList, ref moveCount);
            
        
        //     int count = 0;
        //     while(count<4)
        //     {
        //         BoardPrinter.PrintBitboard(board);
        //         Console.WriteLine($"Enter move ");
        //         string move = Console.ReadLine();
        //         board.MovePiece(MoveUtility.MoveFromName(move));
        //         Console.WriteLine(board.colorToMove);

        //         // moveGenerator.GenerateKnightMoves(board, moveList, ref moveCount);
        //         // moveGenerator.GenerateKingMoves(board, moveList, ref moveCount);
        //         // moveGenerator.GeneratePawnMoves(board, moveList, ref moveCount);

        //         // for(int i = 0; i < moveCount; i++)
        //         // {
        //         //     Console.WriteLine($"StartSquare = {IndexToSquare(moveList[i].StartSquare)} and Target Square = {IndexToSquare(moveList[i].TargetSquare)}");
        //         // } 
               
        //         count++;
               
        //     }
        
        // }
        
        // MakeMove(board);

        moveGenerator.GenerateKnightMoves(board, moveList, ref moveCount);
        moveGenerator.GenerateKingMoves(board, moveList, ref moveCount);
        moveGenerator.GeneratePawnMoves(board, moveList, ref moveCount);
        
        // for(int i = 0; i < moveCount; i++)
        // {
        //     Console.WriteLine($"StartSquare = {IndexToSquare(moveList[i].StartSquare)} and Target Square = {IndexToSquare(moveList[i].TargetSquare)}");
        // }
        
        // Move move = new Move(1,18);
        // board.MovePiece(move);
        // BoardPrinter.PrintBitboard(board);

        // for (int i = 0; i<64; i++)
        // {
        //     ulong rookAttack = AttackTables.GenerateRookMask(i);    
        //     string s = $"Rook an square {IndexToSquare(i)}";
        //     Console.WriteLine($"Bitboatd = {rookAttack}");
        //     PrintUlongBitboard(rookAttack, s);
            
        // }
        
        // for (int i = 0; i<64; i++)
        // {
        //     ulong bishopAttack = AttackTables.GenerateBishopAttacks(i, 0UL);    
        //     string s = $"bishop attack on fly {IndexToSquare(i)}";
        //     Console.WriteLine($"Bitboard = {bishopAttack}");
        //     PrintUlongBitboard(bishopAttack, s);
            
        // }
        
        
        // int[] relevantRookBits = new int[64];
        // for(int rank = 7; rank >= 0; rank--)
        // {
        //     for (int file = 0; file <= 7; file++)
        //     {
        //         int square = rank*8 + file;

        //         ulong rookAttack = AttackTables.GenerateBishopMask(square);
                
        //         int count = 0;
        //         while(rookAttack != 0)
        //         {
                        
                        
        //             // Console.WriteLine($"Bitboard = {bishopAttack}");
        //             // PrintUlongBitboard(bishopAttack, "with blocker on 36 ");
                    
        //             int possibleMoves = BitOperations.TrailingZeroCount(rookAttack);
        //             rookAttack &=rookAttack-1;
        //             count+=1;
                    
        //         }
        //         relevantRookBits[square] = count;
        //         Console.Write($"{relevantRookBits[square]}, ");
        //         //Console.Write($"{count}, ");
        //     }
        // }    

        
        
        /*
        6, 5, 5, 5, 5, 5, 5, 6, 
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5, 
        5, 5, 7, 9, 9, 7, 5, 5, 
        5, 5, 7, 9, 9, 7, 5, 5, 
        5, 5, 7, 7, 7, 7, 5, 5, 
        5, 5, 5, 5, 5, 5, 5, 5, 
        6, 5, 5, 5, 5, 5, 5, 6
        */


        //Generate knight moves
        // moveGenerator.GenerateKnightMoves(board, moveList, ref moveCount);
        
        
        // for(int rank = 7; rank >= 0; rank--)
        // {
        //     for (int file = 0; file <= 7; file++)
        //     {
        //         int square = rank*8 + file;
                
        //         AttackTables.CalculateRookTable(square);
        //         Console.Write($"{AttackTables.RookMagicNumbers[square]}, ");       


        //     }
        // }

        // for(int i = 0; i < 64; i++)
        // {
        //     AttackTables.CalculateBishopTable(i);
        //     Console.Write($"{AttackTables.BishopMagicNumbersArr[i]}, ");
        // }
        
        // AttackTables.PrintResult();
           
        
        Console.ReadLine(); 
    }

    static string IndexToSquare(int index)
    {
        char file = (char)('a' + (index % 8));
        char rank = (char)('1' + (index / 8));
        
        return $"{file}{rank}";
    }
    static void PrintUlongBitboard(ulong bitboard, string title)
    {
        Console.WriteLine($"{title}:");
        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file <= 7; file++)
            {
                int square = rank * 8 + file;
                ulong mask = 1UL << square;
                char bitChar = (bitboard & mask) != 0 ? '1' : '.';
                Console.Write(bitChar + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
    }
     

}
