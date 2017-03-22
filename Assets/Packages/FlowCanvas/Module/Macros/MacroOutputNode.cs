using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Macros{

	[DoNotList]
	[Icon("MacroOut")]
	[Description("Defines the Output ports of the Macro")]
	public class MacroOutputNode : FlowNode{

		private Macro macro{
			get {return (Macro)graph;}
		}

		protected override void RegisterPorts(){
			for (var i = 0; i < macro.outputDefinitions.Count; i++){
				var def = macro.outputDefinitions[i];
				if (def.type == typeof(Flow)){
					AddFlowInput(def.name, (f)=> {macro.exitActionMap[def.ID](f); }, def.ID );
				} else {
					macro.exitFunctionMap[def.ID] = AddValueInput(def.name, def.type, def.ID).GetValue;
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
			if (port is ValueOutput || port is FlowOutput){
				menu.AddItem(new GUIContent(string.Format("Promote to new Output '{0}'", port.name)), false, ()=>{
					var def = new MacroPortDefinition(port.name, port.type);
					if (macro.AddOutputDefinition(def)){
						GatherPorts();
						BinderConnection.Create(port, GetInputPort(def.ID));
					}
				});
			}
			return menu;
		}

		protected override void OnNodeInspectorGUI(){

			if (GUILayout.Button("Add Flow Output")){
				macro.AddOutputDefinition(new MacroPortDefinition("Flow Output", typeof(Flow)));
				GatherPorts();
			}
			
			if (GUILayout.Button("Add Value Output")){
				EditorUtils.ShowPreferedTypesSelectionMenu(typeof(object), (t)=>
				{
					macro.AddOutputDefinition(new MacroPortDefinition(string.Format("{0} Output", t.FriendlyName() ), t));
					GatherPorts();
				});
			}

			EditorUtils.ReorderableList(macro.outputDefinitions, (i)=>
			{
				var def = macro.outputDefinitions[i];
				GUILayout.BeginHorizontal();
				def.name = UnityEditor.EditorGUILayout.TextField(def.name, GUILayout.Width(0), GUILayout.ExpandWidth(true));
				GUILayout.Label(def.type.FriendlyName(), GUILayout.Width(0), GUILayout.ExpandWidth(true));
				if (GUILayout.Button("X", GUILayout.Width(20)))
					macro.outputDefinitions.RemoveAt(i);
				GUILayout.EndHorizontal();
			});

			if (GUI.changed){
				GatherPorts();
			}
		}
			
		#endif
	}
}