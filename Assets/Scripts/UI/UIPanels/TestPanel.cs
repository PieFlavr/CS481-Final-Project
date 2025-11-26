using UnityEngine;
using UnityEngine.UI;

public class TestPanel : UIPanel
{
    [SerializeField] private Button backButton;

    public override void Initialize()
    {
        base.Initialize();
        if(backButton != null)
        {
            Debug.Log("[TestPanel] Back button reference found, adding listener.");
            backButton.onClick.AddListener(OnBackPressed);
        }
        else
        {
            Debug.LogWarning("[TestPanel] Back button reference is missing!");
        }
    }

    private void OnBackPressed()
    {
        AudioManager.Instance.PlaySFX("ClickSFX");
        Debug.Log("[TestPanel] Back button pressed!");
        UIManager.Instance.ClosePanel(panelName);
        GameManager.Instance.ChangeState(GameState.Playing); // TODO: Remove this junk ASAP and slap it onto an event manager -L
    }

    protected override void OnPanelShow()
    {
        base.OnPanelShow();
        if (backButton != null)
            backButton.onClick.AddListener(OnBackPressed);
    }

    protected override void OnPanelHide()
    {
        base.OnPanelHide();
        if(backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackPressed);
        }
    }
}
