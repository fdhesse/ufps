using UnityEngine;
using System.Collections;

//using ParadoxNotion;
using ParadoxNotion.Design;
using FlowCanvas.Nodes;
using FlowCanvas;

namespace Flowgraph
{
    [Category("Actions/Character")]
    public class OnDeath : FlowControlNode
    {
        ValueInput<GameObject> target;
        FlowOutput raised;

        public override void OnGraphStoped()
        {
            if (target != null && target.GetValue() != null)
            {
                //EventHandler.UnregisterEvent(target.GetValue(), "OnDeath", OnEvent);
                vp_GlobalEvent<Transform>.Unregister("TransmitKill", TransmitKill);
            }
        }

        //Fire output flow
        void OnEvent()
        {
            raised.Call(new Flow());
        }

        void TransmitKill(Transform targetTransform)
        {
            if (!vp_Gameplay.IsMaster) 
                return;

            if (target!=null && targetTransform && targetTransform.gameObject == target.GetValue())
            {
                OnEvent();
            }
        }

        protected override void RegisterPorts()
        {
            target = AddValueInput<GameObject>("target");
            raised = AddFlowOutput("Out");

            AddFlowInput("In", (f) =>
            {
                //Debug.LogWarning("OnDeath");
                //EventHandler.RegisterEvent(target.GetValue(), "OnDeath", OnEvent);
                vp_GlobalEvent<Transform>.Register("TransmitKill", TransmitKill);	
            });
        }
    }
}