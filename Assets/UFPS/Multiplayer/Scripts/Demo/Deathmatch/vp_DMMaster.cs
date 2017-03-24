/////////////////////////////////////////////////////////////////////////////////
//
//	vp_DMMaster.cs
//	© Opsive. All Rights Reserved.
//	https://twitter.com/Opsive
//	http://www.opsive.com
//
//	description:	an example of how to extend the base (vp_MPMaster) class
//					with a call to show the deathmatch scoreboard when the game
//					pauses on end-of-match, and to restore it when game resumes
//
//					TIP: study the base class to learn how the game state works
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class vp_DMMaster : vp_MPMaster
{
	

	/// <summary>
	/// 
	/// </summary>
	[PunRPC]
	protected override void ReceiveFreeze(PhotonMessageInfo info)
	{

		if (!info.sender.IsMasterClient)
			return;

		base.ReceiveFreeze(info);

		//vp_DMDemoScoreBoard.ShowScore = true;
        if (vp_MPTeamManager.TeamCount <= 1)
        {
            GameAPI.Win = true;
        }
        else
        {
            var scores = new int[vp_MPTeamManager.TeamCount];

            int myteam = -1;
            foreach (vp_MPTeam t in vp_MPTeamManager.Instance.Teams)
            {
                var dmteam = t as vp_DMTeam;
                if (dmteam != null)
                    scores[dmteam.Number] = dmteam.Score * 1000;

                foreach (vp_MPNetworkPlayer p in vp_MPNetworkPlayer.Players.Values)
                {
                    if (p.ID == PhotonNetwork.player.ID)
                    {
                        myteam = p.TeamNumber;
                    }

                    //give a little bonus to person number
                    scores[p.TeamNumber] += 1;
                }
            }

            GameAPI.Win = true;
            for(int i=0; i<scores.Length; ++i)
            {
                if (myteam != i && scores[myteam] < scores[i])
                {
                    GameAPI.Win = false;
                    break;
                }
            }
        }

        vp_GlobalEvent.Send("EndGame");
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
