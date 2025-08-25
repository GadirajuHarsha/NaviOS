using UnityEngine;
using System; // For TimeSpan if needed for logging/display
using Sydewa; // Reference the namespace for LightingManager

public class CognitiveLoadManager : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the LightingManager in the scene.")]
    [SerializeField] private LightingManager lightingManager;

    [Header("Core CLI Settings")]
    [Tooltip("The current real-time Cognitive Load Index.")]
    [SerializeField] public float currentCLI;
    [Tooltip("The decay rate when it is daytime.")]
    [SerializeField] private float dayDecayRate = 5f;
    [Tooltip("The decay rate when it is nighttime.")]
    [SerializeField] private float nightDecayRate = 2f;
    [Tooltip("The maximum CLI cap during the day.")]
    [SerializeField] private float dayMaxCLI = 30f;
    [Tooltip("The maximum CLI cap at night (higher baseline).")]
    [SerializeField] private float nightMaxCLI = 45f;
    [Tooltip("The minimum possible CLI value.")]
    [SerializeField] private float _minCLI = 0f;

    // Public properties to access key values
    public float MinCLI => _minCLI;
    public float MaxCLI => IsNight() ? nightMaxCLI : dayMaxCLI; // Dynamically get the max CLI

    [Header("Cognitive Load Thresholds")]
    public float moderateThreshold = 30f;
    public float highThreshold = 60f;

    [Header("Driving Duration / Fatigue Load")]
    [SerializeField] private float drivingDurationLoadRate = 0.5f;
    [SerializeField] private float maxDrivingDurationLoad = 40f;
    [SerializeField] private float fatigueDecayRate = 0.1f;

    private float _drivingDurationLoad = 0f;
    private DateTime _driveStartTime;
    private bool _isCarMoving = false;

    public enum CognitiveLoadState
    {
        Low,
        Moderate,
        High
    }

    public delegate void OnCognitiveLoadStateChange(CognitiveLoadState newState);
    public static event OnCognitiveLoadStateChange onCognitiveLoadStateChange;

    private CognitiveLoadState _lastLoadState;

    void Awake()
    {
        // Attempt to find the LightingManager script if not assigned
        if (lightingManager == null)
        {
            lightingManager = FindObjectOfType<LightingManager>();
        }
    }

    void Start()
    {
        _lastLoadState = GetCurrentLoadState();
        onCognitiveLoadStateChange?.Invoke(_lastLoadState);
        _driveStartTime = DateTime.Now;
    }

    void Update()
    {
        // Determine the current decay rate and max cap based on time of day
        float currentDecayRate = IsNight() ? nightDecayRate : dayDecayRate;
        float currentMaxCLI = IsNight() ? nightMaxCLI : dayMaxCLI;

        // 1. General CLI Decay (for transient events)
        currentCLI -= currentDecayRate * Time.deltaTime;
        currentCLI = Mathf.Max(currentCLI, _minCLI);

        // 2. Accumulate Driving Duration Load (if car is moving)
        if (_isCarMoving)
        {
            _drivingDurationLoad += drivingDurationLoadRate * Time.deltaTime;
        }
        else
        {
            if (_drivingDurationLoad > 0)
            {
                _drivingDurationLoad -= fatigueDecayRate * Time.deltaTime;
            }
        }
        _drivingDurationLoad = Mathf.Clamp(_drivingDurationLoad, 0f, currentMaxCLI); // Clamp passive load against the dynamic cap

        // 3. Combine transient CLI with sustained fatigue load
        currentCLI = Mathf.Max(currentCLI, _drivingDurationLoad);

        // 4. No hard clamping here, so events can push CLI past the max cap

        // 5. Check for state change and notify
        CognitiveLoadState currentLoadState = GetCurrentLoadState();
        if (currentLoadState != _lastLoadState)
        {
            _lastLoadState = currentLoadState;
            onCognitiveLoadStateChange?.Invoke(currentLoadState);
            Debug.Log($"CLI State Changed to: {currentLoadState} (CLI: {currentCLI:F1})");
        }
    }

    public CognitiveLoadState GetCurrentLoadState()
    {
        if (currentCLI >= highThreshold)
        {
            return CognitiveLoadState.High;
        }
        else if (currentCLI >= moderateThreshold)
        {
            return CognitiveLoadState.Moderate;
        }
        else
        {
            return CognitiveLoadState.Low;
        }
    }

    /// <summary>
    /// Adds a transient amount to the CLI, which can exceed the passive cap.
    /// </summary>
    public void AddToCLI(float amount)
    {
        currentCLI += amount;
        Debug.Log($"CLI increased by {amount}. New CLI: {currentCLI:F1}");
        CheckAndNotifyStateChange();
    }

    /// <summary>
    /// Sets the CLI to a specific value, which can exceed the passive cap.
    /// </summary>
    public void SetCLI(float value)
    {
        currentCLI = Mathf.Max(value, _minCLI);
        Debug.Log($"CLI set to {value}. New CLI: {currentCLI:F1}");
        CheckAndNotifyStateChange();
    }

    public void SetCarMovingStatus(bool isMoving)
    {
        if (_isCarMoving != isMoving)
        {
            _isCarMoving = isMoving;
            Debug.Log($"Car Moving Status: {isMoving}");
            if (isMoving)
            {
                _driveStartTime = DateTime.Now;
            }
        }
    }

    private void CheckAndNotifyStateChange()
    {
        CognitiveLoadState currentLoadState = GetCurrentLoadState();
        if (currentLoadState != _lastLoadState)
        {
            _lastLoadState = currentLoadState;
            onCognitiveLoadStateChange?.Invoke(currentLoadState);
            Debug.Log($"CLI State Changed to: {currentLoadState} (CLI: {currentCLI:F1}) due to explicit change.");
        }
    }

    public TimeSpan GetDrivingDuration()
    {
        if (_isCarMoving)
        {
            return DateTime.Now - _driveStartTime;
        }
        return TimeSpan.Zero;
    }

    /// <summary>
    /// Checks if it is currently nighttime based on the LightingManager's time.
    /// This now uses a fixed hour range for a more reliable check.
    /// </summary>
    private bool IsNight()
    {
        if (lightingManager == null) return false;

        float currentTime = lightingManager.TimeOfDay; // Use the public TimeOfDay value
        // Night is defined as between 7 PM and 6 AM
        return currentTime >= 19f || currentTime < 6f;
    }
}
