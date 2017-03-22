using System;
using UnityEngine;
using ParadoxNotion;

namespace FlowCanvas.Macros{

	[Serializable]
	public class MacroPortDefinition : ISerializationCallbackReceiver{
		
		//serialization callbacks from Unity since this class is serialized in a Macro (ScriptableObject)
		void ISerializationCallbackReceiver.OnBeforeSerialize(){
			_type = type != null? type.FullName : null;
		}

		//serialization callbacks from Unity since this class is serialized in a Macro (ScriptableObject)
		void ISerializationCallbackReceiver.OnAfterDeserialize(){
			type = ReflectionTools.GetType(_type, /*fallback?*/ true);
		}

		[SerializeField]
		private string _ID;
		[SerializeField]
		private string _name;
		[SerializeField]
		private string _type;

		//The ID of the definition port
		public string ID{
			get
			{
				if (string.IsNullOrEmpty(_ID)){ //for correct update prior versions
					_ID = name;
				}
				return _ID;
			}
			private set {_ID = value;}
		}

		//The name of the definition port
		public string name{
			get {return _name;}
			set {_name = value;}
		}

		///The Type of the definition port
		public Type type {get; set;}

		public MacroPortDefinition(string name, Type type){
			this.ID = System.Guid.NewGuid().ToString();
			this.name = name;
			this.type = type;
		}
	}
}