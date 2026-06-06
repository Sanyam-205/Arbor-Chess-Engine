using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using static Board;
using System.IO.Pipelines;

class Program
{
    static void Main()
    {
        
        Board board = new Board();
        MoveGenerator moveGenerator = new MoveGenerator(); 
        Search search = new Search();
        Evaluation evaluation = new Evaluation();

        // perft6 - kiwipete
        // fen0 - starting position
        // fen1 - middlegame (mine possibly)
        // perft2 - middlegame (perft)
        // fen3 - scillian defense
        // fen13 - endgame (mine)

        string fen = TestPositions.fen0;
        // string fen1 = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 1";
        
        //"8/8/2K5/7q/1k6/8/8/6r1 b - - 0 1"
        FenUtility.LoadFromFen(fen, board);
        Console.WriteLine(evaluation.EvaluatePosition(board));

        Move move1 = new Move(12,20);
        board.MakeMove(move1);
        BoardPrinter.PrintBitboard(board);
        Console.WriteLine($"Evaluation after move 1 : {evaluation.EvaluatePosition(board)}");

        Move move2 = new Move(57,42);
        board.MakeMove(move2);
        BoardPrinter.PrintBitboard(board);
        Console.WriteLine($"Evaluation after move 2 : {evaluation.EvaluatePosition(board)}");
        
        Move move3 = new Move(11,19);
        board.MakeMove(move3);
        BoardPrinter.PrintBitboard(board);
        Console.WriteLine($"Evaluation after move 3 : {evaluation.EvaluatePosition(board)}");
        

        // int infinity = 9999999;
        // int searchDepth = 6;

        // Stopwatch stopwatch = Stopwatch.StartNew();
        

        // for (int i = 0; i < 100; i++)
        // {
        //     search.StartSearch(board, moveGenerator, evaluation, searchDepth, -infinity, infinity);
        // }

        // // long nodes = PerftTool.Perft(board, moveGenerator, searchDepth);

        // stopwatch.Stop();

        // double averageMilliseconds = stopwatch.Elapsed.TotalMilliseconds / 100.0;
        
        // Console.WriteLine($"Leaf count =  {search.leafCount} \nNode count = {search.nodeCount}");
        
        // double nps = (search.nodeCount * 100) / stopwatch.Elapsed.TotalSeconds;
        
        // Console.WriteLine($"Average Time: {stopwatch.Elapsed.TotalMicroseconds:F2} ms");
        // Console.WriteLine($"NPS: {nps:N0}");
        
        // Console.WriteLine($"Time: {averageMilliseconds:F2} ms");
        // Console.WriteLine($"NPS: {nps:N0}");

        // Move move = new Move(12,20,0);
        // board.MakeMove(move);

        // Move move = new Move (42, 56);
        // board.MakeMove(move);
        // Console.WriteLine(board.phaseScore);
        // board.UnmakeMove(move);
        // Console.WriteLine(board.phaseScore);

        
        
    
    #region particular fen perft check
        /*
        string stockfishPath = @"D:\Stockfish\stockfish\stockfish-windows-x86-64-avx2.exe";
        string testFen = TestPositions.perft11; 
        int depth = 5;

        using (var stockfish = new StockfishWrapper(stockfishPath))
        {
            Console.WriteLine($"--- Starting Perft Test (Depth {depth}) ---");

            // Get Stockfish results
            var sfResults = stockfish.GetPerftResults(testFen, depth);

            // Get Your Engine results
            Board board = new Board();
            MoveGenerator moveGen = new MoveGenerator();
            FenUtility.LoadFromFen(testFen, board);
            
            // Assuming PerftTool.PerftDivide is modified to return a Dictionary
            var myResults = PerftTool.PerftDivide(board, moveGen, depth);

            if (!Compare(sfResults, myResults))
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("\n!!! STOPPING: MISMATCH DETECTED !!!");
                Console.ResetColor();
                Console.WriteLine($"To reproduce, set testFen = \"{testFen}\"");
             
            }

                Console.WriteLine("Result: OK");
        }
        Console.WriteLine("\nTesting sequence complete.");
        */
    #endregion        
    
    #region Perft from txt file
        /*
        string stockfishPath = @"D:\Stockfish\stockfish\stockfish-windows-x86-64-avx2.exe";
        string fenFilePath = @"D:\Chess Engine\Program.cs\PerftPositions.txt";
        int testCount = 1410;
        int depth = 5;

        List<string> testFens = GetRandomFens(fenFilePath, testCount);

        using (var stockfish = new StockfishWrapper(stockfishPath))
        {
            for (int i = 0; i < testFens.Count; i++)
            {
                string currentFen = testFens[i];
                Console.WriteLine($"\n[Test {i + 1}/{testFens.Count}] FEN: {currentFen}");

                // 1. Get Truth from Stockfish
                var sfResults = stockfish.GetPerftResults(currentFen, depth);

                // 2. Setup Your Engine
                Board board = new Board();
                MoveGenerator moveGen = new MoveGenerator();
                FenUtility.LoadFromFen(currentFen, board);

                // 3. Run Your Engine (Ensure PerftDivide returns Dictionary<string, long>)
                var myResults = PerftTool.PerftDivide(board, moveGen, depth);

                // 4. Compare and Break if Bug Found
                if (!Compare(sfResults, myResults))
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("\n!!! STOPPING: MISMATCH DETECTED !!!");
                    Console.ResetColor();
                    Console.WriteLine($"To reproduce, set testFen = \"{currentFen}\"");
                    break; 
                }

                Console.WriteLine("Result: OK");
            }
        }
        */
    #endregion
        



        Console.ReadLine();



    }

/*
    static void Compare(Dictionary<string, long> sf, Dictionary<string, long> mine)
    {
        bool errorFound = false;
        foreach (var move in sf.Keys)
        {
            if (!mine.ContainsKey(move))
            {
                Console.WriteLine($"[Error] Move {move} is MISSING in your engine.");
                errorFound = true;
            }
            else if (mine[move] != sf[move])
            {
                Console.WriteLine($"[Error] Move {move}: SF={sf[move]}, Yours={mine[move]}");
                errorFound = true;
            }
        }

        foreach (var move in mine.Keys)
        {
            if (!sf.ContainsKey(move))
            {
                Console.WriteLine($"[Error] Move {move} is EXTRA (Illegal) in your engine.");
                errorFound = true;
            }
        }

        if (!errorFound) Console.WriteLine("All nodes match perfectly!");
    }
*/
    
