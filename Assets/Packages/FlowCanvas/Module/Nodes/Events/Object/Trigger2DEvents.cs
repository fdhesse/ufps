using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Name("Trigger2D")]
	[Category("Events/Object")]
	[Description("Called when 2D Trigger based event happen on target")]
	public class Trigger2DEvents : EventNode<Collider2D> {

		private GameObject other;
		private FlowOutput enter;
		private FlowOutput stay;
		private FlowOutput exit;

		protected override string[] GetTargetMessageEvents(){
			return new string[]{ "OnTriggerEnter2D", "OnTriggerStay2D", "OnTriggerExit2D" };
		}

		protected override void RegisterPorts(){
			enter = AddFlowOutput("Enter");
			stay = AddFlowOutput("Stay");
			exit = AddFlowOutput("Exit");
			AddValueOutput<GameObject>("Other", ()=> { return other; });
		}

		void OnTriggerEnter2D(Collider2D other){
			this.other = other.gameObject;
			enter.Call(new Flow(1));
		}

		void OnTriggerStay2D(Collider2D other){
			this.other = other.gameObject;
			stay.Call(new Flow(1));
		}

		void OnTriggerExit2D(Collider2D other){
			this.other = other.gameObject;
			exit.Call(new Flow(1));
		}
	}
}