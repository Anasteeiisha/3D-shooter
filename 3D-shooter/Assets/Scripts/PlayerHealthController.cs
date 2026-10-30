using UnityEngine;
using TMPro; // Добавляем библиотеку для работы с текстом

public class PlayerHealthController : MonoBehaviour
{
    [Header("MVC Refs")]
    [SerializeField] private PlayerHealthModel _model;
    [SerializeField] private TextMeshProUGUI _healthTextUI; // Сюда мы просто перетащим сам объект текста с экрана

    private void Start()
    {
        // Проверяем, прикреплен ли скрипт модели
        if (_model != null)
        {
            _model.OnHealthChanged += UpdateView;
            UpdateView(_model.CurrentHealth);
        }
        else
        {
            Debug.LogError("[ОШИБКА] На игроке в поле Model скрипта PlayerHealthController не перетащена модель здоровья!");
        }
    }

    private void OnDestroy()
    {
        if (_model != null)
        {
            _model.OnHealthChanged -= UpdateView;
        }
    }

    private void UpdateView(int currentHealth)
    {
        // Напрямую обновляем текст на экране без вызова сторонних скриптов
        if (_healthTextUI != null)
        {
            _healthTextUI.text = "HP: " + currentHealth;
        }
    }
}