using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Эта строчка обязательно нужна для управления плавным цветом!

public class PlayerHealthModel : MonoBehaviour
{
    public static PlayerHealthModel Instance;

    public event Action<int> OnHealthChanged;

    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;
    private bool _isDead = false; 

    [Header("UI Экраны Оповещений")]
    [SerializeField] private GameObject _deathWindow;       
    [SerializeField] private Image _damageFlashImage; // Вернули тип Image для плавного изменения прозрачности
    [SerializeField] private float _flashDuration = 0.2f; // Время плавного исчезновения красного экрана

    private Coroutine _flashCoroutine;

    public int CurrentHealth => _currentHealth;

    private void Awake()
    {
        Instance = this;
        _currentHealth = _maxHealth;
        
        if (_deathWindow != null) _deathWindow.SetActive(false);
        // При старте игры делаем картинку полностью прозрачной, чтобы она не мешала
        if (_damageFlashImage != null) _damageFlashImage.color = new Color(1, 0, 0, 0);
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return; 

        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth);

        // ПЛАВНАЯ ВСПЫШКА: Если мы живы, запускаем плавное таяние красного цвета
        if (_currentHealth > 0 && _damageFlashImage != null)
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashDamageCoroutine());
        }

        if (_currentHealth <= 0)
        {
            StartCoroutine(PlayerDieCoroutine());
        }
    }

    // Тот самый корутин плавного затухания, который радует глаз!
    private IEnumerator FlashDamageCoroutine()
    {
        // Мгновенно делаем экран полупрозрачным красным (альфа = 0.4f)
        _damageFlashImage.color = new Color(1, 0, 0, 0.4f);
        
        float elapsed = 0f;
        while (elapsed < _flashDuration)
        {
            elapsed += Time.deltaTime;
            // Математически плавно уменьшаем прозрачность от 0.4 до 0 на протяжении 0.2 сек
            float alpha = Mathf.Lerp(0.4f, 0f, elapsed / _flashDuration);
            _damageFlashImage.color = new Color(1, 0, 0, alpha);
            yield return null; // Ждем следующий кадр
        }

        // На всякий случай жестко сбрасываем прозрачность в ноль
        _damageFlashImage.color = new Color(1, 0, 0, 0);
    }

    private IEnumerator PlayerDieCoroutine()
    {
        _isDead = true;
        
        if (_damageFlashImage != null) _damageFlashImage.color = new Color(1, 0, 0, 0);
        if (_deathWindow != null) _deathWindow.SetActive(true);

        GameObject liveEnemy = GameObject.FindWithTag("Enemy");
        string killerName = liveEnemy != null ? liveEnemy.name : "Враг";
        KillfeedManager.Instance?.AddLog(killerName, "Игрок", Color.red, Color.cyan);

        MatchManager.Instance?.RegisterKill(false);

        if (TryGetComponent<PlayerMovement>(out var movement)) movement.enabled = false;
        if (TryGetComponent<PlayerShooting>(out var shooting)) shooting.enabled = false;

        yield return new WaitForSeconds(1.0f);

        if (EnemySpawner.Instance != null) 
        {
            EnemySpawner.Instance.RespawnPlayer(gameObject);
        }

        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth);
        
        if (_deathWindow != null) _deathWindow.SetActive(false);
        if (movement != null) movement.enabled = true;
        if (shooting != null) shooting.enabled = true;
        
        _isDead = false; 
    }
}