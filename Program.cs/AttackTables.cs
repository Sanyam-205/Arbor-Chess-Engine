using System;
using System.Numerics;
using System.Reflection.Metadata;
public static class AttackTables
{
    /// const ulong fileAMask = 72340172838076673; 
    /// const ulong fileHMask = 9259542123273814144; 
    /// const ulong fileGMask = 4629771061636907072;
    /// const ulong fileBMask = 144680345676153346;
    /// const ulong fileABMask = 217020518514230019;
    /// const ulong fileGHMask = 13889313184910721216;
    /*
        A    A B C D E F G H 
        8     
        7     
        6     
        5     
        4     
        3     
        2      
        1      
    

        00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000
        00000000 00000001 00000001 00000001 00000001 00000001 00000001 01111110
        10000000 10000000 10000000 10000000 10000000 10111111 00000000 00000000
    
    
    
    



    */

    const ulong fileAMask  = 0x0101010101010101UL;
    const ulong fileBMask  = 0x0202020202020202UL;
    const ulong fileGMask  = 0x4040404040404040UL;
    const ulong fileHMask  = 0x8080808080808080UL;
    const ulong fileABMask = 0x0303030303030303UL;
    const ulong fileGHMask = 0xC0C0C0C0C0C0C0C0UL;


    //Relevant bits for bishop and rook
    public static readonly int[] bishopRelevantBits = 
    {
        6, 5, 5, 5, 5, 5, 5, 6, 
        5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 7, 7, 7, 7, 5, 5, 
        5, 5, 7, 9, 9, 7, 5, 5, 
        5, 5, 7, 9, 9, 7, 5, 5, 
        5, 5, 7, 7, 7, 7, 5, 5, 
        5, 5, 5, 5, 5, 5, 5, 5, 
        6, 5, 5, 5, 5, 5, 5, 6

    };

    public static readonly int[] rookRelevantBits =
    {
        12, 11, 11, 11, 11, 11, 11, 12, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        11, 10, 10, 10, 10, 10, 10, 11, 
        12, 11, 11, 11, 11, 11, 11, 12
    };
    
    public static readonly ulong[] rookMagicNumbers = 
    {
        4932145288825086017, 1407942091866145, 9223530503985170497, 4652236007864312065, 9288726442182161, 9223653659001235457, 2305845242865189524, 288812044386566466, 
        2378041342031831296, 1197405240328704, 144703014484115712, 1245262893451053312, 47287822244184320, 2201173098624, 2401514589348864, 91763608383406592, 
        36029072433758208, 14141320456524800000, 4512400619347969, 9295465915581792296, 10376312233293578308, 563018740924424, 1224981334326509584, 9799835282604949508, 
        146508000566182048, 45071249369464898, 5188164369434812416, 360446319199782912, 11529496555421697040, 3188970903270588928, 563534102528260, 89138858557572, 
        13835692477789011968, 577903313710743680, 72655730511972480, 76565767806582944, 2252351717244936, 14015347178059726976, 36031013222617121, 13651538518098176, 306809924191830144, 9223585617256964096, 7642609117674078344, 4702180773330161664, 2596325460124123648, 1549942510455890944, 36033195201925121, 328764972031033604, 
        2882444500080427041, 2306054116522070020, 281547995635714, 144678172659565057, 432627163759413248, 4846436183632380228, 2306405967757574436, 306526251789975808, 5224175842628763685, 54052129061470272, 72066463149596672, 3494810903229497472, 108108383437064192, 2413931601441917952, 324260272690708992, 1297036968181629062
    };
        
    
    public static readonly ulong[] whitePawnAttacks = new ulong [64];
    public static readonly ulong[] blackPawnAttacks = new ulong[64];

    public static readonly ulong[] knightAttacks = new ulong[64];
    public static readonly ulong[] kingAttacks = new ulong[64];

    static AttackTables()
    {
        GeneratePawnAttacks();
        GenerateKnightAttacks();
        GenerateKingAttacks();
    }

