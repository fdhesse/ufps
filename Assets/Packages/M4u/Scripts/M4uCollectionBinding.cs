//----------------------------------------------
// MVVM 4 uGUI
// © 2015 yedo-factory
//----------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using LuaInterface;

namespace M4u
{
    /// <summary>
    /// M4uCollectionBinding. Bind Collection
    /// </summary>
    [AddComponentMenu("M4u/CollectionBinding")]
    public class M4uCollectionBinding : M4uBindingSingle
	{
		public GameObject Data;
		public string SavePath;
        public string OnChanged;

        private bool isChange = false;
		private Action onChanged = null;
		private ICollection saveCollection = null;
		private List<GameObject> saveGos = new List<GameObject>();
		private List<object> saveObjs = new List<object>();

        private LuaTable luaViewModel;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //if (luaViewModel != null)
            //    luaViewModel.Dispose();
        }

		public override void Start ()
		{
			base.Start ();

			if (!isLuaBinding && !string.IsNullOrEmpty(SavePath))
			{
				string[] names = SavePath.Split('.');
				object parent = Root.Context;
				object value = null;
				object obj = null;
				PropertyInfo pi = null;
				FieldInfo fi = null;
				for (int i = 0; i < names.Length; i++)
				{
					bool isLast = (i == names.Length - 1);
					string name = names[i];
					ParseMember (isLast, ref name, ref parent, ref value, ref obj, ref pi, ref fi);
				}
				saveCollection = GetMember<ICollection> (obj, pi, fi);
			}

            if (!string.IsNullOrEmpty(OnChanged))
			{
				string[] names = OnChanged.Split('.');
				string name = "";
				object parent = Root.Context;
				object value = null;
				object obj = null;
				PropertyInfo pi = null;
				FieldInfo fi = null;
				for (int i = 0; i < names.Length; i++)
				{
					bool isLast = (i == names.Length - 1);
					name = names[i];
					ParseMember(isLast, ref name, ref parent, ref value, ref obj, ref pi, ref fi);
				}
                if (isLuaBinding) {
                    luaViewModel = value as LuaTable;
                    if (luaViewModel != null)
                    {
                        var func = luaViewModel.GetLuaFunction(name);
                        if (func != null) onChanged = () => func.Call();
                    }
                }else
                    onChanged = (Action)Delegate.CreateDelegate(typeof(Action), parent, name);
			}
			OnChange ();
		}

        public override void Update()
        {
            base.Update();

			object value = Values [0];
			if (value != null)
			{
				Type type = value.GetType ();
				int count = 0;
				ICollection collection = null;
				if (type.IsPrimitive)
				{
					if (int.TryParse (value.ToString (), out count) && saveObjs.Count != count)
					{
						isChange = true;
					}
				}
                else if (value is LuaTable)
                {
                    //luaViewModel = value as LuaTable;
                    //if (luaViewModel.Length != saveObjs.Count) 
                    //    isChange = true;
                    //else
                    //    for (int i = saveObjs.Count - 1; i >= 0; i--)
                    //    {
                    //        if (saveObjs[i] != luaViewModel[i+1]) { isChange = true; break; }
                    //    }
                }
                else {
					collection = value as ICollection;
					if (collection != null)
					{
						if (saveObjs.Count != collection.Count)
						{
							isChange = true; 
						}
						else if(type.IsArray) 
						{
							var data = (Array)value;
							for (int i = saveObjs.Count - 1; i >= 0; i--)
							{
								if (saveObjs[i] != data.GetValue(i)) { isChange = true; break; }
							}
						}
						else if(value is IList) 
						{
							var data = (IList)value;
							for (int i = saveObjs.Count - 1; i >= 0; i--)
							{
								if (saveObjs[i] != data[i]) { isChange = true; break; }
							}
						}
					}
				}

				if(isChange)
				{
					isChange = false;

					foreach (var go in saveGos)
					{
						Destroy(go);
					}
					saveGos.Clear();

                    if (isLuaBinding)
                    {
                        foreach (var ob in saveObjs)
                        {
                            ((LuaTable)ob).Dispose();
                        }
                    }
					saveObjs.Clear();

					var saveList = (saveCollection != null) ? (saveCollection as IList) : null;
					var saveDic = (saveCollection != null && saveList == null) ? (saveCollection as IDictionary) : null;
					if (saveList != null) { saveList.Clear(); }
					if (saveDic != null) { saveDic.Clear(); }

					if (type.IsPrimitive)
					{
						for (int i = 0; i < count; i++)
						{
							CreateData (i, saveList, saveDic);
						}
					}
                    else if (luaViewModel != null)
                    {
                        for (int i = 1; i <= luaViewModel.Length; i++)
                        {
                            CreateData(luaViewModel[i], saveList, saveDic);
                        }
                    }
					else if(collection != null)
					{
						foreach (var obj in collection)
						{
							CreateData(obj, saveList, saveDic); 
						}
					}

					if(onChanged != null) { onChanged(); }
				}
			}
        }

		private GameObject CreateData(object obj, IList saveList, IDictionary saveDic)
		{
			var go = (GameObject)Instantiate(Data);
			var root = go.GetComponent<M4uContextRoot>();
			var pos = go.transform.localPosition;
			var rot = go.transform.eulerAngles;
			var scale = go.transform.localScale;
			go.transform.SetParent(transform);
			go.transform.localPosition = pos;
			go.transform.eulerAngles = rot;
			go.transform.localScale = scale;
			if (root != null && root.Context == null && (obj is M4uContextInterface))
			{
				root.Context = (M4uContextInterface)obj;
			}
            if (root != null && root.Context == null && (obj is LuaTable))
            {
                root.Context = new LuaContextInterface(obj as LuaTable);
            }

			saveGos.Add(go);
			saveObjs.Add (obj);
			if (saveList != null) { saveList.Add(go); }
			else if (saveDic != null) { saveDic.Add(obj, go); }
			return go;
		}

        public void PostChange(int index)
        {
            if (index < 1)
                Debug.LogErrorFormat("update binding:{0} index start from 1", Path);

            if (isLuaBinding)
            {
                //update ourself but not set isChange flag
                base.OnChange();

                luaViewModel = Values[0] as LuaTable;

                if (luaViewModel != null && index <= saveGos.Count)
                {
                    var data = luaViewModel[index] as LuaTable;
                    var root = saveGos[index-1].GetComponent<M4uContextRoot>();
                    var context = root.Context as LuaContextInterface;
                    if (context != null && data != null)
                    {
                        context.ViewModel = data;
                        context.UpdateViewModel();
                    }
                }
            }
        }

		public override void OnChange ()
		{
			base.OnChange ();
            if (isLuaBinding)
                luaViewModel = Values[0] as LuaTable;
            isChange = true;
		}

        public override string ToString()
        {
            return "Collection=" + GetBindStr(Path) + "/" + ((Data != null) ? Data.name : "None");
        }
	}
}