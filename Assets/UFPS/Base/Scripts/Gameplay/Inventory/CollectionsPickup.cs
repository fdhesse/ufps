/////////////////////////////////////////////////////////////////////////////////
//
//	CollectionsPickup.cs
//					
///////////////////////////////////////////////////////////////////////////////// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CollectionPickupInfo
{
    public int ObjectID = 0;
    public int PickerID = 0;
    public float ChangeCount = 0.0f;
}

public class CollectionsPickup : MonoBehaviour
{
    private bool _HasBeenGiven = false;
    private bool _NeedRemove = false;

    public float Count = 0;
    public int ID = 0;
    public int DropperID = 0;

    private float _DropTime = 0;

	protected virtual void Awake()
	{
        _HasBeenGiven = false;
        _NeedRemove = false;
        _DropTime = Time.time;
	}

    protected virtual void OnDestroy()
    {
    }

	protected virtual void Update()
	{
		//TryRemoveOnDeplete();
	}


	/// <summary>
	/// removes the pickup if it has been depleted and the pickup
	/// sound has stopped playing
	/// </summary>
	protected virtual void TryRemoveOnDeplete()
	{
        //if (!_NeedRemove)
        //{
        //    return;
        //}

        //vp_Utility.Destroy(gameObject);
	}

	protected virtual void OnTriggerEnter(Collider col)
	{
        if( !vp_Gameplay.IsMaster )
        {
            return;
        }

        TryGiveTo(col);
	}


	/// <summary>
	/// 
	/// </summary>
	public void TryGiveTo(Collider col)
	{
        
		// only do something if the trigger is still active
        if (_HasBeenGiven || _NeedRemove )
			return;
        
        //添加到对应的位置        
        Transform player = col.transform.root;
        if (player != null )
        {
            vp_PlayerEventHandler playerEventHandler = player.GetComponent<vp_PlayerEventHandler>();
            int playerID = vp_MPMaster.GetViewIDOfTransform(player);

            if (playerEventHandler == null)
            {
                return;
            }

            //正在死亡的角色不能捡取
            if( playerEventHandler.Dead.Active )
            {
                return;
            }

            //为了防止阵亡的玩家生成道具后立即捡起来
            if( Time.time - _DropTime < 5.0f && DropperID == playerID )
            {
                return;
            }

            CollectionPickupInfo pickUpInfo = new CollectionPickupInfo();
            pickUpInfo.ObjectID = ID;
            pickUpInfo.PickerID = playerID;
            pickUpInfo.ChangeCount = Count;
            vp_GlobalEvent<CollectionPickupInfo>.Send("OnPickUpCollections", pickUpInfo);

            _HasBeenGiven = true;
        }  
	}

	protected virtual void OnTriggerExit()
	{


	}


}

