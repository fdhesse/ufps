using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class ZombieWithinSight : Conditional
{
    public bool GDB;

    public float ConeAngle = 90;
    public float ConeDistance = 5;

    public float ProbabilityToAvoidTrigger = .5f;

    public string ZombieTag = "Enemy";

    public SharedTransform Target;

    private Animator _animator;

    private GameObject[] _targets;
    private List<GameObject> _lastTargetsInSight = new List<GameObject>();

    public override void OnAwake()
    {
        _targets = GameObject.FindGameObjectsWithTag(ZombieTag); // todo create a pool or make sure new zombies will be registered in this array
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override TaskStatus OnUpdate()
    {
        var targetsInSight = TargetsInSight();
        var ret = TaskStatus.Failure;

        if (null != Target.Value && IsTargetInSight(Target.Value.gameObject))
        {
            _lastTargetsInSight = targetsInSight;
            return TaskStatus.Success;
        }

        foreach (var targetInSight in targetsInSight)
        {
            if (_lastTargetsInSight.Contains(targetInSight)) continue;

            if (null == Target.Value && 
                Random.Range(0, 1f) >= ProbabilityToAvoidTrigger)
            {
                Target.Value = targetInSight.transform;
                ret = TaskStatus.Success;
            }
        }

        _lastTargetsInSight = targetsInSight;

        return ret;
    }

    List<GameObject> TargetsInSight()
    {
        var currentTargetsInSight = new List<GameObject>();

        foreach (var target in _targets)
        {
            if (target == null || target.transform == transform) continue;
            if (IsTargetInSight(target))
                currentTargetsInSight.Add(target);
        }
        return currentTargetsInSight;
    }

    bool IsTargetInSight(GameObject target)
    {
        var angle = Vector3.Angle(transform.forward, target.transform.position - transform.position);

        var dst = Vector3.Distance(transform.position, target.transform.position);
        return (dst < ConeDistance && angle < ConeAngle);
    }
}
