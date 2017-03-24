/////////////////////////////////////////////////////////////////////////////////
//
//	ExitZone.cs
//					
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExitField : MonoBehaviour
{
    public ExitZone Zone = null;

	void Start()
	{

	}
    
	/// <summary>
	/// 
	/// </summary>
	void OnTriggerEnter(Collider col)
	{  

        if( Zone != null && col != null )
        {
            //vp_Controller.

            Transform player = col.transform.root;
            if( player != null )
            {
                vp_PlayerEventHandler playerEventHandler = player.GetComponent<vp_PlayerEventHandler>();
                if (playerEventHandler != null)
                {
                    Zone.SendMessage("PlayerEnterZone",  playerEventHandler, SendMessageOptions.DontRequireReceiver );
                }            
            }
        }
	}


    void OnTriggerExit(Collider other)
    {
        Transform player = other.transform.root;
        if (player != null)
        {
            vp_PlayerEventHandler playerEventHandler = player.GetComponent<vp_PlayerEventHandler>();
            if (playerEventHandler != null)
            {
                Zone.SendMessage("PlayerExitZone", playerEventHandler, SendMessageOptions.DontRequireReceiver);
            }
        }
    }


}