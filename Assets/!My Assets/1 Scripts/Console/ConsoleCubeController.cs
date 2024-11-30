using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ConsoleCubeController : NetworkBehaviour
{
    #region Variables and References
    //----------------------------------------------------------------------------------------------------
    // Console Settings
    [Header("Console Settings")]
    [Tooltip("Used to identify console and its controlled cube")]
    [SerializeField] int consoleNumber = 1;

    //----------------------------------------------------------------------------------------------------
    // Movement Settings
    [Header("Movement Settings")]
    [Tooltip("Console cube's rotation speed (Acceleration)")]
    [SerializeField] float rotationSpeed= 20f;

    [Tooltip("Console cube's max rotation speed")]
    [SerializeField] float maxRotationSpeed = 5f;

    // Rigidbody Settings
    [Header("Rigidbody Settings")]
    [Tooltip("Rigidbody reference for the controlling console cube")]
    [SerializeField] Rigidbody cubeRb;

    [Tooltip("Rigidbody mass (Default 20).")]
    [SerializeField] float rbMass = 20f;

    [Tooltip("Rigidbody drag (Default 1).")]
    [SerializeField] float rbDrag = 1f;

    [Tooltip("Rigidbody angular drag (Default 0.2).")]
    [SerializeField] float rbAngularDrag = 0.2f;

    [Tooltip("Rigidbody interpolation method (Default Interpolate).")]
    [SerializeField] RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

    [Tooltip("Rigidbody collision detection method (Default Continuous).")]
    [SerializeField] CollisionDetectionMode collisionDetection = CollisionDetectionMode.Continuous;

    [Tooltip("Ridigbody Gravity Flag")]
    [SerializeField] bool useGravity = true;

    //----------------------------------------------------------------------------------------------------
    // Cube Control Input and Position Sync Variables
    bool useConsoleInput = false;
    float horizontalInputs;
    float verticalInputs;

    [SyncVar]
    Vector3 syncPosition;

    [SyncVar]
    Quaternion syncRotation;

    // these are used to sync the position of the cubes.
    // i dont remeber exactly why network transform or other stuff isnt used
    // but it works sooo...
    //----------------------------------------------------------------------------------------------------
    #endregion

    #region Mirror Methods
    // Sync position at server start
    public override void OnStartServer()
    {
        base.OnStartServer();
        syncPosition = cubeRb.position;
        syncRotation = cubeRb.rotation;
    }
    #endregion

    #region Unity Methods
    // Sets up rigidbody values
    void Start()
    {
        if (cubeRb != null)
        {
            cubeRb.useGravity = useGravity;
            cubeRb.mass = rbMass;
            cubeRb.drag = rbDrag;
            cubeRb.angularDrag = rbAngularDrag;
            cubeRb.maxAngularVelocity = maxRotationSpeed;
            cubeRb.interpolation = interpolation;
            cubeRb.collisionDetectionMode = collisionDetection;
        }
    }

    void FixedUpdate()
    {
        if (!isServer) return;
        syncPosition = cubeRb.position;
        syncRotation = cubeRb.rotation;

        if (!useConsoleInput) return;

        // Only control our own cube :)
        if (!name.Contains($"Console {consoleNumber} Model")) return;

        ServerSideMovement();
    }
    #endregion

    #region Movements (Server)
    //----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Move the cube when it receives inputs.
    /// </summary>
    void ServerSideMovement()
    {
        Vector3 torqueDirection = new Vector3(verticalInputs, 0, -horizontalInputs);
        if (torqueDirection == Vector3.zero) return;
        cubeRb.AddTorque(torqueDirection * rotationSpeed, ForceMode.Acceleration);
    }

    [Command(requiresAuthority = false)]
    void CmdSetMovementInput(float horizontal, float vertical)
    {
        useConsoleInput = true;
        horizontalInputs = horizontal;
        verticalInputs = vertical;
    }
    #endregion

    #region Local Movements
    //----------------------------------------------------------------------------------------------------
    /// <summary>
    /// Receive inputs from PlayerController.cs.
    /// </summary>
    /// <param name="horizontal">Input.GetAxisRaw("Horizontal")</param>
    /// <param name="vertical">Input.GetAxisRaw("Vertical")</param>
    public void PlayerInputMethod(float horizontal, float vertical)
    {
        if (!name.Contains($"Console {consoleNumber} Model")) return;

        CmdSetMovementInput(horizontal, vertical);
    }   
    #endregion
}