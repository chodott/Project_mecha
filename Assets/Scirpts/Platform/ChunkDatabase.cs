using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    public GameObject chunkPrefab;
    public List<int> _entranceIndexes = new List<int>();
    public List<int> _exitIndexes = new List<int>();
}

[CreateAssetMenu(fileName = "ChunkDatabase", menuName = "Scriptable Objects/ChunkData")]
public class ChunkDatabase : ScriptableObject
{
    public List<ChunkData> chunks = new List<ChunkData>();
}
