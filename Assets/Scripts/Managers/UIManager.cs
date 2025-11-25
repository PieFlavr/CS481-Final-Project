using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Fields and Properties

    public static UIManager Instance { get; private set; } // Singleton instance

    [Header("UI Panels")]
    [SerializeField] private List<UIPanel> panels = new List<UIPanel>(); // Must add panels in inspector
    
    private Dictionary<string, UIPanel> panelLibrary = new Dictionary<string, UIPanel>();
    private Stack<UIPanel> navigationStack = new Stack<UIPanel>();
    private UIPanel currentOverlayPanel;

    #endregion



    #region Unity Methods
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[UIManager] Duplicate instance detected -- destroying the new one!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializePanels();
    }
    
    #endregion



    #region Panel Management

    /// <summary>
    /// Opens a UI panel by its name.
    /// </summary>
    /// <param name="panelName">The name of the panel to open.</param>
    public void OpenPanel(string panelName)
    {
        if (!panelLibrary.TryGetValue(panelName, out UIPanel panel))
        {
            Debug.LogError($"[UIManager] Panel not found: {panelName}");
            return;
        }

        // Handle based on layer type
        if (panel.Layer == UILayer.Overlay)
        {
            if (currentOverlayPanel == panel && panel.IsVisible)
            {
                Debug.LogWarning($"[UIManager] Overlay panel {panelName} is already open.");
                return;
            }
            // Stack-based navigation for overlay panels
            if (currentOverlayPanel != null && currentOverlayPanel != panel)
            {
                navigationStack.Push(currentOverlayPanel);
                currentOverlayPanel.Hide();
            }
            currentOverlayPanel = panel;
            panel.Show();
        }
        else
        {
            // Non-overlay panels just show (HUD, tooltips, etc.)
            panel.Show();
        }
    }

    /// <summary>
    /// Closes a UI panel by its name.
    /// Does NOT auto-navigate back for overlay panels.
    /// Removes the panel from the navigation stack if present.
    /// </summary>
    /// <param name="panelName"></param>
    public void ClosePanel(string panelName)
    {
        if (!panelLibrary.TryGetValue(panelName, out UIPanel panel))
            return;

        panel.Hide();

        if (panel.Layer == UILayer.Overlay)
        {
            // Closing current overlay does NOT auto-navigate
            if (currentOverlayPanel == panel)
            {
                currentOverlayPanel = null;
            }
            RemoveFromStack(panel);
        }
    }

    /// <summary>
    /// Navigates back to the previous panel in the stack.
    /// </summary>
    public void GoBack()
    {
        if (navigationStack.Count == 0)
        {
            Debug.LogWarning("[UIManager] Navigation stack is empty, cannot go back.");
            return;
        }

        // Hide current overlay
        if (currentOverlayPanel != null)
        {
            currentOverlayPanel.Hide();
            currentOverlayPanel = null;
        }

        // Pop and show previous panel
        currentOverlayPanel = navigationStack.Pop();
        if (currentOverlayPanel == null)
        {
            Debug.LogError("[UIManager] Popped a null panel from the navigation stack!");
            return;
        }
        currentOverlayPanel.Show();
    }

    /// <summary>
    /// Closes all overlay panels, including the current overlay and all panels in the navigation stack.
    /// </summary>
    public void CloseAllOverlays()
    {
        // Close current overlay
        if (currentOverlayPanel != null)
        {
            currentOverlayPanel.Hide();
            currentOverlayPanel = null;
        }

        // Close all stacked overlays
        while (navigationStack.Count > 0)
        {
            var panel = navigationStack.Pop();
            panel.Hide();
        }
    }

    #endregion



    #region Accessors

    /// <summary>
    /// Retrieves a UI panel by its name with type safety.
    /// </summary>
    public T GetPanel<T>(string panelName) where T : UIPanel
    {
        if (panelLibrary.TryGetValue(panelName, out UIPanel panel))
        {
            return panel as T;
        }
        Debug.LogWarning($"[UIManager] Panel not found: {panelName}");
        return null;
    }

    /// <summary>
    /// Checks if a UI panel is currently open (visible) by its name.
    /// </summary>
    /// <param name="panelName">The name of the panel to check.</param>
    /// <returns>True if the panel is open; otherwise, false.</returns>
    public bool IsPanelOpen(string panelName)
    {
        if (panelLibrary.TryGetValue(panelName, out UIPanel panel))
        {
            return panel.IsVisible;
        }
        return false;
    }

    #endregion Accessors



    #region Private Methods
    /// <summary>
    /// Safely removes a panel from the navigation stack.
    /// </summary>
    /// <param name="panel"></param>
    private void RemoveFromStack(UIPanel panel)
    {
        if (navigationStack.Count == 0)
            return;

        var tempStack = new Stack<UIPanel>();

        // Flip the stack, removing all instances of 'panel'
        while (navigationStack.Count > 0)
        {
            var top = navigationStack.Pop();
            if (top != panel)
                tempStack.Push(top);
        }

        // Restore without the removed panel(s)
        while (tempStack.Count > 0)
            navigationStack.Push(tempStack.Pop());
    }

    /// <summary>
    /// Initializes all UI panels and builds the panel library.
    /// </summary>
    private void InitializePanels()
    {
        Debug.Log("[UIManager] Initializing UI Panels...");
        panelLibrary.Clear();
        navigationStack.Clear();
        
        foreach (var panel in panels)
        {
            if (panel != null)
            {
                panel.Initialize();
                if (!panelLibrary.ContainsKey(panel.PanelName))
                {
                    panelLibrary.Add(panel.PanelName, panel);
                    panel.Hide(); // Start hidden
                }
                else
                {
                    Debug.LogWarning($"[UIManager] Duplicate panel name detected: {panel.PanelName}");
                }
            }
        }
        Debug.Log("[UIManager] UI Panels initialization complete.");
    }

    #endregion
}