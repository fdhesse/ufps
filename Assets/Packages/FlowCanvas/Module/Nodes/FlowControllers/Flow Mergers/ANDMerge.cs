using UnityEngine;
using System.Collections;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{

	[Name("AND")]
	[Category("Flow Controllers/Flow Merge")]
	[Description("Calls Out when all inputs are called together in the same frame")]
	public class ANDMerge : FlowControlNode, IMultiPortNode {
		
		[SerializeField]
		private int _portCount = 2;

		private FlowOutput fOut;
		private bool[] calls;

		public int portCount{
			get {return _portCount;}
			set {_portCount = value;}
		}

		protected override void RegisterPorts(){
			calls = new bool[portCount];
			fOut = AddFlowOutput("Out");
			for (var _i = 0; _i < portCount; _i++){
				var i = _i;
				AddFlowInput(i.ToString(), (f)=> { calls[i] = true; Check(f); } );
			}
		}

		void Check(Flow f){
			StartCoroutine(Reset());
			for (var i = 0; i < calls.Length; i++){
				if (calls[i] == false){
					return;
				}
			}
			fOut.Call(f);
		}

		IEnumerator Reset(){
			yield return null;
			for (var i = 0; i < calls.Length; i++){
				calls[i] = false;
			}
		}
	}
}