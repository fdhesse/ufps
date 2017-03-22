using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//using ParadoxNotion;
using ParadoxNotion.Design;
using FlowCanvas.Nodes;


public class SpawnInfo
{
    public int id;
    public string path;
    public Vector3 pos;
    public Quaternion quat;

    //public static readonly byte[] memVector2 = new byte[2 * 4];

    //private static short Serialize(ExitGames.Client.Photon.StreamBuffer outStream, object customobject)
    //{
    //    int len = 0;
    //    SpawnInfo info = (SpawnInfo)customobject;
    //    //lock (memVector2)
    //    {
    //        //byte[] bytes = memVector2;
    //        //int index = 0;
    //        //ExitGames.Client.Photon.Protocol.Serialize(vo.id, bytes, ref index);

    //        var bytes = ExitGames.Client.Photon.Protocol.Serialize(info.id);
    //        len += bytes.Length;
    //        outStream.Write(bytes, 0, bytes.Length);

    //        bytes = ExitGames.Client.Photon.Protocol.Serialize(info.path.Length);
    //        len += bytes.Length;
    //        outStream.Write(bytes, 0, bytes.Length);

    //        bytes = ExitGames.Client.Photon.Protocol.Serialize(info.path);

    //        len += bytes.Length;
    //        outStream.Write(bytes, 0, bytes.Length);

    //        bytes = ExitGames.Client.Photon.Protocol.Serialize(info.pos);
    //        len += bytes.Length;
    //        outStream.Write(bytes, 0, bytes.Length);

    //        bytes = ExitGames.Client.Photon.Protocol.Serialize(info.quat);
    //        len += bytes.Length;
    //        outStream.Write(bytes, 0, bytes.Length);
    //    }

    //    return (short)len;
    //}

    //private static object Deserialize(ExitGames.Client.Photon.StreamBuffer inStream, short length)
    //{
    //    SpawnInfo info = new SpawnInfo();
    //    //lock (memVector2)
    //    {
    //        var bytes = new byte[inStream.Length];

    //        inStream.Read(bytes, 0, bytes.Length);
    //        int offset = 0;
    //        ExitGames.Client.Photon.Protocol.Deserialize(out info.id, bytes, ref offset);
    //        var id = ExitGames.Client.Photon.Protocol.Deserialize(bytes);
    //        int len = 0;
    //        ExitGames.Client.Photon.Protocol.Deserialize(out len, bytes, ref offset);

    //        var dest = new byte[len];
    //        System.Array.Copy(bytes, 0, dest, offset, len);
    //        info.path = (string)ExitGames.Client.Photon.Protocol.Deserialize(dest);
    //        offset += len;

    //        len = 3 * 4;
    //        dest = new byte[len];
    //        System.Array.Copy(bytes, 0, dest, offset, len);
    //        info.pos = (Vector3)ExitGames.Client.Photon.Protocol.Deserialize(dest);
    //        offset += len;

    //        len = 4 * 4;
    //        dest = new byte[len];
    //        System.Array.Copy(bytes, 0, dest, offset, len);
    //        info.quat = (Quaternion)ExitGames.Client.Photon.Protocol.Deserialize(dest);
    //        //return ExitGames.Client.Photon.Protocol.Deserialize(bytes);
    //    }
    //    return info;
    //}

    public static void Init()
    {
        //ExitGames.Client.Photon.PhotonPeer.RegisterType(typeof(SpawnInfo), (byte)250, Serialize, Deserialize);
    }
}

public class MonsterManager
{
    private static MonsterManager instance;

    private int uid = 1;
    private GameObject[] monsters;

    public static MonsterManager Instance { get { return instance; } }

    public int NextId { get { return uid++; } }

    public bool LevelLoaded { get; private set; }

    public MonsterManager(int maxCount)
    {
        instance = this;

        monsters = new GameObject[maxCount];

        vp_GlobalEvent.Register("LevelLoaded", OnLevelLoaded);
    }

    public GameObject GetMonster(int id)
    {
        return monsters[id];
    }

    public void AddMonster(int id, GameObject monster)
    {
        monsters[id] = monster;
    }

    public void Clear()
    {
        monsters = null;
        vp_GlobalEvent.Unregister("LevelLoaded", OnLevelLoaded);
    }

    private void OnLevelLoaded()
    {
        LevelLoaded = true;
    }
}

namespace Flowgraph
{
    [Category("Actions/Spawn")]
    public class SpawnMonster : LatentActionNode<string, Transform>
    {
        public GameObject Monster { get; private set; }

        public override IEnumerator Invoke(string prefabPath, Transform transform)
        {
            // damage is always done in singleplayer, but only in multiplayer if you are the master
            if (!vp_Gameplay.IsMaster)
                yield break;

            if (MonsterManager.Instance == null)
                yield break;

            while (!MonsterManager.Instance.LevelLoaded)
                yield return null;

            var id = MonsterManager.Instance.NextId;
            //var monster = (GameObject)UnityEngine.Object.Instantiate(prefab, transform.position, transform.rotation);

            var spawnInfo = new SpawnInfo() { id = id, path = prefabPath, pos = transform.position, quat = transform.rotation };
            vp_GlobalEvent<SpawnInfo>.Send("SpawnMonster", spawnInfo);
                yield return null;

            Monster = MonsterManager.Instance.GetMonster(id);
            while (Monster == null)
                yield return null;
        }
    }


    [Category("Actions/Spawn")]
    public class SpawnMonsters : LatentActionNode<List<string>, Transform, float>
    {
        public GameObject Monster1 { get; private set; }
        public GameObject Monster2 { get; private set; }
        public GameObject Monster3 { get; private set; }
        public GameObject Monster4 { get; private set; }

        private int[] monsterIds = new int[4];

        public override IEnumerator Invoke(List<string> prefabPath, Transform transform, float radius)
        {

            // damage is always done in singleplayer, but only in multiplayer if you are the master
            if (!vp_Gameplay.IsMaster)
                yield break;

            if (MonsterManager.Instance == null)
                yield break;

            while (!MonsterManager.Instance.LevelLoaded)
                yield return null;

            float angel = 360f / prefabPath.Count;

            int index = 0;

            for (int i = 0; i < prefabPath.Count; ++i)
            {
                if (!string.IsNullOrEmpty(prefabPath[i]))
                {
                    var id = MonsterManager.Instance.NextId;
                    if (index < monsterIds.Length) monsterIds[index++] = id;

                    var pos = transform.position + Quaternion.AngleAxis(angel * i, Vector3.up) * Vector3.one * radius;
                    var quat = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up);
                    var spawnInfo = new SpawnInfo() { id = id, path = prefabPath[i], pos = pos, quat = quat };
                    vp_GlobalEvent<SpawnInfo>.Send("SpawnMonster", spawnInfo);
                }
            }
            yield return null;

            Monster1 = MonsterManager.Instance.GetMonster(monsterIds[0]);
            while (!Monster1) yield return null;

            Monster2 = MonsterManager.Instance.GetMonster(monsterIds[1]);
            while (!Monster2) yield return null;

            Monster3 = MonsterManager.Instance.GetMonster(monsterIds[2]);
            while (!Monster3) yield return null;

            Monster4 = MonsterManager.Instance.GetMonster(monsterIds[3]);
            while (!Monster4) yield return null;
        }
    }
}
