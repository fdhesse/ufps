/////////////////////////////////////////////////////////////////////////////////
//
//	BattleTimeUtil.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleTimeUtil : MonoBehaviour
{

    // instance
    protected static BattleTimeUtil m_Instance = null;
    protected float _CurTime = 0.0f;
    public float CurTime
    {
        get { return _CurTime; }
    }
    public static BattleTimeUtil Instance
    {
        get
        {
            if ( m_Instance == null )
            {
                m_Instance = Object.FindObjectOfType<BattleTimeUtil>();
            }
            return m_Instance;
        }
    }

    void OnEnable()
    {
        _CurTime = 0.0f;   
    }

    public void OnBattleBegin()
    { 
            
    }

    void Update()
    {
        _CurTime += Time.deltaTime;
    }

}