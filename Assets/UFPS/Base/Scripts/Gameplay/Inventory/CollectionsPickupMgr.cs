/////////////////////////////////////////////////////////////////////////////////
//
//	CollectionsPickupMgr.cs
//					
///////////////////////////////////////////////////////////////////////////////// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;



public class CollectionsPickupMgr : MonoBehaviour

{
    private static int _CollectionPickupID = 0;
    private static Dictionary<int, CollectionsPickup> _CollectionPickupsTable = new Dictionary<int, CollectionsPickup>();

    public CollectionsPickup ObjectModel = null;

    static bool IsWorking = false;

    public static bool IsWork()
    {
        return IsWorking;
    }

    void OnEnable()
    {
        _CollectionPickupID = 0;
        vp_GlobalEvent<CollectionDropInfo>.Register("AddCollectionsDrop", AddDrop);
        vp_GlobalEvent<int>.Register("RemoveCollectionDrop", RemoveDrop);

        if( vp_Gameplay.GetBattleMode() == eCombatMode.ECM_PVP_COORP )
        {
            IsWorking = true;
        }
        else
        {
            IsWorking = false;
        }

        
    }

    void OnDisable()
    {
        vp_GlobalEvent<CollectionDropInfo>.Unregister("AddCollectionsDrop", AddDrop);
        vp_GlobalEvent<int>.Unregister("RemoveCollectionDrop", RemoveDrop);
    }

	protected virtual void Awake()
	{
        
	}

    protected virtual void OnDestroy()
    {
        foreach( KeyValuePair<int,CollectionsPickup> kv in _CollectionPickupsTable )
        {
            GameObject.Destroy(kv.Value);
        }
        _CollectionPickupsTable.Clear();
    }

	protected virtual void Update()
	{
	}

    static public int GenCollectionID()
    {
        ++_CollectionPickupID;
        return _CollectionPickupID;
    }

    public void AddDrop(CollectionDropInfo info)
    {
        if( !IsWorking )
        {
            return;
        }

        if (ObjectModel == null)
        {
            return;
                
        }
        if (_CollectionPickupsTable.ContainsKey(info.ID))
        {
            return;
        }
        CollectionsPickup tempObject = GameObject.Instantiate(ObjectModel);
        if (tempObject)
        {
            tempObject.ID = info.ID;
            tempObject.DropperID = info.DropperID;
            tempObject.transform.parent = this.transform;
            tempObject.transform.position = info.Position;
            tempObject.Count = info.Count;
            tempObject.gameObject.SetActive( true );
            _CollectionPickupsTable[info.ID] = tempObject;
        }
    }

    public void RemoveDrop( int ID )
    {
        if (_CollectionPickupsTable.ContainsKey(ID))
        {
            CollectionsPickup obj = _CollectionPickupsTable[ID];
            if( obj != null )
            {
                obj.gameObject.SetActive(false);
                GameObject.Destroy(obj.gameObject);
            }
            _CollectionPickupsTable.Remove( ID );
        }
    }
}

