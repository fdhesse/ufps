using System.Collections;
using ParadoxNotion.Design;


namespace FlowCanvas.Nodes{

	[Name("On Disable")]
	[Category("Events/Graph")]
	[Description("Called when the Graph is Disabled")]
	public class DisableEvent : EventNode {

		private FlowOutput stoped;

		public override void OnGraphStoped(){
			stoped.Call( new Flow(1) );
		}

		protected override void RegisterPorts(){
			stoped = AddFlowOutput("Out");
		}
	}
}