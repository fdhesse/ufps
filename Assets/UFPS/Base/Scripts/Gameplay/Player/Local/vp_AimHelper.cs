using UnityEngine;
using System.Collections.Generic;

public class vp_AimHelper : MonoBehaviour
{
    public static List<vp_AimHelper> AimHelpers = new List<vp_AimHelper>();

    void Awake()
    {
        AimHelpers.Add(this);
    }

    void OnDestroy()
    {
        AimHelpers.Remove(this);
    }
}