    public static void GeneratePawnAttacks()
    {
        for(int square = 0; square < 64; square++)
        {
            ulong pawnBitboard = 1UL << square;

            whitePawnAttacks[square] = 0; //initialize whitePawnAttacks at every index to 0

            //if pawnBitboard is not on file A, the result will be non zero. So it only works on bits that do not correspond to file A. 
                                                    /// a | b   |   a & b
                                                    /// -------------------
                                                    /// 0 | 0   |    0
                                                    /// 0 | 1   |    0
                                                    /// 1 | 0   |    0
                                                    /// 1 | 1   |    1
                                            //This table because ehh... why not
            /// Since pawnBitboard contain 1 bit turned on for each 64 bits, bit 0,8,16,24,32,40,48,56 will be turned on when square is 0,8,....56
            /// So for every 8th bit in pawnBitboard this code block will be skipped.
            ///----------------------------------------------------------------------------------------------------------------------------------//
            ///----------------------------------------------------------------------------------------------------------------------------------//
            ///Since white pawns not on file A can attack pawns to their left, we add the corresponding bit (square) left shifted by 7 bits to the attack table. How we reached the number 7? -> A pawn on lets say square 12 can attack square 19 and 21. 19 is to it's left and 21 to right. 12th square is represented like this in bitboard 0001 0000 0000 0000. 
            /// Left shifting it by 7 gives us 19 which is this 1000 0000 0000 0000 0000
            /// Left shifting it by 9 gives us 21 which is this 0010 0000 0000 0000 0000 0000
            ///----------------------------------------------------------------------------------------------------------------------------------//
            ///Done this shift logic before but got confused at this so decided to explain it
            ///----------------------------------------------------------------------------------------------------------------------------------//
            
            if((pawnBitboard & fileAMask) == 0) 
            {
                whitePawnAttacks[square] |= pawnBitboard << 7; 
            }

            if((pawnBitboard & fileHMask) == 0) //only works on bits not on file h
            {
                whitePawnAttacks[square] |= pawnBitboard << 9;
            }


            //Same logic as that of white pawns but reversed since they capture downwards. Thats why right shift >> (instead of left shift <<) and also invert 7 and 9.

            blackPawnAttacks[square] = 0;
            
            if((pawnBitboard&fileAMask)== 0) 
            {
                blackPawnAttacks[square]|= pawnBitboard >> 9;
            }
            
            if((pawnBitboard & fileHMask) == 0)
            {
                blackPawnAttacks[square]|= pawnBitboard >> 7;
            }

        }    
    }


    public static void GenerateKnightAttacks()
    {
        
        /// offsets - 6, 15, 17, 10 << for forward knight move for white
        /// offsets - 6, 15, 17, 10 >> for backward knight move for white
        /// when a knight is at the middle of the board it's attack table will include all these values with both left and right shift
        /// knight attack table will include all these only for these places
        /*
        A    A B C D E F G H 
        8    0 0 0 0 0 0 0 0
        7    0 0 1 1 1 1 0 0
        6    0 0 1 1 1 1 0 0
        5    0 0 1 1 1 1 0 0 
        4    0 0 1 1 1 1 0 0
        3    0 0 1 1 1 1 0 0
        2    0 0 0 0 0 0 0 0 
        1    0 0 0 0 0 0 0 0 
        */
        /// when a knight is anywhere on A file, it cannot move 15,6 with left shift and 10,17 with right shift
        /// when a knight is anywhere on B file, it cannot move 6 with left shift and 10 with right shift
        /// when a knight is anywhere on G file, it cannot move 10 with left shift and 6 with right shift
        /// when a knight is anywhere on H file, it cannot move 17,10 with left shift and 15,6 with right shift

        /// SINCE BITBOARD IS NOT ACTUALLY A TABLE, WE DON'T NEED MASKS FOR RANKS. Because if knight 'falls off' the board, it would exceed the allocated 64 bit. So that is not possible at all
        

        for (int square = 0; square < 64; square++)
        {
            knightAttacks[square] = 0;

            ulong knightBitboard = 1UL << square;

            if((knightBitboard & fileABMask) == 0) //knight is not on file A or file B
            {
                knightAttacks[square] |= knightBitboard << 6;  // left shift by 6
                knightAttacks[square] |= knightBitboard >> 10; // right shift by 10
            }
            if((knightBitboard & fileGHMask) == 0) //knight is not on file G or file H
            {
               knightAttacks[square] |= knightBitboard >> 6;  //right shift by 6
               knightAttacks[square] |= knightBitboard << 10; //left shift by 10
            }
            
            ///A knight will fall off the board (loop around) in two cases.
            /// case 1. It moves two squares left or right. In this case it doesn't matter if its on A file or B file, G file or H file. IT WILL FALL. So we handle it using fileABMASK and fileGHMASK.
            /// case 2. It moves 1 square left or right. In this case it will only fall if it is on file A or file H so we can ignore specific checks for file B and file G. We handle it using fileAMask, fileHMask, like we did with pawn attack tables.
            
            if((knightBitboard & fileAMask) == 0) //knight is not on file A
            {
                knightAttacks[square] |= knightBitboard << 15;
                knightAttacks[square] |= knightBitboard >> 17;
            }
            if((knightBitboard & fileHMask) == 0) //knight is not on file H
            {
                knightAttacks[square] |= knightBitboard >> 15;
                knightAttacks[square] |= knightBitboard << 17;
            }
        }

        
    } 
    
    
    public static void GenerateKingAttacks()
    {
        for (int square = 0; square<64; square++)
        {
            kingAttacks[square] = 0;

            ulong kingBitboard = 1UL << square;

            ///king will only fall from the board if it is on file A or file H.
            /// Assume king is in middle of the board at square 28. It can move to 29,27 (1); 35,21(7); 36,20 (8); 37,21 (9);

            if((kingBitboard & fileAMask) == 0)
            {
                
                kingAttacks[square] |= kingBitboard <<7;
                kingAttacks[square] |= kingBitboard >>9;
                kingAttacks[square] |= kingBitboard >>1;
            }  
            if((kingBitboard & fileHMask) == 0)
            {
                kingAttacks[square] |= kingBitboard <<1;
                kingAttacks[square] |= kingBitboard >>7;
                kingAttacks[square] |= kingBitboard <<9;
                
            }  
            kingAttacks[square] |= kingBitboard <<8;
            kingAttacks[square] |= kingBitboard >>8;
        }
    }


