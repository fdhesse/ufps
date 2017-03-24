/////////////////////////////////////////////////////////////////////////////////
//	OperationObject.cs
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class OperationStateChangeParam
{
    public int PlayerID = 0;
    public int ToStateID = 0;
    public int ObjectID = 0;
    public int Result = 0;
    public float StateTime = 0.0f;
    public float SavePercent = 0.0f;
}

public class OperationObject :  MonoBehaviour
{
    public enum eOperationObjectState
    {
        EOOS_IDEL,
        EOOS_BEING_OPERATION,
        EOOS_INTERRUPT,
        EOOS_DROP,
        EOOS_FINISHING,
        EOOS_FINISHED,
        EOOS_COUNT,
    };

    public enum eChangeOperationObjectStateResult
    {
        ECOOSR_SUCCESS,
        ECOOSR_ERROR_INVALID_OBJECT,
        ECOOSR_ERROR_INVALID_STATE,        
    };

    private eOperationObjectState _CurState = eOperationObjectState.EOOS_IDEL;
    private eOperationObjectState _TryChangeToState = eOperationObjectState.EOOS_IDEL;
    private eOperationObjectState _SaveRequestState = eOperationObjectState.EOOS_COUNT;

    private float _CurStateTime = 0.0f;

    //操作进度条的时间
    //private float _OperationStartTime = 0.0f;

    //中断开始的时间
    //private float _InterruptStartTime = 0.0f;

    //回落开始的时间
    //private float _DropStartTime = 0.0f;

    //结束后播放动画的时间
    //private float _FishingStartTime = 0.0f;

    //保存中断或停止时的进度条( 0 ~ 1 )
    private float _SavePercent = 0.0f;

    private int _SaveOperationPlayerID = 0; //当前操作的PlayerID

    public float OperationSec = 1.0f;   //Editor
    public float HoldSec = 1.0f;    //Editor
    public float DropSec = 2.0f;    //Editor
    public float FinishingTime = 1.0f; //Editor

    public int ID = 0;

    

    public List<vp_OptionSwitch> SwitchList = new List<vp_OptionSwitch>();

    //public List<GameObject> StateListenerVec = new List<GameObject>();

    vp_PlayerEventHandler mPlayer = null;

    public eOperationObjectState GetCurState() { return _CurState; }
    public float GetCurStateTime() { return _CurStateTime; }
    static public Dictionary<int, OperationObject> ID2OperationObjTable = new Dictionary<int, OperationObject>();

    //the state synced from master
    private eOperationObjectState _SyncState = eOperationObjectState.EOOS_COUNT;

    static public OperationObject GetObject( int id )
    {
        if( ID2OperationObjTable.ContainsKey( id ) )
        {
            return ID2OperationObjTable[id];
        }

        return null;
    }

    public void OnInteract( List<object> objVec )
    {
        if (objVec == null)
        {
            return;
        }

        if( objVec.Count < 2 )
        {
            return;
        }

        vp_PlayerEventHandler tempPlayer = objVec[0] as vp_PlayerEventHandler;
        vp_OptionSwitch os = objVec[1] as vp_OptionSwitch;

        if( os != null )
        {
            RequestChangeState(tempPlayer, os.ChangeState, true);
        }   
             
    }

    //只有在本地调用
    public void RequestChangeState(vp_PlayerEventHandler player, eOperationObjectState newState, bool needWait )
    {
        if( !EnableChangeState( newState ) )
        {
            return;
        }      

        //目前没有什么人操作
        if( _SaveOperationPlayerID == 0 )
        {
            if (player != null)
            {
                _SaveOperationPlayerID = vp_MPMaster.GetViewIDOfTransform(player.transform);
            }
            else
            {
                _SaveOperationPlayerID = 0;
            }
        }
        //已经有人操作了
        else
        {
            if (player != null)
            {
                //操作者不是自己不允许操作
                if ( _SaveOperationPlayerID != vp_MPMaster.GetViewIDOfTransform(player.transform) )
                {
                    return;
                }
            }
        }

        mPlayer = player;
        


        //c端
        if( !_IsMaster() )
        {
            if (player != null && _SaveRequestState != newState )
            {
                //发送请求
                vp_GlobalEvent<int, int, int>.Send("C2MChangeOperationState", _SaveOperationPlayerID, ID, (int)newState);

                //等待response
                _SaveRequestState = newState;
                if (needWait)
                {
                    player.SendMessage("SetPlayerOperationState", true);
                }                
            }            
        }
        //m端 直接操作
        else
        {
             _TryChangeToState = newState;     
        }                        
    }


