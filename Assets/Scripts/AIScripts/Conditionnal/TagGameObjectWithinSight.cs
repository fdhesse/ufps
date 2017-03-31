using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class PlayerWithinSight : Conditional
{
    public float ConeAngle = 90;
    public float ConeDistance = 5;

    public float ProbabilityToAvoidTrigger = .5f;

    public string PlayerTag = "Player";

    public SharedTransform Target;

    private GameObject _player;

    private Animator _animator;
    private bool _lastTimePlayerInSight;

    public override void OnAwake()
    {
        _player = GameObject.FindGameObjectWithTag(PlayerTag);
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override TaskStatus OnUpdate()
    {
        if (IsPlayerInSight())
        {
            if (!_lastTimePlayerInSight)
            {
                _lastTimePlayerInSight = true;
                if (Random.Range(0, 1f) < ProbabilityToAvoidTrigger)
                {
                    Target.Value = null;
                    return TaskStatus.Failure;
                }
            }
            else if (Target.Value == _player.transform)
                return TaskStatus.Success;
            else
                return TaskStatus.Failure;

            Target.Value = _player.transform;

            return TaskStatus.Success;
        }

        //if player not in sight
        _lastTimePlayerInSight = false;
        Target.Value = null;
        return TaskStatus.Failure;
    }

    bool IsPlayerInSight()
    {
        if (_player == null) return false;

        var angle = Vector3.Angle(transform.forward, _player.transform.position - transform.position);
        var dst = Vector3.Distance(transform.position, _player.transform.position);

        return (dst < ConeDistance && angle < ConeAngle);
    }
}
