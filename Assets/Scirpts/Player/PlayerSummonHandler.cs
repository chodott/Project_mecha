using UnityEngine;

public class PlayerSummonHandler : MonoBehaviour
{
    [SerializeField] private GameObject _rushPrefab;
    [SerializeField] private LayerMask _platformLayer;
    private RushController _rush;


    void Start()
    {
        GameObject obj = Instantiate(_rushPrefab);
        _rush = obj.GetComponent<RushController>();
        obj.SetActive(false);
    }

    public void TrySpawnRush(Vector3 playerPos)
    {
        Vector3 playerPosition = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100f, _platformLayer);
        if (hit.collider != null)
        {
            float targetY = hit.point.y;
            Vector3 targetPos = new Vector3(playerPosition.x, targetY, 0);
            _rush.RequestSpawn(targetPos);
        }
    }
}
