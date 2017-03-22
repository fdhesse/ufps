using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.Sensing
{
    [TaskDescription("Returns success if the holder is seeing X actor in radius, otherwise failure.")]
    [HelpURL("http://www.opsive.com/")]
    [TaskCategory("Sensing")]
    [TaskIcon("Assets/Behavior Designer/Third Party/UltimateFPS/Editor/Icon.png")]

    public class CanSee : Conditional
    {
        public float fieldOfViewAngle;
        public SharedGameObject target;

        public override void OnAwake()
        {

        }

        public override TaskStatus OnUpdate()
        {
            if (withinSight(target.Value.transform, fieldOfViewAngle))
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

        public bool withinSight(Transform targetTransform, float fieldOfViewAngle)
        {
            Vector3 direction = targetTransform.position - transform.position;
            return Vector3.Angle(direction, transform.forward) < fieldOfViewAngle;
        }

    }
}
