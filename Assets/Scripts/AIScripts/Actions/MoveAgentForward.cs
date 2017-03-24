using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class MoveForward : Action
{
    public float Speed = 2;

    private CollisionDetector _collisionDetector;

    private Animator _animator;

    public override void OnAwake()
    {
        _collisionDetector = gameObject.GetComponentInChildren<CollisionDetector>();
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override TaskStatus OnUpdate()
    {
        if (_animator.GetBool("isDead")) return TaskStatus.Inactive;

        if ( _collisionDetector && !_collisionDetector.Colliding)
        {
            _animator.SetBool("isIdle", false);

            transform.position += transform.forward * Speed * Time.deltaTime;
            return TaskStatus.Running;
        }
        _animator.SetBool("isIdle", true);
        return TaskStatus.Failure;
    }
}
