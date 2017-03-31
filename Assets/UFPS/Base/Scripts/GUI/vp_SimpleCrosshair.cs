/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SimpleCrosshair.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	this script is just a stub for your own a way cooler crosshair
//					system. it simply draws a classic FPS crosshair center screen.
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_SimpleCrosshair : MonoBehaviour
{

	// crosshair texture
	public Texture m_ImageCrosshair = null;

    private Texture mFireCrosshair = null;

	public bool Hide = false;					// use this if you want to hide the crosshair without disabling it (crosshair needs to be enabled for interaction to work)
	public bool HideOnFirstPersonZoom = true;
	public bool HideOnDeath = true;
    public float MinCrossfairSize = 30;
    public float MaxCrosshairSize = 200;

    public Camera FPSCamera;

	protected vp_FPPlayerEventHandler m_Player = null;
    private vp_FPAccuracyController _accuracyController;

    private Vector3 _SaveCameraPos = new Vector3();
    private Quaternion _SaveCameraRot = new Quaternion();

    public List<string> TagName = new List<string>();
    public List<Color> TagColor = new List<Color>();

    public Color FriendColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);

    private Dictionary<string, Color> _Tag2ColorTable = new Dictionary<string, Color>();
    private Color CurColor = new Color();
    public float CrossHairDistanceRatio = 0.4f;

    public float Interval = 0.3f;
    private float _LastUpdateTime = 0.0f;
    public float Distance
    {
        get
        {
            return null == _accuracyController
                ? (m_ImageCrosshair.width + 30)
                : CrossHairDistanceRatio * Screen.width * (_accuracyController.CurrentAccuracy / FPSCamera.fieldOfView);
        }
    }

	    public float Size
    {
        get
        {
            return null == _accuracyController
                ? MinCrossfairSize
                : Screen.width * (_accuracyController.CurrentAccuracy / FPSCamera.fieldOfView);
        }
    }
	
	protected virtual void Awake()
	{
        mFireCrosshair = m_ImageCrosshair;
	    _accuracyController = GetComponent<vp_FPAccuracyController>();
        m_Player = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler; // cache the player event handler


        TagName.Add("Enemy");
        TagColor.Add( new Color( 1.0f, 0.0f, 0.0f, 1.0f ) );

        int len = Mathf.Min(TagColor.Count, TagName.Count);

        for( int i = 0; i < len; ++i )
        {
            if( i < TagColor.Count && i < TagName.Count )
            {
                if (!_Tag2ColorTable.ContainsKey(TagName[i]))
                {
                    _Tag2ColorTable[TagName[i]] = TagColor[i];
                }                
            }
        }
        
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{

		// allow this monobehaviour to talk to the player event handler
		if (m_Player != null)
			m_Player.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{

		// unregister this monobehaviour from the player event handler
		if (m_Player != null)
			m_Player.Unregister(this);

	}


	/// <summary>
	/// draws the crosshair texture smack in the middle of the screen
	/// </summary>
	void OnGUI()
	{

		if (m_ImageCrosshair == null)
			return;

		if (Hide)
			return;

		if(HideOnFirstPersonZoom && m_Player.Zoom.Active && m_Player.IsFirstPerson.Get())
			return;

		if(HideOnDeath && m_Player.Dead.Active)
			return;

        if (CurColor != null)
        {
            GUI.color = CurColor;
        }   

        if (mFireCrosshair == null || (mFireCrosshair != null && mFireCrosshair.name == m_ImageCrosshair.name))
        {
            var rect = new Rect((Screen.width * 0.5f) - m_ImageCrosshair.width * .5f,
                                    (Screen.height * 0.5f) - m_ImageCrosshair.height * .5f - Distance,
                                    m_ImageCrosshair.width,
                                    m_ImageCrosshair.height);
            GUI.DrawTexture(rect, m_ImageCrosshair);
            GUIUtility.RotateAroundPivot(90, new Vector2(Screen.width * .5f, Screen.height * .5f));
            GUI.DrawTexture(rect, m_ImageCrosshair);
            GUIUtility.RotateAroundPivot(90, new Vector2(Screen.width * .5f, Screen.height * .5f));
            GUI.DrawTexture(rect, m_ImageCrosshair);
            GUIUtility.RotateAroundPivot(90, new Vector2(Screen.width * .5f, Screen.height * .5f));
            GUI.DrawTexture(rect, m_ImageCrosshair);
        }
        else
        {
            GUI.DrawTexture(
                new Rect((Screen.width * 0.5f) - (Size * 0.5f),
                         (Screen.height * 0.5f) - (Size * 0.5f),
                         Size,
                         Size),
                m_ImageCrosshair);            
        }

        //GUI.color = Color.white;
	}
	
	
	protected virtual Texture OnValue_Crosshair
	{
		get { return m_ImageCrosshair; }
		set { m_ImageCrosshair = value; }
	}
	
    void Update()
    {

        if ( Time.time - _LastUpdateTime < Interval )
        {
            return;
        }

        _LastUpdateTime = Time.time;

        if( Camera.main != null )
        {
            CurColor = Color.white;

            Vector3 pos = Camera.main.transform.position;
            Vector3 dir = Camera.main.transform.forward;

            float offsetDis = Vector3.Distance( pos, _SaveCameraPos );
            
            float offsetAngle = Quaternion.Angle( Camera.main.transform.rotation, _SaveCameraRot );

            //if( offsetDis> 0.2f || offsetAngle > 0.5f )
            {
                _SaveCameraRot = Camera.main.transform.rotation;
                _SaveCameraPos = pos;

                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(pos, dir, out hitInfo))
                {
                    //Debug.Log("Current Hit : " + hitInfo.transform.name);

                    if( _Tag2ColorTable.ContainsKey( hitInfo.transform.tag ) )
                    {
                        CurColor = _Tag2ColorTable[hitInfo.transform.tag];
                    }
                    else if( hitInfo.transform.tag == "Player" )
                    {
                        vp_MPTeam teamInfo = vp_MPMaster.GetTeamInfoByTransform( hitInfo.transform );
                        if( teamInfo != null )
                        {
                            if( vp_MPLocalPlayer.Instance.TeamNumber == teamInfo.Number )
                            {
                                CurColor = FriendColor;
                            }
                            else
                            {
                                if (_Tag2ColorTable.ContainsKey("Enemy"))
                                {
                                    CurColor = _Tag2ColorTable["Enemy"];
                                }  
                            }
                        }                        
                    }

                }

            }

        }
    }

}

