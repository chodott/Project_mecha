using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;

public class ChunkBakingTool : EditorWindow
{
    [MenuItem("Tools/Chunk Baking Tool")]
    public static void ShowWindow() => GetWindow<ChunkBakingTool>("Chunk Baking Tool");

    public ChunkDatabase database;
    public Transform parent;
    public float rowGap = 2.0f;
    public float firstRowX = -8.0f;
    public float maxRowCount = 9;


    private void OnGUI()
    {
        database = (ChunkDatabase)EditorGUILayout.ObjectField("Chunk Database", database, typeof(ChunkDatabase), false);
        parent = (Transform)EditorGUILayout.ObjectField("Parent Transform", parent, typeof(Transform), true);

        if(GUILayout.Button("Bake Chunks"))
        {
            BakeChunks();
        }
    }

    private void BakeChunks()
    {
        database.chunks.Clear();

        int platformLayer = LayerMask.NameToLayer("Platform");
        int layerMask = 1 << platformLayer;
        Physics2D.SyncTransforms();

        foreach (Transform chunkTransform in parent)
        {
            ChunkData newChunkData = new ChunkData();
            var platforms = chunkTransform.GetComponentsInChildren<Transform>()
                .Where(t  => t.gameObject.layer == platformLayer)
                .ToList();

            if(platforms.Count == 0)
            {
                continue;
            }

            platforms.Sort((a, b) => a.position.y.CompareTo(b.position.y));
            float lowestY = platforms.First().position.y;
            float highestY = platforms.Last().position.y;

            for (int index = 0; index<maxRowCount; ++index)
            {
                float checkX = firstRowX + index * rowGap;
                Collider2D exitHit = Physics2D.OverlapPoint(new Vector2(checkX, highestY), layerMask);
                if (exitHit != null)
                {
                    newChunkData._exitIndexes.Add(index);
                }

                Collider2D entranceHit = Physics2D.OverlapPoint(new Vector2(checkX, lowestY), layerMask);
                if (entranceHit == null)
                {
                    newChunkData._entranceIndexes.Add(index);
                }
            }
            database.chunks.Add(newChunkData);

   
        }
        UnityEditor.EditorUtility.SetDirty(database);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("Baking Done");
    }
}
