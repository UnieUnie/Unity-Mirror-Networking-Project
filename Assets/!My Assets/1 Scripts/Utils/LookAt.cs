using UnityEngine;

public class LookAt : MonoBehaviour
{
    [Header("LookAt Settings")]
    [Tooltip("LookAt Target")]
    [SerializeField] Transform target;

    [Tooltip("Which Axis Should Be Facing The Target (X,Y,Z)")]
    [SerializeField] Vector3Int rotationAxes = new Vector3Int(1, 1, 1);

    [Tooltip("LookAt Speed")]
    [SerializeField] float lookAtSpeed = 5f;

    [Tooltip("LookAt Rotation Smoothness (Lower Is Smoother)")]
    [SerializeField] float lookAtSmoothness = 0.1f;

    [Tooltip("Delay In Seconds Before LookAt Rotation Starts")]
    [SerializeField] float lookAtDelay = 0f;

    [Header("Debug")]
    [SerializeField] bool showDebugLines;

    float delayTimer;   // Count down to start LookAt
    bool canFollow;     // Flags for delay function
    Vector3 currentVelocity;
    Quaternion targetRotation;

    void Start()
    {
        delayTimer = lookAtDelay;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (!canFollow)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0)
            {
                canFollow = true;
            }
            return;
        }

        if (target == null) return;

        CalculateTargetRotation();
        ApplyRotation();
        DrawDebug();
    }

    /// <summary>
    /// Locks axes that shouldn't rotate
    /// Calculates target rotation using Quaternion.LookRotation
    /// </summary>
    void CalculateTargetRotation()
    {
        Vector3 direction = target.position - transform.position;

        // Lock axes that shouldn't rotate
        direction.x *= rotationAxes.x;
        direction.y *= rotationAxes.y;
        direction.z *= rotationAxes.z;

        if (direction == Vector3.zero) return;
        targetRotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// Applies the calculated target rotation smoothly using Mathf.SmoothDampAngle
    /// </summary>
    void ApplyRotation()
    {
        // Mathf.SmoothDampAngle uses euler angle, so converting to euler first
        Vector3 current = transform.rotation.eulerAngles;
        Vector3 target = targetRotation.eulerAngles;
        Vector3 newRotation = current;

        // Multiply the axis value by the target rotation
        newRotation.x = Mathf.SmoothDampAngle(current.x, target.x * rotationAxes.x, ref currentVelocity.x, lookAtSmoothness, lookAtSpeed);
        newRotation.y = Mathf.SmoothDampAngle(current.y, target.y * rotationAxes.y, ref currentVelocity.y, lookAtSmoothness, lookAtSpeed);
        newRotation.z = Mathf.SmoothDampAngle(current.z, target.z * rotationAxes.z, ref currentVelocity.z, lookAtSmoothness, lookAtSpeed);

        transform.rotation = Quaternion.Euler(newRotation);
    }

    void DrawDebug()
    {
        if (!showDebugLines || target == null) return;

        Debug.DrawLine(transform.position, target.position, Color.yellow);
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.blue);
    }
}