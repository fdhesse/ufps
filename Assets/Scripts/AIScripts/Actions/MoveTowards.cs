using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class MoveTowards : Action
{
    public SharedTransform Target;

    public float Speed = 2;
    public float RotationSpeed = 5;

    public float MinWaitTime = .5f;
    public float MaxWaitTime = 2f;
    private float _waitTime;

    public float ThresholdCompleteDistance = 0.1f;

    private Animator _animator;

    private float _startTime;
    private bool _waitTimeOver;

    public override void OnAwake()
    {
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override void OnStart()
    {
        _waitTimeOver = false;
        _startTime = Time.time;
        _waitTime = Random.Range(MinWaitTime, MaxWaitTime);
    }

    public override TaskStatus OnUpdate()
    {
        if (_animator.GetBool("isDead") || Target.Value == null)
        { 
            return TaskStatus.Inactive;

        }

        if (!_waitTimeOver && Time.time - _startTime < _waitTime) return TaskStatus.Running;
        _waitTimeOver = true;

        var dst = Vector3.Distance(transform.position, Target.Value.position);
        if (dst < ThresholdCompleteDistance)
        {
            _animator.SetBool("isIdle", true);
            return TaskStatus.Success;
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(Target.Value.position - transform.position), RotationSpeed * Time.deltaTime);
        transform.position += transform.forward * Speed * Time.deltaTime;

        _animator.SetBool("isIdle", false);

        return TaskStatus.Running;
    }
}