    public void ChangeStateImm( eOperationObjectState newState )
    {
        _TryChangeToState = newState;
        Update();
    }

    public void ChangeStateNextFrame(eOperationObjectState newState)
    {
        _TryChangeToState = newState;
    }

    public void ForceChangeState(eOperationObjectState newState, float stateTime)
    {
        _TryChangeToState = newState;
        _CurStateTime = stateTime;
        Update();
    }

    void Awake()
    {
        _SavePercent = 0.0f;
        _ChangeToState(eOperationObjectState.EOOS_IDEL);
        _TryChangeToState = eOperationObjectState.EOOS_IDEL;

        if (!ID2OperationObjTable.ContainsKey(ID))
        {
            ID2OperationObjTable.Add(ID, this);
        }
    }
    void OnDestroy()
    {
        if (ID2OperationObjTable.ContainsKey(ID))
        {
            ID2OperationObjTable.Remove(ID);
        }
    }

    void OnEnable()
    {
        _SavePercent = 0.0f;
        _ChangeToState(eOperationObjectState.EOOS_IDEL);
        _TryChangeToState = eOperationObjectState.EOOS_IDEL;
    }


    public bool RefreshMC(int playerID)
    {
        eOperationObjectState oldState = _CurState;
        eOperationObjectState newState = _TryChangeToState;
        _TryChangeToState = eOperationObjectState.EOOS_COUNT;


        switch ((eOperationObjectState)_CurState)
        {
            case eOperationObjectState.EOOS_IDEL:
                {
                    switch (newState)
                    {
                        case eOperationObjectState.EOOS_BEING_OPERATION:
                            {
                                _ChangeToState(eOperationObjectState.EOOS_BEING_OPERATION);
                                EnterOperation(0.0f);
                            }
                            break;
                        default:
                            break;
                    }

                    //更新
                    if (_CurState == eOperationObjectState.EOOS_IDEL)
                    {

                    }

                }
                break;
            case eOperationObjectState.EOOS_BEING_OPERATION:
                {
                    switch (newState)
                    {
                        case eOperationObjectState.EOOS_INTERRUPT:
                        case eOperationObjectState.EOOS_DROP:
                            {
                                //_DropStartTime = curTime;
                                LeaveOperation(0.0f);
                                _ChangeToState(newState);
                            }
                            break;
                        default:
                            break;
                    }

                    //更新
                    if (_CurState == eOperationObjectState.EOOS_BEING_OPERATION)
                    {
                        UpdateOperation(0.0f);
                    }
                }
                break;
            //被打断了
            case eOperationObjectState.EOOS_INTERRUPT:
                {
                    switch (newState)
                    {
                        //重新操作
                        case eOperationObjectState.EOOS_BEING_OPERATION:
                            {
                                _ChangeToState(eOperationObjectState.EOOS_BEING_OPERATION);
                                EnterOperation(0.0f);
                            }

                            break;
                        default:
                            break;
                    }

                    //更新
                    if (_CurState == eOperationObjectState.EOOS_INTERRUPT)
                    {
                        if (_CurStateTime >= HoldSec)
                        {
                            //_DropStartTime = curTime;
                            _ChangeToState(eOperationObjectState.EOOS_DROP);
                        }
                    }
                }
                break;
            //开始回落
            case eOperationObjectState.EOOS_DROP:
                {
                    switch (newState)
                    {
                        //重新操作
                        case eOperationObjectState.EOOS_BEING_OPERATION:
                            {
                                _SavePercent = Mathf.Clamp(_SavePercent - MiscUtils.GetPercentAbsClamped(_CurStateTime, DropSec), 0.0f, 1.0f);
                                _ChangeToState(eOperationObjectState.EOOS_BEING_OPERATION);
                                EnterOperation(0.0f);
                            }
                            break;
                        default:
                            break;
                    }

                    //更新
                    if (_CurState == eOperationObjectState.EOOS_DROP)
                    {
                        float percent = GetOperationPercent(0);
                        if (_CurStateTime >= DropSec || percent <= 0.0f)
                        {
                            _SavePercent = 0.0f;
                            _ChangeToState(eOperationObjectState.EOOS_IDEL);
                        }
                    }
                }
                break;
            case eOperationObjectState.EOOS_FINISHING:
                {
                    if (_CurStateTime >= FinishingTime)
                    {
                        _ChangeToState(eOperationObjectState.EOOS_FINISHED);
                    }
                }
                break;
            case eOperationObjectState.EOOS_FINISHED:
                {
                    //debug
                    //_SavePercent = 0.0f;
                    //_ChangeToState( eOperationObjectState.EOOS_IDEL );
                    //
                }
                break;
            default:
                break;

        }

        if( oldState != _CurState )
        {            
            if( !( _CurState == eOperationObjectState.EOOS_BEING_OPERATION || _CurState == eOperationObjectState.EOOS_FINISHED ) )
            {
                _SaveOperationPlayerID = 0;
            }

            //更新switch List
            int localPlayerID = 0;
            if (mPlayer != null)
            {
                localPlayerID = vp_MPMaster.GetViewIDOfTransform(mPlayer.transform);
            }
            //有人在操作
            if ( _SaveOperationPlayerID > 0 )
            {
                //操作人是自己
                if (_SaveOperationPlayerID == localPlayerID)
                {
                    foreach (vp_OptionSwitch os in SwitchList)
                    {
                        if (os != null)
                        {
                            os.gameObject.SetActive(false);
                            foreach (OperationObject.eOperationObjectState e in os.BelongState)
                            {
                                if (e == _CurState)
                                {
                                    os.gameObject.SetActive(true);
                                    break;
                                }
                            }
                        }
                    }
                }
                //操作人不是自己保存的则不允许操作
                else
                {
                    foreach (vp_OptionSwitch os in SwitchList)
                    {
                        if (os != null)
                        {
                            os.gameObject.SetActive(false);
                        }
                    }
                }
            }
            //没有人操作,则更新操作按钮的状态
            else
            {
                foreach (vp_OptionSwitch os in SwitchList)
                {
                    if (os != null)
                    {
                        os.gameObject.SetActive(false);
                        foreach (OperationObject.eOperationObjectState e in os.BelongState)
                        {
                            if (e == _CurState)
                            {
                                os.gameObject.SetActive(true);
                                break;
                            }
                        }
                    }
                }
            }

            if (mPlayer != null)
            {
                mPlayer.SendMessage("OnOptionStateSwitch", this, SendMessageOptions.DontRequireReceiver);
            }

            if (_CurState != eOperationObjectState.EOOS_BEING_OPERATION && _CurState != eOperationObjectState.EOOS_FINISHING)
            {
                mPlayer = null;
            }

            if (_IsMaster() && vp_Gameplay.IsMultiplayer)
            {
                OperationStateChangeParam changeParam = new OperationStateChangeParam();

                changeParam.ToStateID = (int)GetCurState();
                changeParam.StateTime = GetCurStateTime();
                changeParam.Result = (int)eChangeOperationObjectStateResult.ECOOSR_SUCCESS;
                changeParam.PlayerID = playerID;
                changeParam.ObjectID = ID;
                changeParam.SavePercent = _SavePercent;

                //M2CChangeOperationObjState(changeParam);

                vp_GlobalEvent<OperationStateChangeParam>.Send("M2CChangeOperationStateImm", changeParam);
            }


            return true; 
        }

        return false;
    }

