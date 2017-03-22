using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Services;

namespace FlowCanvas.Nodes{

	///Latent Action Nodes do not return any value and can span within multiple frames and have up to 5 parameters. They need Flow execution.
	abstract public class LatentActionNodeBase : SimplexNode {
		
		private FlowOutput outFlow;
		private FlowOutput doing;
		private FlowOutput done;

		private Queue<IEnumerator> queue = new Queue<IEnumerator>();
		private UnityEngine.Coroutine currentCoroutine;
		private bool graphPaused;

		public override string name{
			get	{ return queue.Count > 0? string.Format("{0} [{1}]", base.name, queue.Count.ToString()) : base.name; }
		}

		sealed public override void OnGraphStarted(){ queue = new Queue<IEnumerator>();	currentCoroutine = null; }
		sealed public override void OnGraphStoped(){ Break(); }
		sealed public override void OnGraphPaused(){ graphPaused = true; }
		sealed public override void OnGraphUnpaused(){	graphPaused = false; }

		//begins a new coroutine
		protected void Begin(IEnumerator enumerator, Flow f){
			
			if (!queue.Contains(enumerator)){
				queue.Enqueue(enumerator);
			}

			if (currentCoroutine == null){
				currentCoroutine = MonoManager.current.StartCoroutine( InternalCoroutine(enumerator, f) );
			}
		}

		//breaks all coroutine queues
		protected void Break(){
			if (currentCoroutine != null){
				MonoManager.current.StopCoroutine(currentCoroutine);
				queue = new Queue<IEnumerator>();
				currentCoroutine = null;
				outFlow.parent.SetStatus(NodeCanvas.Status.Resting);
				OnBreak();
				done.Call(new Flow());
			}
		}

		IEnumerator InternalCoroutine(IEnumerator enumerator, Flow f){
			var parentNode = outFlow.parent;
			parentNode.SetStatus(NodeCanvas.Status.Running);
			outFlow.Call(f);

			while(enumerator.MoveNext()){
				while(graphPaused){
					yield return null;
				}
				doing.Call(f);
				yield return enumerator.Current;
			}

			parentNode.SetStatus(NodeCanvas.Status.Resting);
			done.Call(f);
			currentCoroutine = null;

			if (queue.Count > 0){
				queue.Dequeue();
				if (queue.Count > 0){
					Begin( queue.Peek(), f );
				}
			}
		}

		protected override void OnRegisterPorts(FlowNode node){
			//to make update safe from previous version, the ID (2nd string), is same as the old version. The first string, is the actual name.
			outFlow = node.AddFlowOutput("Start", "Out");
			doing   = node.AddFlowOutput("Update", "Doing");
			done    = node.AddFlowOutput("Finish", "Done");
		}

		///Called when Break input is called.
		virtual public void OnBreak(){}
	}

	abstract public class LatentActionNode : LatentActionNodeBase{
		abstract public IEnumerator Invoke();
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			node.AddFlowInput("In", (f)=> { Begin( Invoke(), f );  });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}
	}	

	abstract public class LatentActionNode<T1> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value), f );  });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}
	}

	abstract public class LatentActionNode<T1, T2> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value), f );  });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3, T4> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c, T4 d);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			var p4 = node.AddValueInput<T4>(parameters[3].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value, p4.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3, T4, T5> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c, T4 d, T5 e);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			var p4 = node.AddValueInput<T4>(parameters[3].Name.SplitCamelCase());
			var p5 = node.AddValueInput<T5>(parameters[4].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value, p4.value, p5.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3, T4, T5, T6> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			var p4 = node.AddValueInput<T4>(parameters[3].Name.SplitCamelCase());
			var p5 = node.AddValueInput<T5>(parameters[4].Name.SplitCamelCase());
			var p6 = node.AddValueInput<T6>(parameters[5].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3, T4, T5, T6, T7> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			var p4 = node.AddValueInput<T4>(parameters[3].Name.SplitCamelCase());
			var p5 = node.AddValueInput<T5>(parameters[4].Name.SplitCamelCase());
			var p6 = node.AddValueInput<T6>(parameters[5].Name.SplitCamelCase());
			var p7 = node.AddValueInput<T7>(parameters[6].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3, T4, T5, T6, T7, T8> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			var p4 = node.AddValueInput<T4>(parameters[3].Name.SplitCamelCase());
			var p5 = node.AddValueInput<T5>(parameters[4].Name.SplitCamelCase());
			var p6 = node.AddValueInput<T6>(parameters[5].Name.SplitCamelCase());
			var p7 = node.AddValueInput<T7>(parameters[6].Name.SplitCamelCase());
			var p8 = node.AddValueInput<T8>(parameters[7].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3, T4, T5, T6, T7, T8, T9> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h, T9 i);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			var p4 = node.AddValueInput<T4>(parameters[3].Name.SplitCamelCase());
			var p5 = node.AddValueInput<T5>(parameters[4].Name.SplitCamelCase());
			var p6 = node.AddValueInput<T6>(parameters[5].Name.SplitCamelCase());
			var p7 = node.AddValueInput<T7>(parameters[6].Name.SplitCamelCase());
			var p8 = node.AddValueInput<T8>(parameters[7].Name.SplitCamelCase());
			var p9 = node.AddValueInput<T9>(parameters[8].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value, p9.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

	abstract public class LatentActionNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : LatentActionNodeBase{
		abstract public IEnumerator Invoke(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h, T9 i, T10 j);
		sealed protected override void OnRegisterPorts(FlowNode node){
			base.OnRegisterPorts(node);
			var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
			var p2 = node.AddValueInput<T2>(parameters[1].Name.SplitCamelCase());
			var p3 = node.AddValueInput<T3>(parameters[2].Name.SplitCamelCase());
			var p4 = node.AddValueInput<T4>(parameters[3].Name.SplitCamelCase());
			var p5 = node.AddValueInput<T5>(parameters[4].Name.SplitCamelCase());
			var p6 = node.AddValueInput<T6>(parameters[5].Name.SplitCamelCase());
			var p7 = node.AddValueInput<T7>(parameters[6].Name.SplitCamelCase());
			var p8 = node.AddValueInput<T8>(parameters[7].Name.SplitCamelCase());
			var p9 = node.AddValueInput<T9>(parameters[8].Name.SplitCamelCase());
			var p10 = node.AddValueInput<T10>(parameters[9].Name.SplitCamelCase());
			node.AddFlowInput("In", (f)=> { Begin( Invoke(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value, p9.value, p10.value), f );   });
			node.AddFlowInput("Break", (f)=> { Break(); } );
		}		
	}

}