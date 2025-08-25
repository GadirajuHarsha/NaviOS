using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls the autonomous movement of the main car along a predefined path
/// and communicates its driving status and speed to the CognitiveLoadManager.
/// </summary>
public class DrivingSimulator : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the CognitiveLoadManager in the scene.")]
    [SerializeField] private CognitiveLoadManager cliManager;
    [Tooltip("Reference to the DisplayAdapter in the scene.")]
    [SerializeField] private DisplayAdapter displayAdapter;

    [Header("Path Settings")]
    [Tooltip("List of Waypoints the car will follow in order, each with a target speed.")]
    [SerializeField] private List<Waypoint> waypoints;
    [Tooltip("Speed at which the car rotates to face the next waypoint.")]
    [SerializeField] private float rotationSpeed = 5f;
    [Tooltip("Distance threshold to consider a waypoint 'reached'.")]
    [SerializeField] private float waypointReachedThreshold = 0.5f;

    [Header("Driving Behavior")]
    [Tooltip("If true, the car will loop back to the start after reaching the end of the path.")]
    [SerializeField] private bool loopPath = true;
    [Tooltip("Delay in seconds before restarting the path if looping.")]
    [SerializeField] private float loopDelay = 2f;
    [Tooltip("The rate at which the car accelerates or decelerates.")]
    [SerializeField] private float accelerationRate = 2f;

    // Public property to get the current speed for the DisplayAdapter
    public float CurrentSpeed { get; private set; } = 0f;

    private int currentWaypointIndex = 0;
    private bool isDriving = false; // Internal flag to control movement
    private float currentLoopDelayTimer = 0f;

    // --- Unity Lifecycle Methods ---

    void Awake()
    {
        // Ensure CLI Manager is assigned
        if (cliManager == null)
        {
            cliManager = FindObjectOfType<CognitiveLoadManager>();
            if (cliManager == null)
            {
                Debug.LogError("DrivingSimulator: CognitiveLoadManager not found in scene!", this);
                enabled = false;
            }
        }
        // Ensure DisplayAdapter is assigned
        if (displayAdapter == null)
        {
            displayAdapter = FindObjectOfType<DisplayAdapter>();
            if (displayAdapter == null)
            {
                Debug.LogError("DrivingSimulator: DisplayAdapter not found in scene!", this);
                enabled = false;
            }
        }
    }

    void Start()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("DrivingSimulator: No waypoints assigned! Please assign waypoints in the Inspector.", this);
            enabled = false;
            return;
        }

        // Initialize car position and rotation to the first waypoint
        transform.position = waypoints[0].transform.position;
        if (waypoints.Count > 1)
        {
            Vector3 initialLookDirection = (waypoints[1].transform.position - transform.position).normalized;
            if (initialLookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(initialLookDirection);
            }
        }

        StartDriving();
    }

    void Update()
    {
        if (!isDriving)
        {
            if (loopPath && currentWaypointIndex >= waypoints.Count)
            {
                currentLoopDelayTimer -= Time.deltaTime;
                if (currentLoopDelayTimer <= 0)
                {
                    RestartPath();
                }
            }
            return;
        }

        MoveAlongPath();

        // Pass the current speed to the display adapter
        displayAdapter?.SetCarSpeed(CurrentSpeed);
    }

    // --- Core Driving Logic ---

    /// <summary>
    /// Handles the car's movement and rotation along the current path segment.
    /// </summary>
    private void MoveAlongPath()
    {
        if (currentWaypointIndex >= waypoints.Count)
        {
            Debug.Log("End of path reached by main car.");
            StopDriving();
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex].transform;
        float targetSpeed = waypoints[currentWaypointIndex].targetSpeed;

        // Smoothly accelerate or decelerate to the target speed
        CurrentSpeed = Mathf.Lerp(CurrentSpeed, targetSpeed, accelerationRate * Time.deltaTime);

        // 1. Move the car towards the target waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, CurrentSpeed * Time.deltaTime);

        // 2. Rotate the car to face the target waypoint for smooth turning
        Vector3 directionToWaypoint = targetWaypoint.position - transform.position;
        if (directionToWaypoint.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToWaypoint);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 3. Check if the current waypoint has been reached
        if (Vector3.Distance(transform.position, targetWaypoint.position) < waypointReachedThreshold)
        {
            Debug.Log($"Main car reached Waypoint {currentWaypointIndex}.");
            currentWaypointIndex++;
        }
    }

    /// <summary>
    /// Starts the car's autonomous driving.
    /// </summary>
    public void StartDriving()
    {
        if (!isDriving)
        {
            isDriving = true;
            Debug.Log("Main car started driving.");
            cliManager?.SetCarMovingStatus(true);
        }
    }

    /// <summary>
    /// Stops the car's autonomous driving.
    /// </summary>
    public void StopDriving()
    {
        if (isDriving)
        {
            isDriving = false;
            Debug.Log("Main car stopped driving.");
            cliManager?.SetCarMovingStatus(false);
            currentLoopDelayTimer = loopDelay;
        }
    }

    /// <summary>
    /// Resets the car to the start of the path and begins driving again.
    /// </summary>
    public void RestartPath()
    {
        currentWaypointIndex = 0;
        transform.position = waypoints[0].transform.position;
        if (waypoints.Count > 1)
        {
            Vector3 initialLookDirection = (waypoints[1].transform.position - transform.position).normalized;
            if (initialLookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(initialLookDirection);
            }
        }
        Debug.Log("Main car path restarted.");
        StartDriving();
    }

    // --- Editor Visualization ---

    /// <summary>
    /// Draws gizmos in the Scene view to visualize the waypoints and path.
    /// </summary>
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i].transform != null)
            {
                Gizmos.DrawWireSphere(waypoints[i].transform.position, 0.7f);

                if (i < waypoints.Count - 1 && waypoints[i + 1].transform != null)
                {
                    Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
                }
            }
        }

        if (isDriving && currentWaypointIndex < waypoints.Count && waypoints[currentWaypointIndex].transform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waypoints[currentWaypointIndex].transform.position, 0.8f);
            Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].transform.position);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 3f);
    }
}
