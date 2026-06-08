using System;
using System.Linq;
public static class UCIUtility
{
    
    public static void Loop(Board board, MoveGenerator moveGenerator, Evaluation evaluation, Search search)
    {
        // Console.WriteLine("Custom Engine UCI initialized.")

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
                    break;

                case "position":
                    ParsePosition(input, board, moveGenerator);
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
                    //AbortSearch = true; //TODO: Complete the stop flag integration
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
        // Example inputs:
        // "position startpos"
        // "position startpos moves e2e4 e7e5"
        // "position fen rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 moves e2e4"

        string[] tokens = input.Split(' ');
        int movesIndex = Array.IndexOf(tokens, "moves");

        // 1. Setup the initial board state
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

        // 2. Apply any moves played after the initial position
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

        for (int i = 0; i < moveCount; i++)
        {
            Move move = moveList[i];
            if (move.StartSquare == startSquare && move.TargetSquare == targetSquare)
            {
                bool isPromotion = move.Flag >= (int)Move.MoveFlag.promoteToQueen && move.Flag <= (int)Move.MoveFlag.promoteToBishop;
                
                if (isPromotion)
                {
                    if (move.Flag == promotionFlag)
                    {
                        board.MakeMove(move);
                        return;
                    }
                }
                else if (!isPromotion && promotionFlag == 0)
                {
                    board.MakeMove(move);
                    return;
                }
            }
        }
    }

    private static void ParseGo(string input, Board board, MoveGenerator moveGenerator, Evaluation evaluation, Search search)
    {
        string[] tokens = input.Split(' ');
        
        int depth = 5; // Default depth
        int wtime = 0, btime = 0, winc = 0, binc = 0;
        int movetime = 0;

        // Parse common GUI parameters
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i] == "perft" && i + 1 < tokens.Length)
            {
                if (int.TryParse(tokens[i + 1], out int perftDepth))
                {
                    PerftTool.PerftDivide(board, moveGenerator, perftDepth);
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
        }

        // TODO: Pass time parameters to your search if you implement time management.
        // For a bare-bones engine, you can just rely on fixed depth to start.

        // 1. Call your actual search function
        Move bestMove = search.GetBestMove(board, moveGenerator, evaluation, depth); 

        // 2. The critical step: tell the GUI what move you chose
        Console.WriteLine($"bestmove {BoardUtility.MoveToUci(bestMove)}");
    }





}