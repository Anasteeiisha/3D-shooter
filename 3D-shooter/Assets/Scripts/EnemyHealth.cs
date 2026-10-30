using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour {
    [SerializeField] private int _health = 100;
    private int _currentHealth;
    private Renderer _renderer;
    private bool _isDead = false;

    public int CurrentHealth => _currentHealth;

    private void Awake() {
        _currentHealth = _health;
        _renderer = GetComponent<Renderer>(); // Находим рендер для вспышки
    }

    public void TakeDamage(int damage) {
        if (_isDead) return; //игнорируем урон

        _currentHealth -= damage;

        if (_renderer != null) StartCoroutine(HitFlashCoroutine()); // Запускаем красную вспышку

        if (_currentHealth <= 0) Die();
    }

    private IEnumerator HitFlashCoroutine() {
        if (_renderer == null) yield break;
        // Ищем правильное свойство цвета в шейдере
        string colorPropertyName = _renderer.material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
        Color oldColor = _renderer.material.GetColor(colorPropertyName); // Запоминаем старый цвет
        
        _renderer.material.SetColor(colorPropertyName, Color.red); // Красим в красный
        yield return new WaitForSeconds(0.1f); // Ждем 0.1 сек
        
        if (!_isDead && _renderer != null) _renderer.material.SetColor(colorPropertyName, oldColor); // Возвращаем цвет
    }

    private void Die() {
        if (_isDead) return; // Защита от повторного вызова
        _isDead = true;

        if (TryGetComponent<EnemyAI>(out var ai)) ai.DisableAI(); // Отключаем ИИ
        if (TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false; // Отключаем движение

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = false; // Включаем физику
            rb.constraints = RigidbodyConstraints.None; // Снимаем ограничения
            rb.AddForce(-transform.forward * 4f + Vector3.up * 2f, ForceMode.Impulse); // Отлет трупа
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse); // Случайное вращение в воздухе
        }

        // Отправляем данные менеджерам (игрок убил врага)
        KillfeedManager.Instance?.AddLog("Игрок", gameObject.name, Color.cyan, Color.red); // Лента убийств
        MatchManager.Instance?.RegisterKill(true); // true = синяя команда (игрок) убила

        if (EnemySpawner.Instance != null) {
            EnemySpawner.Instance.SpawnEnemy(gameObject.name); // Спавним нового врага
        }

        Destroy(gameObject, 3f); // Удаляем труп через 3 сек
    }
}