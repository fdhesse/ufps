#define DO_EDITOR_BINDING //comment this out to test the real performance without editor binding specifics

using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Services;
using UnityEngine;
using NodeCanvas;


namespace FlowCanvas{

	public class BinderConnection : Connection {

		[SerializeField]
		private string _sourcePortName;
		[SerializeField]
		private string _targetPortName;

		[System.NonSerialized]
		private Port _sourcePort;
		[System.NonSerialized]
		private Port _targetPort;

		///The source port ID name this binder is connected to
		public string sourcePortID{
			get {return sourcePort != null? sourcePort.ID : _sourcePortName;}
			private set {_sourcePortName = value;}
		}

		///The target port ID name this binder is connected to
		public string targetPortID{
			get {return targetPort != null? targetPort.ID : _targetPortName;}
			private set {_targetPortName = value;}
		}

		///The source Port
		public Port sourcePort{
			get
			{
				if (_sourcePort == null){
					if (sourceNode is FlowNode){ //In case it's 'MissingNode'
						_sourcePort = (sourceNode as FlowNode).GetOutputPort(_sourcePortName);
					}
				}
				return _sourcePort;
			}
		}

		///The target Port
		public Port targetPort{
			get
			{
				if (_targetPort == null){
					if (targetNode is FlowNode){ //In case it's 'MissingNode'
						_targetPort = (targetNode as FlowNode).GetInputPort(_targetPortName);
					}
				}
				return _targetPort;
			}
		}

		//The binder type. In case of Value connection, BinderConnection<T> is used, else it's basicaly a Flow binding
		private System.Type bindingType{
			get { return GetType().RTIsGenericType()? GetType().RTGetGenericArguments()[0] : typeof(Flow); }
		}

		//Called after the node has GatherPorts to gather the references and validate the binding connection
		public void GatherAndValidateSourcePort(){
			_sourcePort = null;
			if (sourcePort != null && TypeConverter.HasConvertion(sourcePort.type, bindingType)){
				sourcePort.connections ++;
			} else {
				graph.RemoveConnection(this, false);
			}
		}

		//Called after the node has GatherPorts to gather the references and validate the binding connection
		public void GatherAndValidateTargetPort(){
			_targetPort = null;
			if (targetPort != null && targetPort.type == bindingType){
				targetPort.connections ++;
			} else {
				graph.RemoveConnection(this, false);
			}
		}


		///Create a NEW binding connection object between two ports
		public static BinderConnection Create(Port source, Port target){

			if (source == null || target == null){
				Debug.LogError("Source Port or Target Port is null when making a new Binder Connection");
				return null;
			}

			if (!source.CanAcceptConnections()){
				Debug.LogWarning("Source port can accept no more connections");
				return null;
			}

			if (!target.CanAcceptConnections()){
				Debug.LogWarning("Target port can accept no more connections");
				return null;				
			}

			if (source.parent == target.parent){
				Debug.LogWarning("Can't connect ports on the same parent node");
				return null;
			}


			if (source is FlowOutput && !(target is FlowInput) ){
				Debug.LogWarning("Flow ports can only be connected to other Flow ports");
				return null;
			}

			if ( (source is FlowInput && target is FlowInput) || (source is ValueInput && target is ValueInput) ){
				Debug.LogWarning("Can't connect input to input");
				return null;
			}

			if ( (source is FlowOutput && target is FlowOutput) || (source is ValueOutput && target is ValueOutput) ){
				Debug.LogWarning("Can't connect output to output");
				return null;
			}

			if (!TypeConverter.HasConvertion(source.type, target.type)){
				Debug.LogWarning(string.Format("Can't connect ports. Type '{0}' is not assignable from Type '{1}' and there exists no internal convertion for those types.", target.type.FriendlyName(), source.type.FriendlyName() ));
				return null;
			}

			
			#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(source.parent.graph, "Connect Ports");
			#endif


			if (source is FlowOutput && target is FlowInput){
				var flowBind = new BinderConnection();
				flowBind.OnCreate(source, target);
				return flowBind;
			}

			if (source is ValueOutput && target is ValueInput){
				var valueBind = (BinderConnection)System.Activator.CreateInstance( typeof(BinderConnection<>).RTMakeGenericType(new System.Type[]{target.type}));
				valueBind.OnCreate(source, target);
				return valueBind;
			}

			return null;
		}

		///Called in runtime intialize to actualy bind the delegates
		virtual public void Bind(){
			
			if (!isActive){
				return;
			}

			if (sourcePort is FlowOutput && targetPort is FlowInput){
				(sourcePort as FlowOutput).BindTo( (FlowInput)targetPort );

				#if UNITY_EDITOR && DO_EDITOR_BINDING
				(sourcePort as FlowOutput).Append( (f)=> {BlinkStatus(f);} );
				#endif
			}
		}


		///UnBinds the delegates
		virtual public void UnBind(){
			if (sourcePort is FlowOutput){
				(sourcePort as FlowOutput).UnBind();
			}
		}

		///Initialize some references when a connection has been made
		void OnCreate(Port source, Port target){
			sourceNode = source.parent;
			targetNode = target.parent;
			sourcePortID = source.ID;
			targetPortID = target.ID;
			sourceNode.outConnections.Add(this);
			targetNode.inConnections.Add(this);
			
			source.connections ++;
			target.connections ++;

			//for live editing
			if (Application.isPlaying){
				Bind();
			}
		}

		//callback from base class. The connection reference is already removed from target and source Nodes
		public override void OnDestroy(){
			if (sourcePort != null) //check null for cases like the SwitchInt, where basicaly the source port is null since pin is removed first
				sourcePort.connections --;
			if (targetPort != null)
				targetPort.connections --;

			//for live editing
			if (Application.isPlaying){
				UnBind();
			}
		}



		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		private int blinks;
		
		protected override TipConnectionStyle tipConnectionStyle{ get {return TipConnectionStyle.None;} }
		protected override bool canRelink{ get {return false;} }

		//TODO: Better implement blinking in base class?
		protected void BlinkStatus(Flow f){
			if (Application.isPlaying){
				blinks ++;
				if (blinks <= 1){
					MonoManager.current.StartCoroutine(Internal_BlinkStatus(f));
				}
			}
		}

		IEnumerator Internal_BlinkStatus(Flow f){
			status = Status.Running;
			while (blinks > 0){
				yield return null;
				blinks --;
			}
			status = Status.Resting;
		}

		#endif
	}
}