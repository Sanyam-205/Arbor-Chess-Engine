using System;
using System.Diagnostics.Contracts;
// We use a struct because unlike a class, struct is not stored on heap. So no garbage collecter. TLDR, it's faster
public struct Move
{
    

    // Here, we use ushort because to store a move, we only need 16 bits. 6 bits for target square, 6 for start square and 4 for storing flags like castle, promotion, en passant.
    // 6 bits for starting and target square because the max value for target square can be 63 which is exactly 111111 in binary.
    // target square is assigned the first 6 bits (0-5)
    // start square is assigned the next 6 bits (6-11)
    // flags are assigned the next 6 bits (12-15)


    // when a piece has to move from start to target square, we need to shift the 6 start square bits by 6 using << (shift left) operator. 


    // const keyword means constant, which means that a cons value will never be changed during runtime. Makes it faster for compiler to store and lookup a value rather than having to calculate it again and again.
    // 0b_ tells the computer that the value written after this is pure binary, that is of base 2
    // _ is to make the  value human readable. Serves no purpose for the computer
    // targetMask is 63 in decimal
    // startMask is 4032 in decimal

    public readonly ushort Value;
    private const ushort TargetMask = 0b_0000_0000_0011_1111;
    private const ushort StartMask = 0b_0000_1111_1100_0000;


    //promotion values 
    public static readonly int[] flagToBaseIndex = {0,3,1,0,2};
    public enum MoveFlag
    {
        normalMove, promoteToQueen, promoteToKnight, promoteToRook, promoteToBishop, enPassantCapture, doublePawnPush, whiteKingSideCastle, whiteQueenSideCastle, blackKingSideCastle, blackQueenSideCastle
    }
    /*
        Normal move -               0 0 0 0     (0)    
        
        Queen Promotion -           0 0 0 1     (1)
        Knight Promotion -          0 0 1 0     (2)
        Rook Promotion -            0 0 1 1     (3)
        Bishop Promotion -          0 1 0 0     (4)
        
        enPassant Capture -         0 1 0 1     (5)
        doublePawnPush -            0 1 1 0     (6)
        
        whiteKingSideCastle -            0 1 1 1     (7)
        whiteQueenSideCastle -           1 0 0 0     (8)
        blackKingSideCastle -            1 0 0 1     (9)
        blackQueenSideCastle -           1 0 1 0     (10)
    */


    public Move(ushort value) => Value = value; //this is a constructor which i don't know the purpose of. duhh

    // THIS IS ACTUALLY RESPONSIBLE FOR MOVES.
    public Move(int StartSquare, int TargetSquare, int Flag = 0)
    {
        //the method Move gets three integer values, startSquare, targetSquare and flag. Our Value is a 16 bit unsigned integer, with 3 sets of bits representing different things. We keep the targetSquare as is, shift startSquare by 6 bits and flag by 12 bits.
        // For example, if we have to make a move e2e3, it will generate [0000][011000][101000] here, 011000 represent 12 which is index of e2 and 101000 is 20 which is index of e3. (Calculated using this ->(16 8 4 2 1 0))

        Value = (ushort)(TargetSquare | (StartSquare << 6) | (Flag << 12)); //in c#, integer values are stored as 32 bits by default. We cast this value to 16 bit using (ushort) just like we would do when casting (integer) to (float)
        
    }

    //finally we calculate starting square, target square and flag form the 16 bit move Value we calculated using Move function.
    public int TargetSquare => Value & TargetMask;
    public int StartSquare => (Value & StartMask) >> 6;

    public int Flag => Value >> 12;

}