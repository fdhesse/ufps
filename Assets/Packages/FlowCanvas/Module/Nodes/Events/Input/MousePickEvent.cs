using UnityEngine;
using System.Collections;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace FlowCanvas.Nodes{

	[Name("Mouse Pick")]
	[Category("Events/Input")]
	[Description("Called when any collider is clicked with the specified mouse button. PickInfo contains the information of the raycast event")]
	public class MousePickEvent : EventNode, IUpdatable {

		public enum ButtonKeys
		{
			Left = 0,
			Right = 1,
			Middle = 2
		}

		public ButtonKeys buttonKey;
		public LayerMask mask = -1;

		private FlowOutput o;
		private RaycastHit hit;

		protected override void RegisterPorts(){
			o = AddFlowOutput("Object Picked");
			AddValueOutput<RaycastHit>("Pick Info", ()=>{ return hit; });
		}

		public void Update(){
			if (Input.GetMouseButtonDown((int)buttonKey)){
				if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask )){
					o.Call(new Flow(1));
				}
			}
		}
	}
}