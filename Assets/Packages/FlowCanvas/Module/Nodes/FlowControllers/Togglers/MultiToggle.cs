using UnityEngine;
using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{

	[Description("Whenever any input is called the current output is called as well. Calling '+' or '-' also changes the current output")]
	[Category("Flow Controllers/Togglers")]
	[ContextDefinedOutputs(typeof(int))]
	public class MultiToggle : FlowControlNode, IMultiPortNode {
		
		[SerializeField]
		private int _portCount = 4;
		private int current;

		public int portCount{
			get {return _portCount;}
			set {_portCount = value;}
		}
		
		public override string name {
			get {return base.name + " " + string.Format( "[{0}]", current.ToString() );}
		}

		public override void OnGraphStarted(){ current = 0; }
		public override void OnGraphStoped(){ current = 0; }

		protected override void RegisterPorts(){
			var outs = new List<FlowOutput>();
			for (int i = 0; i < portCount; i++){
				outs.Add( AddFlowOutput(i.ToString()) );
			}
			AddFlowInput("In", (f)=> { outs[current].Call(f); });
			AddFlowInput("+", (f)=> { current = (int)Mathf.Repeat( current + 1, portCount); outs[current].Call(f); });
			AddFlowInput("-", (f)=> { current = (int)Mathf.Repeat( current - 1, portCount); outs[current].Call(f); });
			AddValueOutput<int>("Current", ()=>{ return current; });
		}
	}
}