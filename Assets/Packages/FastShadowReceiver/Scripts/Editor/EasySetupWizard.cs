//
// EasySetupWizard.cs
//
// Fast Shadow Receiver
//
// Copyright 2015 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace FastShadowReceiver.Editor {
	public class EasySetupWizard : EditorWindow {
		private static readonly Vector2 WINDOW_SIZE = new Vector2(424, 350);
		static EasySetupWizard s_instance = null;
		public static void CreateWizard(ProjectorManager projectorManager)
		{
			if (s_instance != null) {
				s_instance.Close();
			}
			EasySetupWizard window = ScriptableObject.CreateInstance<EasySetupWizard>();
#if UNITY_5_0
			window.title = "Easy Setup Wizard of Projector Manager"; // obsolete from Unity 5.1
#else
			window.titleContent = new GUIContent("Easy Setup Wizard of Projector Manager");
#endif
			window.maxSize = 
			window.minSize = WINDOW_SIZE;
			window.Initialize(projectorManager);
			window.ShowUtility();
			s_instance = window;
		}

		private ProjectorManager m_projectorManager;
		private SerializedObject m_serializedObject;
		private int m_currentPage = 0;
		private GUIStyle m_errorStyle;
		private static readonly string[] s_pageTitles = new string[] {
			"Step 1/7. Set Mesh Shadow Receiver",
			"Step 2/7. Set Environment Information",
			"Step 3/7. Choose Layers for Shadow Receivers",
			"Step 4/7. Add AutoProjector component to Projector prefabs in Assets",
			"Step 5/7. Add AutoProjector component to Projector objects in the scene",
			"Step 6/7. Setup Shadow Distance and Planes",
			"Step 7/7. Pack Blob Shadow Textures for Distant Shadows",
			"Setup was Completed!",
		};

		void Initialize(ProjectorManager projectorManager)
		{
			m_projectorManager = projectorManager;
			m_serializedObject = new SerializedObject(projectorManager);
			m_currentPage = 0;
			m_errorStyle = new GUIStyle();
			m_errorStyle.richText = true;
			m_errorStyle.wordWrap = true;
		}

		bool m_isGoingBack = false;
		bool m_isAbleToGoNext = true;
		void OnFocus()
		{
			// check if m_projectorManager is still alive.
			if (m_projectorManager == null) {
				Close();
				return;
			}
			if (m_serializedObject != null) {
				m_serializedObject.Update();
			}
			m_environmentMeshes = null;
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

		void OnGUI()
		{
			GUILayout.Label(s_pageTitles[m_currentPage]);

			Rect mainArea = new Rect(10, 30, WINDOW_SIZE.x - 20, WINDOW_SIZE.y - 70);
			Rect buttonArea = new Rect(10, mainArea.yMax + 10, WINDOW_SIZE.x - 20, 30);
			GUILayout.BeginArea(mainArea);
			bool ret = true;
			do {
				switch (m_currentPage) {
				case 0:
					ret = OnGUISetMeshShadowReceiver();
					break;
				case 1:
					ret = OnGUISetEnvironment();
					break;
				case 2:
					ret = OnGUISelectLayers();
					break;
				case 3:
					ret = OnGUIAttachAutoProjectorToPrefabs();
					break;
				case 4:
					ret = OnGUIAttachAutoProjectorToSceneObjects();
					break;
				case 5:
					ret = OnGUISetupPlaneShadows();
					break;
				case 6:
					ret = OnGUICreatePackedTextures();
					break;
				default:
					ret = true;
					break;
				}
			} while (!ret);
			GUILayout.EndArea();

			GUILayout.BeginArea(buttonArea);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayoutOption buttonOption = GUILayout.Width(100);
			if (m_currentPage == 0) {
				GUI.enabled = false;
			}
			if (GUILayout.Button("Back", buttonOption)) {
				m_isGoingBack = true;
				--m_currentPage;
			}
			GUI.enabled = true;
			GUILayout.Space(20);
			if (m_currentPage == s_pageTitles.Length - 1) {
				if (GUILayout.Button("Finish", buttonOption)) {
					Close();
				}
			}
			else {
				if (!m_isAbleToGoNext) {
					GUI.enabled = false;
				}
				if (GUILayout.Button("Next", buttonOption)) {
					m_isGoingBack = false;
					++m_currentPage;
				}
				GUI.enabled = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		bool OnGUISetMeshShadowReceiver()
		{
			MeshShadowReceiver receiver = null;
			if (0 < m_projectorManager.receivers.Count) {
				receiver = m_projectorManager.receivers[0] as MeshShadowReceiver;
			}
			MeshShadowReceiver newReceiver = EditorGUILayout.ObjectField("Mesh Shadow Receiver", receiver, typeof(MeshShadowReceiver), true) as MeshShadowReceiver;
			if (receiver != newReceiver) {
				Undo.RecordObject(m_projectorManager, "Easy Setup: Select Shadow Receiver");
				if (receiver != null) {
					m_projectorManager.RemoveReceiver(receiver);
				}
				if (newReceiver != null) {
					m_projectorManager.AddReceiver(newReceiver);
				}
				EditorUtility.SetDirty(m_projectorManager);
				receiver = newReceiver;
			}
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Create a New Shadow Receiver")) {
				GameObject go = new GameObject("Shadow Receiver");
				newReceiver = go.AddComponent<MeshShadowReceiver>();
				EditorSettings editorSettings = EditorSettings.GetSettings(UnityEditor.MonoScript.FromScriptableObject(this));
				Renderer renderer = newReceiver.GetComponent<Renderer>();
				renderer.sharedMaterial = editorSettings.m_defaultReceiverMaterial;
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
				renderer.castShadows = false;
#else
				renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#endif
				renderer.useLightProbes = false;
				renderer.receiveShadows = false;
				newReceiver.hasNormals = true;
				go.transform.parent = m_projectorManager.transform;
				Undo.RegisterCreatedObjectUndo(go, "Easy Setup: Create Shadow Receiver");
				Undo.RecordObject(m_projectorManager, "Easy Setup: Create Shadow Receiver");
				if (receiver != null) {
					m_projectorManager.RemoveReceiver(receiver);
				}
				if (newReceiver != null) {
					m_projectorManager.AddReceiver(newReceiver);
				}
				EditorUtility.SetDirty(m_projectorManager);
				receiver = newReceiver;
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.TextArea("Mesh Shadow Receiver is necessary for casting shadows on the environment mesh surface. " +
			                   "If the environment is flat or doesn't have high curvature points, you don't need Mesh Shadow Receiver. " +
			                   "Instead, shadows can be casted on an infinite plane or a tangent plane at a point where raycast hits.");
			m_isAbleToGoNext = true;
			return true;
		}

		string m_errorString = null;
		MeshFilter[] m_environmentMeshes = null;
		MeshFilter[] environmentMeshes
		{
			get {
				if (m_environmentMeshes == null) {
					if (0 < m_projectorManager.receivers.Count) {
						MeshShadowReceiver receiver = m_projectorManager.receivers[0] as MeshShadowReceiver;
						if (receiver.meshTransform != null) {
							m_environmentMeshes = receiver.meshTransform.gameObject.GetComponentsInChildren<MeshFilter>();
						}
					}
				}
				return m_environmentMeshes;
			}
		}
		bool OnGUISetEnvironment()
		{
			MeshShadowReceiver receiver = null;
			if (0 < m_projectorManager.receivers.Count) {
				receiver = m_projectorManager.receivers[0] as MeshShadowReceiver;
			}

			GameObject rootObject = null;
			MeshTreeBase meshTree = null;
			if (receiver != null) {
				if (receiver.meshTransform != null) {
					rootObject = receiver.meshTransform.gameObject;
				}
				meshTree = receiver.meshTree;
				GameObject newRootObject = EditorGUILayout.ObjectField("Root Object", rootObject, typeof(GameObject), true) as GameObject;
				if (newRootObject != rootObject) {
					Undo.RecordObject(receiver, "Easy Setup: Set Root Object");
					receiver.meshTransform = (newRootObject == null) ? null : newRootObject.transform;
					EditorUtility.SetDirty(receiver);
					rootObject = newRootObject;
					m_environmentMeshes = null;
				}
			}
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_environmentLayers"));
			m_serializedObject.ApplyModifiedProperties();
			bool findObject = false;
			if (receiver != null && receiver.meshTransform != null) {
				// check Environment Layers
				for (int i = 0; i < environmentMeshes.Length; ++i) {
					if ((m_projectorManager.environmentLayers & (1 << environmentMeshes[i].gameObject.layer)) != 0) {
						findObject = true;
						break;
					}
				}
				if (!findObject) {
					GUILayout.TextArea("<color=red>No child objects of Root Object found in Environment Layers.</color>", m_errorStyle);
				}
			}
			GUILayout.TextArea("The objects in the Environment Layers will be ignored by Projectors. They will not receive the shadows casted by the Projectors. Instead, shadow receiver will receive the shadows on behalf of them.");
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Edit Layers")) {
				Selection.activeObject = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0];
			}
			if (receiver != null) {
				if (rootObject == null) {
					GUI.enabled = false;
				}
				if (GUILayout.Button("Collect Layers from Root Object")) {
					LayerMask layers = 0;
					MeshFilter[] meshes = rootObject.GetComponentsInChildren<MeshFilter>();
					foreach (MeshFilter mesh in meshes) {
						layers |= (1 << mesh.gameObject.layer);
					}
					if (layers != m_projectorManager.environmentLayers) {
						Undo.RecordObject(m_projectorManager, "Easy Setup: Collect Layers from Root Object");
						m_projectorManager.environmentLayers = layers;
						EditorUtility.SetDirty(m_projectorManager);
						m_serializedObject.Update();
					}
				}
				GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			if (receiver == null) {
				m_isAbleToGoNext = m_projectorManager.environmentLayers != 0;
				return true;
			}
			MeshTreeBase newMeshTree = EditorGUILayout.ObjectField("Mesh Tree", meshTree, typeof(MeshTreeBase), false) as MeshTreeBase;
			if (newMeshTree != meshTree) {
				Undo.RecordObject(receiver, "Easy Setup: Set Mesh Tree");
				receiver.meshTree = newMeshTree;
				meshTree = newMeshTree;
				EditorUtility.SetDirty(receiver);
			}
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (rootObject == null || !findObject) {
				GUI.enabled = false;
			}
			if (GUILayout.Button("Search Assets folder")) {
				newMeshTree = MeshShadowReceiverEditor.FindMeshTreeFromMeshTransform(rootObject.transform);
				if (newMeshTree == null) {
					EditorUtility.DisplayDialog("No mesh tree found for the Root Object.", "", "OK");
				}
				else {
					Undo.RecordObject(receiver, "Easy Setup: Set Mesh Tree");
					receiver.meshTree = newMeshTree;
					meshTree = newMeshTree;
					EditorUtility.SetDirty(receiver);
				}
			}
			if (GUILayout.Button ("Create a New Mesh Tree")) {
				CreateMeshTreeWizard.CreateWizard(rootObject.transform, m_projectorManager.environmentLayers, receiver);
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();
			m_errorString = null;
			LayerMask missingLayers = 0;
			LayerMask existingLayers = 0;
			bool multiObjectMeshTree = false;
			if (meshTree != null && rootObject != null) {
				if (meshTree is TerrainMeshTree) {
					Terrain terrain = rootObject.GetComponent<Terrain>();
					if (terrain == null) {
						m_errorString = "<color=red>The mesh tree is a TerrainMeshTree but Root Object is not a terrain object.</color>";
					}
					else if (terrain.terrainData != ((TerrainMeshTree)meshTree).terrainData) {
						m_errorString = "<color=red>The mesh tree was not the one which was created from the Root Object</color>";
					}
					else if ((m_projectorManager.environmentLayers & (1 << rootObject.layer)) == 0) {
						m_errorString = "<color=red>Environment Layers doesn't contain the layer of the Root Object.</color>";
						missingLayers |= (1 << rootObject.layer);
					}
				}
				else {
					Object srcMesh = ((MeshTree)meshTree).srcMesh;
					if (srcMesh is Mesh) {
						MeshFilter mesh = rootObject.GetComponent<MeshFilter>();
						if (mesh == null || mesh.sharedMesh != srcMesh) {
							m_errorString = "<color=red>The mesh tree was created from a single mesh but the Root Object doesn't have the mesh.</color>";
						}
						else if ((m_projectorManager.environmentLayers & (1 << rootObject.layer)) == 0) {
							m_errorString = "<color=red>Environment Layers doesn't contain the layer of the Root Object.</color>";
							missingLayers |= (1 << rootObject.layer);
						}
					}
					else {
						multiObjectMeshTree = true;
						if (srcMesh != null) {
							if (PrefabUtility.GetPrefabParent(rootObject) != srcMesh && rootObject != srcMesh) {
								PrefabType type = PrefabUtility.GetPrefabType(srcMesh);
								if (type == PrefabType.Prefab || type == PrefabType.ModelPrefab) {
									m_errorString = "<color=red>The mesh tree was created from a prefab object but the Root Object was not instantiated from the prefab.</color>";
								}
								else {
									m_errorString = "<color=red>The mesh tree was not created from the Root Object.</color>";
								}
							}
						}
						if (m_errorString == null) {
							MeshFilter[] meshes = rootObject.GetComponentsInChildren<MeshFilter>();
							LayerMask meshTreeLayers = ((MeshTree)meshTree).layerMask;
							foreach (MeshFilter mesh in meshes) {
								if ((meshTreeLayers & (1 << mesh.gameObject.layer)) != 0) {
									if ((m_projectorManager.environmentLayers & (1 << mesh.gameObject.layer)) == 0) {
										missingLayers |= (1 << mesh.gameObject.layer);
									}
									else {
										existingLayers |= (1 << mesh.gameObject.layer);
									}
								}
							}
							if (missingLayers != 0) {
								m_errorString = "<color=red>Some of the mesh tree layers are missing in Environment Layers.</color>";
							}
						}
					}
				}
			}
			if (m_errorString != null) {
				GUILayout.TextArea(m_errorString, m_errorStyle);
				if (missingLayers != 0) {
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Add mesh tree layers to Environment Layers")) {
						Undo.RecordObject(m_projectorManager, "Easy Setup: Add mesh tree layers to Environment Layers");
						m_projectorManager.environmentLayers |= missingLayers;
						EditorUtility.SetDirty(m_projectorManager);
						m_serializedObject.Update();
					}
					EditorGUILayout.EndHorizontal();
					if (multiObjectMeshTree && existingLayers != 0) {
						EditorGUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Rebuild the mesh tree with Environment Layers")) {
							((MeshTree)meshTree).layerMask &= ~missingLayers;
							if (((MeshTree)meshTree).srcMesh == null) {
								((MeshTree)meshTree).srcMesh = rootObject;
							}
							MeshTreeEditor.BuildMeshTree(meshTree);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			m_isAbleToGoNext = rootObject != null && meshTree != null && m_errorString == null && m_projectorManager.environmentLayers != 0;
			return true;
		}

		bool OnGUISelectLayers()
		{
			MeshShadowReceiver receiver = null;
			if (0 < m_projectorManager.receivers.Count) {
				receiver = m_projectorManager.receivers[0] as MeshShadowReceiver;
			}
			if (receiver == null) {
				if (m_isGoingBack) {
					--m_currentPage;
				}
				else {
					++m_currentPage;
				}
				return false;
			}

			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_receiverLayerMask"));
			m_serializedObject.ApplyModifiedProperties();
			GUILayout.TextArea("Receiver Layer Mask is used to assign a unique layer to each shadow receiver instance so that only the projector associated with the shadow receiver instance can cast a shadow on it.\n" +
			                   "If \"Nothing\" is specified, all the unnamed layers will be used.");
			m_isAbleToGoNext = true;
			return true;
		}

		string m_searchFolder = "Assets";
		List<GameObject> m_projectorPrefabs;
		Vector2 m_scrollPosition = new Vector2(0,0);

		bool OnGUIAttachAutoProjectorToPrefabs()
		{
			m_searchFolder = EditorGUILayout.TextField("Search Folder", m_searchFolder);
			EditorGUILayout.TextArea("Use semicolon \";\" as a separator to specify multiple folders");
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Search for Prefab")) {
				string[] searchFolders = m_searchFolder.Split(new char[]{',',';'});
				for (int i = 0; i < searchFolders.Length; ++i) {
					searchFolders[i] = searchFolders[i].Trim();
				}
				string[] gameObjects = AssetDatabase.FindAssets("t:GameObject", searchFolders);
				if (m_projectorPrefabs == null) {
					m_projectorPrefabs = new List<GameObject>();
				}
				m_projectorPrefabs.Clear();
				m_scrollPosition = new Vector2(0,0);
				foreach (string guid in gameObjects) {
					string path = AssetDatabase.GUIDToAssetPath(guid);
					GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
					GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
					Projector[] projectors = go.GetComponentsInChildren<Projector>(true);
					if (projectors != null && 0 < projectors.Length) {
						foreach (Projector projector in projectors) {
							m_projectorPrefabs.Add(PrefabUtility.GetPrefabParent(projector.gameObject) as GameObject);
						}
					}
					Object.DestroyImmediate(go);
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();
			if (m_projectorPrefabs != null && 0 < m_projectorPrefabs.Count) {
				GUILayout.TextArea("AutoProjector component is necessary for a projector to work with ProjectorManager.\n" +
				                   "Check the checkboxes below to add AutoProjector component.");
			}
			else {
				GUILayout.TextArea("AutoProjector component is necessary for a projector to work with ProjectorManager.");
			}
			EditorGUI.indentLevel++;
			m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
			bool allProjectorsHaveAutoProjector = true;
			if (m_projectorPrefabs != null) {
				foreach (GameObject go in m_projectorPrefabs) {
					EditorGUILayout.BeginHorizontal();
					AutoProjector autoProjector = go.GetComponent<AutoProjector>();
					bool bAdded = autoProjector != null;
					if (!bAdded) {
						allProjectorsHaveAutoProjector = false;
					}
					EditorGUILayout.ObjectField(go, typeof(GameObject), true);
					GUILayout.Label("in");
					EditorGUILayout.ObjectField(PrefabUtility.FindPrefabRoot(go), typeof(GameObject), true);
					bool b = EditorGUILayout.Toggle(bAdded);
					if (b != bAdded) {
						if (b) {
							Undo.AddComponent<AutoProjector>(go);
						}
						else {
							Undo.DestroyObjectImmediate(autoProjector);
						}
						EditorUtility.SetDirty(go);
					}
					EditorGUILayout.EndHorizontal();
				}
				if (m_projectorPrefabs.Count == 0) {
					EditorGUILayout.LabelField("Nothing found.");
				}
			}
			EditorGUILayout.EndScrollView();
			EditorGUI.indentLevel--;
			if (m_projectorPrefabs != null && 0 < m_projectorPrefabs.Count) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (allProjectorsHaveAutoProjector) {
					GUI.enabled = false;
				}
				if (GUILayout.Button("Add AutoProjector component to all the projector prefabs")) {
					foreach (GameObject go in m_projectorPrefabs) {
						AutoProjector autoProjector = go.GetComponent<AutoProjector>();
						if (autoProjector == null) {
							Undo.AddComponent<AutoProjector>(go);
							EditorUtility.SetDirty(go);
						}
					}
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
			}
			m_isAbleToGoNext = true;//m_projectorPrefabs != null;
			return true;
		}

		Projector[] m_sceneProjectors = null;
		bool OnGUIAttachAutoProjectorToSceneObjects()
		{
			if (m_sceneProjectors == null) {
				m_sceneProjectors = Object.FindObjectsOfType<Projector>();
			}
			if (m_sceneProjectors != null && 0 < m_sceneProjectors.Length) {
				GUILayout.TextArea("AutoProjector component is necessary for a projector to work with ProjectorManager.\n" +
				                   "Check the checkboxes below to add AutoProjector component.");
			}
			else {
				GUILayout.TextArea("AutoProjector component is necessary for a projector to work with ProjectorManager.");
			}
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("List of Projectors in the Scene");
			if (GUILayout.Button("Update")) {
				m_sceneProjectors = Object.FindObjectsOfType<Projector>();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel++;
			m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
			bool allProjectorsHaveAutoProjector = true;
			if (m_sceneProjectors != null && 0 < m_sceneProjectors.Length) {
				foreach (Projector projector in m_sceneProjectors) {
					EditorGUILayout.BeginHorizontal();
					AutoProjector autoProjector = projector.GetComponent<AutoProjector>();
					bool bAdded = autoProjector != null;
					if (!bAdded) {
						allProjectorsHaveAutoProjector = false;
					}
					EditorGUILayout.ObjectField(projector, typeof(Projector), true);
					bool b = EditorGUILayout.Toggle(bAdded);
					if (b != bAdded) {
						if (b) {
							Undo.AddComponent<AutoProjector>(projector.gameObject);
						}
						else {
							Undo.DestroyObjectImmediate(autoProjector);
						}
						EditorUtility.SetDirty(projector.gameObject);
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndScrollView();
			EditorGUI.indentLevel--;
			if (m_sceneProjectors != null && 0 < m_sceneProjectors.Length) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (allProjectorsHaveAutoProjector) {
					GUI.enabled = false;
				}
				if (GUILayout.Button("Add AutoProjector component to all the projectors in the scene")) {
					foreach (Projector projector in m_sceneProjectors) {
						AutoProjector autoProjector = projector.GetComponent<AutoProjector>();
						if (autoProjector == null) {
							Undo.AddComponent<AutoProjector>(projector.gameObject);
							EditorUtility.SetDirty(projector.gameObject);
						}
					}
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
			}
			m_isAbleToGoNext = true;
			return true;
		}

		Vector2 m_infinitePlaneScrollPos = Vector2.zero;
		bool OnGUISetupPlaneShadows()
		{
			if (m_projectorManager.mainCamera == null) {
				m_projectorManager.mainCamera = Camera.main;
				m_serializedObject.Update();
			}
			GUILayout.TextArea("Set the main camera and fadeout distance so that projectors whose distance from the main camera is longer than fadeout distance can be turned off. " +
			                   "If a projector uses a shader which has \"_Alpha\" parameter to contorol shadow intensity, the shadow can fadeout.");
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_mainCamera"));
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_projectorFadeoutDistance"));
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_fadeDuration"));

			EditorGUILayout.Separator();

			GUILayout.TextArea("If you set Raycast Plane Mask or Infinite Plane, the projectors which are turned off by fadeout distance still can cast shadows on the plane.");
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_raycastPlaneMask"));
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_useInfinitePlane"));
			if (m_projectorManager.useInfinitPlane) {
				m_infinitePlaneScrollPos = EditorGUILayout.BeginScrollView(m_infinitePlaneScrollPos);
				EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_infinitePlaneTransform"));
				EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_infinitePlaneNormal"));
				EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_infinitePlaneHeight"));
				EditorGUILayout.EndScrollView();
			}

			m_serializedObject.ApplyModifiedProperties();
			m_isAbleToGoNext = m_projectorManager.mainCamera != null && (m_projectorManager.infinitPlaneTransform != null || !m_projectorManager.useInfinitPlane);
			return true;
		}

		bool OnGUICreatePackedTextures()
		{
			if (m_projectorManager.raycastPlaneMask == 0 && !m_projectorManager.useInfinitPlane) {
				if (m_isGoingBack) {
					--m_currentPage;
				}
				else {
					++m_currentPage;
				}
				return false;
			}
			GUILayout.TextArea("Combine all the textures used by Projectos to reduce draw calls of Plane shadows. " + 
			                   "If you set only a single texture, all the Plane shadows are drawn with the texture." +
			                   "If you don't set anything, Plane shadows will be disabled.");
			m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_blobShadowTextures"), true);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Collect from Projectors in Assets")) {
				string[] gameObjects = AssetDatabase.FindAssets("t:GameObject");
				Undo.RecordObject(m_projectorManager, "EasySetup: Collect from Projectors in Assets");
				foreach (string guid in gameObjects) {
					string path = AssetDatabase.GUIDToAssetPath(guid);
					GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
					GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
					AutoProjector[] projectors = go.GetComponentsInChildren<AutoProjector>(true);
					if (projectors != null && 0 < projectors.Length) {
						foreach (AutoProjector projector in projectors) {
							Texture2D tex = projector.projector.projector.material.GetTexture(m_projectorManager.blobShadowTextureName) as Texture2D;
							if (tex != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex))) {
								m_projectorManager.AddBlobShadowTextureIfNotExist(tex);
							}
						}
					}
					Object.DestroyImmediate(go);
				}
				EditorUtility.SetDirty(m_projectorManager);
				m_serializedObject.Update();
			}
			if (GUILayout.Button("Collect from Projectors in the scene")) {
				Undo.RecordObject(m_projectorManager, "EasySetup: Collect from projectors in the scene");
				AutoProjector[] projectors = Object.FindObjectsOfType<AutoProjector>();
				foreach (AutoProjector projector in projectors) {
					Texture2D tex = projector.projector.projector.material.GetTexture(m_projectorManager.blobShadowTextureName) as Texture2D;
					if (tex != null && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex))) {
						m_projectorManager.AddBlobShadowTextureIfNotExist(tex);
					}
				}
				EditorUtility.SetDirty(m_projectorManager);
				m_serializedObject.Update();
			}
			EditorGUILayout.EndHorizontal();
			if (m_projectorManager.blobShadowTextures != null && 1 < m_projectorManager.blobShadowTextures.Length) {
				EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_packedBlobShadowTexture"));
				EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_blobShadowTextureRects"), true);
				EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_packedTexturePadding"));
				EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_packedTextureMaxSize"));
				EditorGUILayout.EndScrollView();
				if (GUILayout.Button("Pack Blob Shadow Textures")) {
					ProjectorManagerEditor.PackBlobShadowTextures(m_projectorManager, m_serializedObject);
				}
			}
			else {
				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.PropertyField(m_serializedObject.FindProperty("m_shadowTexName"));
			m_serializedObject.ApplyModifiedProperties();
			return true;
		}
	}
}
