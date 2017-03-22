using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Nodes{

	[Category("Events")]
	[Color("ff5c5c")]
	///Base class for event nodes.
	abstract public class EventNode : FlowNode {

		public override string name{
			get {return string.Format("➥ {0}", base.name.ToUpper());}
		}
	}


	///Base class for event nodes that require a specific target Component or GameObject(use Transform for GameObjects)
	abstract public class EventNode<T> : EventNode where T:Component{
		
		public BBParameter<T> target;

		public override string name{
			get {return string.Format("{0} ({1})", base.name.ToUpper(), ( target.isNull && !target.useBlackboard? "Self" : target.ToString()) );}
		}

		///The event message names to subscribe on the target agent. Null if none required.
		virtual protected string[] GetTargetMessageEvents(){ return null; }

		public override void OnGraphStarted(){

			if (target.isNull && !target.useBlackboard){
				target.value = graphAgent.GetComponent<T>();
			}

			if (target.isNull){
				Fail(string.Format("Target is missing component of type '{0}'", typeof(T).Name));
				return;
			}
			
			var targetEvents = GetTargetMessageEvents();
			if (targetEvents != null && targetEvents.Length != 0){
				RegisterEvents(target.value, targetEvents);
			}
		}

		public override void OnGraphStoped(){
			UnRegisterEvents(target.value, GetTargetMessageEvents());
		}
	}
}