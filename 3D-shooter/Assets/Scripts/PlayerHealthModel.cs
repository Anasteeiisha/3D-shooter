using System;
using UnityEngine;

public class PlayerHealthModel : MonoBehaviour
{
    public event Action<int> OnHealthChanged;

    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;

    public int CurrentHealth => _currentHealth; 

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth);

        // Если здоровье закончилось, вызываем проигрыш
        if (_currentHealth <= 0)
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                gameManager.GameOver(); 
            }
        }
    }
}