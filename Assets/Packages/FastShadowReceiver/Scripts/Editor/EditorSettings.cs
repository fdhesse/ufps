//
// EditorSettings.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;

namespace FastShadowReceiver {
	public class EditorSettings : ScriptableObject {
		public Material m_defaultReceiverMaterial;
		public Material m_defaultShadowmapReceiverMaterial;
		public Material m_defaultProjectorManagerMaterial;
		public void Initialize(string editorPath)
		{
			string materialFolder = editorPath.Replace("FastShadowReceiver/Scripts/Editor", "FastShadowReceiver/Materials");
			m_defaultReceiverMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath(materialFolder + "/Invisible.mat", typeof(Material)) as Material;
			m_defaultShadowmapReceiverMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath(materialFolder + "/MultiplyShadowMap.mat", typeof(Material)) as Material;
			m_defaultProjectorManagerMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath(materialFolder + "/QuadShadows.mat", typeof(Material)) as Material;
		}
		public static EditorSettings GetSettings(UnityEditor.MonoScript editorScript)
		{
			string editorPath = UnityEditor.AssetDatabase.GetAssetPath(editorScript);
			editorPath = System.IO.Path.GetDirectoryName(editorPath);
			string path = editorPath + "/EditorSettings.asset";
			EditorSettings settings = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(EditorSettings)) as EditorSettings;
			if (settings == null) {
				settings = ScriptableObject.CreateInstance<EditorSettings>();
				settings.Initialize(editorPath);
				UnityEditor.AssetDatabase.CreateAsset(settings, path);
			}
			return settings;
		}
	}
}
