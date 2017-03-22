using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class AIZombieMoveTowardTransform : Action
{
    public float Speed = 1;
    public float CheckTime = 1;
    public float MaxRotationDelta = 1;

    public SharedTransform Target;
    
    public override TaskStatus OnUpdate()
    {
        var sqrm = Vector3.SqrMagnitude(transform.position - Target.Value.position);
        if (sqrm < 0.1f)
            return TaskStatus.Success;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Target.Value.position - transform.position), MaxRotationDelta * Time.deltaTime);
        transform.position += transform.forward * Speed * Time.deltaTime;

        return TaskStatus.Running;
    }
}