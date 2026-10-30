using UnityEngine;
using TMPro;

public class PlayerHealthController : MonoBehaviour {
    [Header("MVC Refs")]
    [SerializeField] private PlayerHealthModel _model;
    [SerializeField] private TextMeshProUGUI _healthTextUI;

    private void Start() {
        // Проверяем, прикреплен ли скрипт модели
        if (_model != null) {
            _model.OnHealthChanged += UpdateView; // Подписываемся на событие изменения здоровья
            UpdateView(_model.CurrentHealth); // показываем текущее здоровье при старте
        }
        else {
            Debug.LogError("[ОШИБКА] На игроке в поле Model скрипта PlayerHealthController не перетащена модель здоровья!");
        }
    }

    private void OnDestroy() {  // Отписываемся от события при уничтожении объекта (защита от утечек памяти)
        if (_model != null) {
            _model.OnHealthChanged -= UpdateView;
        }
    }

    private void UpdateView(int currentHealth) {
        // Напрямую обновляем текст на экране без вызова сторонних скриптов
        if (_healthTextUI != null) {
            _healthTextUI.text = "HP: " + currentHealth;
        }
    }
}