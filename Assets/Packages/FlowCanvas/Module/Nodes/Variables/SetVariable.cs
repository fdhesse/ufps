using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[DoNotList]
	[Name("Set Of Type<T>")]
	[Category("Variables/Set Blackboard Variable")]
	[Description("Set a Blackboard variable value")]
	[AppendListTypes]
	public class SetVariable<T> : FlowNode {
		
		[BlackboardOnly]
		public BBParameter<T> targetVariable;

		[HideInInspector]
		public OperationMethod operation = OperationMethod.Set;

		public override string name {
			get { return string.Format("{0}{1}{2}", targetVariable.ToString(), OperationTools.GetOperationString(operation), "Value"); }
		}

		protected override void RegisterPorts(){
			var o = AddFlowOutput("Out");
			var v = AddValueInput<T>("Value");
			AddValueOutput<T>("Value", ()=> {return targetVariable.value; });
			AddFlowInput("In", (f)=> { DoSet(v.value); o.Call(f); });
		}

		void DoSet(T value){
			if (operation != OperationMethod.Set){
				if (typeof(T) == typeof(float))
					targetVariable.value = (T)(object)OperationTools.Operate((float)(object)targetVariable.value, (float)(object)value, operation);
				else if (typeof(T) == typeof(int))
					targetVariable.value = (T)(object)OperationTools.Operate((int)(object)targetVariable.value, (int)(object)value, operation);
				else if (typeof(T) == typeof(Vector3))
					targetVariable.value = (T)(object)OperationTools.Operate((Vector3)(object)targetVariable.value, (Vector3)(object)value, operation);
				else targetVariable.value = value;
			} else {
				targetVariable.value = value;
			}
		}

		public void SetTargetVariableName(string name){
			targetVariable.name = name;
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
			
		protected override void OnNodeInspectorGUI(){
			DrawDefaultInspector();
			if ( typeof(T) == typeof(float) || typeof(T) == typeof(int) || typeof(T) == typeof(Vector3) ){
				operation = (OperationMethod)UnityEditor.EditorGUILayout.EnumPopup("Operation", operation);
			}
			EditorUtils.BoldSeparator();
			base.DrawValueInputsGUI();
		}

		#endif
		
	}
}