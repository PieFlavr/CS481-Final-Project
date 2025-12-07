using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PausePanel : UIPanel
{
    [Header("Pause UI References")]
    [SerializeField] private Image dimBackground;
    [SerializeField] private Image pauseIcon;
    [SerializeField] private Button quitToMenuButton;
    [SerializeField] private Button quitToDesktopButton;

    [Header("Pause Settings")]
    [SerializeField] private float dimAlpha = 0.7f;
    [SerializeField] private float flashSpeed = 1.5f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private string menuSceneName = "MainMenu";

    private float flashTimer = 0f;
    private bool isFlashing = false;

    public override void Initialize()
    {
        base.Initialize();

        // Setup button listeners
        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.AddListener(QuitToMenu);
        }

        if (quitToDesktopButton != null)
        {
            quitToDesktopButton.onClick.AddListener(QuitToDesktop);
        }
    }

    protected override void OnPanelShow()
    {
        base.OnPanelShow();
        
        // Pause the game
        Time.timeScale = 0f;

        // Set dim background
        if (dimBackground != null)
        {
            Color color = dimBackground.color;
            color.a = dimAlpha;
            dimBackground.color = color;
        }

        // Reset flash timer and start flashing
        flashTimer = 0f;
        isFlashing = true;
    }

    protected override void OnPanelHide()
    {
        base.OnPanelHide();
        
        // Resume the game
        Time.timeScale = 1f;
        isFlashing = false;
    }

    private void Update()
    {
        // Handle flashing pause icon when visible
        if (isFlashing && pauseIcon != null)
        {
            FlashPauseIcon();
        }
    }

    private void FlashPauseIcon()
    {
        flashTimer += Time.unscaledDeltaTime * flashSpeed;
        
        // Use PingPong to create a smooth back-and-forth animation
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(flashTimer, 1f));
        
        Color color = pauseIcon.color;
        color.a = alpha;
        pauseIcon.color = color;
    }

    public void QuitToMenu()
    {
        // Resume time before loading
        Time.timeScale = 1f;
        
        // Load the main menu scene
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitToDesktop()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
