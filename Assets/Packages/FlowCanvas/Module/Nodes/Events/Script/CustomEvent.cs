using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;
using UnityEngine;

namespace FlowCanvas.Nodes{

	[Description("Called when a custom event is received on target.\nTo send an event from code use:\n'FlowScriptController.SendEvent(string)'")]
	[Category("Events/Script")]
	public class CustomEvent : EventNode<GraphOwner> {

		[RequiredField]
		public string eventName;

		private FlowOutput received;

		public override string name{
			get {return base.name + string.Format(" [ <color=#DDDDDD>{0}</color> ]", eventName); }
		}

		protected override string[] GetTargetMessageEvents(){
			return new string[]{ "OnCustomEvent" };
		}

		protected override void RegisterPorts(){
			received = AddFlowOutput("Received");
		}

		public void OnCustomEvent(EventData receivedEvent){
			if (receivedEvent.name == eventName){
				
				#if UNITY_EDITOR
				if (NodeCanvas.Editor.NCPrefs.logEvents){
					Debug.Log(string.Format("<b>Event Received from ({0}): </b> '{1}'", graphAgent.name, receivedEvent.name), graphAgent);
				}
				#endif

				received.Call(new Flow(1));
			}
		}
	}


	[Description("Called when a custom value-based event is received on target.\nTo send an event from code use:\n'FlowScriptController.SendEvent<T>(string name, T value)'")]
	[Category("Events/Script")]
	public class CustomEvent<T> : EventNode<GraphOwner> {

		[RequiredField]
		public string eventName;
		private FlowOutput received;
		private T receivedValue;

		public override string name{
			get {return base.name + string.Format(" [ <color=#DDDDDD>{0}</color> ]", eventName); }
		}

		protected override string[] GetTargetMessageEvents(){
			return new string[]{ "OnCustomEvent" };
		}

		protected override void RegisterPorts(){
			received = AddFlowOutput("Received");
			AddValueOutput<T>("Event Value", ()=> { return receivedValue; });
		}

		public void OnCustomEvent(EventData receivedEvent){
			if (receivedEvent.name == eventName){
				if (receivedEvent is EventData<T>){
					receivedValue = (receivedEvent as EventData<T>).value;
				}

				#if UNITY_EDITOR
				if (NodeCanvas.Editor.NCPrefs.logEvents){
					Debug.Log(string.Format("<b>Event Received from ({0}): </b> '{1}'", graphAgent.name, receivedEvent.name), graphAgent);
				}
				#endif

				received.Call(new Flow(1));
			}
		}
	}

}