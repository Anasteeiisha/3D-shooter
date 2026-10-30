using UnityEngine;
using TMPro;

public class MatchManager : MonoBehaviour {
    public static MatchManager Instance; // Синглтон для доступа из других скриптов

    [Header("Настройки матча")]
    [SerializeField] private int _killsToWin = 25;
    [SerializeField] private TextMeshProUGUI _blueScoreText; 
    [SerializeField] private TextMeshProUGUI _redScoreText;

    private int _blueScore = 0;
    private int _redScore = 0;
    private bool _matchEnded = false;

    private void Awake() {
        Instance = this; // Инициализация синглтона
    }

    private void Start() {
        UpdateScoreUI();
    }
    // Вызывается из скриптов здоровья (EnemyHealth, AllyHealth) при убийстве
    public void RegisterKill(bool isBlueTeam) {
        if (_matchEnded) return; // Если матч уже кончился, очки не начисляем

        if (isBlueTeam) _blueScore++; // Синяя команда (игрок) убила
        else _redScore++; // Красная команда (враги) убили

        UpdateScoreUI(); // Обновляем цифры на экране
        CheckWinCondition(); // Проверяем, не победил ли кто-то
    }

    // Обновляет текст счета в UI
    private void UpdateScoreUI() {
        if (_blueScoreText != null) _blueScoreText.text = _blueScore.ToString(); // Пишем синие очки
        if (_redScoreText != null) _redScoreText.text = _redScore.ToString(); // Пишем красные очки
    }

    // Проверяет, набрала ли какая-то команда нужное количество убийств
    private void CheckWinCondition() {
        if (_blueScore >= _killsToWin) EndMatch(true); // Победа синих (игрока)
        else if (_redScore >= _killsToWin) EndMatch(false); // Победа красных (врагов)
    }

    // Завершает матч: блокирует дальнейшие убийства и показывает экран результатов
    private void EndMatch(bool playerWon) {
        _matchEnded = true; // Ставим блокировку

        // Ищем GameManager на сцене (новый метод Unity)
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null) {
            gameManager.ShowMatchResult(playerWon); // Показываем окно победы
        }
    }
}