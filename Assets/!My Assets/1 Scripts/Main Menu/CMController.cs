using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMController : MonoBehaviour
{
    [Header("VC References")]
    [Tooltip("All VC Camera")]
    [SerializeField] CinemachineVirtualCamera[] cameras;

    [Tooltip("Main Menu VC")]
    [SerializeField] CinemachineVirtualCamera mainMenuCamera;

    [Tooltip("Host/Join Menu VC")]
    [SerializeField] CinemachineVirtualCamera hostJoinCamera;

    [Tooltip("Settings Menu VC")]
    [SerializeField] CinemachineVirtualCamera settingsCamera;

    [Tooltip("Credit Page VC")]
    [SerializeField] CinemachineVirtualCamera creditCamera;


    [Header("Canvas References")]
    [Tooltip("All Menu Canvases")]
    [SerializeField] Canvas[] menuOverlays;

    [Tooltip("Main Menu VC")]
    [SerializeField] Canvas mainMenuCanvas;

    //[Tooltip("Host/Join Menu VC")]
    //[SerializeField] Canvas hostJoinCanvas;

    [Tooltip("Settings Menu VC")]
    [SerializeField] Canvas settingsCanvas;

    [Tooltip("Credit Page VC")]
    [SerializeField] Canvas creditCanvas;


    [Header("Settings")]
    [Tooltip("Delay Before Enabling The Specified Canvas (To Counter Transition Time)")]
    [SerializeField] float canvasTransitionDelay = 1.2f;

    public enum MenuState
    {
        MainMenu,
        Settings,
        HostJoin,
        Credits
    }




    void Start()
    {
        SwitchMenuTo(MenuState.MainMenu);
    }

    // this is solely used to go back to main menu (Menu scene)
    // didnt find a good place to put a back button, sooo...
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchMenuTo(MenuState.MainMenu);
        }
    }


    #region TMP Button Methods (Switch Camera)
    public void ToMainMenu()
    {
        Debug.Log("sss");
        SwitchMenuTo(MenuState.MainMenu);
    }

    public void ToSettings()
    {
        Debug.Log("ssssss");
        SwitchMenuTo(MenuState.Settings);
    }

    public void ToHostJoin()
    {
        SwitchMenuTo(MenuState.HostJoin);
    }

    public void ToCredits()
    {
        SwitchMenuTo(MenuState.Credits);
    }

    public void Quit()
    {
        Application.Quit();
    }
    #endregion

    /// <summary>
    /// Switch Camera and Canvas Method
    /// </summary>
    /// <param name="state">The State To Switch Active To</param>
    void SwitchMenuTo(MenuState state)
    {
        // Disable all canvases
        foreach (var canvas in menuOverlays)
        {
            canvas.enabled = false;
        }

        switch (state)
        {
            case MenuState.MainMenu:
                SetActiveCam(mainMenuCamera);
                StartCoroutine(DelayedCanvasSwitch(mainMenuCanvas));
                break;
            case MenuState.Settings:
                SetActiveCam(settingsCamera);
                StartCoroutine(DelayedCanvasSwitch(settingsCanvas));
                break;
            case MenuState.HostJoin:
                SetActiveCam(hostJoinCamera);
                //StartCoroutine(DelayedCanvasSwitch(hostJoinCanvas));
                break;
            case MenuState.Credits:
                SetActiveCam(creditCamera);
                StartCoroutine(DelayedCanvasSwitch(creditCanvas));
                break;
        }
    }

    /// <summary>
    /// Sets Specified Cam to Priority 20, Sets All Other Cams to Priority 10.
    /// </summary>
    /// <param name="activeCam">The VC to be set active (Priority 20)</param>
    void SetActiveCam(CinemachineVirtualCamera activeCam)
    {
        foreach (var cam in cameras)
        {
            cam.Priority = (cam == activeCam) ? 20 : 10;
        }
    }

    /// <summary>
    /// Enables The Specified Canvas And Disables All Others
    /// </summary>
    /// <param name="activeCanvas">The canvas to be set active (Enabled)</param>
    void SetActiveCanvas(Canvas activeCanvas)
    {
        foreach (var canvas in menuOverlays)
        {
            canvas.enabled = (canvas == activeCanvas);
        }
    }

    /// <summary>
    /// Coroutine To Delay Canvas Enabling
    /// </summary>
    IEnumerator DelayedCanvasSwitch(Canvas activeCanvas)
    {
        yield return new WaitForSeconds(canvasTransitionDelay);
        SetActiveCanvas(activeCanvas);
    }
}