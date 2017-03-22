using UnityEngine;
using System.Collections;

using ParadoxNotion.Design;
using FlowCanvas.Nodes;

namespace Flowgraph
{
    [Category("Actions/Game")]
    public class EndGame : CallableActionNode<bool>
    {
        public override void Invoke(bool win)
        {
            if (!vp_Gameplay.IsMaster)
                return;

            Debug.LogWarning("EndGame");
            //GameAPI.Win = win;
            //EventHandler.ExecuteEvent("EndGame");
            vp_GlobalEvent<bool>.Send("NetEndGame", win);
        }
    }
}
