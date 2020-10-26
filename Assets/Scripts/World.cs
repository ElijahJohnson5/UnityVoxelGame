using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    //TODO make at looking private maybe
    public ChunkManager chunkManager;

    //TODO Probably don't need
    //public int initalWorldSize = 11;

    public Material material;
    public Transform player;

    public int playerViewDistanceInChunks = 5;

    public VoxelType[] voxelTypes;

    public ChunkCoord playerChunkCoord;
    private ChunkCoord playerLastChunkCoord;

    public GameObject debugScreen;

    void Start()
    {
        chunkManager = new ChunkManager(this);
        GenerateInitialWorld();
        playerLastChunkCoord = new ChunkCoord(player.position);
        playerChunkCoord = playerLastChunkCoord;

        StartCoroutine(chunkManager.CreateChunkMeshs());
    }

    void Update()
    {
        playerChunkCoord = new ChunkCoord(player.position);
        if (!playerLastChunkCoord.Equals(playerChunkCoord))
        {
            playerLastChunkCoord = playerChunkCoord;
            chunkManager.CheckViewDistance(playerViewDistanceInChunks, playerChunkCoord);
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugScreen.SetActive(!debugScreen.activeSelf);
        }
    }
    
    private void OnDestroy()
    {
        //Have to dispose of the cancellation token in the chunk manager
        chunkManager.Dispose();
    }

    void GenerateInitialWorld()
    {
        // 0, 0
        // Top  [0, 1], [1, 1], 
        // Right [1, 0], [1, -1], 
        // Bottom [0, -1], [-1, -1], 
        // Left [-1, 0], [-1, 1],

        // Top [-1, 2], [0, 2], [1, 2], [2, 2]
        // Right [2, 1], [2, 0], [2, -1], [2, -2]
        // Bottom [1, -2], [0, -2], [-1, -2], [-2, -2]
        // Left [-2, -1], [-2, 0], [-2, 1], [-2, 2]

        ChunkCoord coord = new ChunkCoord(0, 0);
        chunkManager.QueueChunkForCreation(coord);

        for (int step = 1; step <= playerViewDistanceInChunks; step++)
        {

            //Chunks above spawn
            //Debug.Log("Top ");
            for (int x = -(step - 1); x <= step; x++)
            {
                //Debug.LogFormat("[{0}, {1}]", x, step);
                coord = new ChunkCoord(x, step);
                chunkManager.QueueChunkForCreation(coord);
            }

            //Chunks to Right of spawn
            //Debug.Log("Right ");
            for (int z = step - 1; z >= -step; z--)
            {
                //Debug.LogFormat("[{0}, {1}]", step, z);
                coord = new ChunkCoord(step, z);
                chunkManager.QueueChunkForCreation(coord);
            }

            //Chunks on bottom of spawn
            //Debug.Log("Bottom ");
            for (int x = step - 1; x >= -step; x--)
            {
                //Debug.LogFormat("[{0}, {1}]", x, -step);
                coord = new ChunkCoord(x, -step);
                chunkManager.QueueChunkForCreation(coord);
            }

            // Chunks left of spawn
            //Debug.Log("Left ");
            for (int z = -(step - 1); z <= step; z++)
            {
                //Debug.LogFormat("[{0}, {1}]", -step, z);
                coord = new ChunkCoord(-step, z);
                chunkManager.QueueChunkForCreation(coord);
            }
        }

        // Normal generation not around spawn
        /*
        int halfWorldSize = (initalWorldSize / 2);
        for (int x = -halfWorldSize; x <= halfWorldSize; x++)
        {
            for (int z = -halfWorldSize; z <= halfWorldSize; z++)
            {
                ChunkCoord coord = new ChunkCoord(x, z);
                chunkManager.QueueChunkForCreation(coord);
            }
        }
        */
    }

    public void HighlightVoxel(Vector3Int pos)
    {
        //TODO
    }

    public bool CheckForVoxel(Vector3Int pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);
        Chunk currentChunk = chunkManager.GetChunkFromCoord(thisChunk);

        if (pos.y < 0 || pos.y > Chunk.ChunkHeight - 1)
        {
            return false;
        }

        if (currentChunk != null && currentChunk.IsVoxelMapPopulated)
        {
            return voxelTypes[currentChunk.GetVoxelFromGlobalVector3(pos)].isSolid;
        }

        return voxelTypes[GetVoxel(pos)].isSolid;
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        return CheckForVoxel(new Vector3Int(x, y, z));
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        ChunkCoord coord = new ChunkCoord(pos);

        return chunkManager.GetChunkFromCoord(coord);
    }

    public Chunk GetChunkFromVector3(Vector3Int pos)
    {
        ChunkCoord coord = new ChunkCoord(pos);

        return chunkManager.GetChunkFromCoord(coord);
    }

    public ushort GetVoxel(Vector3Int pos)
    {
        if (pos.y > Chunk.ChunkHeight - 1 || pos.y < 0)
        {
            return 0;
        }

        if (pos.y == 0)
        {
            return 1;
        }

        if (pos.y == Chunk.ChunkHeight - 1)
        {
            return 4;
        }

        return 2;
    }
}
