using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float maxVerticalAngle = 80f;

    [Header("References")]
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform cameraPivot;

    float verticalRotation = 0f;
    Vector3 moveDirection;

    // Cached components
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Enable camera for local player only
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
        }

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        HandleMovementInput();
        HandleMouseLook();
        HandleActionInput();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        // Apply movement
        Vector3 movement = transform.TransformDirection(moveDirection) * moveSpeed;
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }

    private void HandleMovementInput()
    {
        // Get input axes
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player (horizontal)
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera (vertical)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleActionInput()
    {
        // Mouse buttons
        if (Input.GetMouseButtonDown(0)) CmdMouseAction(0);
        if (Input.GetMouseButtonDown(1)) CmdMouseAction(1);
        if (Input.GetMouseButtonDown(2)) CmdMouseAction(2);

        // Keyboard actions
        if (Input.GetKeyDown(KeyCode.E)) CmdKeyAction("E");
        if (Input.GetKeyDown(KeyCode.F)) CmdKeyAction("F");
    }

    [Command]
    private void CmdMouseAction(int buttonIndex)
    {
        // Example raycast implementation
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Handle the raycast hit
            Debug.Log($"Mouse {buttonIndex} hit: {hit.transform.name}");
            // You can add your custom logic here
        }
    }

    [Command]
    private void CmdKeyAction(string key)
    {
        // Example implementation
        Debug.Log($"Key {key} pressed");
        // You can add your custom logic here
    }

    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}