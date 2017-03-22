//
// MeshTreeEditor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	public class MeshTreeEditor : EditorBase {
		protected static T CreateMeshTree<T>(string fileName) where T : ScriptableObject
		{
			string folderPath = GetSelectedFolderPath();
			string path = folderPath + fileName;
			path = AssetDatabase.GenerateUniqueAssetPath(path);
			AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), path);
			return AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
		}
		private int m_memoryUsage;
		private bool m_isBuilding;
		private bool m_showBuiltData;
		void OnEnable()
		{
			MeshTreeBase meshTree = target as MeshTreeBase;
			m_isBuilding = meshTree.IsBuilding();
			if (!m_isBuilding) {
				m_memoryUsage = meshTree.GetMemoryUsage();
			}
			m_showBuiltData = true;
		}
		private int m_editingRenderType = -1;
		private string m_editingString;
		private void ApplyEditingRenderType()
		{
			if (0 <= m_editingRenderType) {
				Undo.RegisterCompleteObjectUndo(target, "Inspector");
				SerializedProperty excludeRenderTypes = serializedObject.FindProperty("m_excludeRenderTypes");
				if (m_editingRenderType < excludeRenderTypes.arraySize) {
					SerializedProperty property = excludeRenderTypes.GetArrayElementAtIndex(m_editingRenderType);
					if (m_editingString != property.stringValue) {
						if (string.IsNullOrEmpty(m_editingString)) {
							excludeRenderTypes.DeleteArrayElementAtIndex(m_editingRenderType);
						}
						else if (m_editingString != property.stringValue) {
							property.stringValue = m_editingString;
						}
					}
				}
				else {
					int index = excludeRenderTypes.arraySize;
					excludeRenderTypes.InsertArrayElementAtIndex(index);
					excludeRenderTypes.GetArrayElementAtIndex(index).stringValue = m_editingString;
				}
				EditorUtility.SetDirty(target);
				m_editingRenderType = -1;
				m_editingString = "";
			}
		}
		private void CancelEditingRenderType()
		{
			m_editingRenderType = -1;
			m_editingString = "";
		}
		GUIStyle m_errorStyle = null;
		public override void OnInspectorGUI ()
		{
			MeshTreeBase meshTreeBase = target as MeshTreeBase;
			if (meshTreeBase.IsBuilding()) {
				GUI.enabled = false;
			}
			DrawDefaultInspector();
			if (meshTreeBase is MeshTree) {
				MeshTree meshTree = meshTreeBase as MeshTree;
				Object meshObject = EditorGUILayout.ObjectField(meshTree.srcMesh is Mesh ? "Mesh" : "Root Object", meshTree.srcMesh, typeof(Object), true);
				if (meshObject != meshTree.srcMesh && meshObject is GameObject && Event.current.command) {
					MeshFilter meshFilter = ((GameObject)meshObject).GetComponent<MeshFilter>();
					if (meshFilter != null && meshFilter.sharedMesh != null)  {
						meshObject = meshFilter.sharedMesh;
					}
				}
				if (meshObject != meshTree.srcMesh) {
					Undo.RegisterCompleteObjectUndo(meshTree, "Inspector");
					meshTree.srcMesh = meshObject;
					EditorUtility.SetDirty(meshTree);
				}
				if (meshObject is GameObject) {
					PrefabType prefabType = PrefabUtility.GetPrefabType(meshObject);
					if (prefabType != PrefabType.Prefab && prefabType != PrefabType.ModelPrefab) {
						if (m_errorStyle == null) {
							m_errorStyle = new GUIStyle();
							m_errorStyle.richText = true;
							m_errorStyle.wordWrap = true;
						}
						GUILayout.TextArea("<color=red>A reference to a scene object will not be serialized in Asset data. You will need to set the root object again when you rebuild the tree. Use a prefab instead.</color>", m_errorStyle);
					}
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_layerMask"));
					SerializedProperty excludeRenderTypes = serializedObject.FindProperty("m_excludeRenderTypes");
					excludeRenderTypes.isExpanded = EditorGUILayout.Foldout(excludeRenderTypes.isExpanded, "Exclude Render Types");
					if (excludeRenderTypes.isExpanded) {
						for (int i = 0; i < excludeRenderTypes.arraySize + 1; ++i) {
							string renderType;
							if (m_editingRenderType == i) {
								renderType = m_editingString;
							}
							else if (i < excludeRenderTypes.arraySize) {
								renderType = excludeRenderTypes.GetArrayElementAtIndex(i).stringValue;
							}
							else {
								renderType = "";
							}
							string controlName = "RenderType" + i.ToString();
							GUI.SetNextControlName(controlName);
							string newRenderType = EditorGUILayout.TextField(renderType);
							string focusedControl = GUI.GetNameOfFocusedControl();
							if (m_editingRenderType == i) {
								m_editingString = newRenderType;
								if (focusedControl != controlName || (/*Event.current.isKey &&*/ (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))) {
									ApplyEditingRenderType();
								}
								else if (/*Event.current.isKey &&*/ Event.current.keyCode == KeyCode.Escape) {
									CancelEditingRenderType();
								}
							}
							else if (renderType != newRenderType) {
								ApplyEditingRenderType();
								m_editingRenderType = i;
								m_editingString = newRenderType;
							}
						}
					}
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_scaledOffset"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_fixedOffset"));
					serializedObject.ApplyModifiedProperties();
				}
			}
			if (meshTreeBase is TerrainMeshTree) {
				TerrainMeshTree terrainMeshTree = meshTreeBase as TerrainMeshTree;
				Object terrainObj = EditorGUILayout.ObjectField("Terrain Data", terrainMeshTree.terrainData, typeof(Object), true);
				if (terrainObj != terrainMeshTree.terrainData) {
					TerrainData terrainData = terrainObj as TerrainData;
					if (terrainData == null && terrainObj is GameObject) {
						Terrain terrain = ((GameObject)terrainObj).GetComponent<Terrain>();
						if (terrain != null) {
							terrainData = terrain.terrainData;
						}
					}
					if (terrainData != null || terrainObj == null) {
						terrainMeshTree.terrainData = terrainData;
					}
				}
			}
			if (meshTreeBase.IsBuilding()) {
				GUI.enabled = true;
				m_isBuilding = true;
				EditorGUILayout.LabelField("Building... " + Mathf.FloorToInt(100*meshTreeBase.GetBuildProgress()).ToString() + "%");
			}
			else {
				GUI.enabled = meshTreeBase.IsReadyToBuild();
				if (m_isBuilding) {
					m_isBuilding = false;
					m_memoryUsage = meshTreeBase.GetMemoryUsage();
					EditorUtility.SetDirty(meshTreeBase);
				}
				if (meshTreeBase.IsPrebuilt()) {
					m_showBuiltData = EditorGUILayout.Foldout(m_showBuiltData, "Built Tree Info");
					if (m_showBuiltData) {
						string memorySize;
						float mb = m_memoryUsage/(1024.0f*1024.0f);
						if (1.0f <= mb) {
							memorySize = mb.ToString("f3") + "MB";
						}
						else {
							float kb = m_memoryUsage/1024.0f;
							memorySize = kb.ToString("f3") + "KB";
						}
						EditorGUILayout.LabelField("Memory", memorySize);
						EditorGUILayout.LabelField("Node Count", meshTreeBase.GetNodeCount().ToString());
					}
				}
				if (GUILayout.Button (meshTreeBase.IsPrebuilt() ? "Rebuild" : "Build")) {
					ApplyEditingRenderType();
					m_isBuilding = true;
					BuildMeshTree(meshTreeBase);
				}
			}
			GUI.enabled = true;
		}
		public static void BuildMeshTree(MeshTreeBase meshTreeBase)
		{
			MeshTree meshTree = meshTreeBase as MeshTree;
			GameObject prefab = null;
			if (meshTree != null && meshTree.srcMesh is GameObject) {
				PrefabType prefabType = PrefabUtility.GetPrefabType(meshTree.srcMesh);
				if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab) {
					prefab = (GameObject)meshTree.srcMesh;
					meshTree.srcMesh = PrefabUtility.InstantiatePrefab(prefab);
				}
			}
			meshTreeBase.AsyncBuild();
			if (prefab != null) {
				DestroyImmediate(meshTree.srcMesh);
				meshTree.SetSrcMeshWithoutClear(prefab);
			}
			EditorUtility.SetDirty(meshTreeBase);
		}
		public static string GetSelectedFolderPath()
		{
			Object[] objects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
			if (objects == null || objects.Length == 0) {
				return "Assets/";
			}
			else {
				string path = AssetDatabase.GetAssetPath(objects[0]);
				if (System.IO.File.Exists(path)) {
					path = System.IO.Path.GetDirectoryName(path);
				}
				return path + "/";
			}
		}
	}
}
