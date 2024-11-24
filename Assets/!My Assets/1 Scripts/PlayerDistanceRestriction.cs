using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerDistanceRestriction : NetworkBehaviour
{
    public string playerTag = "Player";

    public float minDistance = 10f;     // Shortest gap the colliders can have
    public float maxDistance = 20f;     // Widest gap the colliders can have
    public float lookAheadDistance = 5f;// How close the player can get to the border before it moves
    public float smoothingSpeed = 5f;

    public NetworkIdentity leftColliderIdentity;
    public NetworkIdentity rightColliderIdentity;

    Collider leftCollider;
    Collider rightCollider;

    List<Transform> players = new List<Transform>();

    void Start()
    {
        if (leftColliderIdentity == null || rightColliderIdentity == null)
        {
            enabled = false;
            return;
        }

        leftCollider = leftColliderIdentity.GetComponent<Collider>();
        rightCollider = rightColliderIdentity.GetComponent<Collider>();

        if (!isServer)
        {
            return;
        }

        // Start coroutine to find players (if you still need it)
        StartCoroutine(FindPlayers());
    }

    IEnumerator FindPlayers()
    {
        while (players.Count < 2)
        {
            players.Clear();
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag(playerTag);
            foreach (GameObject playerObject in playerObjects)
            {
                players.Add(playerObject.transform);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void Update()
    {
        if (!isServer || players.Count < 2)
            return;

        // i am so proud of this btw
        // i cant believe this worked after only ONE hour
        // and is super simple and straigth forward too
        PlayerDistanceRestrictionCalculations();
    }

    void PlayerDistanceRestrictionCalculations()
    {
        // Get player positions
        float player1X = players[0].position.x;
        float player2X = players[1].position.x;

        // Determine leftmost and rightmost positions
        float leftMaxPlayerX = Mathf.Min(player1X, player2X);
        float rightMaxPlayerX = Mathf.Max(player1X, player2X);

        // target positions for colliders with look-ahead
        float targetLeftColliderX = leftMaxPlayerX - lookAheadDistance;
        float targetRightColliderX = rightMaxPlayerX + lookAheadDistance;

        // Calculate target distance between colliders
        float targetPosition = targetRightColliderX - targetLeftColliderX;

        // Clamp distance between min and max distances
        float clampedDistance = Mathf.Clamp(targetPosition, minDistance, maxDistance);

        // Adjust colliders positions based on clamped distance
        float centerX = (targetLeftColliderX + targetRightColliderX) / 2f;

        float leftColliderX = centerX - clampedDistance / 2f;
        float rightColliderX = centerX + clampedDistance / 2f;

        // Set colliders target positions
        Vector3 leftTargetPosition = new Vector3(leftColliderX, leftCollider.transform.position.y, leftCollider.transform.position.z);
        Vector3 rightTargetPosition = new Vector3(rightColliderX, rightCollider.transform.position.y, rightCollider.transform.position.z);

        // Smoothly lerp colliders to target positions
        leftCollider.transform.position = Vector3.Lerp(leftCollider.transform.position, leftTargetPosition, Time.deltaTime * smoothingSpeed);
        rightCollider.transform.position = Vector3.Lerp(rightCollider.transform.position, rightTargetPosition, Time.deltaTime * smoothingSpeed);

        // Sync collider positions to clients
        RpcUpdateColliders(leftCollider.transform.position, rightCollider.transform.position);
    }

    [ClientRpc(channel = Channels.Unreliable)]
    void RpcUpdateColliders(Vector3 leftPosition, Vector3 rightPosition)
    {
        if (isServer) return;

        if (leftCollider != null && rightCollider != null)
        {
            // Smoothly move colliders to updated positions
            leftCollider.transform.position = Vector3.Lerp(leftCollider.transform.position, leftPosition, Time.deltaTime * smoothingSpeed);
            rightCollider.transform.position = Vector3.Lerp(rightCollider.transform.position, rightPosition, Time.deltaTime * smoothingSpeed);
        }
        else
        {
            Debug.LogError("Colliders are not initialized on the client.");
        }
    }
}
