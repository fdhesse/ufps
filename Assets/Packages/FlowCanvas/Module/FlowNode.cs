#if UNITY_EDITOR
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NodeCanvas.Framework;
using NodeCanvas;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

using FlowCanvas.Nodes;


namespace FlowCanvas{

	public interface IMultiPortNode{
		int portCount{get;set;}
	}

	///The base node class for FlowGraph systems
	abstract public class FlowNode : Node, ISerializationCallbackReceiver {
		
		[AttributeUsage(AttributeTargets.Class)]//helper attribute for context menu
		protected class ContextDefinedInputsAttribute : Attribute{
			public Type[] types;
			public ContextDefinedInputsAttribute(params Type[] types){
				this.types = types;
			}
		}

		[AttributeUsage(AttributeTargets.Class)]//helper attribute for context menu
		protected class ContextDefinedOutputsAttribute : Attribute{
			public Type[] types;
			public ContextDefinedOutputsAttribute(params Type[] types){
				this.types = types;
			}
		}


		[SerializeField] //The only thing we really need to serialize port wise, are the input port values that has been set by the user
		private Dictionary<string, object> _inputPortValues;

		private Dictionary<string, Port> inputPorts  = new Dictionary<string, Port>( StringComparer.Ordinal );
		private Dictionary<string, Port> outputPorts = new Dictionary<string, Port>( StringComparer.Ordinal );

		sealed public override int maxInConnections{ get {return -1;} }
		sealed public override int maxOutConnections{ get {return -1;} }
		sealed public override bool allowAsPrime{ get {return false;} }
		sealed public override Type outConnectionType{ get {return typeof(BinderConnection);} }
		sealed public override bool showCommentsBottom{ get{return true;}}

		///Store the changed input port values.
		void ISerializationCallbackReceiver.OnBeforeSerialize(){
			if (_inputPortValues == null){ _inputPortValues = new Dictionary<string, object>(); }
			foreach (var port in inputPorts.Values.OfType<ValueInput>()){
				if (!port.isConnected /*&& !port.isDefaultValue*/){
					_inputPortValues[port.ID] = port.serializedValue;
				}
			}
		}


		//Nothing... Instead, deserialize input port value AFTER GatherPorts and Validation. We DONT want to call GatherPorts here.
		void ISerializationCallbackReceiver.OnAfterDeserialize(){}


		///This is called when the node is created, duplicated or otherwise needs validation
		sealed public override void OnValidate(Graph flowGraph){
			GatherPorts();
		}

		sealed public override void OnParentConnected(int i){}
		sealed public override void OnChildConnected(int i){}
		sealed public override void OnParentDisconnected(int i){}
		sealed public override void OnChildDisconnected(int i){}


		//Connect 2 ports together. Source is always an Input and target an Output port
		static void ConnectPorts(Port source, Port target){
			BinderConnection.Create(source, target);
		}

		///Bind the port delegates. Called at runtime
		public void BindPorts(){
			for (var i = 0; i < outConnections.Count; i++){
				(outConnections[i] as BinderConnection).Bind();
			}
		}

		///Unbind the ports
		public void UnBindPorts(){
			for (var i = 0; i < outConnections.Count; i++){
				(outConnections[i] as BinderConnection).UnBind();
			}
		}

		///Gets an input Port by it's ID name which commonly is the same as it's name
		public Port GetInputPort(string ID){
			Port input = null;
			inputPorts.TryGetValue(ID, out input);
			return input;
		}

		///Gets an output Port by it's ID name which commonly is the same as it's name
		public Port GetOutputPort(string ID){
			Port output = null;
			outputPorts.TryGetValue(ID, out output);
			return output;
		}

		///Gets the BinderConnection of an Input port based on it's ID
		public BinderConnection GetInputConnectionForPortID(string ID){
			return inConnections.OfType<BinderConnection>().FirstOrDefault(c => c.targetPortID == ID);
		}

		///Gets the BinderConnection of an Output port based on it's ID
		public BinderConnection GetOutputConnectionForPortID(string ID){
			return outConnections.OfType<BinderConnection>().FirstOrDefault(c => c.sourcePortID == ID);
		}

