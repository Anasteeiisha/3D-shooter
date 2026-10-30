using System.Collections;
using UnityEngine;
using TMPro;

public class KillfeedManager : MonoBehaviour {
    public static KillfeedManager Instance; // Синглтон для доступа из любых скриптов

    [Header("Настройки UI")]
    [SerializeField] private RectTransform _killfeedContainer;
    [SerializeField] private GameObject _killfeedTextPrefab; 

    private void Awake() {
        Instance = this; // Инициализация синглтона
    }

    // Метод, который мы будем вызывать при смерти любого бота
    public void AddLog(string killerName, string victimName, Color killerColor, Color victimColor) {
        if (_killfeedContainer == null || _killfeedTextPrefab == null) return; // Защита от пустых ссылок

        // Создаем новый текст внутри нашего контейнера
        GameObject newLog = Instantiate(_killfeedTextPrefab, _killfeedContainer);
        TextMeshProUGUI textComponent = newLog.GetComponent<TextMeshProUGUI>();

        if (textComponent != null) {
            // Форматируем строку с цветными именами
            string killerHex = ColorUtility.ToHtmlStringRGB(killerColor); // Переводим цвет в HEX-код
            string victimHex = ColorUtility.ToHtmlStringRGB(victimColor);

            // Собираем строку: "Убийца -> Жертва" с нужными цветами
            textComponent.text = $"<color=#{killerHex}>{killerName}</color> -> <color=#{victimHex}>{victimName}</color>";
            textComponent.fontSize = 18;
            textComponent.alignment = TextAlignmentOptions.Right; // Выравнивание по правому краю
        }

        // Уничтожаем строчку текста через 3 секунды, чтобы она плавно исчезла
        Destroy(newLog, 3f);
    }
}