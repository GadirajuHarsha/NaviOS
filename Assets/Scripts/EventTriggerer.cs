using UnityEngine;
using UnityEngine.UI; // Required for Button, Toggle, Slider components
using TMPro; // Required for TextMeshProUGUI

/// <summary>
/// Manages the user-facing control panel for activating dynamic events
/// that influence the Cognitive Load Index (CLI) in NaviOS.
/// These events simulate real-world driving scenarios to demonstrate the adaptive UI.
/// </summary>
public class EventTriggerer : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the CognitiveLoadManager in the scene.")]
    [SerializeField] private CognitiveLoadManager cliManager;

    [Header("UI References")]
    [Tooltip("TextMeshProUGUI element to display the current CLI value and state on the control panel.")]
    [SerializeField] private TextMeshProUGUI controlPanelCliText;
    [Tooltip("Optional: Reference to the control panel's root GameObject, for toggling its visibility.")]
    [SerializeField] private GameObject controlPanelRoot;

    // --- Event-specific CLI impact values (Tune these in the Inspector) ---
    [Header("CLI Event Impact Values")]
    [SerializeField] private float highTrafficImpact = 20f; // Moderate spike
    [SerializeField] private float suddenObstacleImpact = 45f; // High/critical spike
    [SerializeField] private float complexIntersectionImpact = 25f; // Moderate spike
    [SerializeField] private float weatherConditionsImpact = 15f; // Mild-moderate spike
    [SerializeField] private float driverFatigueImpact = 30f; // Direct fatigue spike
    [SerializeField] private float externalDistractionImpact = 35f; // Moderate-high spike
    [SerializeField] private float navigationRerouteImpact = 28f; // Moderate spike

    void Awake()
    {
        // Attempt to find the CLI Manager if not assigned in Inspector
        if (cliManager == null)
        {
            cliManager = FindObjectOfType<CognitiveLoadManager>();
            if (cliManager == null)
            {
                Debug.LogError("EventTriggerer: CognitiveLoadManager not found! Please assign it or ensure one exists.", this);
                enabled = false; // Disable this script if crucial dependency is missing
            }
        }
    }

    void Update()
    {
        // Continuously update the CLI display on the control panel
        if (controlPanelCliText != null && cliManager != null)
        {
            controlPanelCliText.text = $"CLI: {cliManager.currentCLI:F1}\nState: {cliManager.GetCurrentLoadState()}";
        }
    }

    // --- Public methods for UI Buttons/Toggles to call from the Control Panel ---

    /// <summary>
    /// Activates a simulated 'High Traffic Density' scenario, increasing CLI.
    /// </summary>
    public void TriggerHighTraffic()
    {
        if (cliManager != null)
        {
            cliManager.AddToCLI(highTrafficImpact);
            Debug.Log($"Event Triggered: High Traffic Density (CLI +{highTrafficImpact})");
        }
    }

    /// <summary>
    /// Activates a simulated 'Sudden Obstacle/Brake Ahead' event, causing a significant CLI spike.
    /// </summary>
    public void TriggerSuddenObstacle()
    {
        if (cliManager != null)
        {
            cliManager.AddToCLI(suddenObstacleImpact);
            Debug.Log($"Event Triggered: Sudden Obstacle/Brake Ahead (CLI +{suddenObstacleImpact})");
        }
    }

    /// <summary>
    /// Activates a simulated 'Complex Intersection' scenario, increasing CLI.
    /// </summary>
    public void TriggerComplexIntersection()
    {
        if (cliManager != null)
        {
            cliManager.AddToCLI(complexIntersectionImpact);
            Debug.Log($"Event Triggered: Complex Intersection (CLI +{complexIntersectionImpact})");
        }
    }

    /// <summary>
    /// Activates simulated 'Weather Conditions' (e.g., rain/fog), increasing CLI.
    /// Note: Actual visual effects would be managed by a separate script.
    /// </summary>
    public void TriggerWeatherConditions()
    {
        if (cliManager != null)
        {
            cliManager.AddToCLI(weatherConditionsImpact);
            Debug.Log($"Event Triggered: Weather Conditions (CLI +{weatherConditionsImpact})");
        }
    }

    /// <summary>
    /// Activates a simulated 'Driver Fatigue' state or 'drowsiness', causing a CLI spike.
    /// Note: Your CognitiveLoadManager already accumulates fatigue over time, this is an additional spike.
    /// </summary>
    public void TriggerDriverFatigue()
    {
        if (cliManager != null)
        {
            cliManager.AddToCLI(driverFatigueImpact);
            Debug.Log($"Event Triggered: Driver Fatigue (CLI +{driverFatigueImpact})");
        }
    }

    /// <summary>
    /// Activates a simulated 'External Distraction' (e.g., incoming call/text), increasing CLI.
    /// Note: UI handling for call/text would be in DisplayAdapter or a dedicated script.
    /// </summary>
    public void TriggerExternalDistraction()
    {
        if (cliManager != null)
        {
            cliManager.AddToCLI(externalDistractionImpact);
            Debug.Log($"Event Triggered: External Distraction (CLI +{externalDistractionImpact})");
        }
    }

    /// <summary>
    /// Activates a simulated 'Navigation Reroute' event, causing a CLI spike.
    /// </summary>
    public void TriggerNavigationReroute()
    {
        if (cliManager != null)
        {
            cliManager.AddToCLI(navigationRerouteImpact);
            Debug.Log($"Event Triggered: Navigation Reroute (CLI +{navigationRerouteImpact})");
        }
    }

    /// <summary>
    /// Resets the CLI to its minimum value, useful for starting new demonstrations.
    /// </summary>
    public void ResetCLI()
    {
        if (cliManager != null)
        {
            cliManager.SetCLI(cliManager.MinCLI);
            Debug.Log("Event: CLI Reset to Minimum");
        }
    }

    /// <summary>
    /// Toggles the visibility of the control panel itself.
    /// </summary>
    public void ToggleControlPanelVisibility()
    {
        if (controlPanelRoot != null)
        {
            controlPanelRoot.SetActive(!controlPanelRoot.activeSelf);
            Debug.Log($"Control Panel Visibility Toggled: {controlPanelRoot.activeSelf}");
        }
    }
}
