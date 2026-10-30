using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [Header("UI Конца Игры")]
    [SerializeField] private GameObject _endGameWindow; // Окно результатов
    [SerializeField] private TextMeshProUGUI _endGameText; // Текст победы

    [Header("UI Паузы матча")]
    [SerializeField] private GameObject _pauseMenuPanel; // Скрытое меню паузы

    private bool _isPaused = false;
    private bool _matchEnded = false;

    private void Awake() {
        Time.timeScale = 1f; // В начале время идет нормально
        if (_endGameWindow != null) _endGameWindow.SetActive(false);

        // При старте принудительно прячем меню пауз и блокируем мышь
        ResumeGame();
    }

    private void Update() {
        if (_matchEnded) return; // Если матч кончился, Esc не работает

        // Отслеживаем нажатие Escape
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (_isPaused) ResumeGame(); // Если на паузе - продолжаем
            else PauseGame(); // Если нет - ставим на паузу
        }
    }

    // Ставит игру на паузу: показывает меню, останавливает время, освобождает курсор
    public void PauseGame() {
        _isPaused = true;
        if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f; // Замораживаем время в игре
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Снимает паузу: скрывает меню, запускает время, прячет курсор
    public void ResumeGame() {
        _isPaused = false;
        if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f; // Возобновляем время
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Вызывается из MatchManager при победе одной из команд
    public void ShowMatchResult(bool isBlueWin) {
        _matchEnded = true; // Блокируем паузу

        if (_endGameWindow == null || _endGameText == null) return;
        _endGameWindow.SetActive(true); // Показываем окно конца

        // Меняем текст и цвет в зависимости от победителя
        if (isBlueWin) {
            _endGameText.text = "ПОБЕДА СИНИХ!";
            _endGameText.color = Color.cyan;
        }
        else {
            _endGameText.text = "ПОБЕДА КРАСНЫХ!";
            _endGameText.color = Color.red;
        }

        Time.timeScale = 0f; // Останавливаем игру
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Перезапуск текущего матча (кнопка "Заново")
    public void RestartGameButton() {
        Time.timeScale = 1f; // Возвращаем время перед загрузкой!
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Выход в главное меню (кнопка "Выйти")
    public void GoToMainMenuButton() {
        // время 1, иначе меню тоже застынет
        Time.timeScale = 1f; 
        
        // Загружаем сцену
        SceneManager.LoadScene("MainMenu");
    }
}