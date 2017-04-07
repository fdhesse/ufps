using UnityEngine;

public static class AnimatorSetBoolDebug
{
    public static void SetBoolDebug(this Animator a, string name, bool value)
    {
        Debug.LogFormat("Animator set bool {0} = {1}", name, value);
        a.SetBool(name, value);
    }
}
