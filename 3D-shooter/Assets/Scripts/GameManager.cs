using UnityEngine;
using TMPro; 
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    [Header("UI Конца Игры")]
    [SerializeField] private GameObject _endGameWindow; 
    [SerializeField] private TextMeshProUGUI _endGameText; 

    [Header("UI Паузы матча")]
    [SerializeField] private GameObject _pauseMenuPanel; // Сюда перетащим скрытое меню PauseMenu из StaticCanvas

    private bool _isPaused = false;
    private bool _matchEnded = false; 

    private void Awake()
    {
        Time.timeScale = 1f;
        if (_endGameWindow != null) _endGameWindow.SetActive(false);
        
        // При старте игры сразу принудительно прячем меню паузы и блокируем мышь для боя
        ResumeGame();
    }

    private void Update()
    {
        if (_matchEnded) return;

        // Отслеживаем Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        _isPaused = true;
        if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(true); 

        Time.timeScale = 0f; // Замораживаем ботов
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        _isPaused = false;
        if (_pauseMenuPanel != null) _pauseMenuPanel.SetActive(false); 

        Time.timeScale = 1f; // Размораживаем ботов
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowMatchResult(bool isBlueWin)
    {
        _matchEnded = true; 
        if (_endGameWindow == null || _endGameText == null) return;

        _endGameWindow.SetActive(true);

        if (isBlueWin)
        {
            _endGameText.text = "ПОБЕДА СИНИХ!";
            _endGameText.color = Color.cyan;
        }
        else
        {
            _endGameText.text = "ПОБЕДА КРАСНЫХ!";
            _endGameText.color = Color.red;
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGameButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ВОТ ЭТОТ НОВЫЙ МЕТОД ДЛЯ КНОПКИ ВЫХОДА:
    public void GoToMainMenuButton()
    {
        // КРИТИЧЕСКИ ВАЖНО: Возвращаем время в нормальный режим (1) перед выходом!
        // Если этого не сделать, главное меню тоже застынет в паузе и там ничего нельзя будет нажать.
        Time.timeScale = 1f;
        
        // Загружаем сцену главного меню по её точному имени
        // Убедись, что твоя сцена в папке Scenes называется именно "MainMenu" (или поменяй имя в кавычках ниже)
        SceneManager.LoadScene("MainMenu");
    }
}