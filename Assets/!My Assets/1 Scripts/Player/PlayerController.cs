using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    #region Player Movement Variables
    [Header("Player Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float mouseSensitivity = 1f;
    [SerializeField] float maxVerticalAngle = 75f;
    [SerializeField] Camera playerCamera;
    [SerializeField] CharacterController characterController;

    float verticalRotation = 0f;
    Vector3 moveDirection;
    Vector3 verticalVelocity;
    #endregion

    #region Console Control Variables
    [Header("Console Cube Settings")]
    [SerializeField] ConsoleCubeController[] consoleCubeControllers;
    [SerializeField] float raycastRange = 1.5f;

    [SyncVar]
    bool isConsole1;

    [SyncVar]
    bool isConsole2;
    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Disable camera and character controller for non-local players
        if (!isLocalPlayer)
        {
            if (playerCamera != null)
            {
                playerCamera.gameObject.SetActive(false);
            }
            if (characterController != null)
            {
                characterController.enabled = false;
            }
        }
    }

    void Start()
    {
        if (!isLocalPlayer) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure camera is enabled for local player
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            playerCamera.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (!isConsole1 && !isConsole2)
        {
            HandleMovement();
            HandleMouseLook();
        }

        HandleInputInteraction();

        if (isConsole1 || isConsole2)
        {
            HandleConsoleMovement();
        }
    }

    void HandleMovement()
    {
        if (!isLocalPlayer || characterController == null) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 straightMovement = transform.forward * vertical;
        Vector3 sideMovement = transform.right * horizontal;
        moveDirection = (straightMovement + sideMovement).normalized;

        Vector3 movement = (moveDirection * moveSpeed) + verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (!isLocalPlayer || playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation += mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(-verticalRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    #region Player Interaction/Raycasts
    void HandleInputInteraction()
    {
        if (!isLocalPlayer) return;

        // Mouse 0/1/2 inputs
        if (Input.GetMouseButtonDown(0))
            MouseAction(0);

        if (Input.GetMouseButtonDown(1))
            MouseAction(1);

        if (Input.GetMouseButtonDown(2))
            MouseAction(2);

        // Keyboard E/F inputs
        if (Input.GetKeyDown(KeyCode.E))
            KeyAction("E");

        if (Input.GetKeyDown(KeyCode.F))
            KeyAction("F");
    }

    void MouseAction(int buttonIndex)
    {
        if (!isLocalPlayer || playerCamera == null) return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"Mouse {buttonIndex} hit: {hit.transform.name}");
        }
    }

    void KeyAction(string key)
    {
        if (!isLocalPlayer || playerCamera == null) return;

        if (isConsole1 || isConsole2)
        {
            CmdSetConsoleState(false, false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, raycastRange)) return;
        if (!hit.collider.CompareTag("Console")) return;

        if (consoleCubeControllers == null || consoleCubeControllers.Length == 0)
        {
            consoleCubeControllers = hit.collider.GetComponentsInChildren<ConsoleCubeController>();
        }

        if (key == "F")
        {
            string consoleName = hit.collider.gameObject.name;
            switch (consoleName)
            {
                case "Console Model 1":
                    CmdSetConsoleState(true, false);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Debug.Log("Console 1 activated");
                    break;

                case "Console Model 2":
                    CmdSetConsoleState(false, true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Debug.Log("Console 2 activated");
                    break;
            }
        }
        else // Only E and F are detected, so this is for "E"
        {
            // E functionality hereeeeeeeee
        }
    }

    [Command]
    void CmdSetConsoleState(bool console1, bool console2)
    {
        isConsole1 = console1;
        isConsole2 = console2;
    }
    #endregion

    void HandleConsoleMovement()
    {
        if (!isLocalPlayer) return;

        float consoleHorizontal = Input.GetAxisRaw("Horizontal");
        float consoleVertical = Input.GetAxisRaw("Vertical");

        foreach (var controller in consoleCubeControllers)
        {
            if (controller != null)
            {
                controller.HandleConsoleInput(consoleHorizontal, consoleVertical);
            }
        }
    }
}