using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPanel : UIPanel
{
    [SerializeField] private string menuSceneName;

    public override void Initialize()
    {
        base.Initialize();
        PlayerManager.PlayerDied += () => this.Show();
    }

    public void QuitToMenu()
    {
        // Resume time before loading
        Time.timeScale = 1f;

        // Load the main menu scene
        SceneManager.LoadScene(menuSceneName);
    }
}
