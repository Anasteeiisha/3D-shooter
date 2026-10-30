using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    [Header("Префабы ботов")]
    [SerializeField] private GameObject _enemyPrefab; 
    [SerializeField] private GameObject _allyPrefab;  

    [Header("Точки появления команд")]
    [SerializeField] private Transform[] _enemySpawnPoints; 
    [SerializeField] private Transform[] _allySpawnPoints;  

    [Header("Настройки")]
    [SerializeField] private float _respawnDelay = 4.0f; 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Создаем на старте 5 врагов и 4 союзников с фиксированными именами
        for (int i = 1; i <= 5; i++)
        {
            SpawnNewEnemyDirectly($"Враг {i}");
        }

        for (int i = 1; i <= 4; i++)
        {
            SpawnNewAllyDirectly($"Союзник {i}");
        }
    }

    public void SpawnEnemy(string oldName)
    {
        StartCoroutine(RespawnEnemyCoroutine(oldName));
    }

    public void SpawnAlly(string oldName)
    {
        StartCoroutine(RespawnAllyCoroutine(oldName));
    }

    private IEnumerator RespawnEnemyCoroutine(string oldName)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnNewEnemyDirectly(oldName);
    }

    private IEnumerator RespawnAllyCoroutine(string oldName)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnNewAllyDirectly(oldName);
    }

    private void SpawnNewEnemyDirectly(string enemyName)
    {
        if (_enemyPrefab == null || _enemySpawnPoints.Length == 0) return;

        Transform point = _enemySpawnPoints[Random.Range(0, _enemySpawnPoints.Length)];
        Vector3 spawnPosition = GetSafeNavMeshPosition(point.position);

        GameObject newEnemy = Instantiate(_enemyPrefab, spawnPosition, point.rotation);
        newEnemy.SetActive(true); 
        newEnemy.name = enemyName;
        newEnemy.tag = "Enemy"; 
    }

    private void SpawnNewAllyDirectly(string allyName)
    {
        if (_allyPrefab == null || _allySpawnPoints.Length == 0) return;

        Transform point = _allySpawnPoints[Random.Range(0, _allySpawnPoints.Length)];
        Vector3 spawnPosition = GetSafeNavMeshPosition(point.position);

        GameObject newAlly = Instantiate(_allyPrefab, spawnPosition, point.rotation);
        newAlly.SetActive(true); 
        newAlly.name = allyName;
        newAlly.tag = "Ally"; 
    }

    // ВОТ ЭТОТ МЕТОД, КОТОРЫЙ ТРЕБОВАЛА КОНСОЛЬ: Возрождение Игрока
    public void RespawnPlayer(GameObject playerObject)
    {
        if (_allySpawnPoints.Length == 0 || playerObject == null) return;

        Transform randomPoint = _allySpawnPoints[Random.Range(0, _allySpawnPoints.Length)];

        CharacterController cc = playerObject.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // Отключаем на миг, чтобы сработал телепорт

        playerObject.transform.position = randomPoint.position;
        playerObject.transform.rotation = randomPoint.rotation;

        if (cc != null) cc.enabled = true;
        Debug.Log("[СПАВНЕР] Игрок успешно возрожден на базе союзников!");
    }

    private Vector3 GetSafeNavMeshPosition(Vector3 targetPos)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return targetPos;
    }
}