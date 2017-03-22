using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;

namespace FlowCanvas.Nodes{

	[Name("On Fixed Update")]
	[Category("Events/Graph")]
	[Description("Called every fixed framerate frame, which should be used when dealing with Physics")]
	public class FixedUpdateEvent : EventNode {

		private FlowOutput o;

		protected override void RegisterPorts(){
			o = AddFlowOutput("Out");
		}

		public override void OnGraphStarted(){
			MonoManager.current.onFixedUpdate += this.FixedUpdate;
		}

		public override void OnGraphStoped(){
			MonoManager.current.onFixedUpdate -= this.FixedUpdate;
		}

		void FixedUpdate(){
			o.Call(new Flow(1));
		}
	}
}