using ParadoxNotion.Design;
using NodeCanvas.Framework;
using UnityEngine;

namespace FlowCanvas.Nodes{

	[Name("Mouse")]
	[Category("Events/Object")]
	[Description("Called when mouse based operations happen on target collider")]
	public class MouseAgentEvents : EventNode<Collider> {

		private FlowOutput enter;
		private FlowOutput over;
		private FlowOutput exit;
		private FlowOutput down;
		private FlowOutput up;
		private FlowOutput drag;

		private RaycastHit hit;

		protected override string[] GetTargetMessageEvents(){
			return new string[]{"OnMouseEnter", "OnMouseOver", "OnMouseExit", "OnMouseDown", "OnMouseUp", "OnMouseDrag"};
		}

		protected override void RegisterPorts(){
			down  = AddFlowOutput("Down");
			up    = AddFlowOutput("Up");
			enter = AddFlowOutput("Enter");
			over  = AddFlowOutput("Over");
			exit  = AddFlowOutput("Exit");
			drag  = AddFlowOutput("Drag");
			AddValueOutput<RaycastHit>("Info", ()=>{ return hit; });
		}

		void OnMouseEnter(){
			StoreHit();
			enter.Call(new Flow(1));
		}

		void OnMouseOver(){
			StoreHit();
			over.Call(new Flow(1));
		}

		void OnMouseExit(){
			StoreHit();
			exit.Call(new Flow(1));
		}

		void OnMouseDown(){
			StoreHit();
			down.Call(new Flow(1));
		}

		void OnMouseUp(){
			StoreHit();
			up.Call(new Flow(1));
		}

		void OnMouseDrag(){
			StoreHit();
			drag.Call(new Flow(1));
		}

		void StoreHit(){
			Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity);
		}
	}
}