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

        UCIUtility.Loop(board, moveGenerator, evaluation, search);

        // string fen1 = "8/5pk1/1R2p3/4P1pp/8/4K3/8/5r2 w - - 8 61";
        // string errorFen2 = "Q1Q3Q1/8/8/2P5/8/1K1k3P/8/8 b - - 14 82";


        // perft6 - kiwipete
        // fen0 - starting position
        // fen1 - middlegame (mine possibly)
        // perft2 - middlegame (perft)
        // fen13 - endgame (mine)

        // string fen1 = TestPositions.perft6;
        // FenUtility.LoadFromFen(fen1, board);
        // // 
        // int searchDepth = 5;
        // int infinity = 500000;
        // //
        // Stopwatch stopwatch = new Stopwatch();
        // stopwatch.Start();
        // int loopCount = 1;
        // for(int i = 0; i<loopCount; i++)
        // {
        //     search.ClearHistory();
        //     TT.Clear();
        //     int eval = search.StartSearch(board, moveGenerator, evaluation, searchDepth, -infinity, infinity, 0);
        // }
        // long nodes = PerftTool.Perft(board, moveGenerator, searchDepth);
        // stopwatch.Stop();
        // double time = stopwatch.Elapsed.TotalMilliseconds;
        // double time_sec = time/1000;
        // Console.WriteLine($"Perft test : Nodes: {nodes :N0} \nTime: {time :N0}ms \nNPS: {nodes/time_sec :N0}");
        // Console.WriteLine($"NodeCount = {(search.nodeCount + search.qNodes) :N0}\nAverage time = {stopwatch.Elapsed.TotalMilliseconds / loopCount:N0}ms\nNPS = {(search.nodeCount+search.qNodes) / (stopwatch.Elapsed.TotalSeconds / loopCount):N0}");
        // // // Program.cs (C# Top-Level Statements)
        // //NodeCount = 17,916,356
        // Console.WriteLine($"Evaluation {eval}");
        // search.PrintPrincipalVariation();

        


        // Console.WriteLine($"Evaluation: {eval}");
        // search.PrintPrincipalVariation();


        /* can delete
        // string[] crashTest =
        // {
        //   "position startpos",
        //   "go depth 6",
        //   "position startpos moves b1c3 d7d5",
        //   "go depth 6",
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5",
        //   "go depth 6",  
        //   "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5",
        //   "go depth 6",  
        // };
        */


        // string[] crashTest =
        // {
        //
        // "position startpos",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8 c7b6 c8b8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8 c7b6 c8b8 b6a5 e4a8",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8 c7b6 c8b8 b6a5 e4a8 e2a6 e8e5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8 c7b6 c8b8 b6a5 e4a8 e2a6 e8e5 d1d5 e5d5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8 c7b6 c8b8 b6a5 e4a8 e2a6 e8e5 d1d5 e5d5 a3c5 d5c5",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8 c7b6 c8b8 b6a5 e4a8 e2a6 e8e5 d1d5 e5d5 a3c5 d5c5 a5a4 a8a6",
        // "go depth 6",  
        // "position startpos moves b1c3 d7d5 e2e3 e7e5 d2d4 e5e4 f1b5 c7c6 b5e2 d8g5 e1f1 g5d8 f2f3 g8f6 f3e4 d5e4 c1d2 f8d6 g1h3 c8h3 g2h3 e8g8 f1g1 c6c5 d4c5 d6c5 b2b4 c5b4 c3e4 f6e4 d2b4 d8g5 g1f1 f8d8 d1e1 g5f5 f1g2 f5g5 g2f3 g5f5 f3g2 f5g5 g2f3 b8c6 a1d1 g5f5 f3g2 f5g5 g2f3 a7a5 b4a3 g5f6 f3e4 d8e8 e4d3 a8d8 d3c4 b7b5 c4b5 f6f5 b5c6 f5e4 c6c7 d8c8 c7b6 c8b8 b6a5 e4a8 e2a6 e8e5 d1d5 e5d5 a3c5 d5c5 a5a4 a8a6 e1a5 c5a5",
        // "go depth 6",  
        // };


        // string simulatedInput = string.Join(Environment.NewLine, crashTest);// Combine all commands into a single block of text separated by newlines

        
        
        // using (System.IO.StringReader reader = new System.IO.StringReader(simulatedInput)) // Redirect standard input to read from our string instead of the keyboard
        // {
        //     Console.SetIn(reader);
            
        //     /*// The UCI loop will read commands line-by-line. 
        //     // Because "go depth 6" blocks while searching, it naturally waits 
        //     // for a response before reading the next line!*/
        //     UCIUtility.Loop(board, moveGenerator, evaluation, search);
            
        // }
        
        
        // Console.SetIn(new System.IO.StreamReader(Console.OpenStandardInput()));// Restore standard input for normal console usage
        

        /*
        // Stopwatch stopwatch = new Stopwatch();

        // stopwatch.Start();
        // for(int i = 0; i<10; i++)
        // {
        //     search.StartSearch(board, moveGenerator, evaluation, searchDepth, -infinity, infinity, 0);
        //     Console.WriteLine($"Run {i+1}: Search nodes = {search.nodeCount} , Quiescence nodes = {search.qNodes}, Total Nodes = {search.nodeCount + search.qNodes}");
        // }
        
        // // stopwatch.Stop();
        //
        // double hitRate = 100.0 * search.ttHits / search.ttProbes;
        //
        // double totalNodes = search.nodeCount + search.qNodes;
        // double avgTimeMs = stopwatch.Elapsed.TotalMilliseconds / 10.0;
        //
        // double nps = totalNodes / (stopwatch.Elapsed.TotalSeconds/10);
        //
        // Console.WriteLine($"Search nodes = {search.nodeCount:N0}");
        // Console.WriteLine($"Quiescence nodes = {search.qNodes:N0}");
        // Console.WriteLine($"Total nodes = {totalNodes:N0}");
        //
        // Console.WriteLine($"Average Time = {avgTimeMs:F2}");
        // Console.WriteLine($"NPS = {nps:N0}");
        //
        // Console.WriteLine($"TT Cutoffs: {search.ttCutoffs}");
        // Console.WriteLine($"TT Probes: {search.ttProbes}");
        // Console.WriteLine($"TT Hits: {search.ttHits}");
        // Console.WriteLine($"TT Hit Rate: {hitRate:F2}%");

        */
       
        


        /*
            starting =
                        Search nodes = 34,038
                        Quiescence nodes = 27,550
                        Total nodes = 61,588
                        Average Time = 90.43
                        NPS = 681,079
                        TT Probes: 477477
                        TT Hits: 138568
                        TT Hit Rate: 29.02%
            
            middlegame =
                        Search nodes = 211,100
                        Quiescence nodes = 698,847
                        Total nodes = 909,947
                        Average Time = 1139.25
                        NPS = 798,726
                        TT Probes: 2452722
                        TT Hits: 938756
                        TT Hit Rate: 38.27%
                        NODES DROPPED FROM 1.8M TO 900K

            Endgame =
                        Search nodes = 39,706
                        Quiescence nodes = 85,987
                        Total nodes = 125,693
                        Average Time = 418.82
                        NPS = 300,113
                        TT Probes: 1022593
                        TT Hits: 272416
                        TT Hit Rate: 26.64%
                        HAD 4.1M NODES BEFORE.....SOMETHING SEEMS WRONG

            Middlegame1 =
                        Search nodes = 59,487
                        Quiescence nodes = 115,517
                        Total nodes = 175,004
                        Average Time = 180.53
                        NPS = 969,368
                        TT Probes: 775731
                        TT Hits: 90365
                        TT Hit Rate: 11.65%
                        NODES DROPPED FROM 581K

            Kiwipete = 
                        Search nodes = 184,708
                        Quiescence nodes = 987,582
                        Total nodes = 1,172,290
                        Average Time = 1446.02
                        NPS = 810,701
                        TT Probes: 2096712
                        TT Hits: 786035
                        TT Hit Rate: 37.49%
                        NODES DROPPED FROM 2.59M



        */

        /* AFTER DISABLING MOVE ORDERING IN TT

            Starting -  Search nodes = 181,318
                        Quiescence nodes = 244,335
                        Total nodes = 425,653
                        Average Time = 323.12
                        NPS = 1,317,330
                        TT Probes: 1994498
                        TT Hits: 329782
                        TT Hit Rate: 16.53%
            
            Middlegame - Search nodes = 343,772
                        Quiescence nodes = 1,472,393
                        Total nodes = 1,816,165
                        Average Time = 1988.83
                        NPS = 913,184
                        TT Probes: 3781492
                        TT Hits: 1106137
                        TT Hit Rate: 29.25%

            Endgamae - Search nodes = 975,182
                        Quiescence nodes = 3,192,348
                        Total nodes = 4,167,530
                        Average Time = 4458.27
                        NPS = 934,787
                        TT Probes: 10727002
                        TT Hits: 2794569
                        TT Hit Rate: 26.05%

            Middlegame1 - Search nodes = 181,239
                        Quiescence nodes = 400,624
                        Total nodes = 581,863
                        Average Time = 460.88
                        NPS = 1,262,516
                        TT Probes: 1993629
                        TT Hits: 236814
                        TT Hit Rate: 11.88%

            Kiwipete - Search nodes = 254,924
                        Quiescence nodes = 2,335,084
                        Total nodes = 2,590,008
                        Average Time = 2873.28
                        NPS = 901,413
                        TT Probes: 2804164
                        TT Hits: 921754
                        TT Hit Rate: 32.87%
        
        */

        /* NO LOOP

            Starting - Search nodes = 137,063
                    Quiescence nodes = 163,227
                    Total nodes = 300,290
                    TT Probes: 137063
                    TT Hits: 12898
                    TT Hit Rate: 9.41%
            
            Endgame - Search nodes = 625,498
                    Quiescence nodes = 1,737,163
                    Total nodes = 2,362,661
                    TT Probes: 625498
                    TT Hits: 67113
                    TT Hit Rate: 10.73%
                    STILL ALMOST 50% REDUCTION IN NODE COUNT

            Middlegame - Search nodes = 341,722
                    Quiescence nodes = 1,439,413
                    Total nodes = 1,781,135
                    TT Probes: 341722
                    TT Hits: 56503
                    TT Hit Rate: 16.53%
                    BARELY ANY REDUCTION (1.81M BEFORE)

            Middlegame1 - Search nodes = 180,861
                    Quiescence nodes = 399,472
                    Total nodes = 580,333
                    TT Probes: 180861
                    TT Hits: 6986
                    TT Hit Rate: 3.86%
                    ALMOST NO REDUCTION (1530 NODES ONLY)

            Kiwipete - Search nodes = 249,632
                    Quiescence nodes = 2,172,299
                    Total nodes = 2,421,931
                    TT Probes: 249632
                    TT Hits: 43520
                    TT Hit Rate: 17.43%
                    161k less nodes


12,898
56,503
67,113
 6,986
43,520
        
        */

        /* Full score pruning 

            Starting -Search nodes = 103,208
                    Quiescence nodes = 132,844
                    Total nodes = 236,052
                    TT Cutoffs: 3828
                    TT Probes: 107036
                    TT Hits: 6731
                    TT Hit Rate: 6.29%

            Middlegame -Search nodes = 207,314
                    Quiescence nodes = 984,625
                    Total nodes = 1,191,939
                    TT Cutoffs: 23796
                    TT Probes: 231110
                    TT Hits: 27993
                    TT Hit Rate: 12.11%

            Endgame -Search nodes = 471,783
                    Quiescence nodes = 1,325,398
                    Total nodes = 1,797,181
                    TT Cutoffs: 30345
                    TT Probes: 502128
                    TT Hits: 39324
                    TT Hit Rate: 7.83%

            Middlegame1 -Search nodes = 113,142
                    Quiescence nodes = 299,266
                    Total nodes = 412,408
                    TT Cutoffs: 4859
                    TT Probes: 118001
                    TT Hits: 5380
                    TT Hit Rate: 4.56%

            Kiwipete -Search nodes = 155,644
                    Quiescence nodes = 1,693,114
                    Total nodes = 1,848,758
                    TT Cutoffs: 14206
                    TT Probes: 169850
                    TT Hits: 17290
                    TT Hit Rate: 10.18%




                    
TT Cutoffs: 3828    - starting
TT Cutoffs: 23796   - middlegame
TT Cutoffs: 4859    - middlegame1
TT Cutoffs: 30345   - endgame
TT Cutoffs: 14206   - kiwipete

        
        */


        /*
            Best lines (TT disabled)
            Starting -      b1c3 b8c6 g1f3 g8f6 d2d4 d7d5
            Middlegame -    d5e3 f2e3 f6g5 d4e5 d6e5 e4f5 b5a4
            Middlegame1 -   c4c5 a3b4 a1b1 b6c5 d2d4 g7h6 d4c5 b4c5 d1d4 c5a7 d4a7 a8a7 b1b2 g6e4
            Endgame -       a6a8 f8a8 c6a8 h8h7 a8b7 h7g6 b7b5
            Kiwipete -      e2a6 b4c3 d2c3 e6d5 e4d5 f6d5
            
            Best lines (TT enabled)
            Starting -      b1c3 b8c6 g1f3 g8f6 d2d4 d7d5
            Middlegame -    d5e3 f2e3 f6g5 d4e5 d6e5 e4f5 b5a4
            Middlegame1 -   c4c5 a3b4 a1b1 b6c5 d2d4 g7h6 d4c5 b4c5 d1d4 c5a7 d4a7 a8a7 b1b2 g6e4
            Endgame -       a6a8 f8a8 c6a8 h8h7 a8b7 h7g6 b7b5
            Kiwipete -      e2a6 b4c3 d2c3 e6d5 e4d5 f6d5
        */

        /*
            Evaluation(TT Disabled)
            Starting = Evaluation: 0
            Middlegame = Evaluation: 245
            Endgame = Evaluation: 622
            Middlegame1 = Evaluation: -446
            Kiwipete = Evaluation: 15

            Evaluation(TT Enabled)
            Starting = Evaluation: 0
            Middlegame = Evaluation: 245
            Endgame = Evaluation: 622
            Middlegame1 = Evaluation: -446
            Kiwipete = Evaluation: 15
        
        */



        // PerftTool.testSuiteNodesProcessed = 0; // Reset counter
        // Stopwatch sw = Stopwatch.StartNew();
        // long nodes = PerftTool.PerftTestSuit(board, moveGenerator, 6); // Depth 8 is too deep for AssertNoStateLeak! 
        // Console.WriteLine($"\nTest complete! Processed {nodes:N0} total nodes in {sw.Elapsed.TotalSeconds:F2} seconds.");

            
        // Console.WriteLine($"Processed {nodes} perfectly synced nodes.");
        // string middleGame = TestPositions.fen1;
        // string middleGame1 = TestPositions.perft2;
        // string endGame = TestPositions.fen13;
        // string kiwipete = TestPositions.perft6;

      

        // Console.WriteLine($"Current Hash = {board.currentHash}");
        // Move newMove = new Move(12,20);


        // ZobristKeys.InitializeArray();

        // int binary1 = 0b0000000000000000000000000000000000000000000000000000000010100000; // white kingside       Flag = 7     
        // int binary2 = 0b0000000000000000000000000000000000000000000000000000000000001001; // white queenside      Flag = 8 

        // ulong binary3 = 0b1010000000000000000000000000000000000000000000000000000000000000; // black kingside       Flag = 9
        // long binary4 = 0b0000100100000000000000000000000000000000000000000000000000000000;// black queenside       Flag = 10

        // Console.WriteLine(binary1);
        // Console.WriteLine(binary2);
        // Console.WriteLine(binary3);
        // Console.WriteLine(binary4);


        // int kingSquare = board.GetKingSquare(board.colorToMove);
        // Console.WriteLine(board.IsSquareAttacked(kingSquare, board.colorToMove));
        // ulong pawnMask = (board.colorToMove == 0)? AttackTables.whitePawnAttacks[kingSquare] : AttackTables.blackPawnAttacks[kingSquare];        

        // BoardUtility.PrintUlongBitboard(board.pieceBitboards[(int)Piece.WhitePawns]);
        // int score = search.StartSearch(board, moveGenerator, evaluation, 1, -infinity, infinity, 0);
        // Console.WriteLine(score);
        
        // int score = search.NegaMax(board, moveGenerator, evaluation, 1, -infinity, infinity, 0);
        // Console.WriteLine(score);
        // Move move1 = new Move(23,38);
        // board.MakeMove(move1);
        // BoardPrinter.PrintBitboard(board);
        
        
        // Move move2 = search.GetBestMove(board, moveGenerator, evaluation, searchDepth); 
        // Console.WriteLine(BoardUtility.MoveToUci(move2));


        // string fenFilePath = @"D:\Chess Engine\Program.cs\PerftPositions.txt";
        // int testCount = 100; // Number of random positions to test
        // int testDepth = 5;  // Depth for the test suite

        // List<string> testFens = GetRandomFens(fenFilePath, testCount);

        // Console.WriteLine($"Running rigorous Hash and State Leak test on {testFens.Count} random positions...");
        // long totalNodes = 0;
        // Stopwatch stopwatch = Stopwatch.StartNew();

        // for (int i = 0; i < testFens.Count; i++)
        // {
        //     string currentFen = testFens[i];
        //     Console.WriteLine($"\n[Test {i + 1}/{testFens.Count}] FEN: {currentFen}");
            
        //     FenUtility.LoadFromFen(currentFen, board);
        //     long nodes = PerftTool.PerftTestSuit(board, moveGenerator, testDepth); 
            
        //     Console.WriteLine($"Processed {nodes} perfectly synced nodes.");
        //     totalNodes += nodes;
        // }

        // stopwatch.Stop();
        // Console.WriteLine($"\nTest complete! Processed {totalNodes:N0} total nodes across {testFens.Count} positions in {stopwatch.Elapsed.TotalSeconds:F2} seconds.");



