using UnityEngine;

public class VoxelRenderData
{
    public static readonly Vector3[] voxelVertices =
    {
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
    };

    public static readonly int[,] voxelTriangles =
    {
        // Back Face
        { 0, 3, 1, 2 },
        // Front Face
        { 5, 6, 4, 7 },
        // Top Face
        { 3, 7, 2, 6 },
        // Bottom Face
        { 1, 5, 0, 4 },
        // Left Face
        { 4, 7, 0, 3 },
        // Right Face
        { 1, 2, 5, 6 },
    };

    public static readonly Vector2[] blockUvs =
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };
}

public class VoxelFaceData
{
    public static readonly Vector3Int[] faceChecks =
    {
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
    };
}

public class VoxelTextureData
{
    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / TextureAtlasSizeInBlocks; }
    }
}