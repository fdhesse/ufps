using System.Collections;
using System.Collections.Generic;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{


	[Description("Enumerate a value (usualy a list or array) for each of it's elements")]
	[Category("Flow Controllers/Iterators")]
	[ContextDefinedInputs(typeof(IEnumerable))]
	[ContextDefinedOutputs(typeof(object))]
	public class ForEach : FlowControlNode {
		
		private object current;
		private bool broken;

		protected override void RegisterPorts(){
			var list = AddValueInput<IEnumerable>("Value");
			AddValueOutput<object>("Current", ()=> {return current;});
			var fCurrent = AddFlowOutput("Do");
			var fFinish = AddFlowOutput("Done");
			AddFlowInput("In", (f)=>
			{
				var li = list.value;
				if (li == null){
					fFinish.Call(f);
					return;
				}

				broken = false;
				foreach(var o in li){
					if (broken){
						break;
					}
					current = o;
					fCurrent.Call(f);
				}

				fFinish.Call(f);
			});

			AddFlowInput("Break", (f)=>{ broken = true; });
		}
	}

	[Description("Enumerate a value (usualy a list or array) for each of it's elements")]
	[Category("Flow Controllers/Iterators")]
	public class ForEach<T> : FlowControlNode {
		
		private T current;
		private bool broken;

		protected override void RegisterPorts(){
			var list = AddValueInput<IEnumerable<T>>("Value");
			AddValueOutput<T>("Current", ()=> {return current;} );
			var fCurrent = AddFlowOutput("Do");
			var fFinish = AddFlowOutput("Done");
			AddFlowInput("In", (f)=>
			{
				var li = list.value;
				if (li == null){
					fFinish.Call(f);
					return;
				}

				broken = false;
				foreach(var o in li){
					if (broken){
						break;
					}
					current = o;
					fCurrent.Call(f);
				}
				fFinish.Call(f);
			});

			AddFlowInput("Break", (f)=>{ broken = true; });
		}
	}
}