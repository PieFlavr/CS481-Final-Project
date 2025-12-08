using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class EndPanel : UIPanel
{
    [SerializeField] private string menuSceneName;
    [SerializeField] private TMP_Text endMessageText;
    [SerializeField] private string defeatMessage = "You lost!";
    [SerializeField] private string victoryMessage = "You won!";
    bool won;

    public override void Initialize()
    {
        base.Initialize();
        PlayerManager.PlayerDied += OnPlayerDied;
        PlayerManager.PlayerWon += OnPlayerWon;
    }

    private void OnDestroy()
    {
        PlayerManager.PlayerDied -= OnPlayerDied;
        PlayerManager.PlayerWon -= OnPlayerWon;
    }

    private void OnPlayerDied()
    {
        won = false;
        ShowWithMessage(defeatMessage);
    }

    private void OnPlayerWon()
    {
        won = true;
        ShowWithMessage(victoryMessage);
    }

    private void ShowWithMessage(string message)
    {
        if (endMessageText != null)
        {
            endMessageText.text = message;
        }
        else
        {
            Debug.LogWarning("[EndPanel] End message text is not assigned.");
        }
        Show();
    }

    public void QuitToMenu()
    {
        // Resume time before loading
        Time.timeScale = 1f;

        // Load the main menu scene
        if (!won)
            SceneManager.LoadScene(menuSceneName);

        Hide();
    }
}