    // Generates Rook attack mask for every square, excluding the squares on edge of the board.
    public static ulong GenerateRookMask(int square)
    {
        int rank = square / 8;
        int file = square % 8;
                
        ulong rookMask = 0;
        
        //traversing rank in north direction excluding the 7th rank (topmost rank is 7 since it starts from 0)
        for(int r = rank+1; r<7; r++)
        {
            /// If our current rank is 1, our loop starts from 1+1, so rank 2. Rank 2 is square 16-23, rank 3 is north of rank 2. rank 3 is square 24-31. So the square north of each square in the rank is exactly 8 spaces apart. So we convert the 2d coordinate back into 1d coordinate and multiply it by 8.
            /// rank 2, file 1 = square 17. s = rank*8 + file. 
            
            
            int s = (r*8) + file;
            rookMask |= 1UL << s;

        }
        
        //traversing south 
        for(int r = rank - 1; r>0; r--)
        {
            int s = (r*8) + file;
            rookMask |= 1UL << s;

        }

        //traversing east
        for(int f = file+1; f<7; f++)
        {
            int s = (rank * 8) + f;
            rookMask |= 1UL << s;
        }

        //traversing west
        for(int f = file-1; f > 0; f--)
        {
            int s = (rank*8) + f;
            rookMask |= 1UL << s;
        }
    
        return rookMask;

    }


    // Generates Bishop attack mask for every square, excluding the squares on edge of the board.
    public static void GenerateBishopMask(int square)
    {
        int rank = square / 8;
        int file = square % 8;

        ulong bishopMask = 0;

        // north east traversal
        for (int r = rank + 1, f = file +1; r<7 && f < 7; r++, f++)
        {
            int s = (r*8) + f;
            bishopMask |= 1UL << s;

            
        }
        
        // north west traversal
        for (int r = rank + 1, f = file - 1; r<7 && f>0; r++, f--)
        {          
            int s = (r*8) + f;
            bishopMask |= 1UL << s;

        }

        //sout east traversal
        for (int r = rank - 1, f = file + 1; r>0 && f<7; r--, f++)
        {
            int s = (r*8) + f;
            bishopMask |= 1UL << s;

        }

        //south west traversal
        for (int r = rank - 1, f = file - 1; r>0 && f>0; r--, f--)
        {
            int s = (r*8) + f;
            bishopMask |= 1UL << s;

        }

        

    }

    /// If we have a rook on a1, GenerateRookMask will generate 12 moves. 6 on A file, a2 - a7 and 6 and rank 1, b1 - g1. Each of these squares can be occupied. 
    /// If we use a bitboard to represent when which piece is occupied, we will 4096 bitboards all stored in one array. We got 4096, because each of the 12 squares can be either 0 or 1 (empty or occupied.) So total number of combinations is 2^12. 


