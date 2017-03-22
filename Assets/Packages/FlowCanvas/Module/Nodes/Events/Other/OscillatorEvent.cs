using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Name("OSC Pulse")]
	[Category("Events/Other")]
	[Description("Calls Hi when curve value is greater than 0, else calls Low.\nThe curve is evaluated over time and it's evaluated value is exposed")]
	public class OscillatorEvent : EventNode, IUpdatable {

		public AnimationCurve curve;
		private float time;
		private float value;
		private FlowOutput hi;
		private FlowOutput low;

		public OscillatorEvent(){
			var key1 = new Keyframe(0, 1);
			var key2 = new Keyframe(0.5f, 1);
			var key3 = new Keyframe(0.5f, -1);
			var key4 = new Keyframe(1, -1);
			curve = new AnimationCurve(new Keyframe[]{key1,key2,key3,key4});
			curve.postWrapMode = WrapMode.Loop;
		}

		protected override void RegisterPorts(){
			hi = AddFlowOutput("Hi");
			low = AddFlowOutput("Low");
			AddValueOutput<float>("Value", ()=> { return value; } );
		}

		public override void OnGraphStarted(){ time = 0; }

		public void Update(){
			value = curve.Evaluate(time);
			time += Time.deltaTime;
			Call(value >= 0? hi : low, new Flow(value));
		}
	}
}