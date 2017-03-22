using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[DoNotList]
	///Wraps a MethodInfo into a FlowGraph node
	sealed public class ReflectedMethodNodeWrapper : FlowNode {

		[SerializeField]
		private SerializedMethodInfo _method = null;
		[SerializeField]
		private bool _callable = false;

		private ReflectedMethodNode reflectedMethodNode{get;set;}

		public override string name{
			get
			{
				if (method != null){
					if (!method.IsStatic){
						return method.Name.SplitCamelCase();
					}
					return string.Format("{0}.{1}", method.DeclaringType.FriendlyName(), method.Name.Replace("get_", "").Replace("set_", "Set ") );
				}
				if (_method != null){
					return string.Format("<color=#ff6457>* Missing Function *\n{0}</color>", _method.GetMethodString());
				}
				return "NOT SET";
			}
		}

#if UNITY_EDITOR
		public override string description{
			get { return method != null? DocsByReflection.GetMemberSummary(method) : "Missing Method"; }
		}
#endif

		private MethodInfo method{
			get {return _method != null? _method.Get() : null;}
		}

		private bool callable {
			get {return _callable;}
			set
			{
				if (_callable != value){
					_callable = value;
					GatherPorts();
				}
			}
		}


		///Set a new MethodInfo to be used by ReflectedMethodNode
		public void SetMethod(MethodInfo method, object instance = null){

			//drop hierarchy to base definition
			method = method.GetBaseDefinition();
			
			_method = new SerializedMethodInfo(method);
			_callable = method.ReturnType == typeof(void);
			GatherPorts();

			if (instance != null && !method.IsStatic ){
				var port = (ValueInput)GetFirstInputOfType(instance.GetType());
				if (port != null){
					port.serializedValue = instance;
				}
			}
		}


		///Gather the ports through the wrapper
		protected override void RegisterPorts(){

			if (method == null){
				return;
			}

			if ( method.DeclaringType.RTIsValueType() || method.GetParameters().Any( p => p.IsOut ) ){
				RegisterWithPureReflection();
				return;
			}

			//JIT
			try
			{
				reflectedMethodNode = ReflectedMethodNode.Create(method);
				if (reflectedMethodNode != null){
					reflectedMethodNode.RegisterPorts(this, method, callable);
				}
			}
			
			//AOT
			catch
			{
				RegisterWithPureReflection();
			}
		}






		// For when using pure reflection. Rarely, or on AOT platforms
		///////////////////////////////////////////
		private ValueInput instanceInput;
		private List<ValueInput> inputs;
		private object[] args;
		private object instance;
		private object returnValue;

		void RegisterWithPureReflection(){

			var parameters = method.GetParameters();

			//Flow ports
			if (callable){
				var o = AddFlowOutput(" ");
				AddFlowInput(" ", (f)=> { CallMethod(); o.Call(f); });
			}

			//Instance ports
			if (!method.IsStatic){
				instanceInput = AddValueInput(method.DeclaringType.FriendlyName(), method.DeclaringType);
				if (callable){
					AddValueOutput(method.DeclaringType.FriendlyName(), method.DeclaringType, ()=> { return instance; } );
				}
			}

			//Return value port
			if (method.ReturnType != typeof(void)){
				AddValueOutput("Value", method.ReturnType, ()=> { return callable? returnValue : CallMethod(); } );
			}

			//Parameter ports
			inputs = new List<ValueInput>();
			for (var _i = 0; _i < parameters.Length; _i++){
				var i = _i;
				var parameter = parameters[i];
				var paramName = parameter.Name.SplitCamelCase();
				if (instanceInput != null && paramName == instanceInput.name){ //for rare cases where it's same as instance name like for example in Animation class.
					paramName = paramName + " ";
				}

				if (parameter.IsOut){

					AddValueOutput(paramName, parameter.ParameterType, ()=> { if(callable) return args[i]; else CallMethod(); return args[i]; });
					inputs.Add(new ValueInput<object>(null, null, null)); //add dummy inputs for the shake of getting out args correctly by index

				} else {
					var paramPort = AddValueInput(paramName, parameter.ParameterType);
/*
					if ( !(parameter.RawDefaultValue is System.DBNull) ){
						paramPort.serializedValue = parameter.RawDefaultValue;
					}
*/					
					inputs.Add( paramPort );
				}
			}
		}

		//Calls the method
		object CallMethod(){

			if (args == null){
				args = new object[inputs.Count];
			}

			for (var i = 0; i < inputs.Count; i++){
				args[i] = inputs[i].value;
			}
			
			if (method.IsStatic){

				return returnValue = _method.Get().Invoke(null, args);

			} else {

				instance = instanceInput.value;
				if (instance == null || instance.Equals(null)){
					return returnValue = null;
				}
				return returnValue = _method.Get().Invoke(instance, args);
			}
		}



		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override UnityEditor.GenericMenu OnContextMenu(UnityEditor.GenericMenu menu){
			if (method != null){
				var alterMethods = new List<MethodInfo>();
				foreach(var m in method.ReflectedType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)){
					if (m.Name == method.Name){
						alterMethods.Add(m);
					}
				}

				if (alterMethods.Count > 1){
					foreach(var _m in alterMethods){
						var m = _m;
						var isSame = m.SignatureName() == method.SignatureName();
						menu.AddItem( new GUIContent("Replace Method/" + m.SignatureName()), isSame, ()=>{ SetMethod(m); } );
					}
				}
			}
			return menu;
		}

		protected override void OnNodeInspectorGUI(){
			if (method != null && method.ReturnType != typeof(void) && !method.Name.StartsWith("get_")){
				callable = UnityEditor.EditorGUILayout.Toggle("Callable", callable);
			}

			if (method == null && _method != null){
				GUILayout.Label(_method.GetMethodString());
			}

			base.OnNodeInspectorGUI();
		}

		#endif
	}
}