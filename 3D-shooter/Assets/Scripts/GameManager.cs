using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    private bool _isPaused = false;

    private void Start() {
        pauseMenu.SetActive(false);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (_isPaused) {
                Resume();
            } else {
                Pause();
            }
        }
    }

    public void Resume() {
        pauseMenu.SetActive(false);
        _isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerMovement.enabled = true;

        Time.timeScale = 1f;
    }

    public void Pause() {
        pauseMenu.SetActive(true);
        _isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerMovement.enabled = false;

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
}