using UnityEngine;
[ExecuteInEditMode]
public class MapVisualizer : MonoBehaviour
{
    
    public bool drawGizmos = true;

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        // Draw main platform
        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(40, 0, 0), new Vector3(80, 1, 1));

        // Draw upper platforms
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(new Vector3(15, 7, 0), new Vector3(20, 1, 1));
        Gizmos.DrawCube(new Vector3(40, 7, 0), new Vector3(20, 1, 1));
        Gizmos.DrawCube(new Vector3(65, 7, 0), new Vector3(20, 1, 1));

        // Draw switches
        Gizmos.color = Color.red;
        DrawSwitch(new Vector3(11.5f, 1.5f, 0), "A");

        Gizmos.color = Color.blue;
        DrawSwitch(new Vector3(21.5f, 1.5f, 0), "B");

        // Draw doors
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        DrawDoor(new Vector3(16.5f, 1.5f, 0), new Vector3(3, 3, 1));

        // Labels
#if UNITY_EDITOR
        UnityEditor.Handles.Label(new Vector3(4, 2, 0), "START");
        UnityEditor.Handles.Label(new Vector3(74, 2, 0), "GOAL");
#endif
    }

    void DrawSwitch(Vector3 pos, string label)
    {
        Gizmos.DrawCube(pos, Vector3.one * 2);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(pos + Vector3.up * 2, label);
#endif
    }

    void DrawDoor(Vector3 pos, Vector3 size)
    {
        Gizmos.DrawWireCube(pos, size);
    }
}