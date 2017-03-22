using UnityEngine;
using System.Collections;

using ParadoxNotion.Design;
using FlowCanvas.Nodes;

namespace Flowgraph
{
    [Category("Actions/UI")]
    public class ShowUI : CallableActionNode<string>
    {
        public override void Invoke(string path)
        {
            //EventHandler.ExecuteEvent<string>("ShowUI", path);
            //Debug.Log("ShowUI");
        }
    }
}
