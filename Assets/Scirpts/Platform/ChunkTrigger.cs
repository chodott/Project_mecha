using UnityEngine;

public class ChunkTrigger : NetworkPoolable
{
    private bool _hasTriggered = false;

    public override void OnDespawn()
    {

    }

    public override void OnSpawn()
    {

    }

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
