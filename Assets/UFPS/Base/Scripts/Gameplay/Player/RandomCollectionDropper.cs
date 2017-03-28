/////////////////////////////////////////////////////////////////////////////////
//
//	RandomCollectionDropper.cs
//	随机掉落游戏中收集的道具
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomCollectionDropper : PlayerCollectionDropper
{
    public float MaxDropCount = 3.0f;
    public float MinDropCount = 1.0f;

	/// </summary>
	protected override void Die()
	{

            if( !CollectionsPickupMgr.IsWork() )
            {
                return;
            }

            float dropCount = (int)Random.Range(MinDropCount, MaxDropCount);

            if (dropCount != 0)
            {
                CollectionDropInfo dropInfo = new CollectionDropInfo();
                dropInfo.Count = dropCount;
                dropInfo.DropperID = 0;
                dropInfo.ID = 0;
                dropInfo.Position = this.transform.position;
                vp_GlobalEvent<CollectionDropInfo>.Send("RequestDropCollections", dropInfo);
            }
       
	}

}
