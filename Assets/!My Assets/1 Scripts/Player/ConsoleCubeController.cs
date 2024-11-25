using Mirror;
using UnityEngine;

public class ConsoleCubeController : NetworkBehaviour
{
    [Header("Console Settings")]
    [Tooltip("Console Identifier, used to ensure the correct cube is being controlled")]
    [SerializeField] int consoleNumber = 1; // not working as intended, but it is working somehow



    [Header("Movement Settings")]
    [Tooltip("Speed of Cube Rotation")]
    [SerializeField] float movementForce = 15f;

    [Tooltip("Max Speed of Cube Rotation")]
    [SerializeField] float maxMovementForce = 5f;



    [Header("Rigidbody Settings")]
    [Tooltip("Default Rigidbody Mass (20f)")]
    [SerializeField] float rbMass = 20f;

    [Tooltip("Default Rigidbody Drag (1f)")]
    [SerializeField] float rbDrag = 1f;

    [Tooltip("Default Rigidbody Angular Drag (0.2f)")]
    [SerializeField] float rbAngularDrag = 0.2f;

    [Tooltip("Default Rigidbody Interpolation Method (Interpolate)")]
    [SerializeField] RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

    [Tooltip("Default Rigidbody Collision Detection Method (Continuous)")]
    [SerializeField] CollisionDetectionMode collisionDetection = CollisionDetectionMode.Continuous;
    [SerializeField] bool useGravity = true;

    [Header("References")]
    [Tooltip("i'll hide this from inspector after debug")]
    [SerializeField] Rigidbody cubeRb;



    bool useConsoleInput = false;
    float consoleHorizontal;
    float consoleVertical;

    [SyncVar]
    private Vector3 syncPosition;
    [SyncVar]
    private Quaternion syncRotation;

    private float lerpRate = 15f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (cubeRb != null)
        {
            syncPosition = cubeRb.position;
            syncRotation = cubeRb.rotation;
        }
    }

    void Start()
    {
        if (cubeRb != null)
        {
            InitializeRigidbody();
        }
    }

    void InitializeRigidbody()
    {
        cubeRb.useGravity = useGravity;
        cubeRb.mass = rbMass;
        cubeRb.drag = rbDrag;
        cubeRb.angularDrag = rbAngularDrag;
        cubeRb.maxAngularVelocity = maxMovementForce;
        cubeRb.interpolation = interpolation;
        cubeRb.collisionDetectionMode = collisionDetection;
    }

    void Update()
    {
        if (!isServer) return;
        if (!useConsoleInput) return;
        if (!name.Contains($"Console Model {consoleNumber}")) return;

        ApplyMovement();
    }

    void FixedUpdate()
    {
        if (isServer)
        {
            UpdateSyncVars();
        }
        else
        {
            InterpolatePosition();
        }
    }

    void UpdateSyncVars()
    {
        if (cubeRb != null)
        {
            syncPosition = cubeRb.position;
            syncRotation = cubeRb.rotation;
        }
    }

    void ApplyMovement()
    {
        if (cubeRb == null) return;

        Vector3 torqueDirection = new Vector3(consoleVertical, 0, -consoleHorizontal);
        if (torqueDirection != Vector3.zero)
        {
            cubeRb.AddTorque(torqueDirection * movementForce, ForceMode.Acceleration);
        }
    }

    public void HandleConsoleInput(float horizontal, float vertical)
    {
        if (!name.Contains($"Console Model {consoleNumber}")) return;

        CmdSetMovementInput(horizontal, vertical);
    }

    [Command(requiresAuthority = false)]
    void CmdSetMovementInput(float horizontal, float vertical)
    {
        useConsoleInput = true;
        consoleHorizontal = horizontal;
        consoleVertical = vertical;
    }

    void InterpolatePosition()
    {
        if (cubeRb == null) return;

        cubeRb.position = Vector3.Lerp(cubeRb.position, syncPosition, Time.fixedDeltaTime * lerpRate);
        cubeRb.rotation = Quaternion.Lerp(cubeRb.rotation, syncRotation, Time.fixedDeltaTime * lerpRate);
    }
}