using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class MoveForward : Action
{
    public float Speed = 2;

    public override TaskStatus OnUpdate()
    {
        transform.position += transform.forward * Speed * Time.deltaTime;

        return TaskStatus.Running;
    }
}
