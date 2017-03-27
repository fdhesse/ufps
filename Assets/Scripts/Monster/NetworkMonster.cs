using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorDesigner.Runtime;

public class NetworkMonster : Photon.MonoBehaviour
{
        static int uid = 100;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public static void Init(GameObject ob)
        {
                var be = ob.GetComponent<BehaviorTree>();
                if (!vp_Gameplay.IsMaster)
                        be.enabled = false;

                //treat monster ad player for easy sync & ai etc
                ob.AddComponent<vp_PlayerEventHandler>();
                //TODO:add necessary component for monster here.

                var l = ob.AddComponent<NetworkMonster>();

                var view = ob.AddComponent<PhotonTransformView>();
                view.PositonModel.SynchronizeEnabled = true;
                //view.PositonModel.InterpolateOption = PhotonTransformViewPositionModel.InterpolateOptions.Lerp;
                //view.PositonModel.ExtrapolateOption = PhotonTransformViewPositionModel.ExtrapolateOptions.SynchronizeValues;
                view.RotationModel.SynchronizeEnabled = true;
                AddSurfaceType(ob);//add by deng
                AddPhotonViewToMonster(l, uid++);
        }

        static void AddPhotonViewToMonster(NetworkMonster networkPlayer, int id)
        {
                PhotonView p = (PhotonView)networkPlayer.gameObject.GetComponent<PhotonView>();
                if (p == null)
                        p = (PhotonView)networkPlayer.gameObject.AddComponent<PhotonView>();

                p.viewID = id;
                //p.viewID = (id * 1000) + 1;	// TODO: may crash with 'array index out of range' if a player is deactivated in its prefab
                p.onSerializeTransformOption = OnSerializeTransform.OnlyPosition;
                p.ObservedComponents = new List<Component>();
                //p.ObservedComponents.Add(networkPlayer);
                p.ObservedComponents.Add(networkPlayer.GetComponent<PhotonTransformView>());
                p.synchronization = ViewSynchronization.UnreliableOnChange;

                PhotonNetwork.networkingPeer.RegisterPhotonView(p);
        }

        static private void AddSurfaceType(GameObject go)
        {

        }
}