		///Returns the first input port assignable to the type provided
		public Port GetFirstInputOfType(Type type){
			return inputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowInput)? 0 : 1 ).FirstOrDefault(p => p.type.RTIsAssignableFrom(type) );
		}

		///Returns the first output port of a type assignable to the port
		public Port GetFirstOutputOfType(Type type){
			return outputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowInput)? 0 : 1 ).FirstOrDefault(p => type.RTIsAssignableFrom(p.type) );
		}

		
		///Set the Component or GameObject instance input port to Owner if not connected and not already set to another reference.
		///By convention, the instance port is always considered to be the first.
		//Called from Graph when started.
		public void AssignSelfInstancePort(){
			if (graphAgent == null){
				return;
			}

			var instanceInput = inputPorts.Values.OfType<ValueInput>().FirstOrDefault();
			if (instanceInput != null && !instanceInput.isConnected && instanceInput.isDefaultValue){
				if (instanceInput.type == typeof(GameObject)){
					instanceInput.serializedValue = graphAgent.gameObject;
				}
				if (typeof(Component).RTIsAssignableFrom(instanceInput.type)){
					instanceInput.serializedValue = graphAgent.GetComponent(instanceInput.type);
				}
			}
		}

		///Gather and register the node ports.
		public void GatherPorts(){

			inputPorts.Clear();
			outputPorts.Clear();
			RegisterPorts();

			#if UNITY_EDITOR
			OnPortsGatheredInEditor();
			#endif

			DeserializeInputPortValues();
			ValidateConnections();
		}

		///*IMPORTANT*
		///Override for registration/definition of ports.
		///If RegisterPorts is not overriden, reflection is used to register ports based on guidelines.
		///It's MUCH BETTER AND FASTER if you use the API for port registration instead though!
		virtual protected void RegisterPorts(){
			DoReflectionBasedRegistration();
		}

		///Reflection based registration if RegisterPorts is not overriden. Nowhere used by default in FC.
		void DoReflectionBasedRegistration(){

			//FlowInputs. All void methods with one Flow parameter.
			foreach (var method in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)){
				var parameters = method.GetParameters();
				if (method.ReturnType == typeof(void) && parameters.Length == 1 && parameters[0].ParameterType == typeof(Flow)){
					var nameAtt = method.RTGetAttribute<NameAttribute>(false);
					var name = nameAtt != null? nameAtt.name : method.Name.SplitCamelCase();
					var pointer = method.RTCreateDelegate<FlowHandler>(this);
					AddFlowInput(name, pointer);
				}
			}


			//ValueOutputs. All readable public properties.
			foreach (var prop in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)){
				if (prop.CanRead){
					AddPropertyOutput(prop, this);
				}
			}


			//Search for delegates fields
			foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)){

				if ( typeof(Delegate).RTIsAssignableFrom(field.FieldType) ){

					var nameAtt = field.RTGetAttribute<NameAttribute>(false);
					var name = nameAtt != null? nameAtt.name : field.Name.SplitCamelCase();
					
					var invokeMethod = field.FieldType.GetMethod("Invoke");
					var parameters = invokeMethod.GetParameters();

					//FlowOutputs. All FlowHandler fields.
					if (field.FieldType == typeof(FlowHandler)){
						var flowOut = AddFlowOutput(name);
						field.SetValue(this, (FlowHandler)flowOut.Call);
					}
					
					//ValueInputs. All ValueHandler<T> fields.
					if (invokeMethod.ReturnType != typeof(void) && parameters.Length == 0){
						var delType = invokeMethod.ReturnType;
						var portType = typeof(ValueInput<>).RTMakeGenericType( new Type[]{ delType } );
						var port = (ValueInput)Activator.CreateInstance(portType, new object[]{ this, name, name });

						var getterType = typeof(ValueHandler<>).RTMakeGenericType(new Type[]{ delType });
						var getter = port.GetType().GetMethod("get_value").RTCreateDelegate(getterType, port);
						field.SetValue(this, getter);
						inputPorts[name] = port;
					}
				}
			}
		}




		//Validate ports for connections
		//This is done seperately for Source and Target since we don't get control of when GatherPorts will be called on each node apart from in order of list (and we dont care)
		//So basicaly each node validates it's own inputs and outputs seperately.
		void ValidateConnections(){

			foreach (var cOut in outConnections.ToArray()){ //ToArray because connection might remove itself if invalid
				if (cOut is BinderConnection){
					(cOut as BinderConnection).GatherAndValidateSourcePort();
				}
			}

			foreach (var cIn in inConnections.ToArray()){
				if (cIn is BinderConnection){
					(cIn as BinderConnection).GatherAndValidateTargetPort();
				}
			}
		}


		///Restore the serialized input port values
		void DeserializeInputPortValues(){

			if (_inputPortValues == null){
				return;
			}

			foreach (var pair in _inputPortValues){
				Port inputPort = null;
				if ( inputPorts.TryGetValue(pair.Key, out inputPort)){
					if (inputPort is ValueInput && pair.Value != null && inputPort.type.RTIsAssignableFrom(pair.Value.GetType())){
						(inputPort as ValueInput).serializedValue = pair.Value;
					}
				}
			}
		}

		//Port registration/definition methods, to be used within RegisterPorts override
		//...
		///Add a new FlowInput with name and pointer. Pointer is the method to run when the flow port is called. Returns the new FlowInput object.
		public FlowInput AddFlowInput(string name, FlowHandler pointer, string ID = ""){
			if (string.IsNullOrEmpty(ID)) ID = name;
			return (FlowInput) (inputPorts[ID] = new FlowInput(this, name, ID, pointer) );
		}

		///Add a new FlowOutput with name. Returns the new FlowOutput object.
		public FlowOutput AddFlowOutput(string name, string ID = ""){
			if (string.IsNullOrEmpty(ID)) ID = name;
			return (FlowOutput) (outputPorts[ID] = new FlowOutput(this, name, ID) );
		}

		///Recommended. Add a ValueInput of type T. Returns the new ValueInput<T> object.
		public ValueInput<T> AddValueInput<T>(string name, string ID = ""){
			if (string.IsNullOrEmpty(ID)) ID = name;
			return (ValueInput<T>) (inputPorts[ID] = new ValueInput<T>(this, name, ID) );
		}

		///Recommended. Add a ValueOutput of type T. getter is the function to get the value from. Returns the new ValueOutput<T> object.
		public ValueOutput<T> AddValueOutput<T>(string name, ValueHandler<T> getter, string ID = ""){
			if (string.IsNullOrEmpty(ID)) ID = name;
			return (ValueOutput<T>) (outputPorts[ID] = new ValueOutput<T>(this, name, ID, getter) );
		}

		///Add a WildInput port which is an object type forced to a specific type instead. It's very recommended to use the generic registration methods instead.
		public ValueInput AddValueInput(string name, Type type, string ID = ""){
			if (string.IsNullOrEmpty(ID)) ID = name;
			return (ValueInput) (inputPorts[ID] = ValueInput.CreateInstance(type, this, name, ID) );
		}
		
		///Add a new WildOutput of unkown runtime type (similar to WildInput). getter is a function to get the port value from. Returns the new ValueOutput object.
		public ValueOutput AddValueOutput(string name, Type type, ValueHandler<object> getter, string ID = ""){
			if (string.IsNullOrEmpty(ID)) ID = name;
			return (ValueOutput) (outputPorts[ID] = ValueOutput.CreateInstance(type, this, name, ID, getter) );
		}


		///Register a PropertyInfo as ValueOutput. Used only in reflection based registration.
		public ValueOutput AddPropertyOutput(PropertyInfo prop, object instance){

			if (!prop.CanRead){
				Debug.LogError("Property is write only");
				return null;
			}

			var nameAtt = prop.RTGetAttribute<NameAttribute>(false);
			var name = nameAtt != null? nameAtt.name : prop.Name.SplitCamelCase();

			var getterType = typeof(ValueHandler<>).RTMakeGenericType(new Type[]{ prop.PropertyType });
			var getter = prop.RTGetGetMethod().RTCreateDelegate(getterType, instance);
			var portType = typeof(ValueOutput<>).RTMakeGenericType( new Type[]{ prop.PropertyType } );
			var port = (ValueOutput)Activator.CreateInstance(portType, new object[]{ this, name, name, getter });
			return (ValueOutput) (outputPorts[name] = port);
		}

		/////


		///Alternative Exit Call by providing a FlowOutput (otherwise, use 'flowOutput.Call(Flow f)' directly)
		public void Call(FlowOutput port, Flow f){
			port.Call(f);
		}

		//...
		public void Fail(string error = null){
			status = Status.Failure;
			if (error != null){
				Debug.LogError(string.Format("<b>Flow Execution Error:</b> '{0}' - '{1}'", this.name, error), graph.agent);
			}
		}

		//...
		public void SetStatus(Status status){
			this.status = status;
		}


























		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		private static Port clickedPort;
		private static int dragDropMisses;
		private static GUIStyle _leftLabelStyle;
		private static GUIStyle _rightLabelStyle;

		private Port[] orderedInputs;
		private Port[] orderedOutputs;
		private ValueInput firstValuePort;

		//for input ports
		private static GUIStyle leftLabelStyle{
			get
			{
				if (_leftLabelStyle == null){
					_leftLabelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
					_leftLabelStyle.alignment = TextAnchor.UpperLeft;
				}
				return _leftLabelStyle;
			}
		}

		//for output ports
		private static GUIStyle rightLabelStyle{
			get
			{
				if (_rightLabelStyle == null){
					_rightLabelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
					_rightLabelStyle.alignment = TextAnchor.UpperRight;
				}
				return _rightLabelStyle;
			}
		}

		//when gathering ports and we are in Unity Editor
		//gather the ordered inputs and outputs
		void OnPortsGatheredInEditor(){
			orderedInputs = inputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowInput)? 0 : 1 ).ToArray();
			orderedOutputs = outputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowOutput)? 0 : 1 ).ToArray();
			firstValuePort = orderedInputs.OfType<ValueInput>().FirstOrDefault();
		}

		//Get all output Connections of a port. Used only for when removing
		BinderConnection[] GetOutPortConnections(Port port){
			return outConnections.Cast<BinderConnection>().Where(c => c.sourcePort == port ).ToArray();
		}

		//Get all input Connections of a port. Used only for when removing
		BinderConnection[] GetInPortConnections(Port port){
			return inConnections.Cast<BinderConnection>().Where(c => c.targetPort == port ).ToArray();
		}


		//Seal it...
		sealed protected override void DrawNodeConnections(Rect drawCanvas, bool fullDrawPass, Vector2 canvasMousePos, float zoomFactor){

			var e = Event.current;


			//Port container graphics
			if (fullDrawPass || drawCanvas.Overlaps(nodeRect)){
				GUI.Box(new Rect(nodeRect.x - 8, nodeRect.y + 2, 10, nodeRect.height), string.Empty, (GUIStyle)"nodePortContainer");
				GUI.Box(new Rect(nodeRect.xMax - 2, nodeRect.y + 2, 10, nodeRect.height), string.Empty, (GUIStyle)"nodePortContainer");
			}
			///


			if (fullDrawPass || drawCanvas.Overlaps(nodeRect)){

				var portRect = new Rect(0,0,10,10);

				//INPUT Ports
				if (orderedInputs != null){

					Port instancePort = null;

					for (var i = 0; i < orderedInputs.Length; i++){

						var port = orderedInputs[i];
						var canConnect = true;
						if ((port == clickedPort) ||
							(clickedPort is FlowInput || clickedPort is ValueInput) ||
							(port.isConnected && port is ValueInput) ||
							(clickedPort != null && clickedPort.parent == port.parent) ||
							(clickedPort != null && !TypeConverter.HasConvertion(clickedPort.type, port.type)) )
						{
							canConnect = false;
						}

						portRect.width = port.isConnected? 12:10;
						portRect.height = portRect.width;
						portRect.center = new Vector2(nodeRect.x - 5, port.pos.y);
						port.pos = portRect.center;

						//first gameobject or component port is considered to be the 'instance' port by convention
						if (port == firstValuePort){
							if ( (typeof(Component).IsAssignableFrom(port.type) || port.type == typeof(GameObject)) ){
								instancePort = port;
							}
						}

						//Port graphic
						if (clickedPort != null && !canConnect && clickedPort != port){
							GUI.color = new Color(1,1,1,0.3f);
						}
						GUI.Box(portRect, string.Empty, port.isConnected? (GUIStyle)"nodePortConnected" : (GUIStyle)"nodePortEmpty");
						GUI.color = Color.white;

						//Tooltip
						if (portRect.Contains(e.mousePosition)){
							
							var labelString = (canConnect || port.isConnected || port == clickedPort)? port.type.FriendlyName() : "Can't Connect Here";
							var size = GUI.skin.GetStyle("box").CalcSize(new GUIContent(labelString));
							var rect = new Rect(0, 0, size.x + 10, size.y + 5);
							rect.x = portRect.x - size.x - 10;
							rect.y = portRect.y - size.y/2;
							GUI.Box(rect, labelString);
						
						//Or value
						} else {

							if ( port is ValueInput && !port.isConnected){
								//Only these types are shown their value
								if ( port.type.IsValueType || port.type == typeof(Type) || port.type == typeof(string) || typeof(UnityEngine.Object).IsAssignableFrom(port.type) ){
									var value = (port as ValueInput).serializedValue;
									string labelString = null;
									if (!(port as ValueInput).isDefaultValue ){
										if (value is UnityEngine.Object && value as UnityEngine.Object != null){
											labelString = string.Format("<b><color=#66ff33>{0}</color></b>", (value as UnityEngine.Object).name);
										} else {
											labelString = value.ToStringAdvanced();
										}
									} else if ( port == instancePort ){
										var exists = true;
										if (graphAgent != null && typeof(Component).IsAssignableFrom(port.type) ){
											exists = graphAgent.GetComponent(port.type) != null;
										}
										var color = exists? "66ff33" : "ff3300";
										labelString = string.Format("<color=#{0}><b>♟ <i>Self</i></b></color>", color);
									} else {
										GUI.color = new Color(1,1,1,0.15f);
										labelString = value.ToStringAdvanced();
									}
									var size = GUI.skin.GetStyle("label").CalcSize(new GUIContent(labelString));
									var rect = new Rect(0, 0, size.x, size.y);
									rect.x = portRect.x - size.x - 5;
									rect.y = portRect.y - size.y * 0.3f; //*0.3? something's wrong here. FIX
									GUI.Label(rect, labelString);
									GUI.color = Color.white;
								}
							}
						}

						if (Graph.allowClick){
							//Right click removes connections
							if (port.isConnected && e.type == EventType.ContextClick && portRect.Contains(e.mousePosition)){
								foreach(var c in GetInPortConnections(port)){
									graph.RemoveConnection(c);
								}
								e.Use();
								return;
							}

							//Click initialize new drag & drop connection
							if (e.type == EventType.MouseDown && e.button == 0 && portRect.Contains(e.mousePosition)){
								if (port.CanAcceptConnections() ){
									dragDropMisses = 0;
									clickedPort = port;
									e.Use();
								}
							}

							//Drop on creates connection
							if (e.type == EventType.MouseUp && e.button == 0 && clickedPort != null){
								if (portRect.Contains(e.mousePosition) && port.CanAcceptConnections() ){
									ConnectPorts(clickedPort, port);
									clickedPort = null;
									e.Use();
								}
							}
						}

					}
				}

				//OUTPUT Ports
				if (orderedOutputs != null){

					for (var i = 0; i < orderedOutputs.Length; i++){

						var port = orderedOutputs[i];
						var canConnect = true;
						if ((port == clickedPort) ||
							(clickedPort is FlowOutput || clickedPort is ValueOutput) ||
							(port.isConnected && port is FlowOutput) ||
							(clickedPort != null && clickedPort.parent == port.parent) ||
							(clickedPort != null && !TypeConverter.HasConvertion(port.type, clickedPort.type)) )
						{
							canConnect = false;
						}

						portRect.width = port.isConnected? 12:10;
						portRect.height = portRect.width;
						portRect.center = new Vector2(nodeRect.xMax + 5, port.pos.y);
						port.pos = portRect.center;

						//Port graphic
						if (clickedPort != null && !canConnect && clickedPort != port){
							GUI.color = new Color(1,1,1,0.3f);
						}
						GUI.Box(portRect, string.Empty, port.isConnected? (GUIStyle)"nodePortConnected" : (GUIStyle)"nodePortEmpty");
						GUI.color = Color.white;

						//Tooltip
						if (portRect.Contains(e.mousePosition)){
							var labelString = (canConnect || port.isConnected || port == clickedPort)? port.type.FriendlyName() : "Can't Connect Here";
							var size = GUI.skin.GetStyle("label").CalcSize(new GUIContent(labelString));
							var rect = new Rect(0, 0, size.x + 10, size.y + 5);
							rect.x = portRect.x + 15;
							rect.y = portRect.y - portRect.height/2;
							GUI.Box(rect, labelString);
						}

						if (Graph.allowClick){
							//Right click removes connections
							if (e.type == EventType.ContextClick && portRect.Contains(e.mousePosition)){
								foreach(var c in GetOutPortConnections(port)){
									graph.RemoveConnection(c);
								}
								e.Use();
								return;
							}

							//Click initialize new drag & drop connection
							if (e.type == EventType.MouseDown && e.button == 0 && portRect.Contains(e.mousePosition)){
								if (port.CanAcceptConnections() ){
									dragDropMisses = 0;
									clickedPort = port;
									e.Use();
								}
							}

							//Drop on creates connection
							if (e.type == EventType.MouseUp && e.button == 0 && clickedPort != null){
								if (portRect.Contains(e.mousePosition) && port.CanAcceptConnections() ){
									ConnectPorts(port, clickedPort);
									clickedPort = null;
									e.Use();
								}
							}
						}
						
					}
				}
			}



			///ACCEPT CONNECTION
			if (clickedPort != null && e.type == EventType.MouseUp){

				///ON NODE
				if (nodeRect.Contains(e.mousePosition)){

					var cachePort = clickedPort;
					var menu = new GenericMenu();

					if (cachePort is ValueOutput || cachePort is FlowOutput){
						if (orderedInputs != null){
							foreach (var _port in orderedInputs.Where(p => p.CanAcceptConnections() && TypeConverter.HasConvertion(cachePort.type, p.type) )){
								var port = _port;
								menu.AddItem(new GUIContent(string.Format("To: '{0}'", port.name) ), false, ()=> { ConnectPorts(cachePort, port); } );
							}
						}
					} else {
						if (orderedOutputs != null){
							foreach (var _port in orderedOutputs.Where(p => p.CanAcceptConnections() && TypeConverter.HasConvertion(p.type, cachePort.type) )){
								var port = _port;
								menu.AddItem(new GUIContent(string.Format("From: '{0}'", port.name) ), false, ()=> { ConnectPorts(port, cachePort); } );
							}
						}
					} 

					//append menu items
					menu = OnDragAndDropPortContextMenu(menu, cachePort);

					//if there is only 1 option, just do it
					if (menu.GetItemCount() == 1){
						EditorUtils.GetMenuItems(menu)[0].func();
					} else {
						Graph.PostGUI += ()=> { menu.ShowAsContext(); };
					}

					clickedPort = null;
					e.Use();
					///

				///ON CANVAS
				} else {

					dragDropMisses ++;
					if (dragDropMisses == graph.allNodes.Count && clickedPort != null){
						var cachePort = clickedPort;
						clickedPort = null;
						DoContextPortConnectionMenu(cachePort, e.mousePosition, zoomFactor);
						e.Use();
					}
				}
			}



			//Temp connection line when linking
			if (clickedPort != null && clickedPort.parent == this){
				var xDiff = (clickedPort.pos.x - e.mousePosition.x) * 0.8f;
				xDiff = e.mousePosition.x > clickedPort.pos.x? xDiff : -xDiff;
				var tangA = clickedPort is FlowInput || clickedPort is ValueInput? new Vector2(xDiff, 0) : new Vector2(-xDiff, 0);
				var tangB = tangA * -1;
				Handles.DrawBezier(clickedPort.pos, e.mousePosition, clickedPort.pos + tangA , e.mousePosition + tangB, new Color(0.5f,0.5f,0.8f,0.8f), null, 3);
			}

			//Actualy draw the existing connections
			for (var i = 0; i < outConnections.Count; i++){
				var binder = outConnections[i] as BinderConnection;
				if (binder != null){ //for in case it's MissingConnection
					var sourcePort = binder.sourcePort;
					var targetPort = binder.targetPort;
					if (sourcePort != null && targetPort != null){
						if (fullDrawPass || drawCanvas.Overlaps(RectUtils.GetBoundRect(sourcePort.pos, targetPort.pos) ) ){
							binder.DrawConnectionGUI(sourcePort.pos, targetPort.pos);
						}
					}
				}
			}
		}


		///Let nodes handle ports draged on top of them
		virtual protected GenericMenu OnDragAndDropPortContextMenu(GenericMenu menu, Port port){
			return menu;
		}

		///Context menu for when dragging a connection on empty canvas
		///Unfortunately I can't think of a more automatic way
		void DoContextPortConnectionMenu(Port clickedPort, Vector2 mousePos, float zoomFactor){

			Action<FlowNode> FinalizeConnection = (targetNode) => {

				Port source = null;
				Port target = null;

				if (clickedPort is ValueOutput || clickedPort is FlowOutput){
					source = clickedPort;
					target = targetNode.GetFirstInputOfType(clickedPort.type);
				} else {
					source = targetNode.GetFirstOutputOfType(clickedPort.type);
					target = clickedPort;
				}

				ConnectPorts(source, target);
				Graph.currentSelection = targetNode;
			};


			var menu = new GenericMenu();

			//SimplexWrapper
			UnityEditor.GenericMenu.MenuFunction2 SelectedSimplex = (t) =>{
				var genericType = typeof(SimplexNodeWrapper<>).MakeGenericType(new System.Type[]{ (System.Type)t });
				var wrapper = (FlowNode)graph.AddNode(genericType, mousePos);
				FinalizeConnection(wrapper);
			};

			//MethodInfo Wrapper
			UnityEditor.GenericMenu.MenuFunction2 SelectedMethod = (object m) =>{
				var methodWrapper = graph.AddNode<ReflectedMethodNodeWrapper>(mousePos);
				methodWrapper.SetMethod((MethodInfo)m);
				FinalizeConnection(methodWrapper);
			};

			//Normal FG Node
			GenericMenu.MenuFunction2 SelectedFlowNode = (t) =>{
				var fNode = (FlowNode)graph.AddNode( (Type)t, mousePos);
				FinalizeConnection(fNode);
			};


			///Append reflection
			//Only the instance members are appended, along with extension methods
			if (clickedPort is ValueOutput){
				var methods = clickedPort.type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).ToList();
				methods.AddRange( clickedPort.type.GetExtensionMethods() );
				foreach (var m in methods.OrderBy(m => !m.IsStatic).OrderBy(m => m.IsSpecialName).OrderBy(m => m.DeclaringType != clickedPort.type ) ){
					if (m.IsGenericMethod){
						continue;
					}

					if (m.IsStatic && !m.GetParameters().Any(p => p.ParameterType.IsAssignableFrom(clickedPort.type))){
						continue;
					}

					if (m.IsObsolete()){
						continue;
					}

					var categoryName = clickedPort.type.FriendlyName() + "/";
					if (m.DeclaringType != clickedPort.type){
						categoryName += "More/";
					}
					var name = categoryName + m.SignatureName();
					var icon = UnityEditor.EditorGUIUtility.ObjectContent(null, clickedPort.type).image;
					menu.AddItem(new GUIContent(name, icon), false, SelectedMethod, m);
				}
			}


			//Append FlowControl nodes anyway in case of Flow clicked port
			//...
			if (clickedPort is FlowOutput || clickedPort is FlowInput){
				foreach(var info in EditorUtils.GetScriptInfosOfType(typeof(FlowControlNode))){
					menu.AddItem(new GUIContent(info.category + "/" + info.name, info.icon, info.description), false, SelectedFlowNode, info.type );
				}
			}

			//Append FlowNodes
			//...
			foreach(var info in EditorUtils.GetScriptInfosOfType(typeof(FlowNode))){
				var definedInputTypesAtt = info.type.RTGetAttribute<ContextDefinedInputsAttribute>(true);
				var definedOutputTypesAtt = info.type.RTGetAttribute<ContextDefinedOutputsAttribute>(true);
				if ( (clickedPort is ValueOutput || clickedPort is FlowOutput) && definedInputTypesAtt != null){
					if (definedInputTypesAtt.types.Any(t => t.IsAssignableFrom(clickedPort.type))){
						menu.AddItem(new GUIContent(info.category + "/" + info.name, info.icon, info.description), false, SelectedFlowNode, info.type);
					}
				}
				if ( (clickedPort is ValueInput || clickedPort is FlowInput) && definedOutputTypesAtt != null){
					if (definedOutputTypesAtt.types.Any(t => clickedPort.type.IsAssignableFrom(t) )){
						menu.AddItem(new GUIContent(info.category + "/" + info.name, info.icon, info.description), false, SelectedFlowNode, info.type);
					}
				}
			}
			

			//Append Simplex Nodes
			//...
			foreach (var info in EditorUtils.GetScriptInfosOfType(typeof(SimplexNode))){
				
				var method = info.type.GetMethod("Invoke");
				
				if (clickedPort is ValueOutput){
					var parameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
					if (parameterTypes.Length == 0)
						continue;
					if ( parameterTypes.Any(t => t.IsAssignableFrom(clickedPort.type)) )
						menu.AddItem(new GUIContent(info.category + "/" + info.name, info.icon, info.description), false, SelectedSimplex, info.type);
				}

				if (clickedPort is FlowOutput){
					if ( (method.ReturnType == typeof(void) || method.ReturnType == typeof(IEnumerator)) && !info.type.IsSubclassOf(typeof(ExtractorNode)) )
						menu.AddItem(new GUIContent(info.category + "/" + info.name, info.icon, info.description), false, SelectedSimplex, info.type );
				}

				if (clickedPort is ValueInput){
					if ((method.ReturnType == typeof(void) || method.ReturnType == typeof(IEnumerator)))
						continue;
					if (clickedPort.type.IsAssignableFrom(method.ReturnType))
						menu.AddItem(new GUIContent(info.category + "/" + info.name, info.icon, info.description), false, SelectedSimplex, info.type);
				}

				if (clickedPort is FlowInput){
					if ( (method.ReturnType == typeof(void) || method.ReturnType == typeof(IEnumerator)) && !info.type.IsSubclassOf(typeof(ExtractorNode)) )
						menu.AddItem(new GUIContent(info.category + "/" + info.name, info.icon, info.description), false, SelectedSimplex, info.type );					
				}
			}


			///Append reflection
			//methods accepting port type as a parameter
			if (clickedPort is ValueOutput){
				var types = UserTypePrefs.GetPreferedTypesList(typeof(object));
				foreach (var type in types){
					
					if (type == clickedPort.type){
						continue;
					}

					foreach (var m in type.GetMethods(BindingFlags.Instance| BindingFlags.Static | BindingFlags.Public).OrderBy(m => m.IsSpecialName) ){
						if (m.IsGenericMethod){
							continue;
						}

						if (!m.GetParameters().Any(p => p.ParameterType.IsAssignableFrom(clickedPort.type))){
							continue;
						}

						if (m.IsObsolete()){
							continue;
						}

						var categoryName = "Functions/Reflected/" + type.FriendlyName() + "/";
						var name = categoryName + m.SignatureName();
						var icon = UnityEditor.EditorGUIUtility.ObjectContent(null, type).image;
						menu.AddItem(new GUIContent(name, icon), false, SelectedMethod, m);
					}
				}				
			}

			//methods returning port type
			if (clickedPort is ValueInput){
				var types = UserTypePrefs.GetPreferedTypesList(typeof(object));
				types.Add(clickedPort.type);
				
				foreach (var type in types){
					foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).OrderBy(m => m.IsSpecialName) ){
						
						if (m.IsGenericMethod || m.ReturnType == typeof(void) || !clickedPort.type.IsAssignableFrom(m.ReturnType) ){
							continue;
						}

						if (m.IsObsolete()){
							continue;
						}
						
						var categoryName = "Functions/Reflected/" + type.FriendlyName() + "/";
						var name = categoryName + m.SignatureName();
						var icon = UnityEditor.EditorGUIUtility.ObjectContent(null, type).image;
						menu.AddItem(new GUIContent(name, icon), false, SelectedMethod, m);						
					}
				}
			}

			//all void methods
			if (clickedPort is FlowInput || clickedPort is FlowOutput) {
				foreach (var type in UserTypePrefs.GetPreferedTypesList(typeof(object))){
					foreach (var m in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).OrderBy(m => m.IsSpecialName) ){
						
						if (m.IsGenericMethod || m.ReturnType != typeof(void)){
							continue;
						}

						if (m.IsObsolete()){
							continue;
						}

						var categoryName = "Functions/Reflected/" + type.FriendlyName() + "/";
						var name = categoryName + m.SignatureName();
						var icon = UnityEditor.EditorGUIUtility.ObjectContent(null, type).image;
						menu.AddItem(new GUIContent(name, icon), false, SelectedMethod, m);
					}
				}
			}

			///


			
			//Append Variable related nodes
			//...
			var variables = new Dictionary<IBlackboard, List<Variable>>();
			if (graphBlackboard != null){
				variables[graphBlackboard] = graphBlackboard.variables.Values.ToList();
			}
			foreach(var globalBB in GlobalBlackboard.allGlobals){
				variables[globalBB] = globalBB.variables.Values.ToList();
			}

			if (clickedPort is ValueInput){
	
				//Append the very special Owner variable
				if ( clickedPort.type == typeof(GameObject) ){
					menu.AddItem(new GUIContent("Variables/Self"), false, SelectedFlowNode, typeof(OwnerVariable) );
				}

				//New var
				menu.AddItem(new GUIContent(string.Format("Variables/Graph Variable ({0})", clickedPort.type.FriendlyName() )), false, ()=>
				{
					var genericType = typeof(GetVariable<>).MakeGenericType(new System.Type[]{ clickedPort.type });
					SelectedFlowNode(genericType);
				});

				menu.AddItem(new GUIContent(string.Format("Variables/Get Blackboard Variable/Get Other Of Type ({0})", clickedPort.type.FriendlyName() )), false, ()=>
				{
					var genericType = typeof(GetOtherVariable<>).MakeGenericType(new System.Type[]{ clickedPort.type });
					SelectedFlowNode(genericType);
				});

				//BB var
				var bbVarAdded = false;
				foreach(var pair in variables){
					foreach (var _bbVar in pair.Value){
						var bbVar = _bbVar;
						var finalName = pair.Key == graphBlackboard? bbVar.name : string.Format("{0}/{1}", pair.Key.name, bbVar.name);
						if (clickedPort.type.IsAssignableFrom(bbVar.varType)){
							bbVarAdded = true;
							menu.AddItem(new GUIContent(string.Format("Variables/Get Blackboard Variable/Get {0}", finalName), null, "Variable from Blackboard"), false, ()=>
							{
								var genericType = typeof(GetVariable<>).MakeGenericType(new System.Type[]{bbVar.varType});
								var varNode = (VariableNode)graph.AddNode(genericType, mousePos);
								genericType.GetMethod("SetTargetVariableName").Invoke(varNode, new object[]{finalName});
								FinalizeConnection(varNode);
							});
						}
					}
				}

				if (!bbVarAdded){
					menu.AddDisabledItem(new GUIContent("Variables/Get Blackboard Variable") );
				}
			}

			if (clickedPort is ValueOutput){

				menu.AddItem(new GUIContent(string.Format("Variables/Set Blackboard Variable/Set Of Type ({0})", clickedPort.type.FriendlyName() )), false, ()=>
				{
					var genericType = typeof(SetVariable<>).MakeGenericType(new System.Type[]{clickedPort.type});
					SelectedFlowNode(genericType);
				});

				menu.AddItem(new GUIContent(string.Format("Variables/Set Blackboard Variable/Set Other Of Type ({0})", clickedPort.type.FriendlyName() )), false, ()=>
				{
					var genericType = typeof(SetOtherVariable<>).MakeGenericType(new System.Type[]{clickedPort.type});
					SelectedFlowNode(genericType);
				});

				var bbVarAdded = false;
				foreach(var pair in variables){
					foreach (var _bbVar in pair.Value){
						var bbVar = _bbVar;
						var finalName = pair.Key == graphBlackboard? bbVar.name : string.Format("{0}/{1}", pair.Key.name, bbVar.name);
						if (bbVar.varType.IsAssignableFrom(clickedPort.type)){
							bbVarAdded = true;
							menu.AddItem(new GUIContent(string.Format("Variables/Set Blackboard Variable/Set {0}", finalName), null, "Variable from Blackboard"), false, ()=>
							{
								var genericType = typeof(SetVariable<>).MakeGenericType(new System.Type[]{bbVar.varType});
								var varNode = (FlowNode)graph.AddNode(genericType, mousePos);
								genericType.GetMethod("SetTargetVariableName").Invoke(varNode, new object[]{ finalName });
								FinalizeConnection(varNode);
							});
						}
					}
				}
				
				if (!bbVarAdded){
					menu.AddDisabledItem(new GUIContent("Variables/Set Blackboard Variable") );
				}
			}


			//SHOW
			if (zoomFactor == 1 && NodeCanvas.Editor.NCPrefs.useBrowser){
				menu.ShowAsBrowser(string.Format("Add & Connect (Type of {0})", clickedPort.type.FriendlyName() ), graph.baseNodeType );
			} else {
				Graph.PostGUI += ()=> { menu.ShowAsContext(); };
			}
			Event.current.Use();
		}


		//seal it...Draw the port names in order
		sealed protected override void OnNodeGUI(){

			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			if (orderedInputs != null){
				for (var i = 0; i < orderedInputs.Length; i++){
					var inPort = orderedInputs[i];
					if (inPort is FlowInput){
						GUILayout.Label(string.Format("<b>► {0}</b>", inPort.name), leftLabelStyle);
					} else {
						var enumerableList = typeof(IEnumerable).IsAssignableFrom(inPort.type) && (inPort.type.IsGenericType || inPort.type.IsArray);
						GUILayout.Label(string.Format("<color={0}>{1}{2}</color>", UserTypePrefs.GetTypeHexColor(inPort.type), enumerableList? "#" : string.Empty , inPort.name ), leftLabelStyle);
					}
					if (Event.current.type == EventType.Repaint){
						inPort.pos = new Vector2(inPort.pos.x, GUILayoutUtility.GetLastRect().center.y + nodeRect.y);
					}
				}
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			if (orderedOutputs != null){
				for (var i = 0; i < orderedOutputs.Length; i++){
					var outPort = orderedOutputs[i];
					if (outPort is FlowOutput){
						GUILayout.Label(string.Format("<b>{0} ►</b>", outPort.name), rightLabelStyle);
					} else {
						var enumerableList = typeof(IEnumerable).IsAssignableFrom(outPort.type) && (outPort.type.IsGenericType || outPort.type.IsArray);
						GUILayout.Label(string.Format("<color={0}>{1}{2}</color>", UserTypePrefs.GetTypeHexColor(outPort.type), enumerableList? "#" : string.Empty, outPort.name), rightLabelStyle);
					}
					if (Event.current.type == EventType.Repaint){
						outPort.pos = new Vector2(outPort.pos.x, GUILayoutUtility.GetLastRect().center.y + nodeRect.y);
					}
				}
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		//The inspector panel
		protected override void OnNodeInspectorGUI(){
			DrawDefaultInspector();
			if (this is IMultiPortNode){

				if (GUILayout.Button("Add Port")){
					((IMultiPortNode)this).portCount ++;
					GatherPorts();
				}

				GUI.enabled = ((IMultiPortNode)this).portCount > 1;
				if (GUILayout.Button("Remove Port")){
					var count = ((IMultiPortNode)this).portCount;
					count = Mathf.Max(1, count - 1);
					((IMultiPortNode)this).portCount = count;
					GatherPorts();
				}
				GUI.enabled = true;

			}
			EditorUtils.Separator();
			DrawValueInputsGUI();
		}

		//Show the serialized input port values if any
		protected void DrawValueInputsGUI(){

			foreach (var input in inputPorts.Values.OfType<ValueInput>() ){

				if (input.isConnected){
					EditorGUILayout.LabelField(input.name, "[CONNECTED]");
					continue;
				}

				input.serializedValue = EditorUtils.GenericField(input.name, input.serializedValue, input.type, null);
			}
		}


		//Override of right click node context menu
		protected override GenericMenu OnContextMenu(GenericMenu menu){

			if (outputPorts.Values.Any(p => p is FlowOutput)){ //breakpoints only work with FlowOutputs
				menu.AddItem(new GUIContent("Breakpoint"), isBreakpoint, ()=>{ isBreakpoint = !isBreakpoint; });
			}

			var type = this.GetType();
			if (type.IsGenericType){
				var infos = EditorUtils.GetScriptInfosOfType( type.GetGenericTypeDefinition() );
				foreach(var _info in infos){
					var info = _info;
					menu.AddItem(new GUIContent("Change Type/" + info.name), false, ()=>
					{
						var newNode = this.ReplaceWith(info.type);
						newNode.GatherPorts();
					});
				}
			}

			return menu;
		}

		protected FlowNode ReplaceWith(System.Type t){
			var newNode = graph.AddNode(t, this.nodePosition);
			if (newNode == null) return null;
			foreach(var c in inConnections.ToArray()){
				c.SetTarget(newNode);
			}
			foreach(var c in outConnections.ToArray()){
				c.SetSource(newNode);
			}
			graph.RemoveNode(this);
			return (FlowNode)newNode;
		}

		#endif
	}
}