using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

using FlowCanvas.Macros;
using FlowCanvas.Nodes;


namespace FlowCanvas{

	///Base class for flow graphs.
	[GraphInfo(
		packageName = "FlowCanvas",
		docsURL = "http://flowcanvas.paradoxnotion.com/documentation/",
		resourcesURL = "http://flowcanvas.paradoxnotion.com/downloads/",
		forumsURL = "http://flowcanvas.paradoxnotion.com/forums-page/"
		)]
	[System.Serializable]
	abstract public class FlowGraph : Graph {

		private bool hasInitialized;
		private List<IUpdatable> updatableNodes;

		public override System.Type baseNodeType{ get {return typeof(FlowNode);} }
		public override bool useLocalBlackboard{ get {return false;} }
		sealed public override bool requiresAgent{	get {return false;} }
		sealed public override bool requiresPrimeNode { get {return false;} }
		sealed public override bool autoSort{ get {return false;} }

		protected override void OnGraphStarted(){

			if (!hasInitialized){
				updatableNodes = new List<IUpdatable>();
			}

			for (var i = 0; i < allNodes.Count; i++){
				if (allNodes[i] is MacroNodeWrapper){
					var macroNode = (MacroNodeWrapper)allNodes[i];
					if (macroNode.macro != null){
						macroNode.CheckInstance();
						macroNode.macro.StartGraph(agent, blackboard, false, null);
					}
				}

				if (!hasInitialized){
					if (allNodes[i] is IUpdatable){
						updatableNodes.Add( (IUpdatable)allNodes[i] );
					}					
				}
			}


			if (!hasInitialized){
				for (var i = 0; i < allNodes.Count; i++){
					if (allNodes[i] is FlowNode){
						var flowNode = (FlowNode)allNodes[i];
						flowNode.AssignSelfInstancePort();
						flowNode.BindPorts();
					}
				}
			}

			hasInitialized = true;
		}

		//Update IUpdatable nodes. Basicaly for events like Input, Update etc
		//This is the only thing that udpates per-frame
		protected override void OnGraphUpdate(){
			if (updatableNodes != null && updatableNodes.Count > 0){
				for (var i = 0; i < updatableNodes.Count; i++){
					updatableNodes[i].Update();
				}
			}
		}

		protected override void OnGraphStoped(){
			for (var i = 0; i < allNodes.Count; i++){
				var node = allNodes[i];
				if (node is MacroNodeWrapper){
					var macroNode = (MacroNodeWrapper)node;
					if (macroNode.macro != null){
						macroNode.macro.Stop();
					}
				}
			}
		}




		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override bool canAcceptVariableDrops{get{return true;}}

