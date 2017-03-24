/////////////////////////////////////////////////////////////////////////////////
//
//	ExitZone.cs
//					
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExitZone : MonoBehaviour
{
    private HashSet<int> EnterPlayerIDSet = new HashSet<int>();
    private HashSet<int> AllPlayerIDSet = new HashSet<int>();
    public ExitField Field = null;


    void OnEnable()
    {
        vp_GlobalEvent<bool>.Register("SetExitZoneVisble", SetExitZoneVisble);
    }


    void OnDisable()
    {
        vp_GlobalEvent<bool>.Unregister("SetExitZoneVisble", SetExitZoneVisble);
    }

	void Start()
	{
        if( Field != null )
        {
            Field.gameObject.SetActive( false );
        }

        EnterPlayerIDSet.Clear();
	}

    void Update()
    {
        //只有Master做判断
        if( !vp_Gameplay.IsMaster )
        {
            return;
        }

        if (Field != null )
        {
            bool oldVisible = Field.gameObject.GetActive();
            if (OperationObject.IsAllOperationFished())
            {
                if( !oldVisible )
                {
                    Field.gameObject.SetActive(true);
                }                                
            }

            if( oldVisible != Field.gameObject.GetActive() )
            {
                vp_GlobalEvent<bool>.Send("M2CSetExitZoneVisble", Field.gameObject.GetActive());
            }
        }
    }

    public void RefreshNeedExit()
    {
        //只有Master做判断
        if (!vp_Gameplay.IsMaster)
        {
            return;
        }

        _RefreshAllPlayer( false );
        foreach( int id in AllPlayerIDSet )
        {
            if( !EnterPlayerIDSet.Contains( id ) )
            {
                return;
            }
        }

        //已经都在范围内了
        vp_GlobalEvent<bool>.Send("M2CCoopSupplyModeFinish", true );

    }

    public void PlayerEnterZone(vp_PlayerEventHandler player)
    {
        //只有Master做判断
        if (!vp_Gameplay.IsMaster)
        {
            return;
        }

        if( player != null )
        {
            int id = vp_MPMaster.GetViewIDOfTransform( player.transform );

            if( !EnterPlayerIDSet.Contains( id ) )
            {
                EnterPlayerIDSet.Add(id);
            }

            RefreshNeedExit();
        }


    }

    public void PlayerExitZone(vp_PlayerEventHandler player)
    {
        //只有Master做判断
        if (!vp_Gameplay.IsMaster)
        {
            return;
        }

        if (player != null)
        {
            int id = vp_MPMaster.GetViewIDOfTransform(player.transform);

            if (EnterPlayerIDSet.Contains(id))
            {
                EnterPlayerIDSet.Remove(id);
            }
            RefreshNeedExit();
        }
    }

    private void _RefreshAllPlayer( bool force )
    {
        if (force || AllPlayerIDSet.Count == 0 )
        {
            AllPlayerIDSet.Clear();
            //Object[] tempArray = GameObject.FindObjectsOfType(typeof(vp_PlayerEventHandler));
            //for( int i = 0; i < tempArray.Length; ++i )
            //{
            //    vp_PlayerEventHandler player = tempArray[i] as vp_PlayerEventHandler;
            //    if( player != null )
            //    {
            //        int id = vp_MPMaster.GetViewIDOfTransform( player.transform );
            //        if( !AllPlayerIDSet.Contains( id ) )
            //        {
            //            AllPlayerIDSet.Add( id );
            //        }
            //    }
            //}

            GameObject[] tempArray = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < tempArray.Length; ++i)
            {
                vp_PlayerEventHandler player = tempArray[i].GetComponent<vp_PlayerEventHandler>();
                if (player != null)
                {
                    int id = vp_MPMaster.GetViewIDOfTransform(player.transform);
                    if (!AllPlayerIDSet.Contains(id))
                    {
                        AllPlayerIDSet.Add(id);

                    }
                }
            }

        }
    }

    public void SetExitZoneVisble( bool visible )
    {
        if (Field != null)
        {
            Field.gameObject.SetActive(visible);
        }
     
    }

}