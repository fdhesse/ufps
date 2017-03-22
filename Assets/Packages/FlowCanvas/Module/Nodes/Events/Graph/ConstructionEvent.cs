using System.Collections;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{

	[Name("On Awake")]
	[Category("Events/Graph")]
	[Description("Called only once the first time the Graph is enabled.")]
	public class ConstructionEvent : EventNode {

		private FlowOutput once;
		private bool called = false;

		public override void OnGraphStarted(){
			if (!called){
				called = true;
				once.Call( new Flow(1) );
			}			
		}

		protected override void RegisterPorts(){
			once = AddFlowOutput("Once");
		}
	}
}