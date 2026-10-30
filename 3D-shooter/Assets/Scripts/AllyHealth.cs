using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AllyHealth : MonoBehaviour {
    [SerializeField] private int _health = 100;
    private int _currentHealth;
    private Renderer _renderer;
    private bool _isDead = false;

    public int CurrentHealth => _currentHealth;

    private void Awake() {
        _currentHealth = _health; // Задаем начальное здоровье
        _renderer = GetComponent<Renderer>(); // Находим рендер для вспышки
    }

    public void TakeDamage(int damage) {
        if (_isDead) return; // Если уже мертв, игнорируем урон

        _currentHealth -= damage; // Отнимаем здоровье
        if (_renderer != null) StartCoroutine(FlashBlue()); // Запускаем вспышку
        if (_currentHealth <= 0) Die(); // Если здоровье 0 - умираем
    }

    private IEnumerator FlashBlue() {
        if (_renderer == null) yield break;
        
        // Ищем правильное свойство цвета в шейдере
        string prop = _renderer.material.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
        Color old = _renderer.material.GetColor(prop); // Запоминаем старый цвет
        
        _renderer.material.SetColor(prop, Color.cyan); // Красим в голубой
        yield return new WaitForSeconds(0.1f); // Ждем 0.1 сек
        
        if (!_isDead && _renderer != null) _renderer.material.SetColor(prop, old); // Возвращаем цвет
    }

    private void Die() {
        if (_isDead) return; // Защита от повторного вызова
        _isDead = true;

        if (TryGetComponent<AllyAI>(out var ai)) ai.DisableAI(); // Отключаем ИИ
        if (TryGetComponent<NavMeshAgent>(out var agent)) agent.enabled = false; // Отключаем движение

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.isKinematic = false; // Включаем физику
            rb.constraints = RigidbodyConstraints.None; // Снимаем ограничения
            rb.AddForce(-transform.forward * 4f + Vector3.up * 2f, ForceMode.Impulse); // Отлет трупа
        }

        // Определяем имя убийцы
        GameObject liveEnemy = GameObject.FindWithTag("Enemy"); 
        string killerName = liveEnemy != null ? liveEnemy.name : "Враг";
        
        KillfeedManager.Instance.AddLog(killerName, gameObject.name, Color.red, Color.cyan); // Лента убийств
        MatchManager.Instance?.RegisterKill(false); // Начисляем очко врагам

        if (EnemySpawner.Instance != null) {
            EnemySpawner.Instance.SpawnAlly(gameObject.name); // Спавним нового союзника
        }

        Destroy(gameObject, 3f); // Удаляем труп через 3 сек
    }
}