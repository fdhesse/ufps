using UnityEngine;

[ExecuteInEditMode]
public class DrawViewAsArc : MonoBehaviour
{
    public float shieldArea = 5;

    void Update()
    {
        shieldArea = PlayerPrefs.GetFloat("zombieVisionDistance");
    }
}
