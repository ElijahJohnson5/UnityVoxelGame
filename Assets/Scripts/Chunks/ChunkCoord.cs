using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public ChunkCoord(Vector3Int pos)
    {
        x = Mathf.FloorToInt(pos.x / (float)Chunk.ChunkWidth);
        z = Mathf.FloorToInt(pos.z / (float)Chunk.ChunkWidth);
    }

    public ChunkCoord(Vector3 pos)
    {
        x = Mathf.FloorToInt(pos.x / Chunk.ChunkWidth);
        z = Mathf.FloorToInt(pos.z / Chunk.ChunkWidth);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        ChunkCoord other = obj as ChunkCoord;
        return x == other.x && z == other.z;
    }

    public override int GetHashCode()
    {
        return 17 * x.GetHashCode() + z.GetHashCode();
    }

    public override string ToString()
    {
        return "ChunkCoord: " + x + ", " + z;
    }
}
