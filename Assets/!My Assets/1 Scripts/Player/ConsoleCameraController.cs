using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleCameraController : MonoBehaviour
{
    /// <summary>
    /// This script originally handled adding/removing player cubes from target group.
    /// Player cubes are no longer the directly spawned player prefab, it is instead controlled via in-game interactions.
    /// </summary>
    /*
     * 
     * [Header("Cinemachine Settings")]
    [Tooltip("CM Target Group Weight For Both Targets")]
    [SerializeField] float targetWeight = 1f;

    [Tooltip("CM Target Group Weight For Both Targets")]
    [SerializeField] float targetRadius = 2f;

    [Tooltip("CM follow offset")]
    [SerializeField] Vector3 followOffset = new Vector3(0, 5, -10);

    [Header("Cinemachine References")]
    [Tooltip("CM Target Group Reference")]
    [SerializeField] CinemachineTargetGroup targetGroup;

    [Tooltip("CM Virtual Camera Reference")]
    [SerializeField] CinemachineVirtualCamera virtualCamera;

    [Tooltip("CM Transposer Reference")]
    [SerializeField] CinemachineTransposer transposer;


    void Start()
    {
        CMSetup();

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            // Checks whether player exists in list alreayd
            var playerTransform = player.transform;
            var targets = new List<CinemachineTargetGroup.Target>(targetGroup.m_Targets);

            bool playerExists = false;

            foreach (var target in targets)
            {
                if (target.target == playerTransform)
                {
                    playerExists = true;
                    break;
                }
            }

            // Add new players to CM target group
            if (!playerExists)
            {
                CinemachineTargetGroup.Target newTarget = new CinemachineTargetGroup.Target();
                newTarget.target = playerTransform;
                newTarget.weight = targetWeight;
                newTarget.radius = targetRadius;

                targets.Add(newTarget);

                // Updates target group list of players
                targetGroup.m_Targets = targets.ToArray();
            }
        }
    }

    // Ensures VC and Transposer are assigned (THIS IS REDUNDANT)
    void CMSetup()
    {
        // Ensure VC is assigned
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        }
        else
        {
            Debug.LogError("VC Assign Failed");
        }

        // Ensure transposer is assigned
        if (virtualCamera != null)
        {
            transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        }
        else
        {
            Debug.LogError("VC Transposer Assign Failed");
        }
    }
    */
}