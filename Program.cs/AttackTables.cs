using System;
using System.Numerics;
using System.Reflection.Metadata;
using System.Linq;

public static class AttackTables
{
    
    /*
        A    ABCDEFGH 
        8    00000101 
        7     
        6     
        5     
        4     
        3     
        2      
        1    10010000   


        00000000 00000000 00000000 00000000 00000000 00000000 00000000 10100000 // white kingside
        00000000 00000000 00000000 00000000 00000000 00000000 00000000 00001001 // white queenside 
        
        10100000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 // black kingside
        00001001 00000000 00000000 00000000 00000000 00000000 00000000 00000000 // black queenside
    
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
    
   /* DELETE THIS LATER
    
    // public static readonly ulong[] rookMagicNumbers =
    // {
    //     4647717635104964704, 90107177993244673, 36046391365607424, 72092830084890880, 612493947503149074, 72061993174958600, 216177184455295488, 144117389385294849, 4900057133820690432, 4684025362496159776, 18718223398604804, 148759559585923200, 324963410502489088, 2450521353535883776, 1407426456716544, 1189091074564423808, 121738477321584674, 193660006661365760, 1199567504678913, 4611827305805811714, 72340169734246484, 9711437089883136, 1731779192661610561, 4622947216586408049, 649795023222882313, 4904507956210589696, 11646048637354048, 72621686453837888, 1416175271809280, 22522398339236352, 4611687120094888448, 83846352142468, 144260598530572339, 705887035474688, 1166441445335244800, 36672011874865152, 5215310308582622208, 90074193726538752, 216313523905505792, 1153062277537071360, 144467033988628480, 4609154085863425, 4543459105046544, 4900197938276925537, 563087677980680, 563568495951880, 848831566643456, 1152957798163808268, 70370892259968, 9007338845372544, 2305983884158828672, 144871654236357760, 612794114312241408, 144396714592699136, 8864900981760, 1130302520984064, 585608690170598433, 36288451313793, 577129268269027649, 4071290347295114249, 144396680275429377, 281500880994305, 571821477218564, 144119597163939906
    // };

    */

    public static readonly ulong[] rookMagicNumbers =
    {
        36029071939342368, 54045463275446272, 2377910499137298432, 6989591161460035584, 612502745609541633, 144132814640644100, 180146184671725696, 36030611642794240, 563091695747592, 6920414214892855297, 74450681632657408, 18718154695903232, 75154128036168720, 72198426023986176, 4035788233285044736, 19281177638619264, 36029072433790976, 9007474673717248, 19283235469005312, 23163411730800896, 9572348367769748, 72198882657829888, 12512443182879240, 144170164202526980, 288300755633864740, 4503875579584512, 72092780560646272, 595055695104573824, 571748194189440, 1130300101492864, 576469634295934981, 42786300994257029, 329748210102829096, 211243700850690, 1306079082777153537, 76569991930974208, 2310425774299025664, 140892115583488, 5506886273072, 3459063873074626691, 2450028636902948872, 4503737338970120, 282711931813906, 4701775603295092864, 281509337432080, 2306405993803808772, 72137102507376641, 2450402401606303761, 940201399466066176, 1729452627825002112, 306315233601782016, 79165105668224, 108227214445117568, 72198623586287744, 577026180722983936, 53332035240448, 563032182784258, 1225119977921576977, 2310909630744264738, 1729663784100823073, 4756364483416821762, 55450606181024385, 1729945241265635586, 1105634279817224266
    };

