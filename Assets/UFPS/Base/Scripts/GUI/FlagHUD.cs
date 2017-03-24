/////////////////////////////////////////////////////////////////////////////////
//
//	OperationProgressHUD.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class FlagHUD : MonoBehaviour
{
    public FlagIcon TempIcon = null;
    public Dictionary<int, FlagIcon> ID2FlagTable = new Dictionary<int, FlagIcon>();
    public Canvas UsingCanvas = null;

    protected Quaternion _LastCameraRot = new Quaternion();
    protected Vector3 _LastCameraPos = new Vector3();
    public void AddHUD(  )
    {

    }
	protected virtual void Awake()
	{
        ID2FlagTable.Clear();
	}
	
	
	protected virtual void OnEnable()
	{

	}


	protected virtual void OnDisable()
	{
        foreach( KeyValuePair<int, FlagIcon> kv in ID2FlagTable )
        {
            GameObject.DestroyObject( kv.Value );
        }
        ID2FlagTable.Clear();
	}

    void Update()
    {
        if( TempIcon == null )
        {
            return;
        }

        if( Camera.main == null )
        {
            return;
        }

        float distance = Vector3.Distance(_LastCameraPos, Camera.main.transform.position);
        float rot = Mathf.Abs(Quaternion.Angle(_LastCameraRot, Camera.main.transform.rotation));
        bool needUpdate = false;        

        if( distance > 0.2f || rot > 0.5f  )
        {
            _LastCameraPos = Camera.main.transform.position;
            _LastCameraRot = Camera.main.transform.rotation;
            needUpdate = true;
        }

        foreach (KeyValuePair<int, FlagObject> obj in FlagObject.FlagOjectTable)
        {
            if( !ID2FlagTable.ContainsKey( obj.Key ) )
            {
                FlagIcon icon = GameObject.Instantiate(TempIcon);
                icon.transform.SetParent(this.transform, false);
                if (obj.Value.IconSprite != null)
                {
                    icon.SetImage(obj.Value.IconSprite);   
                }
                ID2FlagTable.Add(obj.Key, icon);
            }
        }

        foreach( KeyValuePair<int, FlagIcon> kv in ID2FlagTable )
        {
            FlagObject obj = FlagObject.GetObject( kv.Key );
            if (obj != null && obj.gameObject.GetActive() && obj.CurVisble)
            {

                //transform icon position
                if( needUpdate )
                {

                    //Debug.Log("V: x:" + obj.ViewPos.x.ToString() + "y:" + obj.ViewPos.y.ToString());
                    //Debug.Log("S: x:" + obj.ScreenPos.x.ToString() + "y:" + obj.ScreenPos.y.ToString());

                    //if in camera
                    if( obj.IsInCamera )
                    {
                        CanvasScaler canvasScaler = UsingCanvas.GetComponent<CanvasScaler>();
                        float resolutionX = canvasScaler.referenceResolution.x;
                        float resolutionY = canvasScaler.referenceResolution.y;
                        float offect = (Screen.width / canvasScaler.referenceResolution.x) * (1 - canvasScaler.matchWidthOrHeight) + (Screen.height / canvasScaler.referenceResolution.y) * canvasScaler.matchWidthOrHeight;

                        RectTransform trs = kv.Value.GetComponent<RectTransform>();
                        if (trs != null)
                        {
                            trs.anchoredPosition3D = new Vector3(obj.ScreenPos.x / offect, obj.ScreenPos.y / offect, 0);
                        }
                        
                        kv.Value.SetArrowVisible( false );
                    }
                   // out side draw arrow and icon in rect side
                    else
                    {
                        kv.Value.SetArrowVisible( obj.EnableArrow );

                        //float curRot = vp_3DUtility.LookAtAngleHorizontal( Camera.main.transform.position, Camera.main.transform.forward, obj.transform.position);
                        //kv.Value.SetRotation(curRot);

                        RectTransform trs = kv.Value.GetComponent<RectTransform>();
                        if (trs != null)
                        {
                            CanvasScaler canvasScaler = UsingCanvas.GetComponent<CanvasScaler>();
                            float resolutionX = canvasScaler.referenceResolution.x;
                            float resolutionY = canvasScaler.referenceResolution.y;
                            float offect = (Screen.width / canvasScaler.referenceResolution.x) * (1 - canvasScaler.matchWidthOrHeight) + (Screen.height / canvasScaler.referenceResolution.y) * canvasScaler.matchWidthOrHeight;

                            //trs.anchoredPosition3D = new Vector3( Screen.width * 0.5f / offect,  Screen.height * 0.5f / offect, 0.0f);     
                            
                            Vector2 targetPos = new Vector2( obj.ScreenPos.x / offect, obj.ScreenPos.y / offect  );
                            Vector2 centerPos = new Vector2( Screen.width * 0.5f / offect,  Screen.height * 0.5f / offect );
                            Vector2 dir = ( targetPos - centerPos ).normalized;

                            if( Mathf.Abs( dir.x ) > 0.001f )
                            {
                                if( dir.x < 0.0f )
                                {
                                    float curRot = Mathf.Asin(dir.y) * 180 / Mathf.PI;
                                    kv.Value.SetRotation(curRot);
                                }
                                else
                                {
                                    float curRot = 180.0f - Mathf.Asin(dir.y) * 180 / Mathf.PI;
                                    kv.Value.SetRotation(curRot);

                                }

                                //float x = centerPos.x + (Screen.width * 0.5f - 40.0f) * dir.x / Mathf.Abs(dir.x) / offect;
                                //float y = centerPos.y + (Screen.width * 0.5f - 40.0f) * dir.y / dir.x / offect;  

                                //trs.anchoredPosition3D = new Vector3( x, y, 0.0f);
                                float offset = 40.0f;
                                Vector2 point = new Vector2();
                                if (detectIntersect(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), obj.ScreenPos, new Vector2(offset, offset), new Vector2(offset, Screen.height - offset), out point))
                                {
                                    trs.anchoredPosition3D = new Vector3(point.x / offect, point.y / offect, 0.0f);
                                }
                                else if (detectIntersect(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), obj.ScreenPos, new Vector2(offset, offset), new Vector2(Screen.width - offset, offset), out point))
                                {
                                    trs.anchoredPosition3D = new Vector3(point.x / offect, point.y / offect, 0.0f);
                                }
                                else if (detectIntersect(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), obj.ScreenPos, new Vector2(Screen.width - offset, offset), new Vector2(Screen.width - offset, Screen.height - offset), out point))
                                {
                                    trs.anchoredPosition3D = new Vector3(point.x / offect, point.y / offect, 0.0f);
                                }
                                else if (detectIntersect(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), obj.ScreenPos, new Vector2(Screen.width - offset, Screen.height - offset), new Vector2( offset, Screen.height - offset), out point))
                                {
                                    trs.anchoredPosition3D = new Vector3(point.x / offect, point.y / offect, 0.0f);
                                }
                            }                            
                        }                        
                    }


                    kv.Value.SetText( obj.CurText );
                  
                }

                if( obj.IsInCamera )
                {
                    kv.Value.SetBarVisible(obj.CurProgressVisible);
                }
                else
                {
                    kv.Value.SetBarVisible(false);
                }

                if (MathUtils.RayUtils.IsPointForward(new Ray(Camera.main.transform.position, Camera.main.transform.forward), obj.transform.position))
                {
                    if (!kv.Value.gameObject.GetActive())
                    {
                        kv.Value.gameObject.SetActive(true);
                    }  
                }
                else
                {
                    if (kv.Value.gameObject.GetActive())
                    {
                        kv.Value.gameObject.SetActive(false);
                    }  
                }

                kv.Value.SetPercent(obj.CurProgress);
              
            }
            else
            {
                if (kv.Value.gameObject.GetActive())
                {
                    kv.Value.gameObject.SetActive(false);
                } 
            }
        }



    }

    bool between( float a, float X0, float X1)
    {
        double temp1 = a - X0;
        double temp2 = a - X1;
        if ((temp1 < 1e-8 && temp2 > -1e-8) || (temp2 < 1e-6 && temp1 > -1e-8))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    // 判断两条直线段是否有交点，有则计算交点的坐标  
    // p1,p2是直线一的端点坐标  
    // p3,p4是直线二的端点坐标  
    bool detectIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 outValue)
    {
        outValue = Vector2.zero;
        if ((Mathf.Abs(p1.x - p2.x) < 1e-6) && (Mathf.Abs(p3.x - p4.x) < 1e-6))
        {
            return false;
        }

        else if ((Mathf.Abs(p1.x - p2.x) < 1e-6)) //如果直线段p1p2垂直与y轴  
        {
            if (between(p1.x, p3.x, p4.x))
            {
                float k = (p4.y - p3.y) / (p4.x - p3.x);
                outValue.x = p1.x;
                outValue.y = k * (outValue.x - p3.x) + p3.y;

                if (between( outValue.y, p1.y, p2.y))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else if ((Mathf.Abs(p3.x - p4.x) < 1e-6)) //如果直线段p3p4垂直与y轴  
        {
            if (between(p3.x, p1.x, p2.x))
            {
                float k = (p2.y - p1.y) / (p2.x - p1.x);
                outValue.x = p3.x;
                outValue.y = k * (outValue.x - p2.x) + p2.y;

                if (between(outValue.y, p3.y, p4.y))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            float k1 = (p2.y - p1.y) / (p2.x - p1.x);
            float k2 = (p4.y - p3.y) / (p4.x - p3.x);

            if (Mathf.Abs(k1 - k2) < 1e-6)
            {
                return false;
            }
            else
            {
                outValue.x = ((p3.y - p1.y) - (k2 * p3.x - k1 * p1.x)) / (k1 - k2);
                outValue.y = k1 * (outValue.x - p1.x) + p1.y;
            }

            if (between(outValue.x, p1.x, p2.x) && between(outValue.x, p3.x, p4.x))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    } 
}

