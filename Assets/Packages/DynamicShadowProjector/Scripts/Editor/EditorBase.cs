//
// EditorBase.cs
//
// Dynamic Shadow Projector
//
// Copyright 2015 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//

using UnityEngine;
using UnityEditor;

namespace DynamicShadowProjector.Editor {
	public class EditorBase : UnityEditor.Editor {
		protected static GUIContent[] s_textureSizeDisplayOption = new GUIContent[] {new GUIContent("16"), new GUIContent("32"), new GUIContent("64"), new GUIContent("128"), new GUIContent("256"), new GUIContent("512")};
		protected static int[] s_textureSizeOption = new int[] {16, 32, 64, 128, 256, 512};
		protected static GUIContent[] s_blurLevelDisplayOption = new GUIContent[] {new GUIContent("0"), new GUIContent("1"), new GUIContent("2 (Not Recommended)"), new GUIContent("3 (Not Recommended)")};
		protected static int[] s_blurLevelOption = new int[] {0, 1, 2, 3};
		GUIStyle m_richTextStyle;
		protected GUIStyle richTextStyle
		{
			get {
				if (m_richTextStyle == null) {
					m_richTextStyle = new GUIStyle();
					m_richTextStyle.richText = true;
					m_richTextStyle.wordWrap = true;
					m_richTextStyle.alignment = TextAnchor.MiddleCenter;
				}
				return m_richTextStyle;
			}
		}
		protected static Material FindMaterial(string shaderName)
		{
			Shader shader = Shader.Find(shaderName);
			if (shader == null) {
				Debug.LogError("Cannot find a shader named " + shaderName);
				return null;
			}
			string path = AssetDatabase.GetAssetPath(shader);
			if (path == null || path.Length < 6) {
				return null;
			}
			path = path.Substring(0, path.Length - 6); // remove "shader" extension
			path += "mat"; // add "mat" extension
			return AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
		}
		public static void ClearMaterialProperties(Material mat)
		{
			if (mat == null) {
				return;
			}
			SerializedObject serialize = new SerializedObject(mat);
			SerializedProperty prop = serialize.FindProperty("m_SavedProperties");
			SerializedProperty propChild = prop.FindPropertyRelative("m_TexEnvs");
			bool modified = false;
			if (propChild != null && propChild.arraySize != 0) {
				propChild.arraySize = 0;
				modified = true;
			}
			propChild = prop.FindPropertyRelative("m_Floats");
			if (propChild != null && propChild.arraySize != 0) {
				propChild.arraySize = 0;
				modified = true;
			}
			propChild = prop.FindPropertyRelative("m_Colors");
			if (propChild != null && propChild.arraySize != 0) {
				propChild.arraySize = 0;
				modified = true;
			}
			if (modified) {
				serialize.ApplyModifiedProperties();
				EditorUtility.SetDirty(mat);
			}
		}
		private static bool RemoveUnuserMaterialPropertyData(Material mat, SerializedProperty prop, string forceRemoveProperty)
		{
			int dst = 0;
			for (int i = 0; i < prop.arraySize; ++i) {
				SerializedProperty elem = prop.GetArrayElementAtIndex(i);
#if (UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9)
                string name = elem.displayName;
#else
				string name = elem.FindPropertyRelative("first").FindPropertyRelative("name").stringValue;
#endif           
				if (mat.HasProperty(name) && (string.IsNullOrEmpty(forceRemoveProperty) || name != forceRemoveProperty)) {
					if (dst != i) {
						prop.MoveArrayElement(i, dst);
					}
					++dst;
				}
			}
			if (dst != prop.arraySize) {
				prop.arraySize = dst;
				return true;
			}
			return false;
		}
		public static void RemoveUnusedMaterialProperties(Material mat, bool isDynamic = true)
		{
			SerializedObject serialize = new SerializedObject(mat);
			SerializedProperty prop = serialize.FindProperty("m_SavedProperties");
			SerializedProperty propChild = prop.FindPropertyRelative("m_TexEnvs");
			bool modified = false;
			if (propChild != null && propChild.arraySize != 0) {
				if (RemoveUnuserMaterialPropertyData(mat, propChild, isDynamic ? "_ShadowTex" : null)) {
					modified = true;
				}
			}
			propChild = prop.FindPropertyRelative("m_Floats");
			if (propChild != null && propChild.arraySize != 0) {
				if (RemoveUnuserMaterialPropertyData(mat, propChild, isDynamic ? "_DSPMipLevel" : null)) {
					modified = true;
				}
			}
			propChild = prop.FindPropertyRelative("m_Colors");
			if (propChild != null && propChild.arraySize != 0) {
				if (RemoveUnuserMaterialPropertyData(mat, propChild, null)) {
					modified = true;
				}
			}
			if (modified) {
				serialize.ApplyModifiedProperties();
				EditorUtility.SetDirty(mat);
			}
		}
	}
}
