using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{

	[Name("On Update")]
	[Category("Events/Graph")]
	[Description("Called per-frame")]
	public class UpdateEvent : EventNode, IUpdatable {

		private FlowOutput o;

		protected override void RegisterPorts(){
			o = AddFlowOutput("Out");
		}

		public void Update(){
			o.Call(new Flow(1));
		}
	}
}