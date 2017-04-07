using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public abstract class HumanizedSpeedAction : Action
{
    public float MinSpeed = 2;
    public float MaxSpeed = 3;

    protected float _speed;

    public override void OnAwake()
    {
        _speed = Random.Range(MinSpeed, MaxSpeed);
    }
}
