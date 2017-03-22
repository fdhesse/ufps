using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DrawViewAsArc))]
public class DrawViewAsArcEditor : Editor {

    public float arrowSize = 1;

    void OnSceneGUI()
    {
        DrawViewAsArc t = target as DrawViewAsArc;

        Handles.color = Color.blue;
        Handles.Label(t.transform.position + Vector3.up * 2,
                             t.transform.position.ToString() + "\nShieldArea: " +
                             t.shieldArea.ToString());

        /*
        Handles.BeginGUI();
        GUILayout.BeginArea(new Rect(Screen.width - 100, Screen.height - 80, 90, 50));
        if (GUILayout.Button("Reset Area"))
            t.shieldArea = 5;
        GUILayout.EndArea();
        Handles.EndGUI();
        */

        Handles.color = new Color(1, 0, 0, 0.2f);
        Handles.DrawSolidArc(t.transform.position, t.transform.up, -t.transform.right,
                                180, t.shieldArea);

        Handles.color = Color.red;
        t.shieldArea = Handles.ScaleValueHandle(t.shieldArea,
                        t.transform.position + t.transform.forward * t.shieldArea,
                        t.transform.rotation, 1, Handles.ConeCap, 1);
    }
}
