using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    private World world;

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 16;

    public ChunkCoord Coord { get; private set; }

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private GameObject chunkObject;

    private int vertexIndex = 0;

    private readonly object meshDataLock = new object();

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    private VoxelState[,,] voxelMap = new VoxelState[ChunkWidth, ChunkHeight, ChunkWidth];

    public Vector3Int position;

    private bool isActive;

    public bool IsActive
    {
        get
        {
            return isActive;
        }

        set
        {
            chunkObject.SetActive(value);
            isActive = value;
        }
    }

    public bool IsVoxelMapPopulated { get; private set; }

    public Chunk(ChunkCoord coord, World world)
    {
        Coord = coord;
        this.world = world;

        chunkObject = new GameObject();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(Coord.x * ChunkWidth, 0f, Coord.z * ChunkWidth);
        chunkObject.name = "Chunk " + Coord.x + ", " + Coord.z;
        position = new Vector3Int(Coord.x * ChunkWidth, 0, Coord.z * ChunkWidth);

        IsVoxelMapPopulated = false;
        IsActive = true;
    }

    public void Init()
    {
        lock (meshDataLock)
        {
            GenerateVoxelMap();
            ClearMeshData();
            UpdateChunk();
        }
    }

    public void Update()
    {
        lock (meshDataLock)
        {
            ClearMeshData();
            UpdateChunk();
        }
    }

    public void EditBlock(Vector3Int pos, ushort newID)
    {
        int x = pos.x - Mathf.FloorToInt(position.x);
        int z = pos.z - Mathf.FloorToInt(position.z);

        if (pos.y < 0 || pos.y > ChunkHeight - 1)
        {
            return;
        }

        voxelMap[x, pos.y, z] = new VoxelState(newID);

        // Update chunks around block
        UpdateSurroundingBlocks(pos);

        world.chunkManager.QueueChunkForUpdate(Coord);
    }

    void UpdateSurroundingBlocks(Vector3Int pos)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3Int currentBlock = pos + VoxelFaceData.faceChecks[i];
            ChunkCoord coord = world.GetChunkFromVector3(currentBlock).Coord;

            if (!coord.Equals(Coord))
            {
                world.chunkManager.QueueChunkForUpdate(coord);
            }
        }
    }

    void UpdateChunk()
    {
        for (int x = 0; x < ChunkWidth; x++)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    if (world.voxelTypes[voxelMap[x, y, z].id].isSolid)
                    {
                        CreateVoxelAtPos(new Vector3Int(x, y, z));
                    }
                }
            }
        }
    }

    void GenerateVoxelMap()
    {
        for (int x = 0; x < ChunkWidth; x++)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = new VoxelState(world.GetVoxel(new Vector3Int(x, y, z) + position));
                }
            }
        }

        IsVoxelMapPopulated = true;
    }

    public ushort GetVoxelFromGlobalVector3(Vector3Int pos)
    {
       return voxelMap[pos.x - position.x, pos.y, pos.z - position.z].id;
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > ChunkWidth - 1 || y < 0 || y > ChunkHeight - 1 || z < 0 || z > ChunkWidth - 1)
        {
            return false;
        }

        return true;
    }

    bool ShouldRenderFace(Vector3Int pos)
    {
        if (!IsVoxelInChunk(pos.x, pos.y, pos.z))
        {
            return !world.CheckForVoxel(pos + position);
        }

        return !world.voxelTypes[voxelMap[pos.x, pos.y, pos.z].id].isSolid;
    }

    void CreateVoxelAtPos(Vector3Int pos)
    {
        for (int i = 0; i < 6; i++)
        {
            if (ShouldRenderFace(pos + VoxelFaceData.faceChecks[i]))
            {
                VoxelState voxelState = voxelMap[pos.x, pos.y, pos.z];

                vertices.Add(pos + VoxelRenderData.voxelVertices[VoxelRenderData.voxelTriangles[i, 0]]);
                vertices.Add(pos + VoxelRenderData.voxelVertices[VoxelRenderData.voxelTriangles[i, 1]]);
                vertices.Add(pos + VoxelRenderData.voxelVertices[VoxelRenderData.voxelTriangles[i, 2]]);
                vertices.Add(pos + VoxelRenderData.voxelVertices[VoxelRenderData.voxelTriangles[i, 3]]);

                AddTexture(world.voxelTypes[voxelState.id].GetTextureID(i));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    private void AddTexture(int textureID)
    {
        float y = textureID / VoxelTextureData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelTextureData.TextureAtlasSizeInBlocks);

        x *= VoxelTextureData.NormalizedBlockTextureSize;
        y *= VoxelTextureData.NormalizedBlockTextureSize;

        y = 1 - y - VoxelTextureData.NormalizedBlockTextureSize;

        float offset = 0f;
        uvs.Add(new Vector2(x - offset, y - offset));
        uvs.Add(new Vector2(x - offset, y + VoxelTextureData.NormalizedBlockTextureSize + offset));
        uvs.Add(new Vector2(x + VoxelTextureData.NormalizedBlockTextureSize + offset, y + offset));
        uvs.Add(new Vector2(x + VoxelTextureData.NormalizedBlockTextureSize + offset, y + VoxelTextureData.NormalizedBlockTextureSize - offset));
    }

    public void CreateMesh()
    {
        Mesh mesh;
        lock (meshDataLock)
        {
            mesh = new Mesh()
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray(),
                uv = uvs.ToArray(),
            };
        }

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }
}

