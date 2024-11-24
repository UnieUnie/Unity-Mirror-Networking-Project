using UnityEngine;
using Cinemachine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// TargetGroupCameraController.cs(this) handles:
/// Adding and removing players from CM Target Group for all clients and server
/// Sync all player camera to see the same thing
/// </summary>

public class TargetGroupCameraController : NetworkBehaviour
{
    // CM Target Group
    public CinemachineTargetGroup targetGroup;  // CM Target Group
    public float targetWeight = 1f; // Target Weight for each player
    public float targetRadius = 2f; // Target Radius for each player

    // Offset
    public Vector3 followOffset = new Vector3(0, 5, -10); // Default CM follow offset value

    // CM components
    public CinemachineVirtualCamera virtualCamera; // CM VC (Public to assign in the Inspector)
    private CinemachineTransposer transposer;      // CM Transposer (retrieved from the VC)


    #region Unity Methods

    void Start()
    {
        CMSetup();
    }

    #endregion


    #region Cinemachine Setup
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
    #endregion


    #region OnStartClient() CM Player Tracking Setup

    // Ensures player tracking is set up when client starts
    public override void OnStartClient()
    {
        base.OnStartClient();

        // Find and add all players to CM target group's targets
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            AddPlayerLocally(player);
        }

        // NOT redundant, ensures when player rejoins, they are added again to the CM target group
        if (isServer)
        {
            RpcAddPlayerToCameraGroup(NetworkClient.localPlayer?.gameObject);
        }
    }
    #endregion


    #region ClientRPC Methods
    // Call Clients to Add/Remove player to CM Locally

    [ClientRpc]
    public void RpcAddPlayerToCameraGroup(GameObject player)
    {
        AddPlayerLocally(player);
    }

    [ClientRpc]
    public void RpcRemovePlayerFromCameraGroup(GameObject player)
    {
        RemovePlayerLocally(player);
    }
    #endregion


    #region Local Player Methods (Add & Remove player to CM)

    // Add player to CM locally
    public void AddPlayerLocally(GameObject player)
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

    // Remove player from CM locally
    public void RemovePlayerLocally(GameObject player)
    {
        var targets = new List<CinemachineTargetGroup.Target>(targetGroup.m_Targets);

        for (int i = targets.Count - 1; i >= 0; i--) // no one said anything about dont use foreach to removing things, i was stuck here forever
        {
            if (targets[i].target == player.transform)
            {
                targets.RemoveAt(i);
            }
        }

        // Updates target group list of players
        targetGroup.m_Targets = targets.ToArray();
    }
    #endregion
}