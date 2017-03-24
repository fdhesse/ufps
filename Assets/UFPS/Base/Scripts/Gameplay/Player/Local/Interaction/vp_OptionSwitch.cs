/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Switch.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	This class will allow the player to interact with an object
//					in the world by input or by a trigger. The script takes a target
//					object and a message can be sent to that target object.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_OptionSwitch : vp_Switch
{
    public OperationObject.eOperationObjectState ChangeState = OperationObject.eOperationObjectState.EOOS_COUNT;
    public List<OperationObject.eOperationObjectState> BelongState = new List<OperationObject.eOperationObjectState>();

    public List<GameObject> MsgTargetVec = new List<GameObject>();
	
	/// <summary>
	/// try to interact with this object
	/// </summary>
	public override bool TryInteract(vp_PlayerEventHandler player)
	{
		
		if(Target == null)
			return false;
		
		if(m_Player == null)
			m_Player = player;
		
		PlaySound();

        List<object> paramVec = new List<object>();
        paramVec.Add(m_Player);
        paramVec.Add(this);

        Target.SendMessage("OnInteract", paramVec, SendMessageOptions.DontRequireReceiver);

        foreach (GameObject target in MsgTargetVec)
        {
            if (target != null)
            {                
                target.SendMessage("OnInteract", paramVec, SendMessageOptions.DontRequireReceiver);
            }
        }

		return true;
		
	}
	
}
