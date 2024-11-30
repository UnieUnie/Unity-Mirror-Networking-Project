using UnityEngine;
using System.Collections.Generic;
using Mirror;
using UnityEngine.SceneManagement;

public class OverlayMenuController : NetworkBehaviour
{
    [Header("Menu Referenecs")]
    [SerializeField] Canvas menuCanvas;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject switchLevelMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject helpMenu;

    [Header("Game Settings")]
    [SerializeField] private List<MonoBehaviour> scriptsToDisable = new List<MonoBehaviour>();

    Dictionary<MenuState, GameObject> menuWindows;
    MenuState currentState = MenuState.None;

    bool isInMenu = false;
    bool originalCursorVisible;
    CursorLockMode originalLockState;

    public enum MenuState
    {
        None,
        Main,
        SwitchLevel,
        Settings,
        Help
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        InitializeMenus();
        StoreCursorState();
        AutoAddPlayerControllers();
    }

    void InitializeMenus()
    {
        menuWindows = new Dictionary<MenuState, GameObject>
        {
            { MenuState.Main, mainMenu },
            { MenuState.SwitchLevel, switchLevelMenu },
            { MenuState.Settings, settingsMenu },
            { MenuState.Help, helpMenu }
        };

        // Ensure all menus and canvas are hidden at start
        menuCanvas.enabled = false;
        foreach (var menu in menuWindows.Values)
        {
            menu.SetActive(false);
        }
    }

    void StoreCursorState()
    {
        originalCursorVisible = Cursor.visible;
        originalLockState = Cursor.lockState;
    }

    void AutoAddPlayerControllers()
    {
        foreach (var controller in FindObjectsOfType<PlayerController>())
        {
            if (!scriptsToDisable.Contains(controller))
            {
                scriptsToDisable.Add(controller);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeInput();
        }
    }

    void HandleEscapeInput()
    {
        if (!isInMenu)
        {
            OpenMenu(MenuState.Main);
        }
        else
        {
            switch (currentState)
            {
                case MenuState.Main:
                    CloseAllMenus();
                    break;
                default:
                    OpenMenu(MenuState.Main);
                    break;
            }
        }
    }

    public void OpenMenu(MenuState menuToOpen)
    {
        if (!menuWindows.ContainsKey(menuToOpen))
            return;

        // Close current menu if any open
        if (currentState != MenuState.None)
        {
            menuWindows[currentState].SetActive(false);
        }

        // Show canvas and new menu
        menuCanvas.enabled = true;
        menuWindows[menuToOpen].SetActive(true);

        // Update state and handle cursor
        currentState = menuToOpen;
        isInMenu = true;
        SetCursorState(true);
        ToggleGameScripts(false);
    }

    void CloseAllMenus()
    {
        menuCanvas.enabled = false;
        foreach (var menu in menuWindows.Values)
        {
            menu.SetActive(false);
        }

        currentState = MenuState.None;
        isInMenu = false;
        SetCursorState(false);
        ToggleGameScripts(true);
    }

    void SetCursorState(bool menuOpen)
    {
        Cursor.visible = menuOpen ? true : originalCursorVisible;
        Cursor.lockState = menuOpen ? CursorLockMode.None : originalLockState;
    }

    void ToggleGameScripts(bool enabled)
    {
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
    }

    // Public helper methods
    public void OpenMainMenu() => OpenMenu(MenuState.Main);
    public void OpenSwitchLevelMenu() => OpenMenu(MenuState.SwitchLevel);
    public void OpenSettingsMenu() => OpenMenu(MenuState.Settings);
    public void OpenHelpMenu() => OpenMenu(MenuState.Help);
    public bool IsInMenu(MenuState menuState) => currentState == menuState;
    public void QuitToMenu() => SceneManager.LoadScene("00 Main Menu");
    public void QuitGame() => Application.Quit();
}