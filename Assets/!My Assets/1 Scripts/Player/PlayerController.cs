// ps for myself...

//  the name of the console's model in the scene is IMPORTANT
//  ConsoleCubeController.cs uses it to determine which console it is:
//  if (!name.Contains($"Console {consoleNumber} Model")) return;
//  this script (PlayerController.cs) also uses it in similar ways:
//  consoleName is assigned by raycast on object tagged with "Console" (the model / the in game object)
//  the model needs the tag "Console" and need to be called Console 1 Model or Console 2 Model!!!!!!!




using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Mirror;
using UnityEngine.Rendering.Universal;

public class PlayerController : NetworkBehaviour
{
    #region Player Movement Variables
    [Header("Player Movement Settings")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float mouseSensitivity = 1f;
    [SerializeField] float maxVerticalAngle = 75f;
    [SerializeField] Camera playerCamera;
    [SerializeField] CharacterController characterController;

    [Header("Player/Console Camera Switch Settings (URP Camera Stack)")]
    [SerializeField] Camera baseCamera;
    [SerializeField] Camera targetCamera;
    UniversalAdditionalCameraData baseCameraData;
    UniversalAdditionalCameraData targetCameraData;

    float verticalRotation = 0f;
    Vector3 moveDirection;
    Vector3 verticalVelocity;
    #endregion

    #region Console Control Variables
    [Header("Console Cube Settings")]
    [SerializeField] ConsoleCubeController[] consoleCubeControllers;
    [SerializeField] readonly float consoleRayCastRange = 1.5f;

    [SyncVar]
    [SerializeField] bool isConsole1;

    [SyncVar]
    [SerializeField] bool isConsole2;

    [Header("Console UI Settings")]
    ConsoleUIController consoleUIController;
    [SerializeField] LayerMask uiLayerMask;
    #endregion

    public override void OnStartClient()
    {
        base.OnStartClient();

        // URP Camera Stack Setup
        // spend 2 hours here instead of using cinemachine. i wanna die
        if (isLocalPlayer)
        {
            baseCamera = playerCamera;
            baseCameraData = baseCamera.GetComponent<UniversalAdditionalCameraData>();
        }

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

    // Local Player Setup (Lock & Hide Cursor, Enable Camera)
    void Start()
    {
        if (!isLocalPlayer) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure local player and its camera's enabled (Somehow this fixes things.... dont delete)
        playerCamera.enabled = true;
        playerCamera.gameObject.SetActive(true);

        baseCamera = playerCamera;
        baseCameraData = baseCamera.GetComponent<UniversalAdditionalCameraData>();
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (!isConsole1 && !isConsole2)
        {
            HandleMovement();
            HandleMouseLook();
        }

        PlayerViewInputControl();
        ConsoleControlSwitch(); // this is KeyCode.F

        if (isConsole1 || isConsole2)
        {
            HandleConsoleMovement();
            DelegateConsoleInput();
        }
    }

    #region Player Movement & Cube Movement
    void HandleMovement()
    {
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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation += mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(-verticalRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleConsoleMovement()
    {
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
    #endregion

    // 
    #region Player Interaction/Raycasts (Player View)
    void PlayerViewInputControl()
    {
        if (Input.GetKeyDown(KeyCode.E))
            KeyAction("E");
    }

    void KeyAction(string key)
    {
        // Raycast with range, from middle of the viewport.
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, consoleRayCastRange)) return;

        if (key == "E")
        {
            Debug.Log("E Key hit");
        }
    }
    #endregion

    void DelegateConsoleInput()
    {
        // Delegate for cross script methods. i dont need to expand it later on, so this should be fine
        if (Input.GetMouseButtonDown(0))
            consoleUIController.OnMouseAction.Invoke(0);

        if (Input.GetMouseButtonDown(1))
            consoleUIController.OnMouseAction.Invoke(1);

        if (Input.GetMouseButtonDown(2))
            consoleUIController.OnMouseAction.Invoke(2);
    }

    // Dedicated Methods For Switching Between Console And Player View/Control
    #region Console and Player Control/View Switch
    /// <summary>
    /// This allows player to toggle between Console Control and Player Control.
    /// </summary>
    void ConsoleControlSwitch()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        // If player is using console, then disabled it and return. If not then continue
        if (isConsole1 || isConsole2)
        {
            CmdSetConsoleState(false, false);
            ConsoleViewSwitch(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }

        // Raycast within range, cast from middle of the viewport.
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, consoleRayCastRange)) return;

        // Continue if hit object is tagged "Console"
        if (!hit.collider.CompareTag("Console")) return;

        // Assign cube controllers if null (should only happen once as player spawns with it unassigned)
        if (consoleCubeControllers == null || consoleCubeControllers.Length == 0)
        {
            consoleCubeControllers = hit.collider.GetComponentsInChildren<ConsoleCubeController>();

            if (consoleCubeControllers == null)
            {
                {
                    consoleCubeControllers = hit.collider.GetComponents<ConsoleCubeController>();
                }
            }
        }

        // Assign Console View Camera (should only happen once as player spawns with it unassigned)
        if (targetCamera == null)
        {
            targetCamera = hit.collider.GetComponentInChildren<Camera>();
            targetCameraData = targetCamera.GetComponent<UniversalAdditionalCameraData>();
            Debug.Log("Cam Found" + targetCamera);
        }

        // The actual functionality of this method
        string consoleName = hit.collider.gameObject.name;
        switch (consoleName)
        {
            case "Console 1 Model":
                CmdSetConsoleState(true, false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ConsoleViewSwitch(true);
                Debug.Log("Console 1 activated");
                break;

            case "Console 2 Model":
                CmdSetConsoleState(false, true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ConsoleViewSwitch(true);
                Debug.Log("Console 2 activated");
                break;
        }
    }

    /// <summary>
    /// Handles player camera and console display camera's transition.
    /// </summary>
    /// <param name="consoleView">True = Console Display Cam. False = player Cam.</param>
    void ConsoleViewSwitch(bool consoleView)
    {
        // adding and removing is not efficient
        // but is easy
        if (consoleUIController == null)
        {
            consoleUIController = targetCamera.GetComponent<ConsoleUIController>();
        }

        if (consoleView)
        {
            if (!baseCameraData.cameraStack.Contains(targetCamera))
            {
                baseCameraData.cameraStack.Add(targetCamera);
                consoleUIController.enabled = (true);
            }

        }
        else
        {
            baseCameraData.cameraStack.Clear();
            consoleUIController.enabled = (false);
        }
    }



    [Command]
    void CmdSetConsoleState(bool console1, bool console2)
    {
        isConsole1 = console1;
        isConsole2 = console2;
    }
    #endregion
}