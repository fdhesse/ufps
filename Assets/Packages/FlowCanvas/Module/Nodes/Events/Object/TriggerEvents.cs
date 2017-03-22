using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Name("Trigger")]
	[Category("Events/Object")]
	[Description("Called when Trigger based event happen on target")]
	public class TriggerEvents : EventNode<Collider> {

		private GameObject other;
		private FlowOutput enter;
		private FlowOutput stay;
		private FlowOutput exit;


		protected override string[] GetTargetMessageEvents(){
			return new string[]{ "OnTriggerEnter", "OnTriggerStay", "OnTriggerExit" };
		}

		protected override void RegisterPorts(){
			enter = AddFlowOutput("Enter");
			stay = AddFlowOutput("Stay");
			exit = AddFlowOutput("Exit");
			AddValueOutput<GameObject>("Other", ()=> { return other; });
		}

		void OnTriggerEnter(Collider other){
			this.other = other.gameObject;
			enter.Call(new Flow(1));
		}

		void OnTriggerStay(Collider other){
			this.other = other.gameObject;
			stay.Call(new Flow(1));
		}

		void OnTriggerExit(Collider other){
			this.other = other.gameObject;
			exit.Call(new Flow(1));
		}
	}
}