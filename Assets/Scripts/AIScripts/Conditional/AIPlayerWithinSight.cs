using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class AIPlayerWithinSight : HumanizedReactionConditional
{
    public float ConeAngle = 90;
    public float ConeDistance = 5;

    public float ProbabilityToAvoidTrigger = .5f;

    public SharedTransform Target;

    private GameObject _player;

    private Animator _animator;
    private bool _lastTimePlayerInSight;

    public override void OnAwake()
    {
        base.OnAwake();

        _player = GameObject.FindGameObjectWithTag("Player");
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
                _animator.SetBool("isIdle", true); //previous action should take care of this
                StartCoroutine(Countdown());
            }

            if (waiting)
                return TaskStatus.Failure;

            if (IsPlayerInSight())
            {
                Target.Value = _player.transform;
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }

        //if player not in sight
        _lastTimePlayerInSight = false;
        if (Target.Value == _player)
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
