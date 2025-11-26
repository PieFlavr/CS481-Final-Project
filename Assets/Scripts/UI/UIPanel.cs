using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public abstract class UIPanel : MonoBehaviour
{
    #region Fields and Properties
    [SerializeField] protected string panelName;
    [SerializeField] protected UILayer layer = UILayer.Default; 
    // NOTE (L): Was heavily debating this layer enum
    // Wanted stacking behavior but also wanted to allow for coexisting panels (HUD + notifcations, etc.)
    // And other possible layering behaviors I don't even know about yet.
    // I think this is a good compromise for now though -L

    public string PanelName => panelName;
    public UILayer Layer => layer;
    protected CanvasGroup canvasGroup;

    private bool isVisible = true;
    public bool IsVisible => isVisible;
    
    public event System.Action OnShown;
    public event System.Action OnHidden;
    #endregion Fields and Properties


    #region Panel Control

    public virtual void Show()
    {
        if(isVisible) return;
        OnPanelShow();
    }
    public virtual void Hide()
    {
        if(!isVisible) return;
        OnPanelHide();
    }
    public void SetInteractable(bool interactable)
    {
        if(canvasGroup == null)
        {
            Debug.LogWarning($"[UIPanel] CanvasGroup component missing on panel {panelName}!");
            return;
        }
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }
    
    #endregion Panel Control



    #region Panel Behavior Methods

    /// <summary>
    /// Called when the panel is shown.
    /// Override this method to implement custom behavior when the panel becomes visible.
    /// </summary>
    protected virtual void OnPanelShow()
    {
        isVisible = true;
        canvasGroup.alpha = 1f;
        SetInteractable(true);
        OnShown?.Invoke();
    }

    /// <summary>
    /// Called when the panel is hidden.
    /// Override this method to implement custom behavior when the panel becomes invisible.
    /// </summary>
    protected virtual void OnPanelHide()
    {
        Debug.Log($"[UIPanel] Hiding panel: {panelName}");
        isVisible = false;
        canvasGroup.alpha = 0f;
        SetInteractable(false);
        OnHidden?.Invoke();
    }
    
    public virtual void Initialize()
    {
        if(canvasGroup != null) return; // Already initialized
        canvasGroup = GetComponent<CanvasGroup>();
        Debug.Log($"[UIPanel] Initializing panel: {panelName}");
        Debug.Log($"[UIPanel] - CanvasGroup named {canvasGroup.name} found.");
        if(canvasGroup == null)
        {
            Debug.LogError($"[UIPanel] CanvasGroup component missing on panel {panelName}!");
            canvasGroup = gameObject.AddComponent<CanvasGroup>();        
        }
    }

    #endregion Panel Behavior Methods



    #region Validation

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(panelName))
        {
            panelName = gameObject.name;
        }
    }

    #endregion Validation
}

public enum UILayer
{
    Default,        // Coexisting panels (HUD, notifications, etc.)
    Overlay,          // Exclusive panels that stack (pause, settings, menu, inventory, etc.)
    // Add more as needed...
}