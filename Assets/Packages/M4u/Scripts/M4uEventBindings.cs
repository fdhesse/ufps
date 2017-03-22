//----------------------------------------------
// MVVM 4 uGUI
// © 2015 yedo-factory
//----------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using LuaInterface;

namespace M4u
{
    /// <summary>
	/// M4uEventBindings. Bind UnityEvent
    /// </summary>
	[AddComponentMenu("M4u/EventBindings")]
	public class M4uEventBindings : M4uBindingMultiple
	{
		public M4uEventType[] Type;
		public EventTriggerType[] TriggerType;

        private LuaFunction LuaFunction;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (LuaFunction != null)
                LuaFunction.Dispose();
        }

		public override void Start ()
		{
			base.Start ();

			if (Path != null && Path.Length > 0)
			{
				for (int i = 0; i < Path.Length; i++)
				{
					string[] names = Path[i].Split('.');
					string name = "";
					object parent = Root.Context;
					object value = null;
					object obj = null;
					PropertyInfo pi = null;
					FieldInfo fi = null;
					for (int j = 0; j < names.Length; j++)
					{
						bool isLast = (j == names.Length - 1);
						name = names[j];
						ParseMember(isLast, ref name, ref parent, ref value, ref obj, ref pi, ref fi);
					}
					//M4uEventBinding.
                        SetEvent(this, Type[i], TriggerType[i], parent, name);
				}
			}
		}

        public void SetEvent(MonoBehaviour m, M4uEventType type, EventTriggerType triggerType, object obj, string name)
        {
            if (type == M4uEventType.ButtonClick)
            {
                UI<Button>(m).onClick.AddListener((UnityAction)Action<UnityAction>(obj, name));
            }
            else if (type == M4uEventType.ToggleValueChanged)
            {
                UI<Toggle>(m).onValueChanged.AddListener((UnityAction<bool>)Action<UnityAction<bool>>(obj, name));
            }
            else if (type == M4uEventType.SliderValueChanged)
            {
                UI<Slider>(m).onValueChanged.AddListener((UnityAction<float>)Action<UnityAction<float>>(obj, name));
            }
            else if (type == M4uEventType.ScrollbarValueChanged)
            {
                UI<Scrollbar>(m).onValueChanged.AddListener((UnityAction<float>)Action<UnityAction<float>>(obj, name));
            }
            else if (type == M4uEventType.DropdownValueChanged)
            {
#if !(UNITY_5_0 || UNITY_5_1)
                UI<Dropdown>(m).onValueChanged.AddListener((UnityAction<int>)Action<UnityAction<int>>(obj, name));
#endif
            }
            else if (type == M4uEventType.InputFieldEndEdit)
            {
                UI<InputField>(m).onEndEdit.AddListener((UnityAction<string>)Action<UnityAction<string>>(obj, name));
            }
            else if (type == M4uEventType.ScrollRectValueChanged)
            {
                UI<ScrollRect>(m).onValueChanged.AddListener((UnityAction<Vector2>)Action<UnityAction<Vector2>>(obj, name));
            }
            else if (type == M4uEventType.EventTrigger)
            {
                var e = new EventTrigger.Entry();
                e.eventID = triggerType;
                e.callback.AddListener((UnityAction<BaseEventData>)Action<UnityAction<BaseEventData>>(obj, name));

                var trigger = UI<EventTrigger>(m);
#if UNITY_5_0
				if(trigger.delegates == null) { trigger.delegates = new List<EventTrigger.Entry>(); }
				trigger.delegates.Add(e);
#else
                if (trigger.triggers == null) { trigger.triggers = new List<EventTrigger.Entry>(); }
                trigger.triggers.Add(e);
#endif
            }
        }

        private static T UI<T>(MonoBehaviour m) where T : Component
        {
            return m.GetComponent<T>();
        }

        private Delegate Action<T>(object obj, string name)
        {
            if (LuaFunction != null)
            {
                var type = typeof(T);
                var method = type.GetMethod("Invoke");
                var parameters = method.GetParameters();

                if (parameters.Length == 0)
                {
                    UnityAction a = () =>
                    {
                        LuaFunction.Call();
                    };
                    return a;
                }
                else
                {
                    var ptype = parameters[0].ParameterType;
                    if (ptype == typeof(BaseEventData))
                    {
                        UnityAction<BaseEventData> a = (arg) =>
                        {
                            LuaFunction.Call(arg);
                        };
                        return a;
                    }
                    else if (ptype == typeof(bool))
                    {
                        UnityAction<bool> a = (arg) =>
                        {
                            LuaFunction.Call(arg);
                        };
                        return a;
                    }
                    else if (ptype == typeof(int))
                    {
                        UnityAction<int> a = (arg) =>
                        {
                            LuaFunction.Call(arg);
                        };
                        return a;
                    }
                    else if (ptype == typeof(float))
                    {
                        UnityAction<float> a = (arg) =>
                        {
                            LuaFunction.Call(arg);
                        };
                        return a;
                    }
                    else if (ptype == typeof(Vector2))
                    {
                        UnityAction<Vector2> a = (arg) =>
                        {
                            LuaFunction.Call(arg);
                        };
                        return a;
                    }
                    else if (ptype == typeof(string))
                    {
                        UnityAction<string> a = (arg) =>
                        {
                            LuaFunction.Call(arg);
                        };
                        return a;
                    }
                }
            }

            return Delegate.CreateDelegate(typeof(T), obj, name);
        }

        public override string ToString()
        {
			string value = "";
			for (int i = 0; i < Type.Length; i++)
			{
				if(value != "") { value += "/"; }
				value += Type[i].ToString() + "=" + GetBindStr(Path[i]);
			}
			return value;
        }
	}
}