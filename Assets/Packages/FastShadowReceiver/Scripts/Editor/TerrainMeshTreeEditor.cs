//
// TerrainMeshTreeEditor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	[CustomEditor(typeof(TerrainMeshTree))]
	public class TerrainMeshTreeEditor : MeshTreeEditor {
		[MenuItem("Assets/Create/FastShadowReceiver/TerrainMeshTree")]
		public static void CreateTerrainMeshTree()
		{
			CreateMeshTree<TerrainMeshTree>("New Terrain Mesh Tree.asset");
		}
	}
}
