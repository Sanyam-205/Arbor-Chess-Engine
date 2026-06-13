using System;
using System.Numerics;

public readonly struct Transposition
{
    // Order the field from most bytes taken to least bybtes taken because of some weird c# compiler rule that injects padding bits.
    public readonly ulong zobristKey; //8bits
    public readonly int score; //4bits
    public readonly Move bestMove; //2bits
    public readonly byte depth; //1bit
    public readonly byte flagType; //1bit

    public Transposition(ulong zobristKey, byte depth, int score, byte flag, Move bestMove) : this()
    {
        this.zobristKey = zobristKey;
        this.score = score;
        this.bestMove = bestMove;
        this.depth = depth;
        this.flagType = flag;
    }
}

public static class TT
{

    //Fixed the Transpotition Table size to 64 MB, since each struct will take 16 bytes, we can store (64*1024*1024)/16 = 4,194,304 distinct structs.
    public static Transposition[] table = new Transposition[4194304];
    public enum Flags
    {
        exactScore, hashBeta, hashAlpha
        //exactScore must be alpha score at the end of the negamax
        //hashBeta must be the fail low cutoff, meaning, the branch leads to a worse outcome. 
        //hashAlpha must the fail high or when alpha>score
    }

    public static void Store(ulong zobristKey, byte depth, int score, byte flag, Move bestMove)
    {
        // zobristKey = zobrist key of the move evaluated at every depth in a branch
        // depth = the depth at which search was done
        // score = the evaluation recieved at 'depth' for the position in 'zobristKey'
        // flag = exactScore ? hashBeta ? hasAlpha ?
        // move = the best move found at each branch.

        //Replacement scheme - If we encounter the same position twice which is to be filled in TT_Entry, then we replace the old entry if it had a depth lesser than the new entry. There are more complex implementations that invlove storing two entries for every positoin but TT is already complex enough for me so lets keep it simple when we can. 
        /*
        if(depth.newEntry > depth.oldEntry)
        {
            depth = depth.newEntry 
        }
        */

        //Collisions - If two or more positions hash to the same key (this shouldn't happen that often because of the pseudo random number generator with xor shifiting and fixed seed) then it is called a collision. The two ways that seemed viable to me were 
        //1. Always replacing the position searched to a lower depth. Should be easier to implement.
        //2. Storing the same entry at the depth of existing depth + 1. I don't know how to do this. We'll see
    
    
        int index = (int)(zobristKey % (ulong)table.Length);
        //Modulo the zobrist key with table.length. It will give us a pure integer value.
        /// For starting position, it gave this hashkey -> 1951233501783077243
        /// Assuming the transposition table is completely empty, there will be 4194304 places left.
        /// index will be -> 1951233501783077243 % 4194304 gives 1,337,943.
        /// So starting position will be stored at 1,337,943 index.
        /// There is also the case of index collisions. Since modulus operator gives us the remainder, there can be multiple instances of different zobrist keys returning to the same index. In that case, we always choose the one which was searched to a greater depth. Implementation -.
        if(table[index].depth <= depth)
        {
            table[index] = new (zobristKey, depth, score, flag, bestMove);
        }
    
    
    
    }
    public static void Clear()
    {
        // Wipes the entire array back to default values (zeros/nulls)
        Array.Clear(table, 0, table.Length);
    }
}