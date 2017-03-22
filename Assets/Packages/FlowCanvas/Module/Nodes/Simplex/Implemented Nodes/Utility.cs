using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Services;
using NodeCanvas.Framework;
using UnityEngine;

namespace FlowCanvas.Nodes{

	[Name("Identity Value<T>")]
	[Category("Functions/Utility")]
	[Description("Use this for organization. It returns exactly what is provided in the input.")]
	public class Identity<T> : PureFunctionNode<T, T>{
		public override string name{get{return null;}}
		public override T Invoke(T value){
			return value;
		}
	}

	[Name("Delta Timed Float")]
	[Category("Functions/Utility")]
	public class DeltaTimed : PureFunctionNode<float, float, float>{
		public override float Invoke(float value, float multiplier){
			return value * multiplier * Time.deltaTime;
		}
	}

	[Category("Functions/Utility")]
	public class DeltaTimedVector3 : PureFunctionNode<Vector3, Vector3, float>{
		public override Vector3 Invoke(Vector3 value, float multiplier){
			return value * multiplier * Time.deltaTime;
		}
	}

	[Category("Functions/Utility")]
	[Description("Returns either one of the two inputs, based on the boolean condition")]
	public class SwitchValue<T> : PureFunctionNode<T, bool, T, T>{
		public override T Invoke(bool condition, T isTrue, T isFalse){
			return condition? isTrue : isFalse;
		}
	}

	[Obsolete]
	[Category("Functions/Utility")]
	public class PickValue<T> : PureFunctionNode<T, int, IList<T>>{
		public override T Invoke(int index, IList<T> values){
			try {return values[index];}
			catch {return default(T);}
		}
	}

	[Category("Functions/Utility")]
	public class GetComponent<T> : PureFunctionNode<T, GameObject> where T:Component{
		private T _component;
		public override T Invoke(GameObject gameObject){
			if (gameObject == null) return null;
			if (_component == null || _component.gameObject != gameObject)
				_component = gameObject.GetComponent<T>();			
			return _component;
		}
	}

	[Category("Functions/Utility")]
	public class GetChildTransforms : PureFunctionNode<Transform[], Transform>{
		public override Transform[] Invoke(Transform parent){
			return parent.transform.Cast<Transform>().ToArray();
		}
	}


	[Category("Functions/Utility")]
	public class Wait : LatentActionNode<float>{

		public float timeLeft{ get; private set; }

		public override IEnumerator Invoke(float time){
			timeLeft = time;
			while (timeLeft > 0){
				timeLeft -= Time.deltaTime;
				timeLeft = Mathf.Max(timeLeft, 0);
				yield return null;
			}
		}
	}


	[Category("Functions/Utility")]
	[Description("Caches the value only when the node is called.")]
	public class Cache<T> : CallableFunctionNode<T, T>{
		public override T Invoke(T value){
			return value;
		}
	}


	[Category("Functions/Utility")]
	public class LogValue : CallableActionNode<object>{
		public override void Invoke(object obj){
			Debug.Log(obj);
		}
	}

	[Category("Functions/Utility")]
	public class LogText : CallableActionNode<string>{
		public override void Invoke(string text){
			Debug.Log(text);
		}
	}

	[Category("Functions/Utility")]
	public class SendEvent : CallableActionNode<GraphOwner, string>{
		public override void Invoke(GraphOwner target, string eventName){
			target.SendEvent(new EventData(eventName));
		}
	}

	[Category("Functions/Utility")]
	public class SendEvent<T> : CallableActionNode<GraphOwner, string, T>{
		public override void Invoke(GraphOwner target, string eventName, T eventValue){
			target.SendEvent(new EventData<T>(eventName, eventValue));
		}
	}

	[Category("Functions/Utility")]
	public class SendGlobalEvent : CallableActionNode<string>{
		public override void Invoke(string eventName){
			Graph.SendGlobalEvent(new EventData(eventName));
		}
	}

	[Category("Functions/Utility")]
	public class SendGlobalEvent<T> : CallableActionNode<string, T>{
		public override void Invoke(string eventName, T eventValue){
			Graph.SendGlobalEvent(new EventData<T>(eventName, eventValue));
		}
	}
}