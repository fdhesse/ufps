using BehaviorDesigner.Runtime;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;

public class AIOrientedMove : HumanizedSpeedAction
{
    public float MaxRotationDelta = 5;

    public float EdgeDistance = 1;

    public float AnimationSpeedFactor = .4f;

    public SharedQuaternion LookRotation;
    public SharedBool UpdateRotation;
    public float ThresholdRotation = 5;

    private Animator _animator;
    private NavMeshAgent _navMeshAgent;

    public override void OnAwake()
    {
        base.OnAwake();
        _animator = gameObject.GetComponentInChildren<Animator>();
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    public override void OnStart()
    {
        _animator.speed = _speed * AnimationSpeedFactor;
    }

    public override TaskStatus OnUpdate()
    {
        if (_animator.GetBool("isDead")) return TaskStatus.Failure;
        NavMeshHit nmh;
        bool colliderInFront = false;
        if (_navMeshAgent.FindClosestEdge(out nmh))
        {
            if (nmh.distance < EdgeDistance &&
                Vector3.Dot((nmh.position - transform.position).normalized, transform.forward) > 0)
            {
                _animator.SetBool("isIdle", true);
                colliderInFront = true;
            }
        }

        if (UpdateRotation.Value)
        {
            if (Quaternion.Angle(transform.rotation, LookRotation.Value) < ThresholdRotation)
                UpdateRotation.Value = false;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, LookRotation.Value, MaxRotationDelta);
        }
        if (!colliderInFront)
        {
            _animator.SetBool("isIdle", false);
            transform.position += transform.forward * _speed * Time.deltaTime;
        }

        return TaskStatus.Running;
    }
}