#region UCI Testing
        // UCIUtility.Loop(board, moveGenerator, evaluation, search);

       
       
    //    string[] crashTest = 
    //    {
    //         "position startpos",
    //         "go depth 6",
    //         "position startpos moves b1c3 d7d5",
    //         "go depth 6",
    //         "position startpos moves b1c3 d7d5 e2e3 e7e5",
    //         "go depth 6",
    //     };

    //     string[] crashTest1 =
    //     {
    //       "position startpos",
    //       "go depth 6",
    //       "position startpos moves b1a3 d7d5",
    //       "go depth 6",
    //       "position startpos moves b1a3 d7d5 e2e3 e7e5",
    //       "go depth 6",  
    //     };

        // string simulatedInput = string.Join(Environment.NewLine, crashTest);// Combine all commands into a single block of text separated by newlines

        
        
        // using (System.IO.StringReader reader = new System.IO.StringReader(simulatedInput)) // Redirect standard input to read from our string instead of the keyboard
        // {
        //     Console.SetIn(reader);
            
        //     /*// The UCI loop will read commands line-by-line. 
        //     // Because "go depth 6" blocks while searching, it naturally waits 
        //     // for a response before reading the next line!*/
        //     UCIUtility.Loop(board, moveGenerator, evaluation, search);
        // }
        
        
        // Console.SetIn(new System.IO.StreamReader(Console.OpenStandardInput()));// Restore standard input for normal console usage
