/////////////////////////////////////////////////////////////////////////////////
//
//	OperationProgressHUD.cs
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEngine.UI;
public class FlagIcon : MonoBehaviour
{
    public OperationProgressHUD Progress = null;
    public Text Label = null;
    public GameObject Icon = null;
    public GameObject Arrow = null;

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
        if (Progress != null)
        {
            Progress.SetPercent(percent);
        }
    }

    public void SetBarColor( float r, float g, float b, float a )
    {
        if (Progress != null)
        {
            Progress.SetBarColor( r, g, b, a );
        }
    }

    public void SetBarVisible( bool visible )
    {
        if( Progress != null )
        {
            Progress.gameObject.SetActive( visible );
        }
    }

    public void SetIconVisible(bool visible)
    {
        if (Icon != null)
        {
            Icon.gameObject.SetActive(visible);
        }
    }

    public void SetArrowVisible(bool visible)
    {
        if (Arrow != null)
        {
            Arrow.gameObject.SetActive(visible);
        }
    }

    public void SetImage( Sprite img )
    {
        if (Icon != null && img != null )
        {
            Image icon = Icon.GetComponent<Image>();
			if( icon != null )
			{
				icon.sprite = img;
			}            
        }
    }

    public void SetTextVisible( bool visible )
    {
        if( Label != null )
        {
            Label.gameObject.SetActive( visible );
        }
    }

    public void SetText( string text )
    {
        if (Label != null)
        {
            Label.text = text;
        }
    }

    public void SetPosition( Vector3 pos )
    {
        this.transform.position = pos;
    }

    public void SetRotation( float rot )
    {
        if( Arrow != null )
        {            
            Arrow.transform.rotation = Quaternion.AngleAxis( rot, Vector3.back );
        }
    }

    public void SetTranslation( Vector3 pos, float rot )
    {
        SetPosition( pos );
        SetRotation( rot );
    }
}

