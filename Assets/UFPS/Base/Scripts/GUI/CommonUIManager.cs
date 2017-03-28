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
    public OperationProgressHUD AmountBar = null;
    public GameObject PVPCoorpUI = null;

    public FlagHUD FlagMgr = null;
    
	void Awake()
	{
		
	}
	

    void Update()
    {

    }
	
	/// <summary>
	/// Makes sure all the necessary properies are set
	/// </summary>
	void Init()
	{

	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{
        switch( vp_Gameplay.GetBattleMode() )
        {
            case eCombatMode.ECM_COORP:
                {
                    if (PVPCoorpUI != null)
                    {
                        PVPCoorpUI.SetActive(false);
                    }		
                }
                break;
            case eCombatMode.ECM_PVP_COORP:
                {
                    if (PVPCoorpUI != null)
                    {
                        PVPCoorpUI.SetActive(true);
                    }		
                }
                break;
            default:
                {
                    if (PVPCoorpUI != null)
                    {
                        PVPCoorpUI.SetActive(false);
                    }		

                }
                break;
        }
	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{
		
		

	}
		
}
