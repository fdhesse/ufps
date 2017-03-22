using System.Collections;
using ParadoxNotion.Design;


namespace FlowCanvas.Nodes{

	[Name("On Enable")]
	[Category("Events/Graph")]
	[Description("Called when the Graph is enabled")]
	public class EnableEvent : EventNode {

		private FlowOutput started;

		public override void OnGraphStarted(){
			started.Call( new Flow(1) );
		}

		protected override void RegisterPorts(){
			started = AddFlowOutput("Out");
		}
	}
}