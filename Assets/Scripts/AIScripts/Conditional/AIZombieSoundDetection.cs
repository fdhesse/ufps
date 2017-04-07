using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIZombieSoundDetection : HumanizedReactionConditional
{
    public AIZombieWithinSight AiZombieWithinSight;

    public SharedQuaternion LookRotation;
    public SharedBool UpdateRotation;

    public float DetectionRadius = 5;
    public float MaximumSoundAge = 1;

    public float ProbabilityToAvoidTrigger = .5f;

    private readonly List<KeyValuePair<float, Vector3>> _listPositions = new List<KeyValuePair<float, Vector3>>();

    private bool _soundsourceLocked;
    private Vector3 _soundsourcePositionLocked;

    public override void OnAwake()
    {
        base.OnAwake();
        vp_Shooter.FireSoundTriggered += OnFireSoundTriggered;
    }

    private void OnFireSoundTriggered(Vector3 position)
    {
        _listPositions.Add(new KeyValuePair<float, Vector3>(Time.time, position));
    }

    public override TaskStatus OnUpdate()
    {
        _listPositions.RemoveAll(kvp => Time.time - kvp.Key > MaximumSoundAge + MaxWaitTime);
        var ret = TaskStatus.Failure;

        if (!_soundsourceLocked)
            FindSoundSourceInRange(ref ret);
        else
        {
            if (waiting)
                ret = TaskStatus.Failure;
            else
            {
                if (_listPositions.Any(kvp => kvp.Value == _soundsourcePositionLocked))
                    ret = TaskStatus.Success;
                else
                {
                    _soundsourceLocked = false;
                    ret = TaskStatus.Failure;
                }
            }
        }

        return ret;
    }

    void FindSoundSourceInRange(ref TaskStatus ret)
    {
        for (int i = 0; i < _listPositions.Count; i++)
        {
            var kvp = _listPositions[i];
            if (Vector3.Distance(kvp.Value, transform.position) > DetectionRadius) continue;
            if (Random.Range(0, 1f) < ProbabilityToAvoidTrigger) continue;

            StartCoroutine(Countdown());
            ret = TaskStatus.Failure;
            //if (gameObject.name.ToLower().Contains("gdb"))
            //    Debug.Log("sound source locked");
            _soundsourceLocked = true;
            UpdateRotation.Value = true;
            _soundsourcePositionLocked = kvp.Value;
            var soundpos = kvp.Value;
            soundpos.y = transform.position.y;
            LookRotation.Value = Quaternion.LookRotation(soundpos - transform.position, transform.up);
            return;
        }
    }
}
