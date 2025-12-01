using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    #region Fields and Properties

    public static UIManager Instance { get; private set; } // Singleton instance

    [Header("UI Panels")]
    [SerializeField] private List<UIPanel> panels = new List<UIPanel>(); // Must add panels in inspector
    [SerializeField] private Transform uiRoot;
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

        InitializeInpsectorPanels();
        InitializeScenePanels();
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
        if (currentOverlayPanel == null)
        {
            Debug.LogWarning("[UIManager] No overlay panel is open, cannot go back.");
            return;
        }

        if (navigationStack.Count == 0)
        {
            Debug.Log("[UIManager] Navigation stack is empty, assuming on root overlay.");
            currentOverlayPanel.Hide();
            currentOverlayPanel = null;
            return;
        }

        // Hide current overlay
        currentOverlayPanel.Hide();

        // Pop and show previous panel
        currentOverlayPanel = navigationStack.Pop();
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

    /// <summary>
    /// Checks if there is any overlay panel currently open.
    /// </summary>
    /// <returns>True if an overlay panel is open; otherwise, false.</returns>
    public bool HasOpenOverlay()
    {
        Debug.Log("[UIManager] Checking for open overlay panel..." + (currentOverlayPanel != null));
        return currentOverlayPanel != null;
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
    /// Registers all UIPanels found in the scene under the uiRoot.
    /// </summary>
    private void InitializeInpsectorPanels()
    {
        Debug.Log("[UIManager] Initializing Inspector UI Panels...");
        panelLibrary.Clear();
        navigationStack.Clear();
        
        // Inspector Assigned panels
        foreach (var panel in panels)
        {
            if (panel != null && !panelLibrary.ContainsKey(panel.PanelName))
            {
                RegisterPanel(panel);
            }
            else
            {
                Debug.LogWarning($"[UIManager] Duplicate inspector panel name detected: {panel.PanelName}");
            }
            
        }
        Debug.Log("[UIManager] UI Panels initialization complete.");
    }

    /// <summary>
    /// Initializes all UIPanels found in the scene under the uiRoot.
    /// </summary>
    private void InitializeScenePanels()
    {
        if (uiRoot == null) return;

        Debug.Log("[UIManager] Initializing Scene UI Panels...");

        UIPanel[] panels = uiRoot.GetComponentsInChildren<UIPanel>(true);
        foreach (var panel in panels)
        {
            if (panel != null && !panelLibrary.ContainsKey(panel.PanelName))
            {
                RegisterPanel(panel);
            }
            else
            {
                Debug.LogWarning($"[UIManager] Duplicate scene panel name detected: {panel.PanelName}");
            }
        }
    }

    /// <summary>
    /// Registers a UIPanel by initializing and hiding it, then adding it to the panel library.
    /// </summary>
    /// <param name="panel">The UIPanel to register.</param>
    private void RegisterPanel(UIPanel panel)
    {
        Debug.Log($"[UIManager] Registering panel: {panel.PanelName}");
        panel.Initialize();
        panelLibrary.Add(panel.PanelName, panel);
        panel.Hide();
    }

    #endregion
}