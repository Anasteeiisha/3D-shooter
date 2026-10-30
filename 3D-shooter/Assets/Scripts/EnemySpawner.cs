using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour {
    public static EnemySpawner Instance; // Синглтон для доступа из других скриптов

    [Header("Префабы ботов")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _allyPrefab;

    [Header("Точки появления команд")]
    [SerializeField] private Transform[] _enemySpawnPoints;
    [SerializeField] private Transform[] _allySpawnPoints;

    [Header("Настройки")]
    [SerializeField] private float _respawnDelay = 4.0f;

    private void Awake() {
        Instance = this; // Инициализация синглтона
    }

    private void Start() {
        for (int i = 1; i <= 5; i++) {   // Создаем на старте 5 врагов и 4 союзников с фиксированными именами
            SpawnNewEnemyDirectly($"Враг {i}");
        }
        for (int i = 1; i <= 4; i++) {
            SpawnNewAllyDirectly($"Союзник {i}");
        }
    }
    // Вызывается из EnemyHealth при смерти. Запускает отложенный спавн
    public void SpawnEnemy(string oldName) {
        StartCoroutine(RespawnEnemyCoroutine(oldName));
    }
    // Вызывается из AllyHealth при смерти. Запускает отложенный спавн
    public void SpawnAlly(string oldName) {
        StartCoroutine(RespawnAllyCoroutine(oldName));
    }

    private IEnumerator RespawnEnemyCoroutine(string oldName) {
        yield return new WaitForSeconds(_respawnDelay); // Ждем задержку
        SpawnNewEnemyDirectly(oldName); // Создаем врага
    }

    private IEnumerator RespawnAllyCoroutine(string oldName) {
        yield return new WaitForSeconds(_respawnDelay); // Ждем задержку
        SpawnNewAllyDirectly(oldName); // Создаем союзника
    }

    private void SpawnNewEnemyDirectly(string enemyName) {   // Мгновенно создает врага в случайной точке
        if (_enemyPrefab == null || _enemySpawnPoints.Length == 0) return; // Защита от пустых ссылок

        Transform point = _enemySpawnPoints[Random.Range(0, _enemySpawnPoints.Length)]; // Выбираем случайную точку
        Vector3 spawnPosition = GetSafeNavMeshPosition(point.position); // Ищем безопасную позицию на NavMesh

        GameObject newEnemy = Instantiate(_enemyPrefab, spawnPosition, point.rotation); // Создаем объект
        newEnemy.SetActive(true);
        newEnemy.name = enemyName; // Даем имя
        newEnemy.tag = "Enemy"; // Ставим тег
    }

    private void SpawnNewAllyDirectly(string allyName) { // Мгновенно создает союзника в случайной точке
        if (_allyPrefab == null || _allySpawnPoints.Length == 0) return;

        Transform point = _allySpawnPoints[Random.Range(0, _allySpawnPoints.Length)];
        Vector3 spawnPosition = GetSafeNavMeshPosition(point.position);

        GameObject newAlly = Instantiate(_allyPrefab, spawnPosition, point.rotation);
        newAlly.SetActive(true);
        newAlly.name = allyName;
        newAlly.tag = "Ally";
    }

    // Возрождение Игрока. Вызывается из PlayerHealthModel
    public void RespawnPlayer(GameObject playerObject) {
        if (_allySpawnPoints.Length == 0 || playerObject == null) return; // Защита

        Transform randomPoint = _allySpawnPoints[Random.Range(0, _allySpawnPoints.Length)]; // Случайная точка союзников

        // Отключаем CharacterController на 1 кадр, чтобы телепорт сработал без багов
        CharacterController cc = playerObject.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; 
        
        playerObject.transform.position = randomPoint.position; // Телепортируем
        playerObject.transform.rotation = randomPoint.rotation; // Поворачиваем
        
        if (cc != null) cc.enabled = true; // Включаем контроллер обратно
        Debug.Log("[СПАВНЕР] Игрок успешно возрожден на базе союзников!");
    }

    // Ищет ближайшую точку на NavMesh, чтобы бот не заспавнился в стене или под полом
    private Vector3 GetSafeNavMeshPosition(Vector3 targetPos) {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 5.0f, NavMesh.AllAreas)) {
            return hit.position; // Если нашли безопасную точку - возвращаем её
        }
        return targetPos; // Если нет - возвращаем исходную (запасной вариант)
    }
}