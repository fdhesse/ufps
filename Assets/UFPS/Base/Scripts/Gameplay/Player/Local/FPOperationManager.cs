/////////////////////////////////////////////////////////////////////////////////
//
//	FPOperationManager.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;


public class FPOperationManager : MonoBehaviour
{
    // event handler property cast as a playereventhandler
    public vp_PlayerEventHandler m_Player = null;

	protected CommonUIManager _CommonUIMgr = null; // 进度条的HUD
    protected OperationObject _CurObject = null;

    protected OperationProgressHUD HUD
    {
        get{
            if (_CommonUIMgr == null)
            {
                _CommonUIMgr = GameObject.FindObjectOfType<CommonUIManager>();
            }

            if (_CommonUIMgr != null)
            {
                return _CommonUIMgr.CenterProgressBar;
            }

            return null;        
        }
    }

	protected virtual void Awake()
	{
		
	}

	protected virtual void OnEnable()
	{

	}

	protected virtual void OnDisable()
	{

	}

    public void OnOptionStateSwitch( OperationObject msgParam )
    {
        if( msgParam != null )
        {
            switch( msgParam.GetCurState() )
            {
                case OperationObject.eOperationObjectState.EOOS_BEING_OPERATION:
                    {
                        if (HUD != null)
                        {
                            HUD.gameObject.SetActive(true);
                            HUD.SetBarColor(0.0f, 1.0f, 1.0f, 1.0f);
                        }

                        _CurObject = msgParam;

                        if( m_Player != null )
                        {
                            m_Player.Operation.TryStart();
                        }
                    }

                    break;
                case OperationObject.eOperationObjectState.EOOS_FINISHING:
                    {
                        if (HUD != null)
                        {
                            HUD.SetBarColor(0.0f, 1.0f, 0.0f, 1.0f);
                        }
                    }
                    break;
                case OperationObject.eOperationObjectState.EOOS_FINISHED:
                    {
                        _CurObject = null;
                        if (HUD != null)
                        {
                            HUD.gameObject.SetActive(false);
                            HUD.SetBarColor(0.0f, 1.0f, 1.0f, 1.0f);
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
                        if (HUD != null)
                        {
                            HUD.gameObject.SetActive(false);
                            HUD.SetBarColor(1.0f, 0.0f, 0.0f, 1.0f);
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


    public void SetPlayerOperationState( bool enable )
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

    void Update()
    {
        float curTime = MiscUtils.GetCurBattleTime();
        if (HUD != null)
        {
            if (_CurObject != null)
            {
                HUD.SetPercent(_CurObject.GetOperationPercent(curTime));
            }
            else
            {
                HUD.SetPercent(0.0f);
                HUD.gameObject.SetActive( false );
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
        
    }
}