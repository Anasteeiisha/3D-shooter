using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private GameObject _enemyPrefab; 
    [SerializeField] private float _spawnInterval = 2.0f; 
    [SerializeField] private Transform[] _spawnPoints;   
    [SerializeField] private int _maxEnemies = 6; 

    private float _nextSpawnTime;

    private void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            // НАДЕЖНЫЙ ВАРИАНТ: считаем врагов на сцене по наличию скрипта здоровья здоровья
            int currentEnemyCount = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length;

            // Если живых капсул меньше 6, продолжаем спавнить
            if (currentEnemyCount < _maxEnemies)
            {
                SpawnEnemy();
            }
            
            _nextSpawnTime = Time.time + _spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        if (_enemyPrefab == null || _spawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, _spawnPoints.Length);
        Transform selectedPoint = _spawnPoints[randomIndex];

        Instantiate(_enemyPrefab, selectedPoint.position, selectedPoint.rotation);
        Debug.Log("[Спавнер] На карту выпущен новый враг!");
    }
}