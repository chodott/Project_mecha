using UnityEngine;

public class ChunkTrigger : MonoBehaviour
{
    private bool _hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasTriggered)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            _hasTriggered = true;
            MapGenerator.Instance.GenerateNextChunk();
        }
    }
}
