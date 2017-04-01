using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

public class AIZombieMoveForward : Action
{
    public float Speed = 1;

    public float DistanceThreshold = 1;

    private Animator _animator;
    private CollisionDetector _collisionDetector;

    public override void OnStart()
    {
        _animator = gameObject.GetComponentInChildren<Animator>();
        _collisionDetector = gameObject.GetComponent<CollisionDetector>();
    }

    public override TaskStatus OnUpdate()
    {
        if (_animator.GetBool("isDead")) return TaskStatus.Inactive;
        if (_collisionDetector.Colliding)
        {
            _animator.SetBool("isIdle", true);
            return TaskStatus.Success;
        }

        _animator.SetBool("isIdle", false);

        transform.position += transform.forward * Speed * Time.deltaTime;

        return TaskStatus.Running;
    }
}