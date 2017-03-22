using System.Linq;
using BehaviorDesigner.Runtime;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

public class AIZombieConeVision : Conditional
{
    public float ConeRadius;
    public float VisionDistance;

    public SharedTransform Target;

    private Transform _player;

    public override void OnAwake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public override TaskStatus OnUpdate()
    {
        if (IsPlayerInConeVision())
        {
            Target.Value = _player;
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }

    private bool IsPlayerInConeVision()
    {
        var dist = Vector3.Distance(transform.position, _player.position);
        if (dist > VisionDistance) return false;

        var angle = Vector3.Angle(transform.forward, _player.transform.position - transform.position);
        return angle < ConeRadius * .5f;
    }
}
