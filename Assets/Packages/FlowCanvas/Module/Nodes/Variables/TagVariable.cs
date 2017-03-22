using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes{

	[DoNotList]
	[Name("Easy Tag")]
	[Description("An easy way to get a Tag name")]
	public class TagVariable : VariableNode {
		
		[TagField]
		public string value = "Untagged";
		
		#if UNITY_EDITOR
		public override string name{
			get
			{
				var size = UnityEditor.EditorGUIUtility.isProSkin? 20 : 12;
				return string.Format("<size={0}><color=#ffffff>{1}</color></size>", size.ToString(), value);
			}
		}
		#endif

		protected override void RegisterPorts(){
			AddValueOutput<string>("Tag", ()=> {return value; });
		}

		public override void SetVariable(object o){
			if (o is string){
				value = (string)o;
			}
		}
	}
}