    private void _UpdateMC()
    {

        float curTime = MiscUtils.GetCurBattleTime();


        int playerID = 0;
        if( mPlayer != null )
        {
            playerID = vp_MPMaster.GetViewIDOfTransform( mPlayer.transform );
        }

        bool hasChanged = RefreshMC( _SaveOperationPlayerID );

        _CurStateTime += Time.deltaTime;
    }


    void _RefreshC( int playerID )
    {
        eOperationObjectState newState = _TryChangeToState;
        _TryChangeToState = eOperationObjectState.EOOS_COUNT;

        if (newState != eOperationObjectState.EOOS_COUNT)
        {

            //更新switch List
            if (_CurState != newState)
            {
                _CurState = newState;

                int localPlayerID = -1;

                if (!(_CurState == eOperationObjectState.EOOS_BEING_OPERATION || _CurState == eOperationObjectState.EOOS_FINISHING))
                {
                    _SaveOperationPlayerID = 0;
                }


                if( mPlayer != null )
                {
                    localPlayerID = vp_MPMaster.GetViewIDOfTransform(mPlayer.transform);
                }
                
                //如果操作的是自己才会修改状态
                if( _SaveOperationPlayerID > 0 )
                {
                    if (_SaveOperationPlayerID == localPlayerID)
                    {
                        foreach (vp_OptionSwitch os in SwitchList)
                        {
                            if (os != null)
                            {
                                os.gameObject.SetActive(false);
                                foreach (OperationObject.eOperationObjectState e in os.BelongState)
                                {
                                    if (e == _CurState)
                                    {
                                        os.gameObject.SetActive(true);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (vp_OptionSwitch os in SwitchList)
                        {
                            if (os != null)
                            {
                                os.gameObject.SetActive(false);
                            }
                        }
                    }
                }
                else
                {
                    foreach (vp_OptionSwitch os in SwitchList)
                    {
                        if (os != null)
                        {
                            os.gameObject.SetActive(false);
                            foreach (OperationObject.eOperationObjectState e in os.BelongState)
                            {
                                if (e == _CurState)
                                {
                                    os.gameObject.SetActive(true);
                                    break;
                                }
                            }
                        }
                    }

                }

                if (mPlayer != null)
                {
                    mPlayer.SendMessage("OnOptionStateSwitch", this, SendMessageOptions.DontRequireReceiver);
                }

                if (_CurState != eOperationObjectState.EOOS_BEING_OPERATION && _CurState != eOperationObjectState.EOOS_FINISHING)
                {
                    mPlayer = null;
                }
            }
        }
    }

    void _UpdateC()
    {

        int playerID = 0;
        if (mPlayer != null)
        {
            playerID = vp_MPMaster.GetViewIDOfTransform(mPlayer.transform);
        }
        _RefreshC(playerID);
        _CurStateTime += Time.deltaTime;
    }

    void Update()
    {
        if (_IsMaster())
        {
            _UpdateMC();
        }        
        else
        {
            _UpdateC();
        }
    }

    void EnterOperation( float curTime )
    {
        //以后会替换为服务器的时间
        _CurStateTime = 0.0f;
    }
    void LeaveOperation( float curTime )
    {
        //_SavePercent = Mathf.Clamp(_SavePercent + MiscUtils.GetPercentAbsClamped(curTime - _OperationStartTime, OperationSec), 0.0f, 1.0f);
        _SavePercent = Mathf.Clamp(_SavePercent + MiscUtils.GetPercentAbsClamped( _CurStateTime, OperationSec), 0.0f, 1.0f);
        Mathf.Clamp(_SavePercent, 0.0f, 1.0f);
    }    

    public float GetOperationPercent( float curSec )
    {
        //float curSec = MiscUtils.GetCurBattleTime();
        switch( _CurState )
        {
            case eOperationObjectState.EOOS_IDEL:
                {
                    return 0.0f;
                }
            case eOperationObjectState.EOOS_BEING_OPERATION:
                {
                    //float percent = Mathf.Clamp( _SavePercent + MiscUtils.GetPercentAbsClamped(curSec - _OperationStartTime, OperationSec), 0.0f, 1.0f );
                    float percent = Mathf.Clamp(_SavePercent + MiscUtils.GetPercentAbsClamped( _CurStateTime, OperationSec), 0.0f, 1.0f);
                    return percent;
                }
            case eOperationObjectState.EOOS_DROP: 
                {
                    float curPercent = MiscUtils.GetPercentAbsClamped(_CurStateTime, DropSec);
                    float percent = Mathf.Clamp(_SavePercent - curPercent, 0.0f, 1.0f);
                    return percent;
                }
            case eOperationObjectState.EOOS_INTERRUPT:
                {
                    return _SavePercent;
                }
            case eOperationObjectState.EOOS_FINISHED:
            case eOperationObjectState.EOOS_FINISHING:
                {
                    return 1.0f;
                }
            default:
                break;
        }

        return 0.0f;
    }

    void UpdateOperation( float curTime )
    {        
        float curPercent = GetOperationPercent( curTime );
        if( curPercent >= 1.0f )
        {
            LeaveOperation(curTime);
            _ChangeToState(eOperationObjectState.EOOS_FINISHING);
            //_FishingStartTime = curTime;
        }
    }

    private void _ChangeToState( eOperationObjectState newState )
    {
        _CurStateTime = 0.0f;
        _CurState = newState;
    }


    private bool _IsMaster()
    {
        //multi mode is true
        //single mode is false 
        bool isMultiPlayer = vp_Gameplay.IsMultiplayer;

        //single mode is true
        //multi mode if is master then true or false
        bool isMaster = vp_Gameplay.IsMaster;

        //change the state imm and broadcast the message
        if (isMaster || (!isMultiPlayer))
        {
            return true;
        }

        return false;
    }

    static public eChangeOperationObjectStateResult TryChangeState( int playerID, int objectID, int toState )
    {
        if( !ID2OperationObjTable.ContainsKey( objectID ) )
        {
            return eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_OBJECT;
        }

        OperationObject obj = ID2OperationObjTable[objectID];

        if( !Enum.IsDefined(typeof( eOperationObjectState ), toState ) )
        {
            return eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_STATE;
        }

        //已经有人操作了,并且不是记录的那个人
        if( obj._SaveOperationPlayerID > 0 && obj._SaveOperationPlayerID != playerID )
        {
            return eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_STATE;
        }        

        eOperationObjectState eToState = (eOperationObjectState)toState;        

        if( obj != null )
        {
            switch (eToState)
            {
                case eOperationObjectState.EOOS_BEING_OPERATION:
                    {
                        if( obj.GetCurState() == eOperationObjectState.EOOS_IDEL 
                            || obj.GetCurState() == eOperationObjectState.EOOS_DROP 
                            || obj.GetCurState() == eOperationObjectState.EOOS_INTERRUPT )
                        {
                            obj._SaveOperationPlayerID = playerID;
                            obj.ChangeStateNextFrame( eToState );
                        }
                        else
                        {
                            return eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_STATE;
                        }
                    }
                    break;
                case eOperationObjectState.EOOS_INTERRUPT:
                    {
                        if ( obj.GetCurState() == eOperationObjectState.EOOS_BEING_OPERATION )
                        {
                            obj._SaveOperationPlayerID = playerID;
                            obj.ChangeStateNextFrame(eToState);
                        }
                        else
                        {
                            return eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_STATE;
                        }
                    }
                    break;
                case eOperationObjectState.EOOS_DROP:
                    {
                        if (obj.GetCurState() == eOperationObjectState.EOOS_BEING_OPERATION)
                        {
                            obj._SaveOperationPlayerID = playerID;
                            obj.ChangeStateNextFrame(eToState);
                        }
                        else
                        {
                            return eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_STATE;
                        }
                    }
                    break;
            }
        }

        return eChangeOperationObjectStateResult.ECOOSR_SUCCESS;
    }

    public bool EnableChangeState( eOperationObjectState newState )
    {
        switch( _CurState )
        {
            case eOperationObjectState.EOOS_IDEL:
                {
                    return newState == eOperationObjectState.EOOS_BEING_OPERATION;
                }
            case eOperationObjectState.EOOS_BEING_OPERATION:
                {
                    return (newState == eOperationObjectState.EOOS_DROP || newState == eOperationObjectState.EOOS_INTERRUPT);
                }
            case eOperationObjectState.EOOS_INTERRUPT:
                {
                    return (newState == eOperationObjectState.EOOS_BEING_OPERATION );
                }
            case eOperationObjectState.EOOS_DROP:
                {
                    return (newState == eOperationObjectState.EOOS_BEING_OPERATION);
                }
            default:
                break;
        }

        return false;
    }

    public void OnRequestChangeStateResult( int result, int playerID, int ObjectID, int stateID, float stateTime, float savePercent )
    {
        if( Enum.IsDefined(typeof( eChangeOperationObjectStateResult ), result ) )
        {
            eOperationObjectState toState = eOperationObjectState.EOOS_COUNT;
            if( Enum.IsDefined( typeof( eOperationObjectState ), stateID ) )
            {
                toState = (eOperationObjectState)stateID;
            }

            _SaveRequestState = eOperationObjectState.EOOS_COUNT;

            if( mPlayer != null )
            {
                mPlayer.SendMessage("SetPlayerOperationState", false);
            }

            _SaveOperationPlayerID = playerID;
            _SavePercent = savePercent;
            ForceChangeState(toState, stateTime);
            _RefreshC(playerID);
            
            eChangeOperationObjectStateResult eResult = (eChangeOperationObjectStateResult)result;            
            switch( eResult )
            {
                case eChangeOperationObjectStateResult.ECOOSR_SUCCESS:
                    {                        
                        
                    }
                    break;
                case eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_STATE:
                    {

                    }
                    break;
                case eChangeOperationObjectStateResult.ECOOSR_ERROR_INVALID_OBJECT:
                    {

                    }
                    break;
                default:
                    {

                    }
                    break;
            }
        }
    }


    public static bool IsAllOperationFished()
    {
        foreach (KeyValuePair<int, OperationObject> kv in ID2OperationObjTable)
        {
            if( kv.Value._CurState != eOperationObjectState.EOOS_FINISHED )
            {
                return false;
            }            
        }
        return true;

        
    }


}


