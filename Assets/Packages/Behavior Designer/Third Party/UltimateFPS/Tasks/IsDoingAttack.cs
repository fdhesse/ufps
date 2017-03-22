using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks.UFPS
{
    [TaskDescription("Returns success if the agent is doing attck, otherwise failure.")]
    [HelpURL("http://www.opsive.com/")]
    [TaskCategory("UFPS")]
    [TaskIcon("Assets/Behavior Designer/Third Party/UltimateFPS/Editor/Icon.png")]
    public class IsDoingAttack : Conditional
    {
        public SharedGameObject agent;
        //public SharedGameObject agentObject;

        public override void OnAwake()
        {
            if (agent == null)
            {
                agent = this.gameObject; //vp_PlayerEventHandler   //GetComponent<vp_PlayerEventHandler>()
            }
            /*
            if (agentObject == null)
            {
                agentObject = this.gameObject;
            }
             
            if (agentObject != null)
            {
                agent = agentObject.Value.gameObject.GetComponent<vp_PlayerEventHandler>();
            }
             */
        }

        public override TaskStatus OnUpdate()
        {
            if (agent == null)
            {
                return TaskStatus.Failure;
            }
            if(agent.Value.GetComponent<vp_PlayerEventHandler>().Attack.Active)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
            //return agent.CurrentWeaponAmmoCount.Get() > 0 ? TaskStatus.Success : TaskStatus.Failure;
            return TaskStatus.Failure;
        }

        public override void OnReset()
        {
            agent = null;
        }
    }
}