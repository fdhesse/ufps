/////////////////////////////////////////////////////////////////////////////////
//
//	CommonUIManager.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CommonUIManager : MonoBehaviour
{

    public OperationProgressHUD CenterProgressBar = null;
    public FlagHUD FlagMgr = null;
    
	protected virtual void Awake()
	{
		
	}
	

    void Update()
    {

    }
	
	/// <summary>
	/// Makes sure all the necessary properies are set
	/// </summary>
	protected virtual void Init()
	{
	
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{
			
		

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{
		
		

	}
		
}
