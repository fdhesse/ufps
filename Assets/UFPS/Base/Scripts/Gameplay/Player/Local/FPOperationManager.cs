/////////////////////////////////////////////////////////////////////////////////
//
//	FPOperationManager.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


public class FPOperationManager : OperationManager
{
	protected CommonUIManager _CommonUIMgr = null; // 进度条的HUD
    protected OperationObject _CurObject = null;

    protected float _OldGotCount = 0;

    protected CommonUIManager HUD
    {
        get{
            if (_CommonUIMgr == null)
            {
                _CommonUIMgr = GameObject.FindObjectOfType<CommonUIManager>();
            }

            if (_CommonUIMgr != null)
            {
                return _CommonUIMgr;
            }

            return null;        
        }
    }

    public override void OnOptionStateSwitch( OperationObject msgParam )
    {
        if( msgParam != null )
        {
            switch( msgParam.GetCurState() )
            {
                case OperationObject.eOperationObjectState.EOOS_BEING_OPERATION:
                    {
                        if (HUD != null && HUD.CenterProgressBar != null && HUD.AmountBar != null )
                        {                            
                            HUD.CenterProgressBar.gameObject.SetActive(true);
                            HUD.CenterProgressBar.SetBarColor(0.0f, 1.0f, 1.0f, 1.0f);
                        }

                        _CurObject = msgParam;
                        if (_CurObject != null)
                        {
                            HUD.AmountBar.gameObject.SetActive(_CurObject.IsMultiSupply() );

                            float amount = 0.0f;
                            float curCount = 0.0f;
                            HUD.AmountBar.SetPercent(_CurObject.GetCurAmountPercent(out curCount, out amount));
                        }

                        if( m_Player != null )
                        {
                            m_Player.Operation.TryStart();
                        }


                    }

                    break;
                case OperationObject.eOperationObjectState.EOOS_FINISHING:
                    {
                        if (HUD != null && HUD.CenterProgressBar != null && HUD.AmountBar != null)
                        {
                            HUD.CenterProgressBar.SetBarColor(0.0f, 1.0f, 0.0f, 1.0f);
                            if( _CurObject != null )
                            {
                                float amount = 0.0f;
                                float curCount = 0.0f;
                                HUD.AmountBar.SetPercent(_CurObject.GetCurAmountPercent(out curCount, out amount));
                            }                            
                        }
                    }
                    break;
                case OperationObject.eOperationObjectState.EOOS_FINISHED:
                case OperationObject.eOperationObjectState.EOOS_INVALID:
                    {
                        _CurObject = null;
                        if (HUD != null && HUD.CenterProgressBar != null && HUD.AmountBar != null )
                        {
                            HUD.CenterProgressBar.gameObject.SetActive(false);                            
                            HUD.CenterProgressBar.SetBarColor(0.0f, 1.0f, 1.0f, 1.0f);

                            HUD.AmountBar.gameObject.SetActive(false);
                        }
                         
                        if (m_Player != null)
                        {
                            m_Player.Operation.TryStop();
                        }                        
                    }
                    break;
                case OperationObject.eOperationObjectState.EOOS_DROP:
                    {
                        _CurObject = null;
                        if (HUD != null && HUD.CenterProgressBar != null && HUD.AmountBar != null )
                        {
                            HUD.CenterProgressBar.gameObject.SetActive(false);
                            HUD.CenterProgressBar.SetBarColor(1.0f, 0.0f, 0.0f, 1.0f);

                            HUD.AmountBar.gameObject.SetActive(false);
                        }

                        if (m_Player != null)
                        {
                            m_Player.Operation.TryStop();
                        }
                    }
                    break;
                default:
                    {
                        /*
                        _CurObject = null;
                        if (HUD != null)
                        {
                            HUD.gameObject.SetActive(false);
                        }
                        */
                    }
                    break;
            }
        }
    }


    public override void SetPlayerOperationState(bool enable)
    {
        if (m_Player != null)
        {
            if( enable )
            {
                if( !m_Player.Operation.Active )
                {
                    m_Player.Operation.TryStart();
                }                
            }
            else
            {
                if( m_Player.Operation.Active )
                {
                    m_Player.Operation.TryStop();
                }                
            }            
        }
    }
    
    public virtual void OnGotCountChanged( float changeCount )
    {
        vp_GlobalEvent<float, float>.Send("OnLocalPlayerGotCountChanged", CurrentGotCount, changeCount );
    }
     

    protected override void _Update()
    {
        float curTime = MiscUtils.GetCurBattleTime();
        if (HUD != null && HUD.CenterProgressBar != null && HUD.AmountBar != null )
        {
            if (_CurObject != null)
            {
                HUD.CenterProgressBar.SetPercent(_CurObject.GetOperationPercent(curTime));
            }
            else
            {
                HUD.CenterProgressBar.SetPercent(0.0f);
                HUD.CenterProgressBar.gameObject.SetActive(false);
                HUD.AmountBar.gameObject.SetActive(false);
            }
        }

        if( m_Player != null )
        {
            //if player has been killed then operation object change to state : EOOS_INTERRUPT
            if( m_Player.Dead.Active )
            {
                if( _CurObject != null )
                {
                    //_CurObject.ChangeStateImm( OperationObject.eOperationObjectState.EOOS_INTERRUPT );

                    _CurObject.RequestChangeState(m_Player, OperationObject.eOperationObjectState.EOOS_INTERRUPT, false);

                    _CurObject = null;
                }

                if( m_Player.Operation.Active )
                {
                    m_Player.Operation.TryStop();
                }
            }
        }

        if( _OldGotCount != CurrentGotCount )
        {
            ScoreChangedInfo info = new ScoreChangedInfo();
            info.ID = vp_MPLocalPlayer.Instance.photonView.viewID;
            info.CurValue = CurrentGotCount;
            info.Change = CurrentGotCount - _OldGotCount;
            info.Type = eScoreBelongType.ESBT_LOCALPLAYER;

            _OldGotCount = CurrentGotCount;

            vp_GlobalEvent<ScoreChangedInfo>.Send("OnScoreChanged", info);

        }
        
    }
}