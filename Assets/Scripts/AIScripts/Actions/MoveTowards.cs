using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class MoveTowards : Action
{
    public SharedTransform Target;

    public float Speed = 2;
    public float RotationSpeed = 5;

    public float ThresholdCompleteDistance = 0.1f;

    public override TaskStatus OnUpdate()
    {
        var dst = Vector3.Distance(transform.position, Target.Value.position);
        if (dst < ThresholdCompleteDistance)
            return TaskStatus.Success;

        transform.rotation = Quaternion.RotateTowards(transform.rotation,
            Quaternion.LookRotation(Target.Value.position - transform.position), RotationSpeed * Time.deltaTime);
        transform.position += transform.forward * Speed * Time.deltaTime;

        return TaskStatus.Running;
    }
}
