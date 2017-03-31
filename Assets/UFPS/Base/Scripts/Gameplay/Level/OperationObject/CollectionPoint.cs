/////////////////////////////////////////////////////////////////////////////////
//	CollectionPoint.cs
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class CollectionPoint : OperationObject
{
    //只允许 BindFlag相等的操作
    public int TeamNumber = 0;
    private float _SaveOldCount = 0.0f;

    protected override void _OnEnable()
    {
        foreach( vp_OptionSwitch os in SwitchList )
        {
            if( os != null )
            {
                if( os.gameObject.GetActive() )
                {
                    os.gameObject.SetActive(IsInLocalPlayerTeam());
                }                
            }
        }
    }

    protected override void _Awake()
    {
        _SaveOldCount = CurCount;
    }

    public override void OnInteract(List<object> objVec)
    {
        if (objVec == null)
        {
            return;
        }

        if (objVec.Count < 2)
        {
            return;
        }
       
        vp_PlayerEventHandler tempPlayer = objVec[0] as vp_PlayerEventHandler;

        if( tempPlayer != null )
        {
            FPOperationManager mgr = tempPlayer.gameObject.GetComponent<FPOperationManager>();
            if (mgr == null)
            {
                return;
            }

            //当玩家身上有资源时,才能操作
            if( mgr.CurrentGotCount <= 0.0f )
            {
                return;
            }

            if( !IsInLocalPlayerTeam() )
            {
                return;
            }
        }

        vp_OptionSwitch os = objVec[1] as vp_OptionSwitch;

        if (os != null)
        {
            RequestChangeState(tempPlayer, os.ChangeState, true);
        }

    }

    public override float ChangeValue()
    {
        Transform curPlayer = vp_MPMaster.GetTransformOfViewID( _SaveOperationPlayerID );

        if (curPlayer == null)
        {
            return 0.0f;
        }

        OperationManager mgr = curPlayer.gameObject.GetComponent<OperationManager>();
        if( mgr == null )
        {
            return 0.0f;
        }

        float saveValue = CurCount;
        CurCount += mgr.CurrentGotCount;

        if (CurCount <= 0.0f)
        {
            CurCount = 0.0f;

        }

        else if (CurCount >= Amount)
        {
            CurCount = Amount;
        }

        return CurCount - saveValue;
    }

    public override void _OnStateHaveChanged()
    {
        _OnEnable();
    }

    public bool IsInLocalPlayerTeam()
    {
        return vp_MPLocalPlayer.Instance.TeamNumber == TeamNumber;
    }

    protected override void OnLocalPlayerGotCountChanged(float curValue, float changeValue)
    {
        //if (_CurState == eOperationObjectState.EOOS_IDEL || _CurState == eOperationObjectState.EOOS_DROP || _CurState == eOperationObjectState.EOOS_INTERRUPT)
        //{
        //    foreach (vp_OptionSwitch os in SwitchList)
        //    {
        //        if ( os != null )
        //        {
        //            foreach (eOperationObjectState belongState in os.BelongState)
        //            {
        //                if (belongState == _CurState)
        //                {
        //                    os.gameObject.SetActive(curValue > 0.0f);
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}
    }

    public override eOperationObjectType GetType()
    {
        return eOperationObjectType.EOOT_COLLECTIONPOINT;
    }

    protected override void _Update()
    {
        if( _SaveOldCount != CurCount )
        {
            ScoreChangedInfo info = new ScoreChangedInfo();
            info.ID = ID;
            info.CurValue = CurCount;
            info.Change = CurCount - _SaveOldCount;
            if( IsInLocalPlayerTeam() )
            {
                info.Type = eScoreBelongType.ESBT_LOCALTEAM;
            }
            else
            {
                info.Type = eScoreBelongType.ESBT_OTHERTEAM;
            }
            

            _SaveOldCount = CurCount;
            vp_GlobalEvent<ScoreChangedInfo>.Send( "OnScoreChanged", info );
        }
    }

}


