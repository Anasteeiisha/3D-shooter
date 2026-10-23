using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private GameObject _pauseMenu;

    [Header("Player Refs")]
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerInput _playerInput; // Чтобы мышка не дергалась в паузе

    private bool _isPaused = false;

    private void Start() {
        _pauseMenu.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (_isPaused) Resume(); else Pause();
        }
    }

    public void Resume() {
        _pauseMenu.SetActive(false);
        _isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _playerMovement.enabled = true;
        _playerInput.enabled = true;
        Time.timeScale = 1f;
    }

    public void Pause()  {
        _pauseMenu.SetActive(true);
        _isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _playerMovement.enabled = false;
        _playerInput.enabled = false;
        Time.timeScale = 0f;
    }

    public void ExitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ExitToMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

     public void GameOver() 
    {
        // Используем логику паузы: включаем меню (можно использовать то же меню паузы как экран Game Over)
        _pauseMenu.SetActive(true); 

        // Освобождаем мышку
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Отключаем управление и останавливаем время
        _playerMovement.enabled = false;
        _playerInput.enabled = false;
        Time.timeScale = 0f;

        Debug.Log("[ИГРА ОКOНЧЕНА] Вы погибли!");
    }
}