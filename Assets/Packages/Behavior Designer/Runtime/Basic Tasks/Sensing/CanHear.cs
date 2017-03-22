using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.Sensing
{
    [TaskDescription("Returns success if the holder is hearing X actor in radius, otherwise failure.")]
    [HelpURL("http://www.opsive.com/")]
    [TaskCategory("Sensing")]
    [TaskIcon("Assets/Behavior Designer/Third Party/UltimateFPS/Editor/Icon.png")]

    public class CanHear : Conditional
    {
        public LayerMask objectLayerMask;
        public float hearingRadius = 100;
        public float hearingThreshold = 0.05f;
        public SharedTransform objectHeard;

        public override void OnAwake()
        {

        }

        public override TaskStatus OnUpdate()
        {
            objectHeard.Value = CheckForNoiseWithinRange(transform, hearingThreshold, hearingRadius, objectLayerMask);
            if (objectHeard.Value != null)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

        
        
        public static Transform CheckForNoiseWithinRange(Transform transform, float linearAudibilityThreshold, float hearingRadius, LayerMask objectLayerMask)
        {
            Transform objectHeard = null;
            var hitColliders = Physics.OverlapSphere(transform.position, hearingRadius, objectLayerMask);
            if (hitColliders != null)
            {
                float maxAudibility = 0;
                AudioSource colliderAudioSource;
                for (int i = 0; i < hitColliders.Length; ++i)
                {
                    if ((colliderAudioSource = hitColliders[i].GetComponent<AudioSource>()) != null && colliderAudioSource.isPlaying)
                    {
                        var audibility = colliderAudioSource.volume / Vector3.Distance(transform.position, hitColliders[i].transform.position);
                        if (audibility > linearAudibilityThreshold)
                        {
                            if (audibility > maxAudibility)
                            {
                                maxAudibility = audibility;
                                objectHeard = hitColliders[i].transform;
                            }
                        }
                    }
                }
            }
            return objectHeard;
        }

    }
}
