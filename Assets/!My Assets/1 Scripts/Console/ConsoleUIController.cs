using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

/// <summary>
/// im about to go coocoo, ill write this after dinner 
/// </summary>
public class ConsoleUIController : NetworkBehaviour
{
    #region Variables and references
    //----------------------------------------------------------------------------------------------------
    // Menu References
    [Header("Menu References")]
    [Tooltip("Console View Canvas")]
    [SerializeField] GameObject consoleViewMenu;
    [Tooltip("Contraption View Canvas")]
    [SerializeField] GameObject contraptionViewMenu;
    [Tooltip("Assembly View Canvas")]
    [SerializeField] GameObject assemblyViewMenu;

    int currentMenuIndex = 0; // To Track Curerent Active Menu
    GameObject[] menus; // Menu Array for Easy Swtiching

    // UI Text References
    [Header("UI Text References")]
    [Tooltip("TMP Text Displaying Current Component Type (e.g. Pressure Plates)")]
    [SerializeField] List<TextMeshProUGUI> componentTypeTexts = new();
    [Tooltip("TMP Text Displayin Current Gate Type (e.g.NAND Gate)")]
    [SerializeField] List<TextMeshProUGUI> gateTypeTexts = new();

    // Gate Switching Variables
    bool canSwitch = false;
    List<LogicGate.GateType> availableGates = new List<LogicGate.GateType>(); // Gates available To Switch
    LogicGate currentGate;  // Current active gate
    int currentIndex = 0;   // Current gate tracking index

    //----------------------------------------------------------------------------------------------------
    // Mouse Action Delegate
    [Tooltip("Mouse Click Delegates")]
    public delegate void MouseActionHandler(int buttonIndex);
    public MouseActionHandler OnMouseAction;

    // Raycast Settings
    [Header("Raycast Settings")]
    [Tooltip("Camera viewing the console.")]
    [SerializeField] Camera consoleViewCamera;
    [Tooltip("Raycast Rnage")]
    [SerializeField] float uiRayCastRange = 2f;
    [Tooltip("UI Layer")]
    [SerializeField] LayerMask uiLayerMask;
    [Tooltip("Non-UI Layer, to be excluded in UI layer bitmask")]
    [SerializeField] LayerMask ignoredLayerMask;

    //----------------------------------------------------------------------------------------------------
    // Switch View Button Settings
    [Header("Switch View Button Settings")]
    [Tooltip("Switch View Button. The one in constant view canvas.")]
    [SerializeField] GameObject switchViewButton;
    [Tooltip("Object's Scale * (1 + scaleValue)")]
    [SerializeField] float scaleValue = 0.2f;
    [Tooltip("Speed of scaling animation.")]
    [SerializeField] float scaleSpeed = 5f;

    // Switch View Button Variables
    Vector3 minScale;
    Vector3 maxScale;
    Vector3 targetScale;
    bool isLooping = false;
    bool isScalingUp = false;

    //----------------------------------------------------------------------------------------------------
    // Player Cube Reference
    [Header("Player Cube Reference")]
    [Tooltip("The cube player controls through console")]
    [SerializeField] GameObject playerCube;
    #endregion


    #region Unity Methods
    void Start()
    {
        OnMouseAction = HandleMouseAction;
        uiLayerMask = 1 << LayerMask.NameToLayer("UI");     // Add UI layer to bitmask
        ignoredLayerMask = ~LayerMask.GetMask("Default");   // Exclude Default layer from bitmask

        MenuSetup();

        SwitchButtonScaleSetup();
    }

    void Update()
    {
        // Because the script is not on the player, it needs to be delegated
        if (Input.GetMouseButtonDown(0)) OnMouseAction?.Invoke(0);
        if (Input.GetMouseButtonDown(1)) OnMouseAction?.Invoke(1);
        if (Input.GetMouseButtonDown(2)) OnMouseAction?.Invoke(2);
    }

    void FixedUpdate()
    {
        SwitchButtonScale();
    }
    #endregion