    public static readonly ulong[] bishopMagicNumbers =
    {
        18313482860503104, 436850684282144800, 2306970026097246528, 10203753554919426, 1441451223877419264, 2319503479259795456, 6917643463943004472, 72587163992576, 10520417664168626432, 2305848511201411200, 1161933106406687242, 145152723128450, 2207881637889, 360296922245236736, 289374985020260872, 4758056994328807690, 9007474703876432, 4764808422959087872, 9296577555461308432, 6755678682152961, 8144795162045317184, 281511628640260, 306807776457868448, 6098014637310642692, 2314859005098460162, 36600543608048288, 4612328413197500992, 9224498211706699778, 9259554765862346752, 225497740262524160, 11601417782117401216, 4576167411615248, 4651127993746394128, 144274136225811456, 1179260589965632, 72204930745106818, 1189513535047336456, 594827273706799872, 1175440061123068160, 38843898973454465, 9224511140564830208, 563508838664338, 329923943245283840, 9232381718609029121, 4611723539429458178, 297536711565514816, 9607567399995652, 148620166658523392, 28859999191334948, 87258346646667520, 37419969937408, 9223372041289728032, 70849814201856, 9436077609844736, 2310364342895805570, 6918305463709270032, 3458910221678682128, 4629749896068484101, 4611686061716932640, 281509345100800, 4612266571573363201, 144185831966548513, 2992973998751880, 9241390867774720644
    };

        public static readonly ulong[] rookMasks =
    {
        282578800148862, 565157600297596, 1130315200595066, 2260630401190006, 4521260802379886, 9042521604759646, 18085043209519166, 36170086419038334, 282578800180736, 565157600328704, 1130315200625152, 2260630401218048, 4521260802403840, 9042521604775424, 18085043209518592, 36170086419037696, 282578808340736, 565157608292864, 1130315208328192, 2260630408398848, 4521260808540160, 9042521608822784, 18085043209388032, 36170086418907136, 282580897300736, 565159647117824, 1130317180306432, 2260632246683648, 4521262379438080, 9042522644946944, 18085043175964672, 36170086385483776, 283115671060736, 565681586307584, 1130822006735872, 2261102847592448, 4521664529305600, 9042787892731904, 18085034619584512, 36170077829103616, 420017753620736, 699298018886144, 1260057572672512, 2381576680245248, 4624614895390720, 9110691325681664, 18082844186263552, 36167887395782656, 35466950888980736, 34905104758997504, 34344362452452352, 33222877839362048, 30979908613181440, 26493970160820224, 17522093256097792, 35607136465616896, 9079539427579068672, 8935706818303361536, 8792156787827803136, 8505056726876686336, 7930856604974452736, 6782456361169985536, 4485655873561051136, 9115426935197958144
    };

    public static readonly ulong[] bishopMask =
    {
        18049651735527936, 70506452091904, 275415828992, 1075975168, 38021120, 8657588224, 2216338399232, 567382630219776, 9024825867763712, 18049651735527424, 70506452221952, 275449643008, 9733406720, 2216342585344, 567382630203392, 1134765260406784, 4512412933816832, 9024825867633664, 18049651768822272, 70515108615168, 2491752130560, 567383701868544, 1134765256220672, 2269530512441344, 2256206450263040, 4512412900526080, 9024834391117824, 18051867805491712, 637888545440768, 1135039602493440, 2269529440784384, 4539058881568768, 1128098963916800, 2256197927833600, 4514594912477184, 9592139778506752, 19184279556981248, 2339762086609920, 4538784537380864, 9077569074761728, 562958610993152, 1125917221986304, 2814792987328512, 5629586008178688, 11259172008099840, 22518341868716544, 9007336962655232, 18014673925310464, 2216338399232, 4432676798464, 11064376819712, 22137335185408, 44272556441600, 87995357200384, 35253226045952, 70506452091904, 567382630219776, 1134765260406784, 2832480465846272, 5667157807464448, 11333774449049600, 22526811443298304, 9024825867763712, 18049651735527936
    };
        
    
    public static readonly ulong[] whitePawnAttacks = new ulong [64];
    public static readonly ulong[] blackPawnAttacks = new ulong[64];

    public static readonly ulong[][] pawnAttacks = {whitePawnAttacks, blackPawnAttacks};

    public static readonly ulong[] knightAttacks = new ulong[64];
    public static readonly ulong[] kingAttacks = new ulong[64];
    public static ulong[][] bishopAttacks = new ulong[64][];
    public static ulong[][] rookAttacks = new ulong [64][];


    static AttackTables()
    {
        GeneratePawnAttackTable();
        GenerateKnightAttackTable();
        GenerateKingAttackTable();
        GenerateBishopAttackTable();
        GenerateRookAttackTable();
     
    }

#region Pawn
    public static void GeneratePawnAttackTable()
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
#endregion



#region Knight
    public static void GenerateKnightAttackTable()
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
#endregion    


    
#region King    
    public static void GenerateKingAttackTable()
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
#endregion


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

#region Bishop

