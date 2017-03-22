using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Name("Keyboard")]
	[Category("Events/Input")]
	[Description("Calls respective outputs when the defined keyboard key is pressed down, held down or released")]
	public class KeyboardEvents : EventNode, IUpdatable {

		public KeyCode keyCode = KeyCode.Space;
		private FlowOutput down;
		private FlowOutput up;
		private FlowOutput pressed;

		public override string name{
			get {return string.Format("<color=#ff5c5c>➥ Keyboard Key '{0}'</color>", keyCode.ToString() ).ToUpper();}
		}

		protected override void RegisterPorts(){
			down = AddFlowOutput("Down");
			pressed = AddFlowOutput("Pressed");
			up = AddFlowOutput("Up");
		}

		public void Update(){
			
			if (Input.GetKeyDown(keyCode))
				down.Call(new Flow(1));

			if (Input.GetKey(keyCode))
				pressed.Call(new Flow(1));
			
			if (Input.GetKeyUp(keyCode))
				up.Call(new Flow(1));
		}
	}
}