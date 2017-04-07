using System.Collections;
using UnityEngine;

public class HumanizedReactionConditional : ConditionalPrioritized
{
    public float MinWaitTime = .5f;
    public float MaxWaitTime = 2f;

    private float _waitTime;

    public bool waiting;

    public override void OnAwake()
    {
        _waitTime = Random.Range(MinWaitTime, MaxWaitTime);
    }

    protected IEnumerator Countdown()
    {
        waiting = true;
        //if (gameObject.name.ToLower().Contains("gdb"))
        //    Debug.Log("Waiting " + _waitTime + " sec(s)");
        yield return new WaitForSeconds(_waitTime);
        //if (gameObject.name.ToLower().Contains("gdb"))
        //    Debug.Log("Waiting DONE");
        waiting = false;
    }

}
