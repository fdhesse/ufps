using UnityEngine;
using System.Collections;

public class MultiplayerInfo
{
    public string ip;
    public int port;
    public int matchId;
    public int teamId;
    public string levelName = "a";
}

public static class GameAPI
{
    public static bool Win = false;
    public static string RequestLevel = "";

    public static void RegisterEvent(string eventName, vp_GlobalCallback handler)
    {
        vp_GlobalEvent.Register(eventName, handler);
        //EventHandler.RegisterEvent(eventName, handler);
    }

    public static void UnregisterEvent(string eventName, vp_GlobalCallback handler)
    {
        vp_GlobalEvent.Unregister(eventName, handler);
        //EventHandler.UnregisterEvent(eventName, handler);
    }

    public static void ExecuteEvent(string eventName)
    {
        vp_GlobalEvent.Send(eventName);
        //EventHandler.ExecuteEvent(eventName);
    }

    public static void StartMultiplayer(MultiplayerInfo info)
    {
        vp_GlobalEvent<MultiplayerInfo>.Send("StartMultiplayer", info);
      //  EventHandler.ExecuteEvent<string, int>("StartPvp", ip, port);
    }
}
