using System.Collections;
using UnityEngine;
using TMPro;

public class KillfeedManager : MonoBehaviour
{
    public static KillfeedManager Instance;

    [Header("Настройки UI")]
    [SerializeField] private RectTransform _killfeedContainer; // Сюда перетащим наш KillfeedContainer
    [SerializeField] private GameObject _killfeedTextPrefab;   // Прераб обычного UI текста TextMeshPro

    private void Awake()
    {
        Instance = this;
    }

    // Метод, который мы будем вызывать при смерти любого бота
    public void AddLog(string killerName, string victimName, Color killerColor, Color victimColor)
    {
        if (_killfeedContainer == null || _killfeedTextPrefab == null) return;

        // Создаем новый текст внутри нашего контейнера
        GameObject newLog = Instantiate(_killfeedTextPrefab, _killfeedContainer);
        TextMeshProUGUI textComponent = newLog.GetComponent<TextMeshProUGUI>();

        if (textComponent != null)
        {
            // Форматируем строку с цветными именами (используем HTML-теги Unity)
            string killerHex = ColorUtility.ToHtmlStringRGB(killerColor);
            string victimHex = ColorUtility.ToHtmlStringRGB(victimColor);

            textComponent.text = $"<color=#{killerHex}>{killerName}</color> -> <color=#{victimHex}>{victimName}</color>";
            textComponent.fontSize = 18;
            textComponent.alignment = TextAlignmentOptions.Right; // Выравнивание по правому краю
        }

        // Уничтожаем строчку текста через 3 секунды, чтобы она плавно исчезла
        Destroy(newLog, 3f);
    }
}