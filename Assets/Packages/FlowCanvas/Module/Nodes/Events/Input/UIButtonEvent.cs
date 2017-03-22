using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace FlowCanvas.Nodes{

	[Name("UI Button")]
	[Category("Events/Object")]
	[Description("Called when the target UI Button is clicked")]
	public class UIButtonEvent : EventNode<Button> {

		private FlowOutput o;

		public override void OnGraphStarted(){
			if (!target.isNull){
				target.value.onClick.AddListener(OnClick);
			}
		}

		public override void OnGraphStoped(){
			if (!target.isNull){
				target.value.onClick.RemoveListener(OnClick);
			}
		}

		protected override void RegisterPorts(){
			o = AddFlowOutput("Clicked");
		}

		void OnClick(){
			o.Call(new Flow(1));
		}
	}
}