using UnityEngine;
using TMPro; // Required for TextMeshProUGUI
using UnityEngine.UI; // Required for Image, CanvasGroup, LayoutElement
using DG.Tweening; // Make sure DOTween is imported and set up

/// <summary>
/// Manages the adaptive display of the NaviOS UI based on the Cognitive Load Index (CLI).
/// Subscribes to CLI state changes from the CognitiveLoadManager and updates UI visibility,
/// detail level, and prominence of alerts accordingly.
/// </summary>
public class DisplayAdapter : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the CognitiveLoadManager in the scene.")]
    [SerializeField] private CognitiveLoadManager cliManager;

    [Header("UI Panels")]
    [Tooltip("The main dashboard panel (e.g., speed, core vehicle status).")]
    [SerializeField] private GameObject dashboardPanel;
    [Tooltip("The LayoutElement for the dashboard panel, to control its flexible width.")]
    [SerializeField] private LayoutElement dashboardPanelLayoutElement;
    [Tooltip("The navigation display panel (map, turn-by-turn).")]
    [SerializeField] private GameObject navigationPanel;
    [Tooltip("The LayoutElement for the navigation panel, to control its flexible width.")]
    [SerializeField] private LayoutElement navigationPanelLayoutElement;
    [Tooltip("The root GameObject of the secondary info panel, which will also contain the critical alert.")]
    [SerializeField] private GameObject secondaryInfoPanelRoot; // This is the parent of both content groups
    [Tooltip("CanvasGroup for the secondary info panel's root, for overall alpha/interactivity.")]
    [SerializeField] private CanvasGroup secondaryInfoPanelCanvasGroup;
    [Tooltip("LayoutElement for the secondary info panel's root, to control its flexible width.")]
    [SerializeField] private LayoutElement secondaryInfoPanelLayoutElement;


    [Header("Dashboard Elements")]
    [Tooltip("TextMeshProUGUI for displaying current speed.")]
    [SerializeField] private TextMeshProUGUI speedText; // Placeholder for actual speed
    [Tooltip("TextMeshProUGUI for displaying the current CLI value and state for debugging.")]
    [SerializeField] private TextMeshProUGUI currentCliDebugText; // For the CLI display on the dashboard

    [Header("Navigation Elements")]
    [Tooltip("TextMeshProUGUI for displaying primary turn-by-turn instructions.")]
    [SerializeField] private TextMeshProUGUI navInstructionText;
    [Tooltip("GameObject representing the navigation map panel/area.")]
    [SerializeField] private GameObject navMapPanel;
    [Tooltip("TextMeshProUGUI for displaying secondary navigation instructions (e.g., 'Next:').")]
    [SerializeField] private TextMeshProUGUI secondaryNavInstructionText;
    [Tooltip("TextMeshProUGUI for displaying the estimated time of arrival (ETA).")]
    [SerializeField] private TextMeshProUGUI navEtaText;

    [Header("Secondary Info Content Group")]
    [Tooltip("The GameObject containing all normal secondary info elements (music, climate, weather).")]
    [SerializeField] private GameObject secondaryContentGroup;
    [Tooltip("CanvasGroup for the normal secondary info content.")]
    [SerializeField] private CanvasGroup secondaryContentCanvasGroup;
    [SerializeField] private TextMeshProUGUI musicTitleText;
    [SerializeField] private TextMeshProUGUI climateText;
    [SerializeField] private TextMeshProUGUI weatherText;
    [Tooltip("GameObject containing music control buttons (e.g., Play, Pause, Skip).")]
    [SerializeField] private GameObject musicControlsGroup; // Added for explicit control

    [Header("General UI Elements")]
    [Tooltip("TextMeshProUGUI for displaying the driver fatigue indicator. This will be visible in Moderate CLI.")]
    [SerializeField] private TextMeshProUGUI fatigueIndicatorText; // Managed for separate control

    [Header("Critical Alert Group")]
    [Tooltip("The GameObject containing the critical alert text.")]
    [SerializeField] private GameObject criticalAlertGroup;
    [Tooltip("CanvasGroup for the critical alert content.")]
    [SerializeField] private CanvasGroup criticalAlertCanvasGroup;
    [SerializeField] private TextMeshProUGUI criticalAlertText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float panelWidthDuration = 0.4f;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;
    [SerializeField] private Ease widthEase = Ease.OutQuad;
    [SerializeField] private float criticalAlertScalePulse = 1.2f; // Scale factor for pulsing
    [SerializeField] private float criticalAlertPulseDuration = 0.8f;
    
    // NEW: Flexible widths for all panels across states
    [Tooltip("Flexible width for the dashboard panel in Low CLI state.")]
    [SerializeField] private float dashboardPanelLowWidth = 1f;
    [Tooltip("Flexible width for the dashboard panel in Moderate CLI state (shrinks slightly).")]
    [SerializeField] private float dashboardPanelModerateWidth = 0.8f;
    [Tooltip("Flexible width for the dashboard panel in High CLI state (shrinks more).")]
    [SerializeField] private float dashboardPanelHighWidth = 0.6f;

    [Tooltip("Flexible width for the navigation panel in Low CLI state.")]
    [SerializeField] private float navigationPanelLowWidth = 2f;
    [Tooltip("Flexible width for the navigation panel in Moderate CLI state (expands significantly).")]
    [SerializeField] private float navigationPanelModerateWidth = 3.5f; // Increased expansion
    [Tooltip("Flexible width for the navigation panel in High CLI state (shrinks significantly).")]
    [SerializeField] private float navigationPanelHighWidth = 1.5f; // Navigation shrinks to give space to critical alert

    [Tooltip("Flexible width for the secondary panel in Low CLI state.")]
    [SerializeField] private float secondaryPanelLowWidth = 1f;
    [Tooltip("Flexible width for the secondary panel in Moderate CLI state (shrinks).")]
    [SerializeField] private float secondaryPanelModerateWidth = 0.7f;
    [Tooltip("Flexible width for the secondary panel in High CLI state (expands considerably).")]
    [SerializeField] private float secondaryPanelHighWidth = 5.0f; // Increased expansion


    private CognitiveLoadManager.CognitiveLoadState _currentLoadState;
    private Tween _criticalAlertPulseTween; // To control the pulsing animation

    // --- Unity Lifecycle Methods ---

    void Awake()
    {
        // Attempt to find the CLI Manager if not assigned in Inspector
        if (cliManager == null)
        {
            cliManager = FindObjectOfType<CognitiveLoadManager>();
            if (cliManager == null)
            {
                Debug.LogError("DisplayAdapter: CognitiveLoadManager not found! Please assign it or ensure one exists.", this);
                enabled = false; // Disable this script if crucial dependency is missing
                return;
            }
        }

        // Ensure initial states are correct for content groups
        SetGroupVisibility(secondaryContentGroup, secondaryContentCanvasGroup, true, 0f); // Alpha 0 duration for instant
        SetGroupVisibility(criticalAlertGroup, criticalAlertCanvasGroup, false, 0f); // Alpha 0 duration for instant
        
        // Set initial panel widths
        if (dashboardPanelLayoutElement != null) dashboardPanelLayoutElement.flexibleWidth = dashboardPanelLowWidth;
        if (navigationPanelLayoutElement != null) navigationPanelLayoutElement.flexibleWidth = navigationPanelLowWidth;
        if (secondaryInfoPanelLayoutElement != null) secondaryInfoPanelLayoutElement.flexibleWidth = secondaryPanelLowWidth;

        // Ensure fatigue indicator starts visible (it will be controlled by HandleLoadStateChange)
        if (fatigueIndicatorText != null) fatigueIndicatorText.gameObject.SetActive(true);

        // Initialize TextMeshPro AutoSizing for all relevant text fields, with custom settings for speed text
        SetupTextMeshProAutoSizing(speedText, 18, 180); // Make speed text 1.5x bigger
        SetupTextMeshProAutoSizing(currentCliDebugText, 12, 100); // Increased font size
        SetupTextMeshProAutoSizing(navInstructionText);
        SetupTextMeshProAutoSizing(secondaryNavInstructionText);
        SetupTextMeshProAutoSizing(navEtaText);
        SetupTextMeshProAutoSizing(musicTitleText);
        SetupTextMeshProAutoSizing(climateText);
        SetupTextMeshProAutoSizing(weatherText);
        SetupTextMeshProAutoSizing(fatigueIndicatorText);
        SetupTextMeshProAutoSizing(criticalAlertText);
    }

    void OnEnable()
    {
        // Subscribe to the cognitive load state change event
        if (cliManager != null)
        {
            CognitiveLoadManager.onCognitiveLoadStateChange += HandleLoadStateChange;
            // Immediately apply current state in case script is enabled after CLI manager is active
            HandleLoadStateChange(cliManager.GetCurrentLoadState());
        }
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or errors when script is disabled/destroyed
        if (cliManager != null)
        {
            CognitiveLoadManager.onCognitiveLoadStateChange -= HandleLoadStateChange;
        }
        _criticalAlertPulseTween?.Kill(); // Stop any ongoing tweens when disabled
    }

    void Update()
    {
        // Continuously update the CLI debug text on the dashboard
        if (currentCliDebugText != null && cliManager != null)
        {
            currentCliDebugText.text = $"CLI: {cliManager.currentCLI:F1}\nState: {cliManager.GetCurrentLoadState()}";
            // Optional: Update speed and other dynamic dashboard elements here
            // speedText.text is now handled by SetCarSpeed()
            musicTitleText.text = "Ode to Joy"; // Static for now
            climateText.text = "72°F"; // Static for now
            weatherText.text = "Clear skies for 2 hr."; // Static for now
            navEtaText.text = "ETA: 10:40 AM"; // Static for now

            // Update fatigue indicator text based on CLI (or a more complex fatigue value from CLIManager)
            if (fatigueIndicatorText != null)
            {
                // Note: The fatigue indicator's visibility is now controlled in HandleLoadStateChange
                // Its text and color are updated here regardless of its group's visibility
                if (cliManager.currentCLI >= cliManager.highThreshold * 0.8f) // Example: high fatigue near critical
                {
                    fatigueIndicatorText.text = "Fatigue: HIGH";
                    fatigueIndicatorText.color = Color.yellow; // Indicate caution
                }
                else if (cliManager.currentCLI >= cliManager.moderateThreshold * 0.8f) // Example: moderate fatigue
                {
                    fatigueIndicatorText.text = "Fatigue: MODERATE";
                    fatigueIndicatorText.color = Color.white;
                }
                else
                {
                    fatigueIndicatorText.text = "Fatigue: LOW";
                    fatigueIndicatorText.color = Color.white;
                }
            }
        }
    }

    /// <summary>
    /// Updates the speed text display with the current car speed.
    /// This is called from the DrivingSimulator script.
    /// </summary>
    public void SetCarSpeed(float speed)
    {
        if (speedText != null)
        {
            speedText.text = $"{Mathf.RoundToInt(speed)}";
        }
    }

    // --- Adaptive UI Logic ---

    /// <summary>
    /// This method is called by the CognitiveLoadManager whenever the CLI state changes.
    /// It orchestrates the UI adaptations based on the new state.
    /// </summary>
    /// <param name="newState">The new CognitiveLoadState (Low, Moderate, High).</param>
    private void HandleLoadStateChange(CognitiveLoadManager.CognitiveLoadState newState)
    {
        Debug.Log($"DisplayAdapter: Adapting UI to new CLI State: {newState}");
        _currentLoadState = newState; // Store the current state

        switch (newState)
        {
            case CognitiveLoadManager.CognitiveLoadState.Low:
                // --- Low Load: All information visible, full detail ---
                SetPanelVisibility(dashboardPanel, true);
                SetPanelVisibility(navigationPanel, true);
                
                // Panel Widths: All at their 'low' state widths
                SetPanelWidth(dashboardPanelLayoutElement, dashboardPanelLowWidth);
                SetPanelWidth(navigationPanelLayoutElement, navigationPanelLowWidth);
                SetPanelWidth(secondaryInfoPanelLayoutElement, secondaryPanelLowWidth);

                // Secondary Panel Content: Show normal content, hide critical alert
                SetGroupVisibility(secondaryContentGroup, secondaryContentCanvasGroup, true, fadeDuration);
                SetGroupVisibility(criticalAlertGroup, criticalAlertCanvasGroup, false, fadeDuration);
                StopCriticalAlertPulse();

                // Fatigue indicator: visible
                if (fatigueIndicatorText != null) fatigueIndicatorText.gameObject.SetActive(true);

                // Navigation detail: full map, full instructions
                if (navMapPanel != null) navMapPanel.SetActive(true);
                if (navInstructionText != null) navInstructionText.text = "Proceed on the path.";
                if (secondaryNavInstructionText != null) secondaryNavInstructionText.gameObject.SetActive(true);
                if (secondaryNavInstructionText != null) secondaryNavInstructionText.text = "";
                if (navEtaText != null) navEtaText.gameObject.SetActive(true);
                break;

            case CognitiveLoadManager.CognitiveLoadState.Moderate:
                // --- Moderate Load: Navigation expands, Dashboard & Secondary shrink. Specific secondary content visible. ---
                SetPanelVisibility(dashboardPanel, true);
                SetPanelVisibility(navigationPanel, true);
                
                // Panel Widths: Dashboard shrinks, Navigation expands, Secondary shrinks
                SetPanelWidth(dashboardPanelLayoutElement, dashboardPanelModerateWidth);
                SetPanelWidth(navigationPanelLayoutElement, navigationPanelModerateWidth); // Navigation expands
                SetPanelWidth(secondaryInfoPanelLayoutElement, secondaryPanelModerateWidth);

                // Secondary Panel Content: Hide most, keep fatigue & climate
                SetGroupVisibility(secondaryContentGroup, secondaryContentCanvasGroup, true, fadeDuration); // Keep group visible for individual control
                if (musicTitleText != null) musicTitleText.gameObject.SetActive(false); // Hide music title
                if (musicControlsGroup != null) musicControlsGroup.SetActive(false); // Hide music controls
                if (weatherText != null) weatherText.gameObject.SetActive(false); // Hide weather
                if (climateText != null) climateText.gameObject.SetActive(true); // Keep climate visible
                
                SetGroupVisibility(criticalAlertGroup, criticalAlertCanvasGroup, false, fadeDuration); // Ensure critical alert is hidden
                StopCriticalAlertPulse();

                // Fatigue indicator: visible and yellow (color handled in Update)
                if (fatigueIndicatorText != null) fatigueIndicatorText.gameObject.SetActive(true);
                
                // Navigation detail: map visible, but instructions more concise
                if (navMapPanel != null) navMapPanel.SetActive(true);
                if (navInstructionText != null) navInstructionText.text = "KEEP PROCEEDING"; // More concise
                if (secondaryNavInstructionText != null) secondaryNavInstructionText.gameObject.SetActive(false); // Hide secondary nav
                if (navEtaText != null) navEtaText.gameObject.SetActive(true); // Still show ETA
                break;

            case CognitiveLoadManager.CognitiveLoadState.High:
                // --- High Load / Critical: Only essential and critical info, secondary panel expands for alert ---
                SetPanelVisibility(dashboardPanel, true);
                SetPanelVisibility(navigationPanel, true);
                
                // Panel Widths: Dashboard shrinks more, Navigation shrinks, Secondary expands significantly for alert
                SetPanelWidth(dashboardPanelLayoutElement, dashboardPanelHighWidth);
                SetPanelWidth(navigationPanelLayoutElement, navigationPanelHighWidth); // Navigation shrinks
                SetPanelWidth(secondaryInfoPanelLayoutElement, secondaryPanelHighWidth); // Secondary expands

                // Secondary Panel Content: Hide normal content, show critical alert
                SetGroupVisibility(secondaryContentGroup, secondaryContentCanvasGroup, false, fadeDuration); // Hide secondary content
                SetGroupVisibility(criticalAlertGroup, criticalAlertCanvasGroup, true, fadeDuration); // Show critical alert
                StartCriticalAlertPulse(); // Start pulsing animation

                // Fatigue indicator: hidden (critical alert takes precedence)
                if (fatigueIndicatorText != null) fatigueIndicatorText.gameObject.SetActive(false);

                // Navigation detail: map hidden, ultra-concise instructions
                if (navMapPanel != null) navMapPanel.SetActive(false);
                if (navInstructionText != null) navInstructionText.text = "DO NOT TURN!"; // Ultra concise
                if (secondaryNavInstructionText != null) secondaryNavInstructionText.gameObject.SetActive(false); // Hide secondary nav
                if (navEtaText != null) navEtaText.gameObject.SetActive(false);

                if (criticalAlertText != null)
                {
                    criticalAlertText.text = "STAY ALERT!";
                    criticalAlertText.color = Color.red; // Ensure it's red
                    // To center vertically, ensure CriticalAlertContainer has a VerticalLayoutGroup
                    // and CriticalAlertText's RectTransform anchors are set to middle-center or stretch.
                }
                break;
        }
    }

    /// <summary>
    /// Helper to set visibility of a GameObject (panel).
    /// </summary>
    private void SetPanelVisibility(GameObject panel, bool isVisible)
    {
        if (panel != null)
        {
            panel.SetActive(isVisible);
        }
    }

    /// <summary>
    /// Helper to set visibility and interactivity of a CanvasGroup and its root GameObject.
    /// Uses DOTween for smooth fading.
    /// </summary>
    private void SetGroupVisibility(GameObject groupRoot, CanvasGroup canvasGroup, bool isVisible, float duration)
    {
        if (groupRoot == null || canvasGroup == null) return;

        if (isVisible)
        {
            groupRoot.SetActive(true);
            canvasGroup.DOFade(1f, duration).SetEase(fadeEase);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.DOFade(0f, duration).SetEase(fadeEase).OnComplete(() =>
            {
                groupRoot.SetActive(false); // Deactivate after fade out
            });
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Helper to smoothly change a panel's flexible width using DOTween.
    /// </summary>
    private void SetPanelWidth(LayoutElement layoutElement, float targetWidth)
    {
        if (layoutElement != null)
        {
            DOTween.To(() => layoutElement.flexibleWidth, x => layoutElement.flexibleWidth = x, targetWidth, panelWidthDuration)
                   .SetEase(widthEase);
        }
    }

    /// <summary>
    /// Starts the pulsing animation for the critical alert text.
    /// </summary>
    private void StartCriticalAlertPulse()
    {
        if (criticalAlertText == null) return;

        StopCriticalAlertPulse(); // Stop any existing pulse first

        // Scale up and down repeatedly
        _criticalAlertPulseTween = criticalAlertText.rectTransform.DOScale(criticalAlertScalePulse, criticalAlertPulseDuration / 2)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo); // Loop indefinitely, yoyo means it scales back down
    }

    /// <summary>
    /// Stops the pulsing animation for the critical alert text and resets its scale.
    /// </summary>
    private void StopCriticalAlertPulse()
    {
        if (_criticalAlertPulseTween != null && _criticalAlertPulseTween.IsActive())
        {
            _criticalAlertPulseTween.Kill(true); // Kill and complete the tween (resets to end value)
        }
        if (criticalAlertText != null)
        {
            criticalAlertText.rectTransform.localScale = Vector3.one; // Ensure scale is reset
        }
    }

    /// <summary>
    /// Helper to set up TextMeshProUGUI for dynamic font scaling.
    /// </summary>
    private void SetupTextMeshProAutoSizing(TextMeshProUGUI tmpText, int minSize = 12, int maxSize = 72)
    {
        if (tmpText != null)
        {
            tmpText.enableAutoSizing = true;
            tmpText.fontSizeMin = minSize;
            tmpText.fontSizeMax = maxSize;
        }
    }
}
