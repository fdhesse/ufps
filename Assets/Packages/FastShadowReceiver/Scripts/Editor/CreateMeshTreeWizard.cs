//
// CreateMeshTreeWizard.cs
//
// Fast Shadow Receiver
//
// Copyright 2015 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	public class CreateMeshTreeWizard : EditorWindow {
		public static void CreateWizard(Transform meshTransform, LayerMask layers, MeshShadowReceiver receiver)
		{
			CreateMeshTreeWizard window = ScriptableObject.CreateInstance<CreateMeshTreeWizard>();
			window.Initialize(meshTransform, layers, receiver);
#if UNITY_5_0
			window.title = "Create Mesh Tree"; // obsolete from Unity 5.1
#else
			window.titleContent = new GUIContent("Create Mesh Tree");
#endif
			window.ShowUtility();
		}

		GameObject m_rootObject;
		MeshShadowReceiver m_receiver;
		LayerMask m_layers;
		Object m_srcMesh;
		int m_currentPage = 0;
		string m_errorString;
		GUIStyle m_errorStyle;
		MeshFilter[] m_meshes = null;
		MeshFilter[] srcMeshes
		{
			get {
				if (m_meshes == null) {
					m_meshes = m_rootObject.GetComponentsInChildren<MeshFilter>();
				}
				return m_meshes;
			}
		}
		MeshTreeBase m_meshTree = null;
		void Initialize(Transform meshTransform, LayerMask layers, MeshShadowReceiver receiver)
		{
			m_rootObject = meshTransform.gameObject;
			m_receiver = receiver;
			m_layers = layers;
			m_currentPage = 0;
			m_srcMesh = null;
			Terrain terrain = m_rootObject.GetComponent<Terrain>();
			if (terrain != null) {
				m_srcMesh = terrain.terrainData;
			}
			m_errorString = null;
			if (m_srcMesh == null) {
				m_meshes = m_rootObject.GetComponentsInChildren<MeshFilter>();
				if (m_meshes == null || m_meshes.Length == 0) {
					// error
					m_currentPage = -1;
					m_errorString = "<color=red>Object \"" + meshTransform.name + "\" does not contain any meshes! Cannot create MeshShadowReceiver.</color>";
				}
				else {
					GameObject prefabObject = PrefabUtility.GetPrefabParent(m_rootObject) as GameObject;
					if (prefabObject != null) {
						m_srcMesh = prefabObject;
					}
					else {
						m_srcMesh = m_rootObject;
					}
				}
			}
			m_errorStyle = new GUIStyle();
			m_errorStyle.richText = true;
			m_errorStyle.wordWrap = true;
		}

		void OnGUI()
		{
			switch (m_currentPage) {
			case -1:
				OnGUIError();
				break;
			case 0:
				OnGUISelectMeshTreeTypeAndCreate();
				break;
			case 1:
				OnGUIBuildMeshTree();
				break;
			}

		}

		void OnGUIError()
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			GUILayout.TextArea(m_errorString, m_errorStyle);
			EditorGUILayout.EndHorizontal();
		}

		bool m_bBinaryTree = true;
		void OnGUISelectMeshTreeTypeAndCreate()
		{
			EditorGUILayout.LabelField("Type of Mesh Tree");
			if (m_srcMesh is TerrainData) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30);
				GUILayout.Label("Terrain Mesh Tree");
				EditorGUILayout.EndHorizontal();
			}
			else {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30);
				bool bBinaryTree = GUILayout.Toggle(m_bBinaryTree, "Binary Mesh Tree");
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30);
				bool bOctree = GUILayout.Toggle(!m_bBinaryTree, "Oct Mesh Tree");
				EditorGUILayout.EndHorizontal();
				if (bOctree && m_bBinaryTree) {
					m_bBinaryTree = !bOctree;
				}
				else if (bBinaryTree && !m_bBinaryTree) {
					m_bBinaryTree = bBinaryTree;
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30);
				if (GUILayout.Button("<color=blue>What's the difference?</color>", m_errorStyle)) {
					Application.OpenURL("http://nyahoon.com/products/fast-shadow-receiver/create-a-mesh-tree");
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.Separator();

				EditorGUILayout.LabelField("Select Source Object");

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30);
				bool bSceneObject = GUILayout.Toggle(m_srcMesh == m_rootObject, "Scene Object");
				EditorGUILayout.ObjectField(m_rootObject, typeof(GameObject), true);
				EditorGUILayout.EndHorizontal();
				if (bSceneObject && m_srcMesh != m_rootObject) {
					m_srcMesh = m_rootObject;
				}

				EditorGUILayout.BeginHorizontal();
				GameObject prefabObject = PrefabUtility.GetPrefabParent(m_rootObject) as GameObject;
				GUILayout.Space(30);
				if (prefabObject == null) {
					GUI.enabled = false;
				}
				bool bPrefabObject = GUILayout.Toggle(m_srcMesh == prefabObject, "Prefab Object");
				if (prefabObject != null) {
					EditorGUILayout.ObjectField(prefabObject, typeof(GameObject), true);
				}
				else {
					GUI.enabled = true;
					if (GUILayout.Button("Create Prefab")) {
						string path = EditorUtility.SaveFilePanelInProject("Create Prefab", m_rootObject.name, "prefab", "Select a path for the prefab of " + m_rootObject.name);
						if (!string.IsNullOrEmpty(path)) {
							prefabObject = PrefabUtility.CreatePrefab(path, m_rootObject, ReplacePrefabOptions.ConnectToPrefab);
							m_srcMesh = prefabObject;
							bSceneObject = false;
						}
					}
				}
				EditorGUILayout.EndHorizontal();
				if (bPrefabObject && m_srcMesh != prefabObject) {
					m_srcMesh = prefabObject;
				}
				if (m_srcMesh == m_rootObject) {
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(30);
					GUILayout.TextArea("<color=red>If you choose Scene Object, the reference to the object cannot be stored in Mesh Tree settings. So, you will need to set the reference again when you rebuild the mesh tree.</color>", m_errorStyle);
					EditorGUILayout.EndHorizontal();
				}
				else if (m_srcMesh == prefabObject) {
					// check if m_rootObject is modified from prefabObject.
					for (int i = 0; i < srcMeshes.Length; ++i) {
						GameObject go = srcMeshes[i].gameObject;
						GameObject prefab = PrefabUtility.GetPrefabParent(go) as GameObject;
						if (go.layer != prefab.layer) {
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(30);
							GUILayout.TextArea("<color=red>The layer of the scene object was modified from the prefab.</color>", m_errorStyle);
							EditorGUILayout.ObjectField(go, typeof(GameObject), true);
							EditorGUILayout.EndHorizontal();
							break;
						}
						if (go.transform.localPosition != prefab.transform.localPosition || go.transform.localRotation != prefab.transform.localRotation || go.transform.localScale != prefab.transform.localScale) {
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space(30);
							GUILayout.TextArea("<color=red>The transform of the scene object was modified from the prefab.</color>", m_errorStyle);
							EditorGUILayout.ObjectField(go, typeof(GameObject), true);
							EditorGUILayout.EndHorizontal();
							break;
						}
					}
				}
			}
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Create Mesh Tree")) {
				string path = GetDefaultPathFromSourceMesh(m_srcMesh, m_bBinaryTree);
				path = EditorUtility.SaveFilePanelInProject("Create New Mesh Tree", System.IO.Path.GetFileName(path), "asset", "Select a path for the new Mesh Tree", System.IO.Path.GetDirectoryName(path));
				if (!string.IsNullOrEmpty(path)) {
					MeshTreeBase meshTreeBase;
					if (m_srcMesh is TerrainData) {
						TerrainMeshTree meshTree = ScriptableObject.CreateInstance<TerrainMeshTree>();
						meshTree.terrainData = m_srcMesh as TerrainData;
						meshTreeBase = meshTree;
					}
					else if (m_bBinaryTree) {
						BinaryMeshTree meshTree = ScriptableObject.CreateInstance<BinaryMeshTree>();
						meshTree.srcMesh = m_srcMesh;
						meshTree.layerMask = m_layers;
						meshTree.scaledOffset = 1.0f;
						meshTreeBase = meshTree;
					}
					else {
						OctMeshTree meshTree = ScriptableObject.CreateInstance<OctMeshTree>();
						meshTree.srcMesh = m_srcMesh;
						meshTree.layerMask = m_layers;
						meshTree.scaledOffset = 1.0f;
						meshTreeBase = meshTree;
					}
					AssetDatabase.CreateAsset(meshTreeBase, path);
					AssetDatabase.Refresh();
					m_meshTree = AssetDatabase.LoadAssetAtPath(path, typeof(MeshTreeBase)) as MeshTreeBase;
					if (m_receiver != null) {
						m_receiver.meshTree = m_meshTree;
					}
					if (m_srcMesh is TerrainData) {
						// just build and close. because TerrainMeshTree doesn't have build options.
						MeshTreeEditor.BuildMeshTree(m_meshTree);
						Close();
					}
					else {
						m_serializedObject = new SerializedObject(m_meshTree);
						++m_currentPage;
					}
				}
			}
		}
		SerializedObject m_serializedObject = null;
		void OnGUIBuildMeshTree()
		{
			GUILayout.Label("One more step is left. Please set the build options and press Build button.");
			EditorGUILayout.Separator();
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_layerMask"));
			SerializedProperty excludeRenderTypes = m_serializedObject.FindProperty("m_excludeRenderTypes");
			EditorGUILayout.LabelField("Exclude Render Types");
			EditorGUI.indentLevel++;
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
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_scaledOffset"));
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_fixedOffset"));
			GUILayout.TextArea("Offset parameters are used to push each vertex along its normal vector to reduce z fighting problem. The offset value is calculated as" + System.Environment.NewLine +
			                         "    offset = 0.00000025 x |v| x <Scaled Offset> + <Fixed Offset>," + System.Environment.NewLine +
			                         "where |v| is the norm of a vertex position vector." + System.Environment.NewLine + 
			                         "Scaled Offset makes it easy to adjust the offset value, because z fighting is caused by floating point rounding error, and maximum possible value of the error is roughly proportional to |v|.");
			m_serializedObject.ApplyModifiedProperties();
			EditorGUI.indentLevel--;
			EditorGUILayout.Separator();
			// check layer mask
			bool hasObject = false;
			LayerMask layers = ((MeshTree)m_meshTree).layerMask;
			for (int i = 0; i < srcMeshes.Length; ++i) {
				if ((layers & (1 << srcMeshes[i].gameObject.layer)) != 0) {
					hasObject = true;
					break;
				}
			}
			if (!hasObject) {
				GUILayout.TextArea("<color=red>No child mesh objects exist in the layers. Please add the layers of the mesh objects to Layer Mask.</color>", m_errorStyle);
				GUI.enabled = false;
			}
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Build", GUILayout.Width(100))) {
				MeshTreeEditor.BuildMeshTree(m_meshTree);
				Close();
			}
			EditorGUILayout.EndHorizontal();
			GUI.enabled = true;
		}
		private int m_editingRenderType = -1;
		private string m_editingString;
		private void ApplyEditingRenderType()
		{
			if (0 <= m_editingRenderType) {
				Undo.RegisterCompleteObjectUndo(m_meshTree, "Inspector");
				SerializedProperty excludeRenderTypes = m_serializedObject.FindProperty("m_excludeRenderTypes");
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
				EditorUtility.SetDirty(m_meshTree);
				m_editingRenderType = -1;
				m_editingString = "";
			}
		}
		private void CancelEditingRenderType()
		{
			m_editingRenderType = -1;
			m_editingString = "";
		}
		public static string GetDefaultPathFromSourceMesh(Object srcMesh, bool preferBinaryTree)
		{
			string path = AssetDatabase.GetAssetPath(srcMesh);
			if (string.IsNullOrEmpty(path)) {
				path = "Assets/";
			}
			else {
				path = System.IO.Path.GetDirectoryName(path);
			}
			path = System.IO.Path.Combine(path, srcMesh.name);
			if (srcMesh is TerrainData) {
				path += ".TerraMeshTree.asset";
			}
			else if (preferBinaryTree) {
				path += ".BinMeshTree.asset";
			}
			else {
				path += ".OctMeshTree.asset";
			}
			return path;
		}
		void OnFocus()
		{
			if (m_rootObject == null) {
				Close ();
			}
			if (m_serializedObject != null) {
				m_serializedObject.Update();
			}
		}
		void OnHierarchyChange()
		{
			OnFocus();
			Repaint();
		}
		void OnProjectChange()
		{
			OnFocus();
			Repaint();
		}
	}
}
