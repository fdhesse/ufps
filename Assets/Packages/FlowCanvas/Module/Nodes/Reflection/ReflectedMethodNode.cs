using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Serialization;
using UnityEngine;

namespace FlowCanvas.Nodes{

	///Wrapped in a ReflectedMethodNodeWrapper it furthers wraps a MethodInfo call to delegates and strong type value usage for much greater performance than simply reflection Invoke.
	abstract public class ReflectedMethodNode{

		protected delegate void ActionCall();
		protected delegate void ActionCall<T1>(T1 a);
		protected delegate void ActionCall<T1, T2>(T1 a, T2 b);
		protected delegate void ActionCall<T1, T2, T3>(T1 a, T2 b, T3 c);
		protected delegate void ActionCall<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d);
		protected delegate void ActionCall<T1, T2, T3, T4, T5>(T1 a, T2 b, T3 c, T4 d, T5 e);
		protected delegate void ActionCall<T1, T2, T3, T4, T5, T6>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);
		protected delegate void ActionCall<T1, T2, T3, T4, T5, T6, T7>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g);
		protected delegate void ActionCall<T1, T2, T3, T4, T5, T6, T7, T8>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h);

		protected delegate TResult FunctionCall<TResult>();
		protected delegate TResult FunctionCall<T1, TResult>(T1 a);
		protected delegate TResult FunctionCall<T1, T2, TResult>(T1 a, T2 b);
		protected delegate TResult FunctionCall<T1, T2, T3, TResult>(T1 a, T2 b, T3 c);
		protected delegate TResult FunctionCall<T1, T2, T3, T4, TResult>(T1 a, T2 b, T3 c, T4 d);
		protected delegate TResult FunctionCall<T1, T2, T3, T4, T5, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e);
		protected delegate TResult FunctionCall<T1, T2, T3, T4, T5, T6, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f);
		protected delegate TResult FunctionCall<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g);
		protected delegate TResult FunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h);

		//required
		public ReflectedMethodNode(){}

		public static ReflectedMethodNode Create(MethodInfo method){

			if (method.ReturnType == typeof(void)){

				Type type = null;
				var argTypes = new List<Type>();
				var parameters = method.GetParameters();
				var length = parameters.Length;
				if (!method.IsStatic){
					length ++;
					argTypes.Add(method.DeclaringType);
				}
				if (length == 0) type = typeof(ReflectedActionNode);
				if (length == 1) type = typeof(ReflectedActionNode<>);
				if (length == 2) type = typeof(ReflectedActionNode<,>);
				if (length == 3) type = typeof(ReflectedActionNode<,,>);
				if (length == 4) type = typeof(ReflectedActionNode<,,,>);
				if (length == 5) type = typeof(ReflectedActionNode<,,,,>);
				if (length == 6) type = typeof(ReflectedActionNode<,,,,,>);
				if (length == 7) type = typeof(ReflectedActionNode<,,,,,,>);
				if (length == 8) type = typeof(ReflectedActionNode<,,,,,,,>);
				if (length >= 9){
					Debug.LogError("ReflectedActionNode currently supports up to 8 parameters");
					return null;
				}

				argTypes.AddRange( parameters.Select(p => p.ParameterType) );
				
				return (ReflectedMethodNode)Activator.CreateInstance( argTypes.Count > 0? type.RTMakeGenericType(argTypes.ToArray()) : type );

			} else {

				Type type = null;
				var argTypes = new List<Type>();
				var parameters = method.GetParameters();
				var length = parameters.Length;
				if (!method.IsStatic){
					length ++;
					argTypes.Add(method.DeclaringType);
				}
				if (length == 0) type = typeof(ReflectedFunctionNode<>);
				if (length == 1) type = typeof(ReflectedFunctionNode<,>);
				if (length == 2) type = typeof(ReflectedFunctionNode<,,>);
				if (length == 3) type = typeof(ReflectedFunctionNode<,,,>);
				if (length == 4) type = typeof(ReflectedFunctionNode<,,,,>);
				if (length == 5) type = typeof(ReflectedFunctionNode<,,,,,>);
				if (length == 6) type = typeof(ReflectedFunctionNode<,,,,,,>);
				if (length == 7) type = typeof(ReflectedFunctionNode<,,,,,,,>);
				if (length == 8) type = typeof(ReflectedFunctionNode<,,,,,,,,>);
				if (length >= 9){
					Debug.LogError("ReflectedFunctionNode currently supports up to 8 parameters");
					return null;					
				}
				
				argTypes.AddRange( parameters.Select(p => p.ParameterType) );
				argTypes.Add(method.ReturnType);

				return (ReflectedMethodNode)Activator.CreateInstance( type.RTMakeGenericType(argTypes.ToArray()) );
			}
		}

		//helper method
		protected string GetName(MethodInfo method, int i){
			if (method == null) return null;
			var parameters = method.GetParameters();
			if (method.IsStatic) return parameters[i].Name.SplitCamelCase();
			var instanceName = method.DeclaringType.FriendlyName();
			if (i == 0) return instanceName;
			var paramName = parameters[i - 1].Name.SplitCamelCase();
			return paramName != instanceName? paramName : paramName + " "; //for rare cases where it's the same like for example Animation class.
		}


		abstract public void RegisterPorts(FlowNode node, MethodInfo method, bool callable);
	
	}



	sealed public class ReflectedActionNode : ReflectedMethodNode {

		private ActionCall call;
		void Call(){ call(); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall>(null);
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(); o.Call(f); } );			
		}
	}

	sealed public class ReflectedActionNode<T1> : ReflectedMethodNode {

		private ActionCall<T1> call;
		private T1 instance;
		void Call(T1 a){ instance = a; call(a); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}

	sealed public class ReflectedActionNode<T1, T2> : ReflectedMethodNode {

		private ActionCall<T1, T2> call;
		private T1 instance;
		void Call(T1 a, T2 b){ instance = a; call(a, b); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1, T2>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}

	sealed public class ReflectedActionNode<T1, T2, T3> : ReflectedMethodNode {

		private ActionCall<T1, T2, T3> call;
		private T1 instance;
		void Call(T1 a, T2 b, T3 c){ instance = a; call(a, b, c); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1, T2, T3>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}

	sealed public class ReflectedActionNode<T1, T2, T3, T4> : ReflectedMethodNode {

		private ActionCall<T1, T2, T3, T4> call;
		private T1 instance;
		void Call(T1 a, T2 b, T3 c, T4 d){ instance = a; call(a, b, c, d); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1, T2, T3, T4>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}

	sealed public class ReflectedActionNode<T1, T2, T3, T4, T5> : ReflectedMethodNode {

		private ActionCall<T1, T2, T3, T4, T5> call;
		private T1 instance;
		void Call(T1 a, T2 b, T3 c, T4 d, T5 e){ instance = a; call(a, b, c, d, e); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1, T2, T3, T4, T5>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}

	sealed public class ReflectedActionNode<T1, T2, T3, T4, T5, T6> : ReflectedMethodNode {

		private ActionCall<T1, T2, T3, T4, T5, T6> call;
		private T1 instance;
		void Call(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f){ instance = a; call(a, b, c, d, e, f); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1, T2, T3, T4, T5, T6>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			var p6 = node.AddValueInput<T6>(GetName(method, 5));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}

	sealed public class ReflectedActionNode<T1, T2, T3, T4, T5, T6, T7> : ReflectedMethodNode {

		private ActionCall<T1, T2, T3, T4, T5, T6, T7> call;
		private T1 instance;
		void Call(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g){ instance = a; call(a, b, c, d, e, f, g); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1, T2, T3, T4, T5, T6, T7>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			var p6 = node.AddValueInput<T6>(GetName(method, 5));
			var p7 = node.AddValueInput<T7>(GetName(method, 6));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}

	sealed public class ReflectedActionNode<T1, T2, T3, T4, T5, T6, T7, T8> : ReflectedMethodNode {

		private ActionCall<T1, T2, T3, T4, T5, T6, T7, T8> call;
		private T1 instance;
		void Call(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h){ instance = a; call(a, b, c, d, e, f, g, h); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<ActionCall<T1, T2, T3, T4, T5, T6, T7, T8>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			var p6 = node.AddValueInput<T6>(GetName(method, 5));
			var p7 = node.AddValueInput<T7>(GetName(method, 6));
			var p8 = node.AddValueInput<T8>(GetName(method, 7));
			var o = node.AddFlowOutput(" ");
			node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value); o.Call(f); } );
			if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
		}
	}





	sealed public class ReflectedFunctionNode<TResult> : ReflectedMethodNode{

		private FunctionCall<TResult> call;
		private TResult returnValue;
		TResult Call(){ return returnValue = call(); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<TResult>>(null);
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=>{ Call(); o.Call(f); });
			}
			node.AddValueOutput<TResult>("Value", ()=> { return callable? returnValue : Call(); } );
		}
	}

	sealed public class ReflectedFunctionNode<T1, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a){ instance = a; return returnValue = call(a); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, TResult>>( null );
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=> { return callable? returnValue : Call(p1.value); } );
		}
	}


	sealed public class ReflectedFunctionNode<T1, T2, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, T2, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a, T2 b){ instance = a; return returnValue = call(a, b); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, T2, TResult>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=>{ return callable? returnValue : Call(p1.value, p2.value); });
		}
	}

	sealed public class ReflectedFunctionNode<T1, T2, T3, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, T2, T3, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a, T2 b, T3 c){ instance = a; return returnValue = call(a, b, c); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, T2, T3, TResult>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=>{ return callable? returnValue : Call(p1.value, p2.value, p3.value); });
		}
	}

	sealed public class ReflectedFunctionNode<T1, T2, T3, T4, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, T2, T3, T4, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a, T2 b, T3 c, T4 d){ instance = a; return returnValue = call(a, b, c, d); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, T2, T3, T4, TResult>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=>{ return callable? returnValue : Call(p1.value, p2.value, p3.value, p4.value); });
		}
	}

	sealed public class ReflectedFunctionNode<T1, T2, T3, T4, T5, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, T2, T3, T4, T5, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a, T2 b, T3 c, T4 d, T5 e){ instance = a; return returnValue = call(a, b, c, d, e); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, T2, T3, T4, T5, TResult>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=>{ return callable? returnValue : Call(p1.value, p2.value, p3.value, p4.value, p5.value); });
		}
	}

	sealed public class ReflectedFunctionNode<T1, T2, T3, T4, T5, T6, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, T2, T3, T4, T5, T6, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f){ instance = a; return returnValue = call(a, b, c, d, e, f); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, T2, T3, T4, T5, T6, TResult>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			var p6 = node.AddValueInput<T6>(GetName(method, 5));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=>{ return callable? returnValue : Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value); });
		}
	}

	sealed public class ReflectedFunctionNode<T1, T2, T3, T4, T5, T6, T7, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, T2, T3, T4, T5, T6, T7, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g){ instance = a; return returnValue = call(a, b, c, d, e, f, g); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, T2, T3, T4, T5, T6, T7, TResult>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			var p6 = node.AddValueInput<T6>(GetName(method, 5));
			var p7 = node.AddValueInput<T7>(GetName(method, 6));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=>{ return callable? returnValue : Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value); });
		}
	}

	sealed public class ReflectedFunctionNode<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : ReflectedMethodNode{

		private FunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, TResult> call;
		private TResult returnValue;
		private T1 instance;
		TResult Call(T1 a, T2 b, T3 c, T4 d, T5 e, T6 f, T7 g, T8 h){ instance = a; return returnValue = call(a, b, c, d, e, f, g, h); }
		public override void RegisterPorts(FlowNode node, MethodInfo method, bool callable){
			call = method.RTCreateDelegate<FunctionCall<T1, T2, T3, T4, T5, T6, T7, T8, TResult>>(null);
			var p1 = node.AddValueInput<T1>(GetName(method, 0));
			var p2 = node.AddValueInput<T2>(GetName(method, 1));
			var p3 = node.AddValueInput<T3>(GetName(method, 2));
			var p4 = node.AddValueInput<T4>(GetName(method, 3));
			var p5 = node.AddValueInput<T5>(GetName(method, 4));
			var p6 = node.AddValueInput<T6>(GetName(method, 5));
			var p7 = node.AddValueInput<T7>(GetName(method, 6));
			var p8 = node.AddValueInput<T8>(GetName(method, 7));
			if (callable){
				var o = node.AddFlowOutput(" ");
				node.AddFlowInput(" ", (f)=> { Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value); o.Call(f); } );
				if (!method.IsStatic) node.AddValueOutput<T1>(GetName(method, 0), ()=> { return instance; });
			}
			node.AddValueOutput<TResult>("Value", ()=>{ return callable? returnValue : Call(p1.value, p2.value, p3.value, p4.value, p5.value, p6.value, p7.value, p8.value); });
		}
	}
}