//----------------------------------------------
// MVVM 4 uGUI
// © 2015 yedo-factory
//----------------------------------------------
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

using M4u;
using LuaFramework;
using LuaInterface;

public class LuaContextInterface : M4uContextInterface
{
    private LuaTable viewmodel;
    //private Dictionary<string, M4uBinding> bindings = new Dictionary<string, M4uBinding>();
    private List<M4uBinding> bindings = new List<M4uBinding>();

    internal LuaTable ViewModel
    {
        get { return viewmodel; }
        set 
        {
            if (viewmodel != null) viewmodel.Dispose();
            viewmodel = value; 
        }
    }

    internal LuaContextInterface(LuaTable viewmodel)
    {
        this.viewmodel = viewmodel;
    }

    internal void AddBinding(M4uBinding binding)
    {
        bindings.Add(binding);
    }

    internal void RemoveBinding(M4uBinding binding)
    {
        bindings.Remove(binding);
    }

    public void UpdateField(string name)
    {
        //M4uBinding binding;
        //if (bindings.TryGetValue(name, out binding))
        foreach (var binding in bindings)
        {
            foreach (var path in binding.Paths)
            {
                if (path == name)
                    binding.OnChange();
            }
        }
    }

    public void UpdateCollection(string name, int index)
    {
        //M4uBinding binding;
        //if (bindings.TryGetValue(name, out binding))
        foreach (var binding in bindings)
        {
            foreach (var path in binding.Paths)
            {
                if (path == name && binding is M4uCollectionBinding)
                {
                    ((M4uCollectionBinding)binding).PostChange(index);
                }
            }
        }
    }

    public void UpdateViewModel()
    {
        foreach(var binding in bindings)
        {
            binding.OnChange();
        }
    }
}


public class LuaViewModel : MonoBehaviour
{
    public M4uContextRoot Root;
    public string ViewModel;

    private LuaContextInterface context;

    void OnDestroy()
    {
        if (context != null && context.ViewModel != null)
        {
            context.ViewModel.Dispose();
        }
    }

    void Awake()
    {
        var luaManager = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);

        if (luaManager && !string.IsNullOrEmpty(ViewModel))
        {
            //var values = luaManager.DoFile("ViewModel/" + LuaPath + ".lua");
            //if (values.Length > 0)
            var viewmodel = luaManager.GetGlobal(ViewModel) as LuaTable;
            if (viewmodel != null)
            {
                //var viewmodel = values[0] as LuaTable;
                context = new LuaContextInterface(viewmodel);

                var func = viewmodel.GetLuaFunction("OnCreate");
                if (func != null) func.Call(gameObject, context);
            }
        }

        Root.Context = context;
    }

    void Start()
    {

    }
}