     // Generates Bishop attack mask for every square, excluding the squares on edge of the board.
   
    public static ulong GenerateBishopMask(int square)
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

        return bishopMask;        

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



    public static void InitializeBishopMagicTable(int square)
    {
        int combinations = 1 << bishopRelevantBits[square];

        bishopAttacks[square] = new ulong[combinations];
        ulong mask = GenerateBishopMask(square);
        int shift = 64 - bishopRelevantBits[square];

        ulong magic = bishopMagicNumbers[square];


        for (int i = 0; i < combinations; i++)
        {
            ulong occupancy = SetOccupancies(i, bishopRelevantBits[square], mask);
            ulong attack = GenerateBishopAttacks(square, occupancy);
            int index = (int)((occupancy * magic) >> shift);
            bishopAttacks[square][index] = attack;
        }
    }

    public static void GenerateBishopAttackTable()
    {
     
        for(int i = 0; i<64; i++)
        {
            InitializeBishopMagicTable(i);
        }
    }

    

    

#endregion


#region rook

   public static ulong GenerateRookMask(int square)  // Generates Rook attack mask for every square, excluding the squares on edge of the board.

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


    /// If we have a rook on a1, GenerateRookMask will generate 12 moves. 6 on A file, a2 - a7 and 6 and rank 1, b1 - g1. Each of these squares can be occupied. 
    /// If we use a bitboard to represent when which piece is occupied, we will 4096 bitboards all stored in one array. We got 4096, because each of the 12 squares can be either 0 or 1 (empty or occupied.) So total number of combinations is 2^12. 


    /// SetOccupancies function takes in 3 parameters. index, bitInMask and attackMask
    /// Using these 3 parameters, it generates another ulong occupancy. Occupancy ulong generates every possible arrangment of pieces present in the attackMask. 
    /// Function of attackMask is to give all possible squares that can be blocked by a piece in regards to a slider piece on one particular square.
    /// Function of index is to map an index to each possible possible combination.
    /// bitsInMask is used because when we are using a loop to generate attack table, each square would not have the same number of bits in the attackMask, For ex, a rook on a1 has 12 bits in mask whereas if it is on a2, there are only 11 bits in the attack mask. To prevent the loop from running unnecessarily, we use this. 
    

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


   
    public static void InitializeRookMagicTable(int square)
    {
        int combinations = 1 << rookRelevantBits[square];
        rookAttacks[square] = new ulong[combinations];
        ulong mask = GenerateRookMask(square);
        int shift = 64 - rookRelevantBits[square];
        ulong magic = rookMagicNumbers[square];

        for(int i = 0; i < combinations; i++)
        {
            ulong occupancy = SetOccupancies(i, rookRelevantBits[square], mask);
            ulong attack = GenerateRookAttacks(square, occupancy);
            int index = (int)((occupancy * magic) >> shift);
            rookAttacks[square][index] = attack;
        }
    }

    public static void GenerateRookAttackTable()
    {
        for(int i = 0; i<64; i++)
        {
            InitializeRookMagicTable(i);
        }
    }



#endregion


#region Queen
    // A Queen is simply a combination of a Rook and a Bishop.
    // We don't need a separate lookup table for Queens
    public static ulong GetQueenAttacks(int square, ulong occupancy)
    {
        return GetRookAttacks(square, occupancy) | GetBishopAttacks(square, occupancy);
    }
#endregion

#region Magic Bitboard Lookups
    public static ulong GetBishopAttacks(int square, ulong occupancy)
    {
        ulong maskedOccupancy = occupancy & bishopMask[square];
        int magicIndex = (int)((maskedOccupancy * bishopMagicNumbers[square]) >> (64 - bishopRelevantBits[square]));
        return bishopAttacks[square][magicIndex];
    }

    public static ulong GetRookAttacks(int square, ulong occupancy)
    {
        ulong maskedOccupancy = occupancy & rookMasks[square];
        int magicIndex = (int)((maskedOccupancy * rookMagicNumbers[square]) >> (64 - rookRelevantBits[square]));
        return rookAttacks[square][magicIndex];
    }
#endregion

//non essential ->
#region magic numbers
      
