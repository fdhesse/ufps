using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;
using UnityEngine;
using System;
using System.Reflection;

namespace FlowCanvas.Nodes{

	[Description("Subscribes to a static C# System.Action Event and is called when the event is raised")]
	[Category("Events/Script")]
	public class StaticCodeEvent : EventNode {

		[SerializeField]
		private string eventName;
		[SerializeField]
		private Type targetType;

		private FlowOutput o;
		private Action pointer;


		public void SetEvent(EventInfo e){
			targetType = e.RTReflectedType();
			eventName = e.Name;
			GatherPorts();			
		}

		public override void OnGraphStarted(){
			
			if (string.IsNullOrEmpty(eventName)){
				Debug.LogError("No Event Selected for 'Static Code Event'");
				return;
			}

			var eventInfo = targetType.RTGetEvent(eventName);
			if (eventInfo == null){
				Debug.LogError(string.Format("Event {0} is not found", eventName) );
				return;
			}

			base.OnGraphStarted();

			pointer = ()=> { o.Call(new Flow(1)); };
			eventInfo.AddEventHandler( null, pointer );
		}

		public override void OnGraphStoped(){

			if (string.IsNullOrEmpty(eventName)){
				return;
			}

			var eventInfo = targetType.RTGetEvent(eventName);
			eventInfo.RemoveEventHandler(null, pointer);
		}

		protected override void RegisterPorts(){
			if (!string.IsNullOrEmpty(eventName)){
				o = AddFlowOutput(eventName);
			}
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
			
		protected override void OnNodeInspectorGUI(){

			base.OnNodeInspectorGUI();

			if (eventName == null && !Application.isPlaying && GUILayout.Button("Select Event")){
				var menu = new UnityEditor.GenericMenu();
				foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(object)))
					menu = EditorUtils.GetStaticEventSelectionMenu(t, null, SetEvent, menu);
				menu.ShowAsContext();
				Event.current.Use();
			}

			if (eventName != null){
				GUILayout.BeginVertical("box");
				UnityEditor.EditorGUILayout.LabelField("Selected Type", targetType.FriendlyName());
				UnityEditor.EditorGUILayout.LabelField("Selected Event", eventName);
				GUILayout.EndVertical();
			}				
		}

		#endif
	}







	[Description("Subscribes to a static C# System.Action<T> Event and is called when the event is raised")]
	[Category("Events/Script")]
	public class StaticCodeEvent<T> : EventNode {

		[SerializeField]
		private string eventName;
		[SerializeField]
		private Type targetType;

		private FlowOutput o;
		private Action<T> pointer;
		private T eventValue;

		public void SetEvent(EventInfo e){
			targetType = e.RTReflectedType();
			eventName = e.Name;
			GatherPorts();			
		}

		public override void OnGraphStarted(){
			
			if (string.IsNullOrEmpty(eventName)){
				Debug.LogError("No Event Selected for 'Static Code Event'");
				return;
			}

			var eventInfo = targetType.RTGetEvent(eventName);
			if (eventInfo == null){
				Debug.LogError(string.Format("Event {0} is not found", eventName) );
				return;
			}

			base.OnGraphStarted();

			pointer = (v)=> { eventValue = v; o.Call(new Flow(1)); };
			eventInfo.AddEventHandler( null, pointer );
		}

		public override void OnGraphStoped(){

			if (string.IsNullOrEmpty(eventName)){
				return;
			}

			var eventInfo = targetType.RTGetEvent(eventName);
			eventInfo.RemoveEventHandler(null, pointer);
		}

		protected override void RegisterPorts(){
			if (!string.IsNullOrEmpty(eventName)){
				o = AddFlowOutput(eventName);
				AddValueOutput<T>("Value", ()=>{ return eventValue; });
			}
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
			
		protected override void OnNodeInspectorGUI(){

			base.OnNodeInspectorGUI();

			if (eventName == null && !Application.isPlaying && GUILayout.Button("Select Event")){
				var menu = new UnityEditor.GenericMenu();
				foreach (var t in UserTypePrefs.GetPreferedTypesList(typeof(object)))
					menu = EditorUtils.GetStaticEventSelectionMenu(t, typeof(T), SetEvent, menu);
				menu.ShowAsContext();
				Event.current.Use();
			}

			if (eventName != null){
				GUILayout.BeginVertical("box");
				UnityEditor.EditorGUILayout.LabelField("Selected Type", targetType.FriendlyName());
				UnityEditor.EditorGUILayout.LabelField("Selected Event", eventName);
				GUILayout.EndVertical();
			}				
		}

		#endif
	}
}