    #region OnTriggerEnter/Exit related functions
    //------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Toggle gate switching and update related text depending on the type of component it collided.
    /// </summary>
    /// <param name="other"></param>
    public void HandleTriggerEnter(Collider other)
    {
        // Continues if is puzzle components
        if (!other.CompareTag("Puzzle")) return;

        // Get the component
        PuzzleComponent component = other.GetComponent<PuzzleComponent>();
        if (component == null) return;

        // Get the component type (e.g. pressure plate)
        string componentType = $"Component: {component.GetType().Name}";

        // Try cast component to logic gate
        LogicGate gate = component as LogicGate;
        Debug.Log(gate);
        if (gate != null) // if it is a gate component, update both component and gate type. And enable switching
        {
            if (gate.canSwitch)
            {
                canSwitch = true;
            }
            currentGate = gate;
            PrepareAvailableGates();
            UpdateGateDisplay();
        }
        else // if is not a gate component, only update component type. Disable switching.
        {
            canSwitch = false;
            UpdateAllTexts(componentType, "");
        }
    }

    /// <summary>
    /// Disables gate switching and update all related text if player exited the gate's collider.
    /// </summary>
    public void HandleTriggerExit(Collider other)
    {
        LogicGate gate = other.GetComponent<LogicGate>();
        if (gate != null && gate == currentGate)
        {
            canSwitch = false;
            currentGate = null;
            UpdateAllTexts("", "");
            // just realised, stnading between multiple gates will probably break somehting.
        }
    }
    #endregion