    /// CalculateRookTable uses SetOccupancy and GenerateRookAttacks to generate the rook attack table.
    public static ulong[] RookMagicNumbersArr = new ulong[64];
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

        RookMagicNumbersArr[square] = finalMagicNumber; 
        rookAttacks[square] = finalTestTable;

         
        
    }    


    
    public static ulong[] BishopMagicNumbersArr = new ulong[64];
    public static void CalculateBishopTable(int square)
    {
        int combinations = 1 << bishopRelevantBits[square];
        ulong[] occupancies = new ulong[combinations];
        ulong[] attacks = new ulong[combinations];
        ulong mask = GenerateBishopMask(square);
        int shift = 64 - bishopRelevantBits[square];

        for (int i = 0; i < combinations; i++)
        {
            occupancies[i] = SetOccupancies(i, bishopRelevantBits[square], mask);
            attacks[i] = GenerateBishopAttacks(square, occupancies[i]);
        }

        bool fail = true;

        ulong finalMagicNumber = 0;
        ulong[] finalTestTable = new ulong[combinations];

        ulong[] testTable = new ulong[combinations];

        while(fail)
        {
            fail = false;
            ulong randomMagicNumber = GenerateRandomMagicNumber();
            Array.Clear(testTable, 0, testTable.Length);

            for(int i = 0; i<combinations; i++)
            {
                int index = (int)((occupancies[i] * randomMagicNumber) >> shift);

                if(testTable[index] == 0)
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

        BishopMagicNumbersArr[square] = finalMagicNumber;
        bishopAttacks[square] = finalTestTable;
    }

    #region random number generation
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
        Console.Write($"{RookMagicNumbersArr[square]}, ");
    }
    #endregion
    
    public static bool VerifyRookMagic(int square, ulong magic)
    {
        
        int bits = rookRelevantBits[square];
        int combinations = 1 << bits;
        ulong mask = GenerateRookMask(square);
        ulong[] testTable = new ulong[combinations];

        for (int i = 0; i < combinations; i++)
        {
            ulong occupancy = SetOccupancies(i, bits, mask);
            ulong actualAttack = GenerateRookAttacks(square, occupancy);
            int index = (int)((occupancy * magic) >> (64 - bits));

            if (testTable[index] == 0) {
                testTable[index] = actualAttack;
            }
            else if (testTable[index] != actualAttack) {
                // Found a collision! This magic number is invalid.
                return false; 
            }
        }
        return true; // Passed for all combinations!
    }
    public static bool VerifyBishopMagic(int square, ulong magic)
    {
        
        int bits = bishopRelevantBits[square];
        int combinations = 1 << bits;
        ulong mask = GenerateBishopMask(square);
        ulong[] testTable = new ulong[combinations];

        for (int i = 0; i < combinations; i++)
        {
            ulong occupancy = SetOccupancies(i, bits, mask);
            ulong actualAttack = GenerateBishopAttacks(square, occupancy);
            int index = (int)((occupancy * magic) >> (64 - bits));

            if (testTable[index] == 0) {
                testTable[index] = actualAttack;
            }
            else if (testTable[index] != actualAttack) {
                // Found a collision! This magic number is invalid.
                return false; 
            }
        }
        return true; // Passed for all combinations!
    }

    public static void PrintResult()
    {
        
        for (int i = 0; i < 64; i++)
        {
            if(VerifyBishopMagic(i,bishopMagicNumbers[i] ) == true) 
            {
                Console.WriteLine($"Correct Bishop Magic Numbers for square {i}\n");

            }
            else 
            {
                Console.WriteLine($"Failed at square {i}!\n");
                //Console.WriteLine($"Magic used: {rookMagicNumbers[i]}");
                //Console.WriteLine($"Relevant bits: {rookRelevantBits[i]}");
            }
        
        }
        
       
    }

#endregion


    // public static void PrintMask()
    // {
    //     for (int i = 0; i < 64; i++)
    //     {
    //         GenerateBishopMask(i);
    //         Console.Write($"{GenerateBishopMask(i)}, " );
    //     }
    // }


}

    