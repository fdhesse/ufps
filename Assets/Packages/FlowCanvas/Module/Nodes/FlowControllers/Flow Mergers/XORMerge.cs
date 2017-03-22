using System.Collections;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{

	[Name("XOR")]
	[Category("Flow Controllers/Flow Merge")]
	[Description("Calls Out when either A or B Input is called, but the other is not in the same frame.\n(A || B) && (A != B)")]
	[System.Obsolete]
	public class XORMerge : FlowControlNode {
		
		private bool a;
		private bool b;
		private FlowOutput fOut;
		protected override void RegisterPorts(){
			fOut = AddFlowOutput("Out");
			AddFlowInput("A", (f)=> { a = true; Check(f); });
			AddFlowInput("B", (f)=> { b = true; Check(f); });
		}

		void Check(Flow f){
			if ( (a || b) && (a != b) ){
				fOut.Call(f);
			}
			StartCoroutine(Reset());
		}

		IEnumerator Reset(){
			yield return null;
			a = false;
			b = false;
		}
	}
}