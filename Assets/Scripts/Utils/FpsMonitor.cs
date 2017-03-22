using UnityEngine;


using System.Collections;

public class FpsMonitor : MonoBehaviour {

    public float updateInterval = 0.5f;

    private float lastInterval = 0;
    private int frames = 0;
    private float fps;
    private GUIStyle style;

    // Use this for initialization
    void Start () {
        lastInterval = Time.realtimeSinceStartup;
        style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = new Color(0, 1, 0);
    }
	
	// Update is called once per frame
	void Update () {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;
        }
	}

    void OnGUI()
    {
        GUILayout.Label(fps.ToString("f0"), style);
    }
}
