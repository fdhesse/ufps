/////////////////////////////////////////////////////////////////////////////////
//
//	PVPCoorpHUD.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
using System;

public class PVPCoorpHUD : MonoBehaviour
{
    public Text TimeLabel = null;
    private float _SaveSec = 0.0f;

	protected virtual void Awake()
	{
		
	}
	

    void Update()
    {
        if( TimeLabel != null )
        {
            float curSec = vp_MPClock.TimeLeft;
            if ((int)_SaveSec != (int)curSec)
            {
                string value = MiscUtils.GetTimeStrBySec_M_S(curSec);
                TimeLabel.text = value;

                _SaveSec = curSec;
            }
        }
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
