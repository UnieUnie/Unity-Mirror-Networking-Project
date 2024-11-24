// this script is a duplicate of the netwokring player movement script
// i remoevd all networking functions for single player testing
// mainly to ensure is not netwokring thats fucking me up

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementForce = 15f;      // Movement rotation speed
    [SerializeField] float maxMovementForce = 5f;    // Max movement rotation speed
    [SerializeField] bool isMoving = false;

    [Header("Rotation Settings")]
    [SerializeField] float horizontalRotationStrength = 1f;    // Horizontal rotation speed
    [SerializeField] float maxVelocityToRotate = .01f;  // Maximum velocity before horizontal rotation is disabled
    [SerializeField] bool isRotating = false;
    [SerializeField] bool canRotate = false;
    [SerializeField] ForceMode rotationForceMode = ForceMode.VelocityChange; // ForceMode.VelocityChange ignores rbMass

    [Header("Rigidbody Settings")]
    [SerializeField] float rbMass = 1f;
    [SerializeField] float rbDrag = 0.2f;
    [SerializeField] float rbAngularDrag = 0.03f;
    [SerializeField] RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;
    [SerializeField] CollisionDetectionMode collisionDetection = CollisionDetectionMode.Continuous;
    [SerializeField] bool useGravity = true;

    [Header("Key Bindings")]
    [SerializeField] KeyCode moveForwardKey = KeyCode.W;
    [SerializeField] KeyCode moveBackwardKey = KeyCode.S;
    [SerializeField] KeyCode moveLeftKey = KeyCode.A;
    [SerializeField] KeyCode moveRightKey = KeyCode.D;
    [SerializeField] KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] KeyCode rotateRightKey = KeyCode.E;

    public Rigidbody rb;

    #region Initialization
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("initialisation callleddd");
            rb.useGravity = useGravity;
            rb.mass = rbMass;
            rb.drag = rbDrag;
            rb.angularDrag = rbAngularDrag;
            rb.maxAngularVelocity = maxMovementForce;
            rb.interpolation = interpolation;
            rb.collisionDetectionMode = collisionDetection;
        }
        else Debug.Log("rb still not existing, fuck");
    }
    #endregion

    #region Update Loop
    void Update()
    {
        HandleMovementInput();

        if (!isRotating && !isMoving && rb.velocity.magnitude < maxVelocityToRotate)
        {
            canRotate = true;
        }
        else
        {
            canRotate = false;
        }

        if (canRotate || isRotating)
        {
            HandleRotationInput();
        }

    }
    #endregion

    #region Movement Handling
    void HandleMovementInput()
    {
        float moveHorizontal = 0f;
        float moveVertical = 0f;

        if (Input.GetKey(moveLeftKey))
        {
            moveHorizontal -= 1f;
        }
        if (Input.GetKey(moveRightKey))
        {
            moveHorizontal += 1f;
        }
        if (Input.GetKey(moveForwardKey))
        {
            moveVertical += 1f;
        }
        if (Input.GetKey(moveBackwardKey))
        {
            moveVertical -= 1f;
        }

        isMoving = (moveHorizontal != 0 || moveVertical != 0);

        if (isMoving && isRotating)
        {
            StopRotation();
            return;
        }

        Vector3 torqueDirection = new Vector3(moveVertical, 0, -moveHorizontal);

        if (torqueDirection != Vector3.zero)
        {
            rb.AddTorque(torqueDirection * movementForce, ForceMode.Acceleration);
        }
    }
    #endregion

    #region Rotation Handling
    void HandleRotationInput()
    {
        if (Input.GetKey(rotateLeftKey) || Input.GetKey(rotateRightKey))
        {
            if (!isRotating)
            {
                StartRotation();
            }

            if (Input.GetKey(rotateLeftKey))
            {
                rb.AddTorque(Vector3.up * -horizontalRotationStrength, rotationForceMode);
            }
            else if (Input.GetKey(rotateRightKey))
            {
                rb.AddTorque(Vector3.up * horizontalRotationStrength, rotationForceMode);
            }
        }
        else if (isRotating && !Input.GetKey(rotateLeftKey) && !Input.GetKey(rotateRightKey))
        {
            StartCoroutine(ResetRotationAfterDelay());
        }
    }

    void StartRotation()
    {
        isRotating = true;
        canRotate = false;
    }

    void StopRotation()
    {
        isRotating = false;
    }

    IEnumerator ResetRotationAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        StopRotation();
    }
    #endregion

    void Movement(Vector3 position, Vector3 velocity, Vector3 angularVelocity)
    {
            rb.position = Vector3.Lerp(rb.position, position, 0.5f);
            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity;
    }
}
