using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator : NetworkBehaviour
{
    [SerializeField] private ChunkDatabase _chunkDatabase;
    [SerializeField] private float _chunkGap;


    private Queue<GameObject> _activeChunks = new Queue<GameObject>();
    private ChunkData _lastSpawnedChunk = null;
    private float _lastSpawnedY = 0f;
    private int maxChunkCount = 5;


    static public MapGenerator Instance { get; private set; }

    private void Start()
    {
        Instance = this;
        GenerateNextChunk();
    }

    public void GenerateNextChunk()
    {
        if(!IsServer)
        {
            return;
        }

        ChunkData nextChunk = _lastSpawnedChunk == null ? GetFirstChunk() : GetNextRandomChunk();
        _lastSpawnedY += _chunkGap;
        Vector3 spawnPosition = new Vector3(0f, _lastSpawnedY, 0f);
        _lastSpawnedChunk = nextChunk;

        GameObject chunk = Instantiate(nextChunk.chunkPrefab, spawnPosition, Quaternion.identity);
        _activeChunks.Enqueue(chunk);
        NetworkObject netObj = chunk.GetComponent<NetworkObject>();
        netObj.Spawn();

        RemoveOldestChunk();
    }

    private void RemoveOldestChunk()
    {
        if (_activeChunks.Count > maxChunkCount)
        {
            GameObject oldChunk = _activeChunks.Dequeue();
            Destroy(oldChunk);
        }
    }

    public ChunkData GetFirstChunk()
    {
        var chunks = _chunkDatabase.Chunks;
        return chunks[Random.Range(0, chunks.Count)];
    }

    public ChunkData GetNextRandomChunk()
    {
        var chunks = _chunkDatabase.Chunks;
        HashSet<int> currentExits = new HashSet<int>(_lastSpawnedChunk._exitIndexes);

        var candidates = _chunkDatabase.Chunks.Where(nextChunk =>
        {
            HashSet<int> entrances = new HashSet<int>(nextChunk._entranceIndexes);
            return entrances.IsSupersetOf(currentExits);
        }).ToList();

        if (candidates.Count != 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        Debug.LogWarning("Can't Find to Connect Chunk");
        return null;
    }
}
