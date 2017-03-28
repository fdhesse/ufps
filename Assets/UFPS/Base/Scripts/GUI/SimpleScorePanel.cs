/////////////////////////////////////////////////////////////////////////////////
//
//	SimpleScorePanel.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum eScoreBelongType
{
    ESBT_LOCALPLAYER = 0,
    ESBT_LOCALTEAM = 1,
    ESBT_OTHERTEAM = 2,
    ESBT_UNKNOWN = 3,
}

public class ScoreChangedInfo
{
    public eScoreBelongType Type = eScoreBelongType.ESBT_UNKNOWN;
    public float Change = 0.0f;
    public float CurValue = 0.0f;
    public int ID = 0;
}

public class SimpleScorePanel : MonoBehaviour
{

    public Image Icon = null;
    public Image BackColor = null;
    public Text ScoreLabel = null;
    public eScoreBelongType BelongType = eScoreBelongType.ESBT_UNKNOWN;
    public int BindID = 0;

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
	    if( ScoreLabel != null )
        {
            ScoreLabel.text = "0";
        }
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

        vp_GlobalEvent<ScoreChangedInfo>.Register("OnScoreChanged", OnScoreChanged);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{
        vp_GlobalEvent<ScoreChangedInfo>.Unregister("OnScoreChanged", OnScoreChanged);
	}

    public void OnScoreChanged( ScoreChangedInfo changeInfo )
    {
        if( ScoreLabel != null && changeInfo != null )
        {
            if( changeInfo.Type == BelongType )
            {
                ScoreLabel.text = ((int)changeInfo.CurValue).ToString();
            }
        }
    }
		
}
