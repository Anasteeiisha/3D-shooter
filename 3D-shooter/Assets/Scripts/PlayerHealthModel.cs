using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthModel : MonoBehaviour {
    public static PlayerHealthModel Instance;

    public event Action<int> OnHealthChanged; // Событие для контроллера (MVC)

    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;
    private bool _isDead = false;

    [Header("UI Экраны Оповещений")]
    [SerializeField] private GameObject _deathWindow;
    [SerializeField] private Image _damageFlashImage;
    [SerializeField] private float _flashDuration = 0.2f;

    private Coroutine _flashCoroutine; // Ссылка на корутину, чтобы ее можно было останавливать

    public int CurrentHealth => _currentHealth;

    private void Awake() {
        Instance = this; // Инициализация синглтона
        _currentHealth = _maxHealth;

        if (_deathWindow != null) _deathWindow.SetActive(false); // Прячем окно смерти
        // При старте делаем картинку урона прозрачной, чтобы она не мешала
        if (_damageFlashImage != null) _damageFlashImage.color = new Color(1, 0, 0, 0);
    }

    public void TakeDamage(int damage) {
        if (_isDead) return; // Если уже мертвы, урон не получаем

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth); // Ограничиваем здоровье (не меньше 0 и не больше макс)
        OnHealthChanged?.Invoke(_currentHealth); // Сообщаем контроллеру, что здоровье изменилось

        // плавная  вспышка
        if (_currentHealth > 0 && _damageFlashImage != null) {
            // Если предыдущая вспышка еще работает, останавливаем её, чтобы начать заново
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashDamageCoroutine());
        }

        if (_currentHealth <= 0) {
            StartCoroutine(PlayerDieCoroutine()); // Запускаем смерть
        }
    }

    // Корутина плавного затухания красного экрана
    private IEnumerator FlashDamageCoroutine() {
        // Мгновенно делаем экран полупрозрачным красным (альфа = 0.4f)
        _damageFlashImage.color = new Color(1, 0, 0, 0.4f);

        float elapsed = 0f; // Счетчик прошедшего времени
        while (elapsed < _flashDuration) {
            elapsed += Time.deltaTime; // Прибавляем время кадра
            // Математически плавно уменьшаем прозрачность от 0.4 до 0
            float alpha = Mathf.Lerp(0.4f, 0f, elapsed / _flashDuration);
            _damageFlashImage.color = new Color(1, 0, 0, alpha);
            yield return null; // Ждем следующий кадр
        }

        // На всякий случай жестко сбрасываем прозрачность в ноль
        _damageFlashImage.color = new Color(1, 0, 0, 0);
    }

    // Корутина смерти и возрождения игрока
    private IEnumerator PlayerDieCoroutine() {
        _isDead = true; // Ставим флаг смерти

        if (_damageFlashImage != null) _damageFlashImage.color = new Color(1, 0, 0, 0); // Убираем красный экран
        if (_deathWindow != null) _deathWindow.SetActive(true); // Показываем окно смерти

        // Определяем убийцу для ленты 
        GameObject liveEnemy = GameObject.FindWithTag("Enemy");
        string killerName = liveEnemy != null ? liveEnemy.name : "Враг";
        KillfeedManager.Instance.AddLog(killerName, "Игрок", Color.red, Color.cyan);

        MatchManager.Instance?.RegisterKill(false); // Начисляем очко врагам (false)

        // Отключаем управление игроком
        if (TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
        if (TryGetComponent<PlayerShooting>(out var shooting)) shooting.enabled = false;

        yield return new WaitForSeconds(1.0f); // Ждем 1 секунду перед возрождением

        if (EnemySpawner.Instance != null) {
            EnemySpawner.Instance.RespawnPlayer(gameObject); // Возрождаем игрока
        }

        _currentHealth = _maxHealth; // Восстанавливаем здоровье
        OnHealthChanged?.Invoke(_currentHealth); // Обновляем UI

        if (_deathWindow != null) _deathWindow.SetActive(false); // Прячем окно смерти
        if (movement != null) movement.enabled = true; // Включаем управление обратно
        if (shooting != null) shooting.enabled = true;

        _isDead = false; // Снимаем флаг смерти
    }
}