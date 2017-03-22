using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Name("Graph Variable<T>")]
	[Category("Variables")]
	[Description("Returns a constant or linked variable value.\nYou can alter between constant or linked at any time using the radio button.")]
	[AppendListTypes]
	public class GetVariable<T> : VariableNode {
		
		public BBParameter<T> value;
		
		#if UNITY_EDITOR
		public override string name{
			get
			{
				var size = typeof(T).IsValueType && !value.useBlackboard && UnityEditor.EditorGUIUtility.isProSkin? 20 : 12;
				return string.Format("<size={0}><color=#ffffff>{1}</color></size>", size.ToString(), value.ToString());
			}
		}
		#endif

		protected override void RegisterPorts(){
			AddValueOutput<T>("Value", ()=> {return value.value; });
		}

		public void SetTargetVariableName(string name){
			value.name = name;
		}

		public override void SetVariable(object o){

			if (o is Variable<T>){
				value.name = (o as Variable<T>).name;
				return;
			}

			if (o is T){
				value.value = (T)o;
				return;
			}
			
			Debug.LogError("Set Variable Error");
		}
	}
}