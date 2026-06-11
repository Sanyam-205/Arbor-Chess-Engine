public static class TestPositions
{
    public const string fen0 = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public const string fen1 = "r3r1k1/1bpp1p1p/p2b1q2/1p1npnN1/B2PP3/1PN1B3/P1PQ1PPP/R3R1K1 b - - 0 1";
    public const string fen2 = "r1bqkbnr/pppp1ppp/2n5/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 3 4"; //scholar's mate
    public const string fen3 = "rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2"; //scillian
    public const string fen4 = "rnbqkbnr/ppppp1pp/8/4Pp2/8/8/PPPP1PPP/RNBQKBNR w KQkq f6 0 1"; //enPassant
    public const string fen5 = "r1bqkb1r/pppppppp/2n2n2/3p4/1q2p1p1/P4N2/1PPPPPPP/RNBQKB1R b KQkq - 0 1"; //custom testing fen
    public const string fen6 = "r3rkq1/p2qbp1q/1q3n2/3p3q/5Bq1/1PN2Q2/P1P1pPPP/3R1RK1 b - - 0 2";
    public const string fen7 = "1R6/5pk1/1P4pp/8/1r6/4p2P/3p2P1/3K4 w - - 0 2";
    public const string fen8 = "11R6/1r3pk1/6pp/8/8/4pQ1P/3p2P1/3K4 w - - 0 2";
    public const string fen9 = "1R6/6k1/6pp/8/5p2/4pnbP/3p2P1/1r2K3 w - - 0 2";
    public const string fen10 = "r3k2r/pp2bppp/1qnppn2/2p5/2bPP3/2N1BN2/PPPQ1PPP/R3K2R w KQkq - 0 1"; // white kingside COULD NOT be done
    public const string fen11 = "r3k2r/pp2bppp/1qnppn2/2p5/2bPP3/2NNB3/PPPQ1PPP/R3K2R w KQkq - 0 1"; // white kingside castling could be done
    public const string fen12 = "1R6/1r4k1/6pp/8/5pP1/4p2P/3p4/3K4 b - g3 0 2";
    public const string fen13 = "5q1k/6p1/R1Q4p/1p1p4/1P6/2P2P2/1P3PKP/3r4 w - - 0 26"; //endgame

    public const string perft2 = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
    public const string perft3 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1 ";
    public const string perft4 = "8/1P6/2K5/8/8/8/2kp4/8 w - - 0 1"; //promotion
    public const string perft5 = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1";
    public const string perft6 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - ";
    
    public const string perft7 = "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1";
    public const string perft7g2g1q = "n1n5/PPPk4/8/8/8/8/4Kp1p/5NqN w - - 0 2";
    public const string perft7b7c8q = "n1Q5/P1Pk4/8/8/8/8/4Kp1p/5NqN b - - 0 2";
    public const string perft7d7c8 = "n1k5/P1P5/8/8/8/8/4Kp1p/5NqN w - - 0 3";
    public const string perft7f1h2 = "n1k5/P1P5/8/8/8/8/4Kp1N/6qN b - - 0 3";
    public const string perft7f2f1q = "n1k5/P1P5/8/8/8/8/4K2N/5qqN w - - 0 4";

    public const string perft7d7c6 = "n1Q5/P1P5/2k5/8/8/8/4Kp1p/5NqN w - - 1 3";
    public const string perft7f1d2 = "n1Q5/P1P5/2k5/8/8/8/3NKp1p/6qN b - - 2 3";
    public const string perft7Af2f1q = "n1Q5/P1P5/2k5/8/8/8/3NK2p/5qqN w - - 0 4";
    public const string perft7f2f1n = "n1Q5/P1P5/2k5/8/8/8/3NK2p/5nqN w - - 0 4";

    public const string perft8 = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8";
    public const string perft9 = "r3k2r/p6p/8/B7/1pp1p3/3b4/P6P/R3K2R w KQkq - 0 1";
    public const string perft10 = "8/k7/3p4/p2P1p2/P2P1P2/8/8/K7 w - - 0 1";
    public const string perft11 = "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1";
}