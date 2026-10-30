using UnityEngine;
using TMPro;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [Header("Настройки матча")]
    [SerializeField] private int _killsToWin = 50;
    [SerializeField] private TextMeshProUGUI _blueScoreText;
    [SerializeField] private TextMeshProUGUI _redScoreText;

    private int _blueScore = 0;
    private int _redScore = 0;
    private bool _matchEnded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void RegisterKill(bool isBlueTeam)
    {
        if (_matchEnded) return;

        if (isBlueTeam) _blueScore++;
        else _redScore++;

        UpdateScoreUI();
        CheckWinCondition();
    }

    private void UpdateScoreUI()
    {
        if (_blueScoreText != null) _blueScoreText.text = _blueScore.ToString();
        if (_redScoreText != null) _redScoreText.text = _redScore.ToString();
    }

    private void CheckWinCondition()
    {
        if (_blueScore >= _killsToWin) EndMatch(true);
        else if (_redScore >= _killsToWin) EndMatch(false);
    }

    private void EndMatch(bool playerWon)
    {
        _matchEnded = true;
        
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.ShowMatchResult(playerWon); 
        }
    }
}