#endregion UCI Testing       
       

//         FenUtility.LoadFromFen(TestPositions.fen0, board);
        
//         for(int i = 0; i<5; i++)
//         {
//             Console.WriteLine("Enter move :");
//             string moveString = Console.ReadLine();
            
//             board.MakeMove(MoveUtility.MoveFromName(moveString));
//             BoardPrinter.PrintBitboard(board);
//             for (int z = 0; z < 64; z++)
//             {
//                 Console.Write(board.pieceOnSquare[z] + "\t");

//                 if ((z + 1) % 8 == 0)
//                 {
//                     Console.WriteLine();
//                 }
//             }
//             Console.WriteLine();

//         }


    //    for(int z = 0; z < 8; z++)
    //     {
    //         Console.WriteLine();
    //         for(int y = 0; y< 8; y++)
    //         {
    //             int i = board.pieceOnSquare[y];
    //             Console.Write(i + ' ');   
    //             Console.WriteLine();
    //         }
    //     }

    // Console.WriteLine(board.pieceOnSquare);
    // FenUtility.LoadFromFen(TestPositions.fen0, board);
    // foreach (int num in board.pieceOnSquare)
    // {
    //     for(int i = 0; i<8; i++)
    //         {
    //             for(int j = 0; j<8; j++)
    //             {
    //                 Console.Write(num + ' '); 
                    
    //             }
    //             Console.WriteLine();
    //         }
    // }

    


       
       
       
       
       
        // perft6 - kiwipete
        // fen0 - starting position
        // fen1 - middlegame (mine possibly)
        // perft2 - middlegame (perft)
        // fen3 - scillian defense
        // fen13 - endgame (mine)

        // string fen = TestPositions.fen0;
        // string fen1 = "1k6/3Q4/7R/8/4P3/8/8/K7 w - - 1 2";
        // string fen2 = "5Q2/4R1pk/4B2p/1p1p4/1P6/2P2P2/1P3P1P/6K1 w - - 1 29";
        
        //"8/8/2K5/7q/1k6/8/8/6r1 b - - 0 1"
        // FenUtility.LoadFromFen(fen, board);
        // Move move = search.GetBestMove(board, moveGenerator, evaluation, 5);
        // search.NegaMax(board, moveGenerator, evaluation, 5, -999999, 999999, 0);
        // search.PrintPrincipalVariation();
        // Console.WriteLine(move);
        // Console.WriteLine(evaluation.EvaluatePosition(board));

        /*// Move move1 = new Move(12,20);
        // board.MakeMove(move1);
        // BoardPrinter.PrintBitboard(board);
        // Console.WriteLine($"Evaluation after move 1 : {evaluation.EvaluatePosition(board)}");

        // Move move2 = new Move(57,42);
        // board.MakeMove(move2);
        // BoardPrinter.PrintBitboard(board);
        // Console.WriteLine($"Evaluation after move 2 : {evaluation.EvaluatePosition(board)}");
        
        // Move move3 = new Move(11,19);
        // board.MakeMove(move3);
        // BoardPrinter.PrintBitboard(board);
        // Console.WriteLine($"Evaluation after move 3 : {evaluation.EvaluatePosition(board)}");*/
        
        /*BoardPrinter.PrintBitboard(board);
        Console.WriteLine(board.colorToMove);

        int infinity = 9999999;
        
        int searchDepth = 6;

        Stopwatch stopwatch = new Stopwatch();*/

        // stopwatch.Start();
        // int bestEval = search.NegaMax(board, moveGenerator, evaluation, 1, -infinity, infinity, 0);
        // stopwatch.Stop();
        // Console.WriteLine($"Time taken  : {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
        // Console.WriteLine($"Total nodes = {search.nodeCount}");
        // Console.WriteLine(search.NegaMax(board, moveGenerator, evaluation, 1, -infinity, infinity, 0));
        // search.PrintPrincipalVariation();
        
        
        // Move move = new Move(12,28,0);
        // board.MakeMove(move);
        // BoardPrinter.PrintBitboard(board);
        // Console.WriteLine(board.colorToMove);

        
        
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
        



        // Console.ReadLine();



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