		//Append menu items in canvas right click context menu
		protected override UnityEditor.GenericMenu OnCanvasContextMenu(UnityEditor.GenericMenu menu, Vector2 mousePos){

			//SimplexNode Wrapper
			System.Action<System.Type> SelectedSimplexNode = delegate(System.Type t){
				var genericType = typeof(SimplexNodeWrapper<>).MakeGenericType(new System.Type[]{ (System.Type)t });
				AddNode(genericType, mousePos);
			};
			
			//MethodInfo Wrapper
			UnityEditor.GenericMenu.MenuFunction2 SelectedMethod = delegate(object m){
				var methodWrapper = AddNode<ReflectedMethodNodeWrapper>(mousePos);
				methodWrapper.SetMethod((MethodInfo)m);
			};
/*
			//FieldInfo Wrapper
			UnityEditor.GenericMenu.MenuFunction2 SelectedFieldGet = delegate(object f){
				var fieldWrapper = AddNode<ReflectedFieldNodeWrapper>(mousePos);
				fieldWrapper.SetField((FieldInfo)f, true);
			};
*/
			//Macro Wrapper
			UnityEditor.GenericMenu.MenuFunction2 SelectedMacro = delegate(object m){
				var macroWrapper = AddNode<MacroNodeWrapper>(mousePos);
				macroWrapper.macro = (Macro)m;
			};


			//Append SimplexNodeWrapper nodes
			menu = EditorUtils.GetTypeSelectionMenu(typeof(SimplexNode), SelectedSimplexNode, menu);


			//Append Reflection
			foreach (var type in UserTypePrefs.GetPreferedTypesList(typeof(object))){
				var icon = type.IsSubclassOf(typeof(UnityEngine.Object))? UnityEditor.EditorGUIUtility.ObjectContent(null, type).image : null;
				var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
				methods.AddRange( type.GetExtensionMethods() );
				foreach (var m in methods.OrderBy(m => !m.IsStatic).OrderBy(m => m.IsSpecialName).OrderBy(m => m.DeclaringType != type) ){
					
					if (m.IsGenericMethod){
						continue;
					}

					if (m.IsObsolete()){
						continue;
					}

					var categoryName = "Functions/Reflected/" + type.FriendlyName() + "/" + (m.IsSpecialName? "Properties/" : "Methods/");
					if (m.DeclaringType != type){ categoryName += "More/"; }
					var name = categoryName + m.SignatureName();
					menu.AddItem(new GUIContent(name, icon, null ), false, SelectedMethod, m);
				}
/*
				foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)){
					if (f.IsReadOnly()){
						var categoryName = "Functions/Reflected/" + type.FriendlyName() + "/Constants/";
						if (f.DeclaringType != type){ categoryName += "More/"; }
						var name = categoryName + f.Name;
						menu.AddItem(new GUIContent(name, icon, null), false, SelectedFieldGet, f);
					}
				}
*/
			}


			//Append Variables both local and global
			var variables = new Dictionary<IBlackboard, List<Variable>>();
			if (blackboard != null){
				variables[blackboard] = blackboard.variables.Values.ToList();
			}
			foreach(var globalBB in GlobalBlackboard.allGlobals){
				variables[globalBB] = globalBB.variables.Values.ToList();
			}

			foreach (var pair in variables){
				foreach(var _bbVar in pair.Value){
					var bbVar = _bbVar;
					var finalName = pair.Key == blackboard? bbVar.name : string.Format("{0}/{1}", pair.Key.name, bbVar.name);
					menu.AddItem(new GUIContent("Variables/Get Blackboard Variable/Get " + finalName, null, "Get Blackboard Variable"), false, ()=> {
						var genericType = typeof(GetVariable<>).MakeGenericType(new System.Type[]{bbVar.varType});
						var varNode = (VariableNode)AddNode(genericType, mousePos);
						genericType.GetMethod("SetTargetVariableName").Invoke(varNode, new object[]{finalName});
					});
					menu.AddItem(new GUIContent("Variables/Set Blackboard Variable/Set " + finalName, null, "Set Blackboard Variable"), false, ()=> {
						var genericType = typeof(SetVariable<>).MakeGenericType(new System.Type[]{bbVar.varType});
						var varNode = (FlowNode)AddNode(genericType, mousePos);
						genericType.GetMethod("SetTargetVariableName").Invoke(varNode, new object[]{finalName});
					});
				}
			}


			//Append Project found Macros
			var projectMacroGUIDS = UnityEditor.AssetDatabase.FindAssets("t:Macro");
			foreach(var guid in projectMacroGUIDS){
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				var macro = (Macro)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Macro));
				if (macro != this){
					menu.AddItem(new GUIContent("MACROS/" + macro.name, null, macro.graphComments), false, SelectedMacro, macro);
				}
			}

			//Append Create new Macro
			menu.AddItem(new GUIContent("MACROS/Create New...", null, "Create a new macro"), false, ()=>
			{
				var newMacro = EditorUtils.CreateAsset<Macro>(true);
				if (newMacro != null){
					var wrapper = AddNode<MacroNodeWrapper>(mousePos);
					wrapper.macro = newMacro;
				}
			});

