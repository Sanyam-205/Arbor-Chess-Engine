using System;
using System.Diagnostics;
using System.Linq;
public static class UCIUtility
{
    
    public static void Loop(Board board, MoveGenerator moveGenerator, Evaluation evaluation, Search search)
    {

        while (true)
        {
            string input = Console.ReadLine() ?? "";

            
            //GUI might send null input if pipeline breaks somehow
            if (string.IsNullOrEmpty(input)) continue;

            string[] tokens = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = tokens[0].ToLower();

            switch (command)
            {
                case "uci":
                    Console.WriteLine("id name ARBOR 1.0");
                    Console.WriteLine("id author Sanyam");
                    // TODO: Print available options here (e.g., Hash size)
                    Console.WriteLine("uciok");
                    break;

                case "isready":
                    // The GUI uses this to ping the engine and ensure it has finished loading
                    Console.WriteLine("readyok");
                    break;

                case "ucinewgame":
                    // Clear hash tables, reset history heuristics, etc.
                    TT.Clear();
                    search.gameKillerMovesHit = 0;
                    search.gameKillerMovesProbed = 0;
                    search.ClearHistory();
                    break;

                case "position":
                    ParsePosition(input, board, moveGenerator);
#region debug
        // for (int z = 0; z < 64; z++)
        // {
        //     Console.Write(board.pieceOnSquare[z] + "\t");

        //     if ((z + 1) % 8 == 0)
        //     {
        //         Console.WriteLine();
        //     }
        // }
#endregion
                    break;

                case "go":
                    //AbortSearch = false; // Reset the flag before starting a new search
                    ParseGo(input, board, moveGenerator, evaluation, search);
                    break;

                case "perft":
                    if (tokens.Length > 1 && int.TryParse(tokens[1], out int pDepth))
                    {
                        PerftTool.PerftDivide(board, moveGenerator, pDepth);
                    }
                    break;

                case "stop":
                    search.abortSearch = true;
                    break;

                case "quit":
                    return; // Exit the loop and close the application

                default:
                    Console.WriteLine($"Unknown command: {input}");
                    break;
            }

        }


    }


    private static void ParsePosition(string input, Board board, MoveGenerator moveGenerator)
    {
        // 1. THE CRITICAL FIX: Split by all whitespace types and remove empty entries.
        // This perfectly cleans double spaces, \r, \n, and tabs.
        string[] tokens = input.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length < 2) return;

        int movesIndex = Array.IndexOf(tokens, "moves");

        // 2. Setup the initial board state
        if (tokens[1] == "startpos")
        {
            FenUtility.LoadFromFen(TestPositions.fen0, board);
        }
        else if (tokens[1] == "fen")
        {
            // Reconstruct the FEN string from the tokens
            int fenEndIndex = movesIndex == -1 ? tokens.Length : movesIndex;
            string fen = string.Join(" ", tokens.Skip(2).Take(fenEndIndex - 2));
            FenUtility.LoadFromFen(fen, board);
        }

