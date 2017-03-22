using System;
using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{

	[Category("Flow Controllers/Switchers")]
	[Description("Branch the Flow based on a comparison between two comparable objects")]
	[ContextDefinedInputs(typeof(IComparable))]
	public class SwitchComparison : FlowControlNode {

		protected override void RegisterPorts(){
			var equal    = AddFlowOutput("==");
			var notEqual = AddFlowOutput("!=");
			var greater  = AddFlowOutput(">");
			var less     = AddFlowOutput("<");

			var a = AddValueInput<IComparable>("A");
			var b = AddValueInput<IComparable>("B");
			AddFlowInput("In", (f)=>{

				var valueA = a.value;
				var valueB = b.value;

				if (valueA == null || valueB == null){

					if (valueA == valueB){
						equal.Call(f);
					}

					if (valueA != valueB){
						notEqual.Call(f);
					}

				} else {

					if (valueA.CompareTo(valueB) == 0){
						equal.Call(f);
					} else {
						notEqual.Call(f);
					}

					if (valueA.CompareTo(valueB) == 1){
						greater.Call(f);
					}

					if (valueA.CompareTo(valueB) == -1){
						less.Call(f);
					}
				}
			});
		}
	}
}