			return menu;
		}

		//Create and set a UnityObject variable node on drop
		protected override void OnDropAccepted(Object o, Vector2 mousePos){
			
			if ( UnityEditor.EditorUtility.IsPersistent(this) && !UnityEditor.EditorUtility.IsPersistent(o) ){
				Debug.LogError("This Graph is an asset. The dragged object is a scene reference. The reference will not persist");
			}

			if (o is Macro){
				var mWrapper = AddNode<MacroNodeWrapper>(mousePos);
				mWrapper.macro = (Macro)o;
				return;
			}

			//Add as Variable
			UnityEditor.GenericMenu.MenuFunction2 SelectedVar = (obj)=>{
				var genericType = typeof(GetVariable<>).MakeGenericType(new System.Type[]{ obj.GetType() });
				var varNode = (VariableNode)AddNode(genericType, mousePos);
				varNode.SetVariable(obj);
			};

			//Add as actions/function
			UnityEditor.GenericMenu.MenuFunction2 SelectedMethod = (m)=>{
				var wrapper = AddNode<ReflectedMethodNodeWrapper>(mousePos);

				//draged source was game object. get the component of the method selected
				if (o is GameObject){
					o = (o as GameObject).GetComponent( ((MethodInfo)m).ReflectedType );
				}

				//same as agent? revert to using "self"
				if (agent != null && o is Component && (o as Component).gameObject == agent.gameObject){
					o = null;
				}

				wrapper.SetMethod( (MethodInfo)m, o);
			};

			var menu = new UnityEditor.GenericMenu();

			if (o is GameObject){

				menu.AddItem(new GUIContent("Graph Variable (GameObject)"), false, SelectedVar, o);
				menu.AddSeparator("/");
				foreach (var component in (o as GameObject).GetComponents<Component>().Where(c => c.hideFlags == 0) ){
					var type = component.GetType();
					var category = type.Name + "/";
					menu.AddItem(new GUIContent(string.Format(category + "Graph Variable ({0})", type.Name)), false, SelectedVar, component );


					var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
					methods.AddRange( type.GetExtensionMethods() );
					foreach (var m in methods.OrderBy(m => !m.IsStatic).OrderBy(m => m.IsSpecialName).OrderBy(m => m.DeclaringType != type) ){
						if (!m.IsGenericMethod){
							var categoryName = category + (m.IsSpecialName? "Properties/" : "Methods/");
							if (m.DeclaringType != type){
								categoryName += "More/";
							}
							var name = categoryName + m.SignatureName();
							var icon = UnityEditor.EditorGUIUtility.ObjectContent(null, type).image;
							menu.AddItem(new GUIContent(name, icon), false, SelectedMethod, m);
						}
					}
				}
			
			} else {

				var type = o.GetType();
				menu.AddItem( new GUIContent( string.Format("Graph Variable ({0})", o.GetType().Name) ), false, SelectedVar, o );
				var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
				methods.AddRange( type.GetExtensionMethods() );
				foreach (var m in methods.OrderBy(m => !m.IsStatic).OrderBy(m => m.IsSpecialName).OrderBy(m => m.DeclaringType != type) ){
					if (!m.IsGenericMethod){
						var categoryName = (m.IsSpecialName? "Properties/" : "Methods/");
						if (m.DeclaringType != type){
							categoryName += "More/";
						}
						var name = categoryName + m.SignatureName();
						var icon = UnityEditor.EditorGUIUtility.ObjectContent(null, type).image;
						menu.AddItem(new GUIContent(name, icon), false, SelectedMethod, m);
					}
				}
			}

			menu.ShowAsContext();
			Event.current.Use();
		}


		protected override void OnVariableDropInGraph(Variable variable, Event e, Vector2 canvasMousePos){
			if (variable != null){
				var menu = new UnityEditor.GenericMenu();
				menu.AddItem(new GUIContent(variable.name + "/" + "Get"), false, ()=>
				{
					var genericType = typeof(GetVariable<>).MakeGenericType(new System.Type[]{variable.varType});
					var varNode = (VariableNode)AddNode(genericType, canvasMousePos);
					genericType.GetMethod("SetTargetVariableName").Invoke(varNode, new object[]{variable.name});
				});
				menu.AddItem(new GUIContent(variable.name + "/" + "Set"), false, ()=>
				{
					var genericType = typeof(SetVariable<>).MakeGenericType(new System.Type[]{variable.varType});
					var varNode = (FlowNode)AddNode(genericType, canvasMousePos);
					genericType.GetMethod("SetTargetVariableName").Invoke(varNode, new object[]{variable.name});					
				});
				menu.ShowAsContext();
				e.Use();
			}
		}

		#endif
	}
}