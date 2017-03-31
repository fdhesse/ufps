/////////////////////////////////////////////////////////////////////////////////
//	OperationObject.cs
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class FlagObject : MonoBehaviour
{
    public bool EnableProgress = false;
    public bool EnableIcon = false;
    public bool EnableArrow = false;
    public bool EnableText = false;
    public bool IsInCamera = false;

    private float _LastRenderTime = 0;
    private float _CurRenderTime = 0;

    public float CurProgress = 0.0f;
    public bool CurIconVisible = false;
    public bool CurArrowVisible = false;
    public bool CurProgressVisible = false;
    public bool CurTextVisible = false;
    public bool CurVisble = true;
    public string CurText = "";
    public Vector3 ViewPos = new Vector3();
    public Vector2 ScreenPos = new Vector3();
    public Vector3 ToPlayerPos = new Vector3();



    public int ID = 0;
    protected vp_FPPlayerEventHandler m_Player = null;

    protected vp_FPPlayerEventHandler LocalPlayer	// lazy initialization of the event handler field
    {
        get
        {
            if (m_Player == null)
                m_Player = GameObject.FindObjectOfType<vp_FPPlayerEventHandler>();
            return m_Player;
        }
    }

    protected vp_FPCamera m_FPCamera = null;
    public vp_FPCamera FPCamera
    {
        get
        {            
            if (m_FPCamera == null)
                m_FPCamera = GameObject.FindObjectOfType<vp_FPCamera>();
            return m_FPCamera;
        }
    }

    static public Dictionary<int, FlagObject> FlagOjectTable = new Dictionary<int, FlagObject>();
    static public int GenID = 0;

    static public FlagObject GetObject( int ID )
    {
        if( FlagOjectTable.ContainsKey( ID ) )
        {
            return FlagOjectTable[ID];
        }

        return null;
    }
    void Awake()
    {
        ID = (++GenID);
        FlagOjectTable.Add( ID, this );
        
    }
    void OnDestroy()
    {
        FlagOjectTable.Remove( ID );
    }

    void OnEnable()
    {

    }
    void Update()
    {
        RefreshCurrentStatus();
    }

    public void RefreshCurrentStatus()
    {
        if (Camera.main == null)
        {
            return;
        }

        //if( _LastRenderTime != _CurRenderTime )
        //{
        //    _LastRenderTime = _CurRenderTime;
        //    IsInCamera = true;
        //}
        //else
        //{
        //    IsInCamera = false;
        //}
        
            
        Transform parent = this.transform.parent;
        if (parent != null)
        {
            OperationObject obj = parent.GetComponent<OperationObject>();
            if( obj != null )
            {
                if( obj.GetType() == OperationObject.eOperationObjectType.EOOT_COLLECTIONPOINT )
                {
                    CollectionPoint pt = obj as CollectionPoint;
                    if (pt != null && !pt.IsInLocalPlayerTeam())
                    {
                        gameObject.SetActive( false );
                    }
                }
                float curTime = MiscUtils.GetCurBattleTime();
                CurProgress = obj.GetOperationPercent( curTime );

                CurIconVisible = true;
                CurArrowVisible = true;

                float dis = 0.0f;
                vp_PlayerEventHandler player = LocalPlayer;
                if( player != null )
                {
                    dis = Vector3.Distance(transform.position, player.transform.position);
                }

                
                CurText = dis.ToString( "f1" ) + "m";
                CurTextVisible = (dis > 1.0f);

                /*
                 * public Transform follow;
                Vector2 position = Camera.main.WorldToScreenPoint(follow.position);
                img.rectTransform.position = position;//位置
                img.rectTransform.localScale = new Vector2(2,2)
                 */

                ViewPos = Camera.main.WorldToViewportPoint(this.transform.position);
                ViewPos.z = 0.0f;


                IsInCamera = (ViewPos.x < 1.0f && ViewPos.x > 0.0f && ViewPos.y < 1.0f && ViewPos.y > 0.0f);
                //ScreenPos = Camera.main.WorldToScreenPoint(this.transform.position);

                ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, this.transform.position);
                
                OperationObject.eOperationObjectState state = obj.GetCurState();

                if ( state == OperationObject.eOperationObjectState.EOOS_FINISHED || state == OperationObject.eOperationObjectState.EOOS_INVALID )
                {
                    CurVisble = false;
                }
                else if( state == OperationObject.eOperationObjectState.EOOS_COUNT 
                    || state == OperationObject.eOperationObjectState.EOOS_IDEL )
                {
                    CurProgressVisible = false;
                    CurVisble = true;
                }
                else
                {
                    CurProgressVisible = EnableProgress;                    
                    CurVisble = true;
                }                                

                return;
            }

            ExitField objField = parent.GetComponent<ExitField>();
            if( objField )
            {
                CurVisble = objField.gameObject.GetActive();

                ViewPos = Camera.main.WorldToViewportPoint(this.transform.position);
                ViewPos.z = 0.0f;
                IsInCamera = (ViewPos.x < 1.0f && ViewPos.x > 0.0f && ViewPos.y < 1.0f && ViewPos.y > 0.0f);
                ScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, this.transform.position);


                float dis = 0.0f;
                vp_PlayerEventHandler player = LocalPlayer;
                if (player != null)
                {
                    dis = Vector3.Distance(transform.position, player.transform.position);
                }


                CurText = dis.ToString("f1") + "m";
                CurTextVisible = (dis > 1.0f);
            }
        }
    }

    void OnWillRenderObject()
    {
        _CurRenderTime = Time.time;
    }

    public Sprite IconSprite = null;
}


