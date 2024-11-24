using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    TargetGroupCameraController cameraController;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // Find camera controller if null
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<TargetGroupCameraController>();

            if (cameraController == null)
            {
                Debug.LogError("TargetGroupCameraController null while adding player.");
                return;
            }
        }

        // Get player object
        GameObject playerObj = conn.identity.gameObject;

        // Add player to camera group for host and client
        cameraController.AddPlayerLocally(playerObj);
        cameraController.RpcAddPlayerToCameraGroup(playerObj);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        GameObject playerObj = conn.identity?.gameObject;

        if (cameraController == null)
        {
            cameraController = FindObjectOfType<TargetGroupCameraController>();

            if (cameraController == null)
            {
                Debug.LogError("TargetGroupCameraController null while removing player.");
                return;
            }
        }

        if (playerObj != null)
        {
            cameraController.RemovePlayerLocally(playerObj);
        }

        base.OnServerDisconnect(conn);
    }
}
