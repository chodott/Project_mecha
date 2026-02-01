using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Rendering.Universal;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private ChunkDatabase _chunkDatabase;
    [SerializeField] private float _chunkGap;
    

    private ChunkData _lastSpawnedChunk = null;
    private float _lastSpawnedY = 0f;


    public Transform playerTransform;
    /*
     1. 플레이어가 일정 높이에 도달할 시
     2. 현재 청크의 출구를 기반으로 입구가 존재하는 다음 청크 결정
     3. 최소 y Gap을 두고 스폰
     */

    private void Start()
    {
        GenerateNextChunk();
    }

    private void Update()
    {
        if(playerTransform.position.y >= _lastSpawnedY)
        {
            GenerateNextChunk();
        }
    }


    public void GenerateNextChunk()
    {
        ChunkData nextChunk = _lastSpawnedChunk == null? GetFirstChunk() : GetNextRandomChunk();
        _lastSpawnedY += _chunkGap;
        Vector3 spawnPosition = new Vector3(0f, _lastSpawnedY, 0f);
        _lastSpawnedChunk = nextChunk;
        Instantiate(nextChunk.chunkPrefab, spawnPosition, Quaternion.identity);
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
