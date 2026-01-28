using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _platformPrefab;
    [SerializeField] private float _spawnInterval = 2f;
    [SerializeField] private float _spawnHeight = 5f;
    [SerializeField] private float _minX = -8f;
    [SerializeField] private float _maxX = 8f;
    private float _timer;
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _spawnInterval)
        {
            SpawnPlatform();
            _timer = 0f;
        }
    }
    private void SpawnPlatform()
    {
        float randomX = Random.Range(_minX, _maxX);
        Vector3 spawnPosition = new Vector3(randomX, _spawnHeight, 0f);
        Instantiate(_platformPrefab, spawnPosition, Quaternion.identity);
    }
}
