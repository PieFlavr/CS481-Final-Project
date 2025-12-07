using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarPanel : UIPanel
{
    [Header("Health Bar Components")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    private StatsComponent playerStats;
    private bool isConnectedToPlayer = false;

    public override void Initialize()
    {
        Debug.Log("[HealthBarPanel] Initialize called");
        base.Initialize();
        // Find player and subscribe to health changes
        FindAndSubscribeToPlayer();
    }

    private void Start()
    {
        // Retry finding player if not connected during Initialize
        if (!isConnectedToPlayer)
        {
            Debug.Log("[HealthBarPanel] Retrying player connection in Start...");
            FindAndSubscribeToPlayer();
        }
    }

    private void Update()
    {
        // Keep trying to find player until connected
        if (!isConnectedToPlayer)
        {
            FindAndSubscribeToPlayer();
        }
    }

    private void FindAndSubscribeToPlayer()
    {
        if (isConnectedToPlayer) return; // Already connected
        
        var player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            Debug.Log("[HealthBarPanel] Player found!");
            playerStats = player.Stats;
            if (playerStats != null)
            {
                Debug.Log($"[HealthBarPanel] Player stats found! Current health: {playerStats.CurrentHealth}/{playerStats.MaxHealth}");
                // Subscribe to health changes
                playerStats.OnHealthPercentChanged += UpdateHealthDisplay;
                playerStats.OnHealthChanged += OnHealthChanged;
                
                // Initialize display
                UpdateHealthDisplay(playerStats.GetHealthPercent());
                
                isConnectedToPlayer = true;
                Debug.Log("[HealthBarPanel] Successfully connected to player!");
            }
            else
            {
                Debug.LogWarning("[HealthBarPanel] Player.Stats is null!");
            }
        }
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        // Optional: Add visual feedback when taking damage
        if (newHealth < oldHealth)
        {
            // Flash red, shake, etc.
        }
    }

    private void UpdateHealthDisplay(float healthPercent)
    {
        Debug.Log($"[HealthBarPanel] Updating health display: {healthPercent:P0}");
        
        // Update fill amount
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;
            
            // Change color based on health
            healthBarFill.color = healthPercent <= lowHealthThreshold 
                ? lowHealthColor 
                : Color.Lerp(lowHealthColor, fullHealthColor, (healthPercent - lowHealthThreshold) / (1f - lowHealthThreshold));
        }
        else
        {
            Debug.LogWarning("[HealthBarPanel] healthBarFill is not assigned!");
        }

        // Update text
        if (healthText != null && playerStats != null)
        {
            healthText.text = $"{Mathf.CeilToInt(playerStats.CurrentHealth)}/{Mathf.CeilToInt(playerStats.MaxHealth)}";
        }
        else if (healthText == null)
        {
            Debug.LogWarning("[HealthBarPanel] healthText is not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthPercentChanged -= UpdateHealthDisplay;
            playerStats.OnHealthChanged -= OnHealthChanged;
        }
    }
}