    #region OnClick related functions
    //------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// This method NEEDS the button to be the same name...
    /// Im too lazy to change anything and it works sooo...
    /// </summary>
    void HandleMouseAction(int buttonIndex)
    {
        if (buttonIndex != 0) return; // rn only needs left click

        // Raycast from window center
        Ray ray = consoleViewCamera.ScreenPointToRay(Input.mousePosition);

        // Bitmask was made at
        if (Physics.Raycast(ray, out RaycastHit hit, uiRayCastRange, uiLayerMask & ignoredLayerMask))
        {
            string hitObjectName = hit.collider.gameObject.name;

            switch (hitObjectName)
            {
                case "Gate Switch Button Left":
                    if (canSwitch) SwitchGateLeft();
                    break;
                case "Gate Switch Button Right":
                    if (canSwitch) SwitchGateRight();
                    break;
                case "Switch View Button":
                    SwitchMenu();
                    Debug.Log("switch view called");
                    break;
            }
            Debug.Log($"Mouse Button {buttonIndex}: Hit {hit.collider.gameObject.name} at position {hit.point}");
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * uiRayCastRange, Color.red);
        }
    }
    #endregion


    #region Menu Switching Methods (Sub-menus)
    //------------------------------------------------------------------------------------------------------------------------
    void MenuSetup()
    {
        // Initialise menu
        menus = new GameObject[] { consoleViewMenu, contraptionViewMenu, assemblyViewMenu };

        foreach (var menu in menus)
        {
            if (menu != null)
                menu.SetActive(false);
        }
        consoleViewMenu.SetActive(true);
        currentMenuIndex = 0;

        // Initialise UI
        UpdateAllTexts("", "");
    }

    void SwitchMenu()
    {
        // main menu (index 0)
        if (currentMenuIndex == 0)
        {
            if (canSwitch)
            {
                currentMenuIndex = 1;
            }
            else
            {
                currentMenuIndex = 2;
            }
        }
        // If on any other menu, go back to main
        else
        {
            currentMenuIndex = 0;
        }

        // Update menu visibility
        foreach (var menu in menus)
        {
            if (menu != null)
            {
                menu.SetActive(false);
            }
        }
        if (menus[currentMenuIndex] != null)
        {
            menus[currentMenuIndex].SetActive(true);
        }
    }
    #endregion


    #region Update Component Type & Gate Type Texts (Main Menu and Sub-menu UIs)
    //------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Create two temp string holding current gate types (e.g. AND gate) and current component types (e.g. Pressure plate).
    /// Call UpdateAllTexts(); to apply these values.
    /// </summary>
    void UpdateGateDisplay()
    {
        if (currentGate == null) return;

        string componentType = $"{currentGate.GetType().Name}";
        string gateType = $"{availableGates[currentIndex]}";

        UpdateAllTexts(componentType, gateType);
    }

    /// <summary>
    /// Change all current component and gate type text to the new updated value.
    /// </summary>
    /// <param name="componentType">Set by UpdateGateDisplay(), showing the current component type(e.g. Pressure plate).</param>
    /// <param name="gateType">Set by UpdateGateDisplay(), showing the current gate types (e.g. AND gate)</param>
    void UpdateAllTexts(string componentType, string gateType)
    {
        foreach (var text in componentTypeTexts)
        {
            if (text != null)
            {
                text.text = componentType;
            }
        }

        foreach (var text in gateTypeTexts)
        {
            if (text != null)
            {
                text.text = gateType;
            }
        }
    }

    /// <summary>
    /// Reset States.
    /// currentGate to Null.
    /// canSwitch to False.
    /// componentType & gateType to empty strings.
    /// </summary>
    void ClearDisplay()
    {
        currentGate = null;
        canSwitch = false;
        UpdateAllTexts("", "");
    }
    #endregion


    #region Switch View Button Animation (Main Menu UI)
    //------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Set Minumim scale to the button's original scale.
    /// Set Maximum scale according to the scaleValue.
    /// </summary>
    void SwitchButtonScaleSetup()
    {
        minScale = switchViewButton.transform.localScale;
        maxScale = new Vector3(
            minScale.x * (1 + scaleValue),
            minScale.y * (1 + scaleValue),
            minScale.z * (1 + scaleValue)
        );
    }

    /// <summary>
    /// This scales the switch button up and down, indicating when switching is allowed.
    /// Each scale up and down is a loop. Loop only starts if switching is allowed.
    /// if switching is not allowed and loop is running, the loop will still finish.
    /// </summary>
    void SwitchButtonScale()
    {
        if (canSwitch && !isLooping)
        {
            isLooping = true;
            isScalingUp = true;
            targetScale = maxScale;
        }

        // Return if not looping
        if (!isLooping) return;

        // Standard Lerp to scale the button's scale
        switchViewButton.transform.localScale = Vector3.Lerp(switchViewButton.transform.localScale, targetScale, Time.fixedDeltaTime * scaleSpeed);

        // Create a loop by setting target value to the start value when it has reached the intended value
        if (Vector3.Distance(switchViewButton.transform.localScale, targetScale) < 0.01f)
        {
            if (isScalingUp)
            {
                isScalingUp = false;
                targetScale = minScale;
            }
            else
            {
                isLooping = false;
            }
        }
    }
    #endregion


    #region Gate Switching Methods
    //------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Setup the list used by
    /// </summary>
    void PrepareAvailableGates()
    {
        // Reset list
        availableGates.Clear();

        // Get current gate type
        LogicGate.GateType currentGateType = currentGate.gateType;

        // Add current gate type
        availableGates.Add(currentGateType);

        // Add the opposite gate type (NOT doesnt have any, thus the if has value)
        LogicGate.GateType? oppositeGateType = LogicGate.GetOppositeGateType(currentGateType);
        if (oppositeGateType.HasValue)
        {
            availableGates.Add(oppositeGateType.Value);
        }

        // reset list's index so it always starts at the current type
        currentIndex = 0;
    }

    /// <summary>
    /// Changes the current gate to a different one. Identical to right,
    /// </summary>
    void SwitchGateLeft()
    {
        if (currentGate == null || availableGates.Count <= 1) return;

        // Invert currentIndex between 1/0
        currentIndex = (currentIndex == 0) ? 1 : 0;
        ApplyNewGateType();
    }

    /// <summary>
    /// Changes the current gate to a different one. Identical to left,
    /// </summary>
    void SwitchGateRight()
    {
        if (currentGate == null || availableGates.Count <= 1) return;

        // Invert currentIndex between 1/0
        currentIndex = (currentIndex == 0) ? 1 : 0;
        ApplyNewGateType();
    }

    /// <summary>
    /// Sets the logic gate type to the new value
    /// </summary>
    void ApplyNewGateType()
    {
        if (currentGate == null || currentIndex >= availableGates.Count) return;

        LogicGate.GateType newType = availableGates[currentIndex];
        currentGate.CmdSetGateType(newType);
        UpdateGateDisplay();
    }
    #endregion
}