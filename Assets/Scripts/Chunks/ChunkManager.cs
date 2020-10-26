using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

//TODO Don't store all chunks in memory

public class ChunkManager
{
    private World world;

    private ConcurrentDictionary<ChunkCoord, Chunk> chunks;
    private List<ChunkCoord> activeChunks;
    private ConcurrentBag<ChunkCoord> chunksToCreateMesh;
    private BlockingCollection<Chunk> chunksToCreate;
    private BlockingCollection<ChunkCoord> chunksToUpdate;

    private CancellationTokenSource ctSource;
    private CancellationToken cancellationToken;

    public ChunkManager(World world)
    {
        ctSource = new CancellationTokenSource();
        cancellationToken = ctSource.Token;

        this.world = world;
        chunks = new ConcurrentDictionary<ChunkCoord, Chunk>();
        activeChunks = new List<ChunkCoord>();
        chunksToCreateMesh = new ConcurrentBag<ChunkCoord>();
        chunksToCreate = new BlockingCollection<Chunk>();
        chunksToUpdate = new BlockingCollection<ChunkCoord>();

        //Make sure to get any exceptions from the task
        Task.Run(() => CreateChunkAction(), cancellationToken).ContinueWith(t => {
            Debug.LogErrorFormat("{0}: {1} {2}", t.Exception.InnerException.GetType().Name, t.Exception.InnerException.Message, t.Exception.InnerException.StackTrace);
        }, TaskContinuationOptions.OnlyOnFaulted);

        //Make sure to get any exceptions from the task
        Task.Run(() => UpdateChunkAction(), cancellationToken).ContinueWith(t => {
            Debug.LogErrorFormat("{0}: {1} {2}", t.Exception.InnerException.GetType().Name, t.Exception.InnerException.Message, t.Exception.InnerException.StackTrace);
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
    public void CheckViewDistance(int viewDistance, ChunkCoord coord)
    {
        for (int i = 0; i < activeChunks.Count; i++)
        {
            ChunkCoord c = activeChunks[i];
            if ((c.x < coord.x - viewDistance || c.x > coord.x + viewDistance) || (c.z < coord.z - viewDistance || c.z > coord.z + viewDistance))
            {
                GetChunkFromChunks(c).IsActive = false;
                activeChunks.RemoveAt(i);
                i--;
            }
        }

        for (int x = coord.x - viewDistance; x <= coord.x + viewDistance; x++)
        {
            for (int z = coord.z - viewDistance; z <= coord.z + viewDistance; z++)
            {
                ChunkCoord checkCoord = new ChunkCoord(x, z);

                Chunk current = GetChunkFromChunks(checkCoord);

                if (current == null)
                {
                    QueueChunkForCreation(checkCoord);
                }
                else if (!current.IsActive)
                {
                    current.IsActive = true;
                    activeChunks.Add(checkCoord);
                }
            }
        }
    }

    public void Dispose()
    {
        CancelTasks();
        ctSource.Dispose();
    }

    private void CancelTasks()
    {
        ctSource.Cancel();
    }

    public IEnumerator CreateChunkMeshs()
    {
        int count = 0;
        while (true)
        {
            ChunkCoord currentCoord;
            if (chunksToCreateMesh.TryTake(out currentCoord))
            {
                GetChunkFromChunks(currentCoord).CreateMesh();
                count++;

                if (count == 5)
                {
                    count = 0;
                    yield return null;
                }
            }
            else
            { 
                yield return null;
            }
        }
    }

    private void CreateChunkAction()
    {
        //Only ever touch thread safe collections from this method
        while (!chunksToCreate.IsCompleted)
        {
            //Wait until we can take from collection
            Chunk chunk = chunksToCreate.Take(cancellationToken);
            chunk.Init();
            chunksToCreateMesh.Add(chunk.Coord);
        }

        Debug.Log("Finished chunk create action");
    }

    private void UpdateChunkAction()
    {
        while (!chunksToUpdate.IsCompleted)
        {
            //Wait until we can take from collection
            ChunkCoord coord = chunksToUpdate.Take(cancellationToken);
            Debug.Log("Should Update: " + coord);

            GetChunkFromChunks(coord).Update();
            chunksToCreateMesh.Add(coord);
        }

        Debug.Log("Finished chunk update action");
    }

    public void QueueChunkForCreation(ChunkCoord coord)
    {
        Chunk chunk = new Chunk(coord, world);
        activeChunks.Add(coord);
        chunks[coord] = chunk;
        chunksToCreate.Add(chunk);
    }

    public void QueueChunkForUpdate(ChunkCoord coord)
    {
        chunksToUpdate.Add(coord);
    }

    public Chunk GetChunkFromCoord(ChunkCoord coord)
    {
        return GetChunkFromChunks(coord);
    }

    private Chunk GetChunkFromChunks(ChunkCoord coord)
    {
        if (chunks.ContainsKey(coord))
        {
            return chunks[coord];
        }

        return null;
    }
}
