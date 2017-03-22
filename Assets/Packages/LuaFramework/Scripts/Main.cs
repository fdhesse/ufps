using UnityEngine;

namespace LuaFramework {

    /// <summary>
    /// </summary>
    public class Main : MonoBehaviour {

        void Start() {
            SpawnInfo.Init();

            AppFacade.Instance.StartUp();   //启动游戏

            //var networkManager = gameObject.AddComponent<UnityEngine.Networking.NetworkManager>();
            //networkManager.StartHost();
        }
    }
}