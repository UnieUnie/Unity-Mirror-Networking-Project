using UnityEngine;
using UnityEditor;

public class MovePivotToCenter : EditorWindow
{
    [MenuItem("Tools/Move Pivot To Center")]
    public static void ShowWindow()
    {
        GetWindow<MovePivotToCenter>("Move Pivot To Center");
    }

    private void OnGUI()
    {
        GUILayout.Label("Move Pivot to Center", EditorStyles.boldLabel);

        if (GUILayout.Button("Move Pivot of Selected Objects"))
        {
            MoveSelectedObjectPivotToCenter();
        }
    }

    private void MoveSelectedObjectPivotToCenter()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj == null) continue;

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning($"No Renderer found on {obj.name}, cannot calculate center.");
                continue;
            }

            // Calculate the center of the object's bounds
            Vector3 center = renderer.bounds.center;

            // Create a new parent object at the center
            GameObject pivotObject = new GameObject($"{obj.name}_Pivot");
            pivotObject.transform.position = center;

            // Reparent the object to the new pivot object
            Transform originalParent = obj.transform.parent;
            pivotObject.transform.SetParent(originalParent);
            obj.transform.SetParent(pivotObject.transform);

            // Adjust the object's local position to keep it in the same world position
            obj.transform.localPosition = Vector3.zero;

            // Select the new pivot object in the Editor
            Selection.activeGameObject = pivotObject;

            Debug.Log($"Pivot moved to center for {obj.name}");
        }
    }
}
