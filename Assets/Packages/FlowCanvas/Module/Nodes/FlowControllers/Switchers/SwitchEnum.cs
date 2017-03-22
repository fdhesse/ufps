using System.Collections.Generic;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Category("Flow Controllers/Switchers")]
	[Description("Branch the Flow based on an enum value.\nThere are 2 ways to set the Enum type:\n1) Drag and Drop an enum connection on top.\n2) Select the type after it has been added in the Prefered Types Editor Window.")]
	[ContextDefinedInputs(typeof(System.Enum))]
	public class SwitchEnum : FlowControlNode {

		[SerializeField]
		private System.Type _type = null;

		protected override void RegisterPorts(){
			if (_type == null)
				return;

			var e = AddValueInput(_type.Name, _type);
			var outs = new List<FlowOutput>();
			foreach (var s in System.Enum.GetNames(_type))
				outs.Add( AddFlowOutput(s) );
			AddFlowInput("In", (f)=>
			{
				var index = (int)System.Enum.Parse(e.value.GetType(), e.value.ToString());
				outs[index].Call(f);
			});
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override UnityEditor.GenericMenu OnDragAndDropPortContextMenu(UnityEditor.GenericMenu menu, Port port){
			if (_type == null && port is ValueOutput && port.type.IsSubclassOf(typeof(System.Enum))){
				menu.AddItem(new GUIContent("Set Enum From Drag"), false, ()=>{
					_type = port.type;
					GatherPorts();
					BinderConnection.Create(port, GetInputPort(_type.Name));
				});
			}
			return menu;
		}

		protected override void OnNodeInspectorGUI(){
			base.OnNodeInspectorGUI();
			if (!Application.isPlaying && (_type == null || _type != null && !_type.IsSubclassOf(typeof(System.Enum)) ) ){
				if (GUILayout.Button("Select Type")){
					EditorUtils.ShowPreferedTypesSelectionMenu(typeof(System.Enum), (t)=> { _type = t; GatherPorts(); });
				}
			}
		}

		#endif
	}
}