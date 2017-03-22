using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Macros{

	[DoNotList]
	[Icon("MacroIn")]
	[Description("Defines the Input ports of the Macro")]
	public class MacroInputNode : FlowNode{

		private Macro macro{
			get {return (Macro)graph;}
		}

		protected override void RegisterPorts(){
			for (var i = 0; i < macro.inputDefinitions.Count; i++){
				var def = macro.inputDefinitions[i];
				if (def.type == typeof(Flow)){
					macro.entryActionMap[def.ID] = AddFlowOutput(def.name, def.ID).Call;
				} else {
					AddValueOutput(def.name, def.type, ()=> { return macro.entryFunctionMap[def.ID](); }, def.ID );
				}				
			}
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override UnityEditor.GenericMenu OnContextMenu(UnityEditor.GenericMenu menu){
			return null;
		}

		protected override UnityEditor.GenericMenu OnDragAndDropPortContextMenu(UnityEditor.GenericMenu menu, Port port){
			if (port is ValueInput || port is FlowInput){
				menu.AddItem(new GUIContent(string.Format("Promote to new Input '{0}'", port.name)), false, ()=>{
					var def = new MacroPortDefinition(port.name, port.type);
					if (macro.AddInputDefinition(def)){
						GatherPorts();
						BinderConnection.Create(GetOutputPort(def.ID), port);
					}
				});
			}
			return menu;
		}		

		protected override void OnNodeInspectorGUI(){

			if (GUILayout.Button("Add Flow Input")){
				macro.AddInputDefinition(new MacroPortDefinition("Flow Input", typeof(Flow)));
				GatherPorts();
			}

			if (GUILayout.Button("Add Value Input")){
				EditorUtils.ShowPreferedTypesSelectionMenu(typeof(object), (t)=>
				{
					macro.AddInputDefinition(new MacroPortDefinition(string.Format("{0} Input", t.FriendlyName() ), t));
					GatherPorts();
				});
			}

			EditorUtils.ReorderableList(macro.inputDefinitions, (i)=>
			{
				var def = macro.inputDefinitions[i];
				GUILayout.BeginHorizontal();
				def.name = UnityEditor.EditorGUILayout.TextField(def.name, GUILayout.Width(0), GUILayout.ExpandWidth(true));
				GUILayout.Label(def.type.FriendlyName(), GUILayout.Width(0), GUILayout.ExpandWidth(true));
				if (GUILayout.Button("X", GUILayout.Width(20))){
					macro.inputDefinitions.RemoveAt(i);
				}
				GUILayout.EndHorizontal();
			});

			if (GUI.changed){
				GatherPorts();
			}
		}

		#endif
		
	}
}