    /// SetOccupancies function takes in 3 parameters. index, bitInMask and attackMask
    /// Using these 3 parameters, it generates another ulong occupancy. Occupancy ulong generates every possible arrangment of pieces present in the attackMask. 
    /// Function of attackMask is to give all possible squares that can be blocked by a piece in regards to a slider piece on one particular square.
    /// Function of index is to map an index to each possible possible combination.
    /// bitsInMask is used because when we are using a loop to generate attack table, each square would not have the same number of bits in the attackMask, For ex, a rook on a1 has 12 bits in mask whereas if it is on a2, there are only 11 bits in the attack mask. To prevent the loop from running unnecessarily, we use this. 
    public static ulong SetOccupancies(int index, int bitsInMask, ulong attackMask)
    {
        ulong occupancy = 0;


        /// For a rook on a1, bitsInMask is 12, so this for loop will generate all possible combinations of occupancies for those 12 squares.
        for (int i = 0; i<bitsInMask; i++)
        {
            int square = BitOperations.TrailingZeroCount(attackMask);
            attackMask &= attackMask-1;
            if ((index & (1 << i)) != 0)
            {
                occupancy |= 1UL << square;
            }
        }


        return occupancy;
    }



    public static ulong GenerateBishopAttacks(int square, ulong occupancy) //Generate bishop attacks on the fly
    {
        int rank = square / 8;
        int file = square % 8;

        ulong bishopAttacks = 0;
        {
        // north east traversal
        for (int r = rank + 1, f = file +1; r<=7 && f<=7; r++, f++)
        {
            int s = (r*8) + f;
            bishopAttacks |= 1UL << s;
            if ((occupancy & (1UL << s)) != 0) break;
        }
        
        // north west traversal
        for (int r = rank + 1, f = file - 1; r<=7 && f>=0; r++, f--)
        {          
            int s = (r*8) + f;
            bishopAttacks |= 1UL << s;
           if ((occupancy & (1UL << s)) != 0) break;

        }

        //sout east traversal
        for (int r = rank - 1, f = file + 1; r>=0 && f<=7; r--, f++)
        {
            int s = (r*8) + f;
            bishopAttacks |= 1UL << s;
            if ((occupancy & (1UL << s)) != 0) break;

        }

        //south west traversal
        for (int r = rank - 1, f = file - 1; r>=0 && f>=0; r--, f--)
        {
            int s = (r*8) + f;
            bishopAttacks |= 1UL << s;
            if ((occupancy & (1UL << s)) != 0) break;

        }
        
        }



        return bishopAttacks;

    }

    
    //Generates rook attacks on the fly.
    public static ulong GenerateRookAttacks(int square, ulong occupancy)
    {
        int rank = square / 8;
        int file = square % 8;
                
        ulong rookAttacks = 0;
        
        //traversing rank in north direction excluding the 7th rank (topmost rank is 7 since it starts from 0)
        for(int r = rank+1; r<=7; r++)
        {
            int s = (r*8) + file;
            rookAttacks |= 1UL << s;
            if((occupancy & ( 1UL << s)) != 0) break;

        }
        
        //traversing south 
        for(int r = rank - 1; r>=0; r--)
        {
            int s = (r*8) + file;
            rookAttacks |= 1UL << s;
            if((occupancy & ( 1UL << s)) != 0) break;

        }

        //traversing east
        for(int f = file+1; f<=7; f++)
        {
            int s = (rank * 8) + f;
            rookAttacks |= 1UL << s;
            if((occupancy & ( 1UL << s)) != 0) break;
        }

        //traversing west
        for(int f = file-1; f >= 0; f--)
        {
            int s = (rank*8) + f;
            rookAttacks |= 1UL << s;
            if((occupancy & ( 1UL << s)) != 0) break;
        }
    
        return rookAttacks;

    }


    /// CalculateROokTable uses SetOccupancy and GenerateRookAttacks to generate the rook attack table.
     
    
    public static ulong[] RookMagicNumbers = new ulong[64];
    public static ulong[][] RookAttacks = new ulong [64][];


