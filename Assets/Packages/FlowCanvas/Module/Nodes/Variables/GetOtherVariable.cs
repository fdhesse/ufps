using UnityEngine;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace FlowCanvas.Nodes{

	[Name("Get Other Of Type<T>")]
	[Category("Variables/Get Blackboard Variable")]
	[Description("Use this to get a variable value from other blackboards than the one this flowscript is using")]
	[AppendListTypes]
	public class GetOtherVariable<T> : VariableNode{
		public override string name{get {return "Get Variable";}}
		protected override void RegisterPorts(){
			var bb = AddValueInput<Blackboard>("Blackboard");
			var varName = AddValueInput<string>("Variable");
			AddValueOutput<T>("Value", ()=>{ return bb.value.GetValue<T>(varName.value); });
		}
		public override void SetVariable(object o){}
	}
}