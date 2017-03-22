//
// ProjectorManagerEditor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using UnityEditor;

namespace FastShadowReceiver.Editor {
	[CustomEditor(typeof(ProjectorManager))]
	public class ProjectorManagerEditor : EditorBase {
		static private bool s_bShowReceivers = true;
		static private bool s_bShowProjectorControls = true;
		static private bool s_bShowPlaneShadows = true;
		static private bool s_bShowPlaneShadowTextures = true;
		void OnEnable()
		{
			ProjectorManager manager = target as ProjectorManager;
			Renderer renderer = manager.GetComponent<Renderer>();
			if (renderer.sharedMaterial == null) {
				renderer.sharedMaterial = editorSettings.m_defaultProjectorManagerMaterial;
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
				renderer.castShadows = false;
#else
				renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#endif
				renderer.useLightProbes = false;
				renderer.receiveShadows = false;
			}
			if (manager.receivers != null) {
				foreach (ReceiverBase receiver in manager.receivers) {
					Renderer r = receiver.GetComponent<Renderer>();
					if (r.sharedMaterial == null) {
						r.sharedMaterial = editorSettings.m_defaultReceiverMaterial;
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
						r.castShadows = false;
#else
						r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
						r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
#endif
						r.useLightProbes = false;
						r.receiveShadows = false;
					}
				}
			}
		}
		public override void OnInspectorGUI ()
		{
			ProjectorManager manager = target as ProjectorManager;
			if (GUILayout.Button("Easy Setup Wizard")) {
				EasySetupWizard.CreateWizard(manager);
			}
			s_bShowReceivers = EditorGUILayout.Foldout(s_bShowReceivers, "Shadow Receivers");
			if (s_bShowReceivers) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_receiverLayerMask"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_receivers"), true);
				--EditorGUI.indentLevel;
			}
			s_bShowProjectorControls = EditorGUILayout.Foldout(s_bShowProjectorControls, "Projector Controls");
			if (s_bShowProjectorControls) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_mainCamera"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_environmentLayers"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_projectorFadeoutDistance"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_fadeDuration"));
				--EditorGUI.indentLevel;
			}
			s_bShowPlaneShadows = EditorGUILayout.Foldout(s_bShowPlaneShadows, "Plane Shadows");
			if (s_bShowPlaneShadows) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_raycastPlaneMask"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_useInfinitePlane"));
				if (manager.useInfinitPlane) {
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_infinitePlaneTransform"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_infinitePlaneNormal"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_infinitePlaneHeight"));
				}
				--EditorGUI.indentLevel;
			}
			s_bShowPlaneShadowTextures = EditorGUILayout.Foldout(s_bShowPlaneShadowTextures, "Plane Shadow Textures");
			if (s_bShowPlaneShadowTextures) {
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_blobShadowTextures"), true);
				if (manager.blobShadowTextures != null && 1 < manager.blobShadowTextures.Length) {
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_packedBlobShadowTexture"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_blobShadowTextureRects"), true);
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_packedTexturePadding"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("m_packedTextureMaxSize"));
					if (GUILayout.Button("Pack Blob Shadow Textures")) {
						PackBlobShadowTextures(manager, serializedObject);
					}
				}
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_shadowTexName"));
				--EditorGUI.indentLevel;
			}
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_manualUpdate"));
			serializedObject.ApplyModifiedProperties();
		}
		public static void PackBlobShadowTextures(ProjectorManager manager, SerializedObject serializedObject)
		{
			string folderPath;
			string fileName;
			string assetPath;
			if (manager.packedBlobShadowTexture != null) {
				assetPath = AssetDatabase.GetAssetPath(manager.packedBlobShadowTexture);
				folderPath = System.IO.Path.GetDirectoryName(assetPath);
				fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
			}
			else {
				assetPath = AssetDatabase.GetAssetPath(manager.blobShadowTextures[0]);
				folderPath = System.IO.Path.GetDirectoryName(assetPath);
				fileName = "PackedBlobShadowTexture";
			}
			string fullPath = EditorUtility.SaveFilePanel("Create Packed Blob Texture", folderPath, fileName, "png");
			if (!string.IsNullOrEmpty(fullPath)) {
				assetPath = fullPath;
				if (assetPath.StartsWith(Application.dataPath)) {
					assetPath = assetPath.Substring(Application.dataPath.Length - 6);
				}
				Texture2D packedTexture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
				if (packedTexture == null) {
					packedTexture = new Texture2D(4, 4, TextureFormat.ARGB32, true);
					SaveTextureAsPNG(packedTexture, fullPath);
					packedTexture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
				}
				Undo.RegisterCompleteObjectUndo(manager, "Pack Blob Shadow Textures");
				// enable read/write blob shadow textures
				bool[] readableFlgas = new bool[manager.blobShadowTextures.Length];
				for (int i = 0; i < manager.blobShadowTextures.Length; ++i) {
					string path = AssetDatabase.GetAssetPath(manager.blobShadowTextures[i]);
					TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
					if (importer != null && !importer.isReadable) {
						readableFlgas[i] = importer.isReadable;
						importer.isReadable = true;
						AssetDatabase.ImportAsset(path);
					}
					else {
						readableFlgas[i] = true;
					}
				}
				TextureImporter packedImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(packedTexture)) as TextureImporter;
				packedImporter.isReadable = true;
				AssetDatabase.ImportAsset(assetPath);
				manager.PackBlobShadowTextures(packedTexture);
				for (int i = 0; i < manager.blobShadowTextures.Length; ++i) {
					string path = AssetDatabase.GetAssetPath(manager.blobShadowTextures[i]);
					TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
					if (importer != null && !readableFlgas[i]) {
						importer.isReadable = false;
						AssetDatabase.ImportAsset(path);
					}
				}
				SaveTextureAsPNG(packedTexture, fullPath);
				packedImporter.isReadable = false;
				packedImporter.wrapMode = TextureWrapMode.Clamp;
				AssetDatabase.ImportAsset(assetPath);
				AssetDatabase.Refresh();
				EditorUtility.SetDirty(manager);
				// repaint inspector view
				serializedObject.FindProperty("m_packedBlobShadowTexture").objectReferenceValue = manager.packedBlobShadowTexture;
				SerializedProperty prop = serializedObject.FindProperty("m_blobShadowTextureRects");
				prop.arraySize = manager.blobShadowTextureRects.Length;
				for (int i = 0; i < manager.blobShadowTextureRects.Length; ++i) {
					prop.GetArrayElementAtIndex(i).rectValue = manager.blobShadowTextureRects[i];
				}
			}
		}
		static void SaveTextureAsPNG(Texture2D texture, string assetPath)
		{
			byte[] png = texture.EncodeToPNG();
			System.IO.File.WriteAllBytes(assetPath, png);
			AssetDatabase.Refresh();
		}
	}
}
