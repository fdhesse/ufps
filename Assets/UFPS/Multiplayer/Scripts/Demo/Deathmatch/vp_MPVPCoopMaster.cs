/////////////////////////////////////////////////////////////////////////////////
//vp_MPVPCoopMaster
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;



public class vp_MPVPCoopMaster : vp_MPMaster
{
	public class CollectionPointRankInfo
    {
        public float Amount = 0;
        public int TeamNumber = 0;
    }

    static public int CompareCollectionPoint(CollectionPointRankInfo pt1, CollectionPointRankInfo pt2)
    {
        if (pt1 == null)
        {
            if ( pt2 == null)
                return 0;
            else
                return -1;
        }
        else
        {
            if (pt2 == null)
            {
                return 1;
            }
            else
            {
                if( pt1.Amount < pt2.Amount )
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }        
    }

	[PunRPC]
	protected override void ReceiveFreeze(PhotonMessageInfo info)
	{

		base.ReceiveFreeze(info);

        if (!info.sender.IsMasterClient)
            return;

        if( !vp_Gameplay.IsMaster )
        {
            return;
        }

		//vp_DMDemoScoreBoard.ShowScore = true;        
        if (vp_MPTeamManager.TeamCount <= 1)
        {
            if( vp_MPTeamManager.TeamCount <= 0 )
            {
                vp_GlobalEvent<bool, int>.Send("OnNetPVPCoopEndGame", true, 0);
            }
            else
            {
                vp_GlobalEvent<bool, int>.Send("OnNetPVPCoopEndGame", false,  vp_MPTeamManager.Instance.Teams[0].Number );
            }
            return;
        }
        else
        {
            List<CollectionPointRankInfo> rankList = new List<CollectionPointRankInfo>();
            CollectionPoint[] tempObj = GameObject.FindObjectsOfType<CollectionPoint>();
            foreach( CollectionPoint pt in tempObj )
            {
                CollectionPointRankInfo rankInfo = new CollectionPointRankInfo();
                rankInfo.Amount = pt.CurCount;
                rankInfo.TeamNumber = pt.TeamNumber;
              
                int i = 0;
                bool hasInsert = false;
                foreach( CollectionPointRankInfo t in rankList )
                {
                    if( t.Amount < rankInfo.Amount  )
                    {
                        rankList.Insert(i, rankInfo);
                        hasInsert = true;
                        break;
                    }
                    ++i;
                }
                if( !hasInsert )
                {
                    rankList.Add( rankInfo );
                }
            }

            //rankList.Sort(CompareCollectionPoint);

            int rank = 0;
            int winTeamID = 0;
            bool isDraw = true;
            float lastAmount = 0;
            foreach( CollectionPointRankInfo newRank in rankList )
            {
                if( rank == 0 )
                {
                    lastAmount = newRank.Amount;
                }
                else if (lastAmount != newRank.Amount )
                {
                    lastAmount = newRank.Amount;
                    isDraw = false;
                }

                //victor
                if ( rank == 0 )
                {
                    winTeamID = newRank.TeamNumber;
                }
                //loser
                else
                {

                }
                ++rank;
            }

            vp_GlobalEvent<bool, int>.Send("OnNetPVPCoopEndGame", isDraw, winTeamID );
            return;
        }

        vp_GlobalEvent<bool, int>.Send("OnNetPVPCoopEndGame", false, 0);
	}


	/// <summary>
	/// 
	/// </summary>
	[PunRPC]
	protected override void ReceiveUnFreeze(PhotonMessageInfo info)
	{

		if (!info.sender.IsMasterClient)
			return;

		base.ReceiveUnFreeze(info);

		vp_DMDemoScoreBoard.ShowScore = false;
		
	}
	
    	
}
