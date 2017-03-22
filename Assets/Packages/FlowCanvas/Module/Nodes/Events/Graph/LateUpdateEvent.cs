using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion.Services;

namespace FlowCanvas.Nodes{

	[Name("On Late Update")]
	[Category("Events/Graph")]
	[Description("Called per-frame, but after normal Update")]
	public class LateUpdateEvent : EventNode {

		private FlowOutput o;

		protected override void RegisterPorts(){
			o = AddFlowOutput("Out");
		}

		public override void OnGraphStarted(){
			MonoManager.current.onLateUpdate += this.LateUpdate;
		}

		public override void OnGraphStoped(){
			MonoManager.current.onLateUpdate -= this.LateUpdate;
		}

		void LateUpdate(){
			o.Call(new Flow(1));
		}
	}
}