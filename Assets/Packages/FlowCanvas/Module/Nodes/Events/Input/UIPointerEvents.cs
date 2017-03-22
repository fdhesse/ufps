using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace FlowCanvas.Nodes{

	[Name("UI Pointer")]
	[Category("Events/Object")]
	[Description("Calls UI Pointer based events on target. The Unity Event system has to be set through 'GameObject/UI/Event System'")]
	public class UIPointerEvents : EventNode<Transform> {

		private FlowOutput pointerDown;
		private FlowOutput pointerPressed;
		private FlowOutput pointerUp;
		private FlowOutput pointerEnter;
		private FlowOutput pointerExit;
		private FlowOutput pointerClick;
		private PointerEventData eventData;

		private bool updatePressed = false;

		protected override string[] GetTargetMessageEvents(){
			return new string[]{ "OnPointerEnter", "OnPointerExit", "OnPointerDown", "OnPointerUp", "OnPointerClick" };
		}

		protected override void RegisterPorts(){
			pointerClick = AddFlowOutput("Click");
			pointerDown  = AddFlowOutput("Down");
			pointerPressed= AddFlowOutput("Pressed");
			pointerUp    = AddFlowOutput("Up");
			pointerEnter = AddFlowOutput("Enter");
			pointerExit  = AddFlowOutput("Exit");
			AddValueOutput<PointerEventData>("Event Data", ()=> { return eventData; });
		}

		void OnPointerDown(PointerEventData eventData){
			this.eventData = eventData;
			pointerDown.Call(new Flow(1));
			updatePressed = true;
			StartCoroutine(UpdatePressed());
		}

		void OnPointerUp(PointerEventData eventData){
			this.eventData = eventData;
			pointerUp.Call(new Flow(1));
			updatePressed = false;
		}


		IEnumerator UpdatePressed(){
			while(updatePressed){
				pointerPressed.Call(new Flow(1));
				yield return null;
			}
		}

		void OnPointerEnter(PointerEventData eventData){
			this.eventData = eventData;
			pointerEnter.Call(new Flow(1));
		}

		void OnPointerExit(PointerEventData eventData){
			this.eventData = eventData;
			pointerExit.Call(new Flow(1));
		}

		void OnPointerClick(PointerEventData eventData){
			this.eventData = eventData;
			pointerClick.Call(new Flow(1));
		}
	}
}