        // 3. Apply any moves played after the initial position
        if (movesIndex != -1)
        {
            for (int i = movesIndex + 1; i < tokens.Length; i++)
            {
                string uciMove = tokens[i];
                ApplyUciMove(uciMove, board, moveGenerator);
            }
        }

        
    }


    private static void ApplyUciMove(string uciMove, Board board, MoveGenerator moveGenerator)
    {
        int startRank, startFile;
        int targetRank, targetFile;
        int targetSquare, startSquare;

        startFile = uciMove[0] - 'a';
        targetFile = uciMove[2] - 'a';

        startRank = uciMove[1] - '1';
        targetRank = uciMove[3] - '1';

        startSquare = startRank*8 + startFile;
        targetSquare = targetRank*8 + targetFile;

        // Console.WriteLine($"Applying UCI Move: {uciMove} | Target StartSq: {startSquare}, Target TargetSq: {targetSquare}");
        // BoardPrinter.PrintBitboard(board);

        //check for promotion
        int promotionFlag = 0;
        if(uciMove.Length == 5)  
        {
            char promotionPiece = uciMove[4];
            promotionFlag = promotionPiece switch
            {
                'q' => 1, // Queen promotion
                'r' => 3, // Rook promotion
                'b' => 4, // Bishop promotion
                'n' => 2, // Knight promotion
                _ => 0    // Default fallback
            };
        }
        
        Move[] moveList = new Move[256];
        int moveCount = 0;
        moveGenerator.GenerateAllPseudoLegalMoves(board, moveList, ref moveCount);

#region debug
        // for (int i = 0; i < moveCount; i++)
        // {
        //     Move move = moveList[i];
        //     if (move.StartSquare == startSquare && move.TargetSquare == targetSquare)
        //     {
        //         bool isPromotion = move.Flag >= (int)Move.MoveFlag.promoteToQueen && move.Flag <= (int)Move.MoveFlag.promoteToBishop;
                
        //         if (isPromotion)
        //         {
        //             if (move.Flag == promotionFlag)
        //             {
        //                 board.MakeMove(move);
        //                 return;
        //             }
        //         }
        //         else if (!isPromotion && promotionFlag == 0)
        //         {
        //             board.MakeMove(move);
        //                     BoardPrinter.PrintBitboard(board);

        //             return;
        //         }
        //     }
        // }
        // bool moveFound = false;
#endregion

        for (int i = 0; i < moveCount; i++)
        {
            Move move = moveList[i];
            
            // 1. Do the squares match?
            if (move.StartSquare == startSquare && move.TargetSquare == targetSquare)
            {
                bool isPromotion = move.Flag >= (int)Move.MoveFlag.promoteToQueen && move.Flag <= (int)Move.MoveFlag.promoteToBishop;

                // 2. Do the flags match?
                if (isPromotion && move.Flag == promotionFlag)
                {
                    board.MakeMove(move);
                    // moveFound = true;
                    return;
                }
                else if (!isPromotion && promotionFlag == 0)
                {
                    board.MakeMove(move);
                    // BoardPrinter.PrintBitboard(board);
                    // moveFound = true;
                    return;
                }
                else
                {
                    // CAUGHT A FLAG ERROR
                    Console.WriteLine($"[DEBUG] Squares matched for {uciMove}, but rejected by flags! move.Flag: {move.Flag}, uciPromoFlag: {promotionFlag}");
                }
            }
        }
#region debug

        // 3. CAUGHT A GENERATOR ERROR
        // if (!moveFound)
        // {
        //     Console.WriteLine("=================================");
        //     Console.WriteLine($"FATAL: ApplyUciMove SILENTLY FAILED to apply '{uciMove}'!");
        //     Console.WriteLine($"Looking for Start: {startSquare}, Target: {targetSquare}");
        //     Console.WriteLine($"Current Turn: {(board.colorToMove == 0 ? "White" : "Black")}");
        //     Console.WriteLine("Here are the moves the generator ACTUALLY saw for this turn:");
        //     for(int i = 0; i < moveCount; i++) 
        //     {
        //         Console.WriteLine($" - {moveList[i].StartSquare} -> {moveList[i].TargetSquare} (Flag: {moveList[i].Flag})");
        //     }
        //     Console.WriteLine("=================================");
        //     Environment.Exit(1);
        // }
        // Console.WriteLine($"FATAL: ApplyUciMove failed to find a legal move for '{uciMove}'!");
        // Console.Out.Flush();
        // Environment.Exit(1);
#endregion



    }

    private static void ParseGo(string input, Board board, MoveGenerator moveGenerator, Evaluation evaluation, Search search)
    {
        string[] tokens = input.Split(' ');
        
        int depth = -1;
        int wtime = 0, btime = 0, winc = 0, binc = 0;
        int movetime = 0;
        int movestogo = 0;

        // Parse common GUI parameters
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i] == "perft" && i + 1 < tokens.Length)
            {
                if (int.TryParse(tokens[i + 1], out int perftDepth))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    long nodes = PerftTool.Perft(board, moveGenerator, perftDepth);
                    sw.Stop();
                    Console.WriteLine($"Node Count = {nodes}, Time = {sw.Elapsed.TotalMilliseconds}, NPS = {nodes/sw.Elapsed.TotalSeconds}");
                }
                return;
            }
            if (tokens[i] == "depth" && i + 1 < tokens.Length)
                int.TryParse(tokens[i + 1], out depth);
            if (tokens[i] == "wtime" && i + 1 < tokens.Length)
                int.TryParse(tokens[i + 1], out wtime);
            if (tokens[i] == "btime" && i + 1 < tokens.Length)
                int.TryParse(tokens[i + 1], out btime);
            if (tokens[i] == "winc" && i + 1 < tokens.Length)
                int.TryParse(tokens[i + 1], out winc);
            if (tokens[i] == "binc" && i + 1 < tokens.Length)
                int.TryParse(tokens[i + 1], out binc);
            if (tokens[i] == "movetime" && i + 1 < tokens.Length)
                int.TryParse(tokens[i + 1], out movetime);
            if (tokens[i] == "movestogo" && i + 1 < tokens.Length)
                int.TryParse(tokens[i + 1], out movestogo);
        }

        int timeLimitMs = -1;

        if (movetime > 0)
        {
            timeLimitMs = movetime;
        }
        else if (wtime > 0 || btime > 0)
        {
            int myTime = board.colorToMove == 0 ? wtime : btime;
            int myInc = board.colorToMove == 0 ? winc : binc;

            int divisor = 30; // Default

            if (movestogo > 0)
            {
                // If the GUI specifies moves to the next time control, allocate time based on that
                // We add a safety margin (+2) so we don't flag on the last move
                divisor = movestogo + 2; 
            }
            else
            {
                // time management based on the game phase (material on board)
                if (board.phaseScore > 20)
                {
                    divisor = 50; // Play faster in the opening (most pieces still on the board)
                }
                else if (board.phaseScore > 8)
                {
                    divisor = 20; // Spend more time in the complex middlegame
                }
                else
                {
                    divisor = 25; // Endgame, return to a baseline
                }
            }

            // Target a fraction of remaining time + half the increment
            timeLimitMs = (myTime / divisor) + (myInc / 2);
            
            // Make sure we don't exceed the actual time left
            if (timeLimitMs >= myTime - 50)
            {
                timeLimitMs = Math.Max(1, myTime - 50);
            }
        }

        if (depth == -1)
        {
            depth = (timeLimitMs != -1) ? 256 : 5;
        }

        // Console.WriteLine("BOARD STATE BEFORE SEARCH:");
        // BoardPrinter.PrintBitboard(board);
        // 1. Call your actual search function
        Move bestMove = search.GetBestMove(board, moveGenerator, evaluation, depth, timeLimitMs); 
        // Console.WriteLine($"info string TT move searched first: {search.ttMoveFirst}");
        // Console.WriteLine($"info string TT move ended up best: {search.ttMoveBest}");

        // if (search.ttMoveFirst > 0)
        // {
        //     Console.WriteLine($"info string TT accuracy: {(100.0 * search.ttMoveBest / search.ttMoveFirst):F2}%");
        // }

#region debug
        // // BoardPrinter.PrintBitboard(board);
        // for (int z = 0; z < 64; z++)
        // {
        //     Console.Write(board.pieceOnSquare[z] + "\t");

        //     if ((z + 1) % 8 == 0)
        //     {
        //         Console.WriteLine();
        //     }
        // }
#endregion
        // 2. The critical step: tell the GUI what move you chose
        Console.WriteLine($"bestmove {BoardUtility.MoveToUci(bestMove)}");
    }





}