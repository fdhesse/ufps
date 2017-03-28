/////////////////////////////////////////////////////////////////////////////////
//
//	PlayerCollectionDropper.cs
//	掉落游戏中收集的道具
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectionDropInfo
{
    public int DropperID = 0;
    public int ID = 0;
    public float Count = 0.0f;
    public Vector3 Position = new Vector3();
}

public class PlayerCollectionDropper : MonoBehaviour
{
    static int CollectionDropID = 0;

	/// </summary>
	protected virtual void Die()
	{
        if (!CollectionsPickupMgr.IsWork())
        {
            return;
        }


        OperationManager mgr = this.gameObject.GetComponent<OperationManager>();
        if (mgr != null)
        {
            float dropCount = mgr.CurrentGotCount;


            if (dropCount != 0)
            {

                mgr.TryDropAllCount();

                CollectionDropInfo dropInfo = new CollectionDropInfo();
                dropInfo.Count = dropCount;
                dropInfo.DropperID = vp_MPMaster.GetViewIDOfTransform(this.transform);
                dropInfo.ID = 0;
                dropInfo.Position = this.transform.position;
                vp_GlobalEvent<CollectionDropInfo>.Send("RequestDropCollections", dropInfo);
            }
        }        
	}

    protected void TryDropCollections( CollectionDropInfo info )
    {
        if (info != null)
        {
            float dropCount = info.Count;
            if (dropCount != 0)
            {

                ++CollectionDropID;
                CollectionDropInfo dropInfo = new CollectionDropInfo();
                dropInfo.Count = dropCount;
                dropInfo.DropperID = info.DropperID;
                dropInfo.ID = CollectionDropID;
                dropInfo.Position = info.Position;
                vp_GlobalEvent<CollectionDropInfo>.Send("OnDropCollections", dropInfo);
            }
        }
    }

    //protected void TryDropCollections(  )
    //{
    //    OperationManager mgr = this.gameObject.GetComponent<OperationManager>();
    //    if (mgr != null)
    //    {
    //        float dropCount = mgr.CurrentGotCount;


    //        if (dropCount != 0)
    //        {

    //            mgr.TryDropAllCount();

    //            ++CollectionDropID;
    //            CollectionDropInfo dropInfo = new CollectionDropInfo();
    //            dropInfo.Count = dropCount;
    //            dropInfo.DropperID = vp_MPMaster.GetViewIDOfTransform(this.transform);
    //            dropInfo.ID = CollectionDropID;
    //            dropInfo.Position = this.transform.position;
    //            vp_GlobalEvent<CollectionDropInfo>.Send("OnDropCollections", dropInfo);
    //        }
    //    }
    //}

}
