using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Name("Collision2D")]
	[Category("Events/Object")]
	[Description("Called when 2D Collision based events happen on target and expose collision information")]
	public class Collision2DEvents : EventNode<Collider2D> {

		private Collision2D collision;
		private FlowOutput enter;
		private FlowOutput stay;
		private FlowOutput exit;

		protected override string[] GetTargetMessageEvents(){
			return new string[]{ "OnCollisionEnter2D", "OnCollisionStay2D", "OnCollisionExit2D" };
		}

		protected override void RegisterPorts(){
			enter = AddFlowOutput("Enter");
			stay = AddFlowOutput("Stay");
			exit = AddFlowOutput("Exit");
			AddValueOutput<GameObject>("Other", ()=> { return collision.gameObject; });
			AddValueOutput<ContactPoint2D>("Contact Point", ()=> { return collision.contacts[0]; });
			AddValueOutput<Collision2D>("Collision Info", ()=> { return collision; });
		}

		void OnCollisionEnter2D(Collision2D collision){
			this.collision = collision;
			enter.Call(new Flow(1));
		}

		void OnCollisionStay2D(Collision2D collision){
			this.collision = collision;
			stay.Call(new Flow(1));
		}

		void OnCollisionExit2D(Collision2D collision){
			this.collision = collision;
			exit.Call(new Flow(1));
		}
	}
}