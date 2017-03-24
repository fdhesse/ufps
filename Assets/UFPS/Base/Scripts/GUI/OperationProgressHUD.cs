/////////////////////////////////////////////////////////////////////////////////
//
//	OperationProgressHUD.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
public class OperationProgressHUD : MonoBehaviour
{
    public GameObject Bar = null;
	protected virtual void Awake()
	{
		
	}
	
	
	protected virtual void OnEnable()
	{

	}


	protected virtual void OnDisable()
	{

	}

    public void SetPercent( float percent )
    {        
        if( Bar != null )
        {
            percent = Mathf.Clamp01( percent );
            Vector3 scale = Bar.transform.localScale;
            scale.x = percent;
            Bar.transform.localScale = scale;
        }
    }

    public void SetBarColor( float r, float g, float b, float a )
    {
        if( Bar != null )
        {
            Image img = Bar.gameObject.GetComponent<Image>();
            if( img != null )
            {
                img.color = new Color(r, g, b, a);
            }
        }
    }
}

