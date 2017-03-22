//
// OctMeshTreeEditor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	[CustomEditor(typeof(OctMeshTree))]
	public class OctMeshTreeEditor : MeshTreeEditor {
		[MenuItem("Assets/Create/FastShadowReceiver/OctMeshTree")]
		public static void CreateOctMeshTree()
		{
			OctMeshTree meshTree = CreateMeshTree<OctMeshTree>("New Oct Mesh Tree.asset");
			meshTree.scaledOffset = 1; // set default value.
			EditorUtility.SetDirty(meshTree);
		}
	}
}
