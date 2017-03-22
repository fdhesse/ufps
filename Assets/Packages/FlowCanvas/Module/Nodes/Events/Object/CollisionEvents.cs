using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Name("Collision")]
	[Category("Events/Object")]
	[Description("Called when Collision based events happen on target and expose collision information")]
	public class CollisionEvents : EventNode<Collider> {

		private Collision collision;
		private FlowOutput enter;
		private FlowOutput stay;
		private FlowOutput exit;


		protected override string[] GetTargetMessageEvents(){
			return new string[]{ "OnCollisionEnter", "OnCollisionStay", "OnCollisionExit" };
		}

		protected override void RegisterPorts(){
			enter = AddFlowOutput("Enter");
			stay = AddFlowOutput("Stay");
			exit = AddFlowOutput("Exit");
			AddValueOutput<GameObject>("Other", ()=> { return collision.gameObject; });
			AddValueOutput<ContactPoint>("Contact Point", ()=> { return collision.contacts[0]; });
			AddValueOutput<Collision>("Collision Info", ()=> { return collision; });
		}

		void OnCollisionEnter(Collision collision){
			this.collision = collision;
			enter.Call(new Flow(1));
		}

		void OnCollisionStay(Collision collision){
			this.collision = collision;
			stay.Call(new Flow(1));
		}

		void OnCollisionExit(Collision collision){
			this.collision = collision;
			exit.Call(new Flow(1));
		}
	}
}