    public static void CalculateRookTable(int square)
    {
        int combinations = 1 << rookRelevantBits[square];
        ulong[] occupancies = new ulong[combinations];
        ulong[] attacks = new ulong[combinations];
        ulong mask = GenerateRookMask(square);
        int shift = 64 - rookRelevantBits[square];
        

        for (int i = 0; i < combinations; i++)
        {
            // generate occupancy for i
            // generate attack for this particular occupancy
            // store them in corresponding arrays

            occupancies[i] = SetOccupancies(i, rookRelevantBits[square], mask);            
            attacks[i] = GenerateRookAttacks(square, occupancies[i]);  
            
        }

        bool fail = true;
        
        ulong finalMagicNumber = 0;
        ulong[] finalTestTable = new ulong[combinations];

        ulong[] testTable = new ulong[combinations];

        while (fail)
        {
            fail = false;
            ulong randomMagicNumber = GenerateRandomMagicNumber();
            Array.Clear(testTable, 0, testTable.Length);
            
            for(int i = 0; i < combinations; i++)
            {
                int index = (int)((occupancies[i] * randomMagicNumber) >> shift);

           
                if (testTable[index] == 0)
                {
                    testTable[index] = attacks[i];   
                }
                else if (testTable[index] == attacks[i])
                {
                    continue;
                }
                else
                {
                    fail = true;
                    break;
                }

            }

            if(!fail)
            {
                finalMagicNumber = randomMagicNumber;
                finalTestTable = testTable;
            }

        } 

        RookMagicNumbers[square] = finalMagicNumber; 
        RookAttacks[square] = finalTestTable;

         
        
    }    

    static ulong randomState = 1804289383UL; // Standard non-zero seed
    public static ulong GetRandomUlong()
    {
        unchecked 
        {
            ulong x = randomState;
            x ^= x >> 12;
            x ^= x << 25;
            x ^= x >> 27;
            randomState = x;
            return x * 2685821657736338717UL;
        }
    }

    public static ulong GenerateRandomMagicNumber()
    {
        return GetRandomUlong() & GetRandomUlong() & GetRandomUlong();
    }

    public static void PrintMagicNumber(int square)
    {
        CalculateRookTable(square);
        Console.Write($"{RookMagicNumbers[square]}, ");
    }


    //Magic number = 36031546068615176
    // 4647733782040838144





}




/*knight attack
        [
        132096
        329728
        659712
        1319424
        2638848
        5277696
        10489856
        4202496
        33816580
        84410376
        168886289
        337772578
        675545156
        1351090312
        2685403152
        1075839008
        8657044482
        21609056261
        43234889994
        86469779988
        172939559976
        345879119952
        687463207072
        275414786112
        2216203387392
        5531918402816
        11068131838464
        22136263676928
        44272527353856
        88545054707712
        175990581010432
        70506185244672
        567348067172352
        1416171111120896
        2833441750646784
        5666883501293568
        11333767002587136
        22667534005174272
        45053588738670592
        18049583422636032
        145241105196122112
        362539804446949376
        725361088165576704
        1450722176331153408
        2901444352662306816
        5802888705324613632
        11533718717099671552
        4620693356194824192
        288234782788157440
        576469569871282176
        1224997833292120064
        2449995666584240128
        4899991333168480256
        9799982666336960512
        1152939783987658752
        2305878468463689728
        1128098930098176
        2257297371824128
        4796069720358912
        9592139440717824
        19184278881435648
        38368557762871296
        4679521487814656
        9077567998918656]
    
    */


/*king attack
        [
        770
        1797
        3594
        7188
        14376
        28752
        57504
        49216
        197123
        460039
        920078
        1840156
        3680312
        7360624
        14721248
        12599488
        50463488
        117769984
        235539968
        471079936
        942159872
        1884319744
        3768639488
        3225468928
        12918652928
        30149115904
        60298231808
        120596463616
        241192927232
        482385854464
        964771708928
        825720045568
        3307175149568
        7718173671424
        15436347342848
        30872694685696
        61745389371392
        123490778742784
        246981557485568
        211384331665408
        846636838289408
        1975852459884544
        3951704919769088
        7903409839538176
        15806819679076352
        31613639358152704
        63227278716305408
        54114388906344448
        216739030602088448
        505818229730443264
        1011636459460886528
        2023272918921773056
        4046545837843546112
        8093091675687092224
        16186183351374184448
        13853283560024178688
        144959613005987840
        362258295026614272
        724516590053228544
        1449033180106457088
        2898066360212914176
        5796132720425828352
        11592265440851656704
        4665729213955833856
        ]
    */