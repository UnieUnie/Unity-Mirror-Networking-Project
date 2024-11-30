using UnityEngine;
using Mirror;
using TMPro;

public class NetworkLevelManager : NetworkBehaviour
{
    [Header("Level Setup")]
    [SerializeField] Transform levelParent;
    [SerializeField] GameObject[] levels = new GameObject[6];

    [SyncVar(hook = nameof(OnLevelIndexChanged))]
    int currentLevelIndex = -1;

    public override void OnStartServer()
    {
        base.OnStartServer();
        DisableAllLevels();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer)  // Only server should enable initial level
        {
            EnableLevel(0);
        }
    }

    // Called by other scripts to switch levels
    public void SwitchToLevel(int levelIndex)
    {
        if (!isServer) return;  // enable on server
        EnableLevel(levelIndex);
    }

    [Server]
    void EnableLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        ProcessLevelChange(levelIndex);
    }

    void OnLevelIndexChanged(int oldIndex, int newIndex)
    {
        if (!isServer)
        {
            ProcessLevelChange(newIndex);
        }
    }

    void ProcessLevelChange(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            return;
        }

        if (levels[levelIndex] == null)
        {
            return;
        }

        DisableAllLevels();

        GameObject level = levels[levelIndex];

        // Enable the level itself
        level.SetActive(true);
    }

    void DisableAllLevels()
    {
        foreach (GameObject level in levels)
        {
            if (level != null)
            {
                level.SetActive(false);
            }
        }
    }
}