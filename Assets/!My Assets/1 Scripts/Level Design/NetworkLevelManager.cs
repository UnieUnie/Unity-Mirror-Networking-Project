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

        if (!isServer) return;
        EnableLevel(0);
    }

    public void SwitchToLevel(int levelIndex)
    {
        if (!isServer) return;
        EnableLevel(levelIndex);
    }

    [Server]
    void EnableLevel(int levelIndex)
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            // this properly despawns stuff. fixing the switching issue
            var networkObjects = levels[currentLevelIndex].GetComponentsInChildren<NetworkIdentity>(true);
            foreach (var netObj in networkObjects)
            {
                NetworkServer.UnSpawn(netObj.gameObject);
            }
        }

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
        if (levelIndex < 0 || levelIndex >= levels.Length) return;
        if (levels[levelIndex] == null) return;

        DisableAllLevels();

        GameObject level = levels[levelIndex];
        level.SetActive(true);

        if (isServer)
        {
            // Spawn all networked objects in the new level
            var networkObjects = level.GetComponentsInChildren<NetworkIdentity>(true);
            foreach (var netObj in networkObjects)
            {
                if (!netObj.isClient && !netObj.isServer)
                {
                    NetworkServer.Spawn(netObj.gameObject);
                }
            }
        }
    }

    void DisableAllLevels()
    {
        foreach (GameObject level in levels)
        {
            if (!(level != null)) return;

            if (isServer)
            {
                // p despawn networked objects before disabling
                var networkObjects = level.GetComponentsInChildren<NetworkIdentity>(true);
                foreach (var netObj in networkObjects)
                {
                    NetworkServer.UnSpawn(netObj.gameObject);
                }
            }
            level.SetActive(false);

        }
    }
}