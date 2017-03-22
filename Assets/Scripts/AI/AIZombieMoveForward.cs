using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

public class AIZombieMoveForward : Action
{
    public float Speed = 1;
    public float CheckTime = 1;

    private float _lastStart;

    private bool _collision;

    public override void OnStart()
    {
        _lastStart = Time.time;
        _collision = false;
    }

    new void OnTriggerEnter(Collider c)
    {
        _collision = true;
        Debug.Log("TriggerEnter");
    }

    new void OnTriggerExit(Collider c)
    {
        _collision = false;
        Debug.Log("TriggerExit");
    }

    public override void OnTriggerStay(Collider other)
    {
        Debug.Log("base.OnTriggerStay(other)");
    }

    public override TaskStatus OnUpdate()
    {
        //if (_collision) return TaskStatus.Failure;
        if (Time.time - _lastStart > CheckTime) return TaskStatus.Success;

        transform.position += transform.forward * Speed * Time.deltaTime;

        return TaskStatus.Running;
    }
}