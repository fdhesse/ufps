//
// BinaryMeshTreeEditor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	[CustomEditor(typeof(BinaryMeshTree))]
	public class BinaryMeshTreeEditor : MeshTreeEditor {
		[MenuItem("Assets/Create/FastShadowReceiver/BinaryMeshTree")]
		public static void CreateBinaryMeshTree()
		{
			BinaryMeshTree meshTree = CreateMeshTree<BinaryMeshTree>("New Binary Mesh Tree.asset");
			meshTree.scaledOffset = 1; // set default value.
			EditorUtility.SetDirty(meshTree);
		}
	}
}