    static bool Compare(Dictionary<string, long> sf, Dictionary<string, long> mine)
    {
        bool match = true;

        // Check for moves your engine missed or got wrong
        foreach (var move in sf.Keys)
        {
            if (!mine.ContainsKey(move))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[MISSING] SF found '{move}' ({sf[move]} nodes), but you didn't.");
                Console.ResetColor();
                match = false;
            }
            else if (mine[move] != sf[move])
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[MISMATCH] '{move}': SF={sf[move]}, Yours={mine[move]}");
                Console.ResetColor();
                match = false;
            }
        }

        // Check for moves your engine generated that are illegal
        foreach (var move in mine.Keys)
        {
            if (!sf.ContainsKey(move))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[EXTRA/ILLEGAL] Your engine generated '{move}', but SF did not.");
                Console.ResetColor();
                match = false;
            }
        }

        return match;
    }
    
    
    
    public static List<string> GetRandomFens(string filePath, int count)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File not found at {filePath}");
            return new List<string>();
        }

        string[] allLines = File.ReadAllLines(filePath);
        
        // If the file has fewer lines than requested, just take them all
        if (allLines.Length <= count) return allLines.ToList();

        Random rng = new Random();
        // Shuffles the lines randomly and takes the first 'count' items
        return allLines.OrderBy(x => rng.Next()).Take(count).ToList();
    }



}