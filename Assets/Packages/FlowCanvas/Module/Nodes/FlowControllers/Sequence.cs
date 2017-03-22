using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{

	[Category("Flow Controllers/Togglers")]
	[Description("Each time input is signaled, the next output in order is called. After the last output, the order loops from the start.\nReset, resets the current index to zero.")]
	[ContextDefinedOutputs(typeof(int))]
	public class Sequence : FlowControlNode, IMultiPortNode {

		[SerializeField]
		private int _portCount = 4;
		public int portCount{
			get {return _portCount;}
			set {_portCount = value;}
		}

		private int current;

		public override void OnGraphStarted(){ current = 0; }
		public override void OnGraphStoped(){ current = 0; }

		public override string name {
			get {return base.name + " " + string.Format( "[{0}]", current.ToString() );}
		}

		protected override void RegisterPorts(){
			var outs = new List<FlowOutput>();
			
			for (var i = 0; i < portCount; i++){
				outs.Add( AddFlowOutput(i.ToString()) );
			}
			
			AddFlowInput("In", (f)=> {
				outs[current].Call(f);
				current = (int)Mathf.Repeat( current + 1, portCount);
			});

			AddFlowInput("Reset", (f)=>{ current = 0; });
			AddValueOutput<int>("Current", ()=>{ return current; });
		}
	}
}