using UnityEngine;

/// <summary>
/// A custom class to represent a point on the car's path, including a target speed.
/// The [System.Serializable] attribute allows this class to be displayed and
/// edited in the Unity Inspector.
/// </summary>
[System.Serializable]
public class Waypoint
{
    [Tooltip("The Transform of the waypoint.")]
    public Transform transform;
    [Tooltip("The target speed the car should reach at this waypoint.")]
    public float targetSpeed;
}
