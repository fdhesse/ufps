/////////////////////////////////////////////////////////////////////////////////
//vp_OptionSwitchMsg
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_OptionSwitchMsg : MonoBehaviour
{
    public GameObject Target = null;
    public string Msg = "";
    public List<string> Params = new List<string>();

	public void OnSend()
    {
        if( Target != null )
        {
             Target.SendMessage( Msg, Params, SendMessageOptions.DontRequireReceiver);
        }
    }
}
