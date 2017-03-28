/////////////////////////////////////////////////////////////////////////////////
//
//	OperationManager.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


public class OperationManager : MonoBehaviour
{
    // event handler property cast as a playereventhandler
    public vp_PlayerEventHandler m_Player = null;
    public float CurrentGotCount = 0;

    public int CurTeamNumber = 0;

	protected virtual void Awake()
	{
		
	}

	protected virtual void OnEnable()
	{

	}

	protected virtual void OnDisable()
	{

	}

    public virtual void OnOptionStateSwitch( OperationObject msgParam )
    {
    }


    public virtual void SetPlayerOperationState( bool enable )
    {
    }

    public virtual void OnGotCountChanged( float changeCount )
    { 
        
    }

    public void ChangePlayerGotCount( List<string> msgParams )
    {
        if( msgParams == null )
        {
            return;
        }

        if( msgParams.Count < 2 )
        {
            return;
        }

        

        int playerID = 0;
        float count = 0.0f;

        if( !int.TryParse(msgParams[0], out playerID) )
        {
            return;
        }
        
        if( !float.TryParse(msgParams[1], out count) )
        {
            return;
        }
        
        if (m_Player != null)
        {            
            if( vp_MPMaster.GetViewIDOfTransform( m_Player.transform ) == playerID )
            {
                float oldCount = CurrentGotCount;

                CurrentGotCount += count;

                if (CurrentGotCount < 0)
                {
                    CurrentGotCount = 0;
                }

                if (oldCount != CurrentGotCount)
                {
                    OnGotCountChanged(CurrentGotCount - oldCount);
                }
            }
        }
    }

    protected virtual void _Update()
    {

    }

    public virtual void TryDropAllCount()
    {
        float dropCount = 0.0f;
        if( this.CurrentGotCount > 0 )
        {
            dropCount = this.CurrentGotCount;
            this.CurrentGotCount = 0;
        }
    }

    void Update()
    {
        vp_MPNetworkPlayer player = vp_MPNetworkPlayer.Get(this.transform);
        if (player != null)
        {
            if (CurTeamNumber != player.TeamNumber)
            {
                CurTeamNumber = player.TeamNumber;
            }

        }

        _Update();
    }
}