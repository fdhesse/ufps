using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class TagGameObjectWithinSight : Conditional
{
    public float ConeAngle = 90;
    public float ConeDistance = 5;

    public string TargetTag;
    //public bool EnableGrabTarget;
    //public float GrabTargetDistance = .25f;

    public SharedTransform Target;

    private GameObject[] _targets;

    private Animator _animator;
    //private float _lastGrabTime;

    public override void OnAwake()
    {
        _targets = GameObject.FindGameObjectsWithTag(TargetTag);
        _animator = gameObject.GetComponentInChildren<Animator>();
    }

    public override TaskStatus OnUpdate()
    {
        foreach (var target in _targets)
        {
            if (target == null || target.transform == transform) continue;

            var angle = Vector3.Angle(transform.forward, target.transform.position - transform.position);

            var dst = Vector3.Distance(transform.position, target.transform.position);
            if (dst > ConeDistance || angle > ConeAngle)
                continue;
            
            Target.Value = target.transform;

            //Debug.Log(dst + " "+ EnableGrabTarget + " " + _lastGrabTime);
            //if (EnableGrabTarget)
            //{
            //    if (dst < GrabTargetDistance && Time.time - _lastGrabTime > 2f)
            //    {
            //        _lastGrabTime = Time.time;
            //        _animator.SetBool("isGrabbing", true);
            //    }
            //    else
            //    {
            //        _animator.SetBool("isGrabbing", false);
            //    }
            //}

            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}
