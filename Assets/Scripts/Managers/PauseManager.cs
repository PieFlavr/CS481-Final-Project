using UnityEngine;

public class PauseManager : MonoBehaviour
{
    private const string PAUSE_PANEL_NAME = "PausePanel";

    void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (IsPaused())
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OpenPanel(PAUSE_PANEL_NAME);
        }
        else
        {
            Debug.LogError("[PauseManager] UIManager instance not found!");
        }
    }

    public void Resume()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClosePanel(PAUSE_PANEL_NAME);
        }
        else
        {
            Debug.LogError("[PauseManager] UIManager instance not found!");
        }
    }

    public bool IsPaused()
    {
        if (UIManager.Instance != null)
        {
            return UIManager.Instance.IsPanelOpen(PAUSE_PANEL_NAME);
        }
        return false;
    }
}
