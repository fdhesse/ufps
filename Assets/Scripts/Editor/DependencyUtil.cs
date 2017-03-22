using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class DependencyUtil : EditorWindow {
	public static string GetFileExt(string path, bool toLower) {
		string ext = null;
		int index = path.LastIndexOf('.');
		if (index != -1)
			ext = path.Substring(index);

		if (toLower)
			ext = ext.ToLower();
		return ext;
	}
	public static string NormalizePath(string path) {
		return path.Replace("\\", "/");
	}
	public static string[] FindDependencies(string prefabPath) {
		string[] prefabList = new string[] { prefabPath };
		return AssetDatabase.GetDependencies(prefabList);
	}
	public static string[] FindDependencies(string[] prefabPaths) {
		//return AssetDatabase.GetDependencies(prefabPaths);
		HashSet<string> pathSet = new HashSet<string>();
		for (int i=0; i<prefabPaths.Length; ++i) {
			string[] ret = FindDependencies(prefabPaths[i]);
			for (int j=0; j<ret.Length; ++j) {
				pathSet.Add(ret[j]);
			}

			string title = "Please wait " + i + " / " + prefabPaths.Length;
			if (EditorUtility.DisplayCancelableProgressBar(title, prefabPaths[i], (float)i / prefabPaths.Length))
				break;
		}
		EditorUtility.ClearProgressBar();
		return pathSet.ToArray();
	}

	public static List<string> FindDependenciesByType(string[] prefabPaths, string extMask) {
		string[] dependencies = FindDependencies(prefabPaths);

		List<string> ret = new List<string>();
		for (int i=0; i<dependencies.Length; ++i) {
			string path = dependencies[i];
			string ext = GetFileExt(path, true);
			if (ext == extMask || extMask == ".*") {
				ret.Add(path);
			}
		}
		return ret;
	}
	public static List<string> FindUsedFiles(string rootPath, string extMask) {
		List<string> filePaths = FindFilePaths(rootPath, ".prefab");
		return FindDependenciesByType(filePaths.ToArray(), extMask);
	}
	public static List<string> FindFilePaths(string rootPath, string extMask) {
		DirectoryInfo dir = new DirectoryInfo(rootPath);
		List<FileInfo> files = new List<FileInfo>(dir.GetFiles("*" + extMask, SearchOption.AllDirectories));

		List<string> filePaths = new List<string>();
		string headerPath = Application.dataPath;

		for (int i=0; i<files.Count; ++i) {
			string filePath = NormalizePath(files[i].FullName);

			int index = filePath.IndexOf(headerPath);
			if (index != -1) {
				// Assets目录需要保留
				filePath = filePath.Substring(index + headerPath.Length - 6);
				//Debug.LogError(filePath);
				filePaths.Add(filePath);
			}
			else {
				Debug.LogError(filePath + " error");
			}
		}
		return filePaths;
	}

	///////////////////////////////////////////////////////////////////////////////
	[MenuItem("Assets/Show All Dependency")]
	static void ShowAllDependency() {
		Object[] selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);

		List<string> filePaths = new List<string>();
		foreach (Object obj in selectedAsset) {
			string path = AssetDatabase.GetAssetPath(obj);
			filePaths.Add(path);
		}

		string[] dependencyFiles = FindDependencies(filePaths.ToArray());

		DependencyUtil window = EditorWindow.GetWindow(typeof(DependencyUtil)) as DependencyUtil;
		window.Init(dependencyFiles.ToList());
		window.Show();
	}
	[MenuItem("Assets/Show All Reference")]
	public static void ShowAllReference() {
		ShowAllReferenceInPath("Assets/Resources/Prefabs/", ".prefab");
	}
	[MenuItem("Assets/Show All Reference UI")]
	public static void ShowAllReferenceUI() {
		ShowAllReferenceInPath("Assets/Resources/Prefabs/UI/", ".prefab");
	}
	public static void ShowAllReferenceInPath(string rootPath, string ext) {
		Object[] selectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);

		List<string> selectedFilePaths = new List<string>();
		foreach (Object obj in selectedAsset) {
			string path = AssetDatabase.GetAssetPath(obj);
			selectedFilePaths.Add(path);
		}

		List<string> filePaths = new List<string>();
		//string[] allAssets = AssetDatabase.GetAllAssetPaths();
		string[] allAssets = FindFilePaths(rootPath, ext).ToArray();
		for (int i=0; i<allAssets.Length; ++i) {
			string path = allAssets[i];
			//Debug.LogError("位置：" + path);
			string[] dependencyFiles = FindDependencies(path);

			for (int j=0; j<selectedFilePaths.Count; ++j) {
				if (dependencyFiles.Contains(selectedFilePaths[j])) {
					filePaths.Add(path);
					break;
				}
			}

			string title = "Please wait " + i + " / " + allAssets.Length;
			if (EditorUtility.DisplayCancelableProgressBar(title, path, (float)i / allAssets.Length))
				break;
		}

		EditorUtility.ClearProgressBar();

		DependencyUtil window = EditorWindow.GetWindow(typeof(DependencyUtil)) as DependencyUtil;
		window.Init(filePaths);
		window.Show();
	}
	[MenuItem("Assets/Show All Used Shader")]
	public static void ShowAllUsedShader() {
		List<string> filePaths = DependencyUtil.FindUsedFiles("Assets/Resources/Prefabs/", ".shader");

		DependencyUtil window = EditorWindow.GetWindow(typeof(DependencyUtil)) as DependencyUtil;
		window.Init(filePaths);
		window.Show();
	}

	class MyFileInfo {
		public string path;
		public Object prefab;
		public FileInfo fileInfo;
	}

	Vector2 m_ScrollPos = Vector2.zero;
	List<MyFileInfo> m_DependencyFiles = new List<MyFileInfo>();
	bool m_SizeOrderAscend = false;

	public void Init(List<string> filePaths) {
		m_DependencyFiles.Clear();

		for (int i=0; i<filePaths.Count; ++i) {
			MyFileInfo info = new MyFileInfo();

			info.path = filePaths[i];
			info.fileInfo = new FileInfo(info.path);

			string ext = GetFileExt(info.path, true);
			info.prefab = AssetDatabase.LoadAssetAtPath(info.path, typeof(Object)) as Object;
			m_DependencyFiles.Add(info);
		}
	}

	void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Count: " + m_DependencyFiles.Count, GUILayout.Width(100));
		EditorGUILayout.EndHorizontal();

		ShowResult();
	}

	void AddLabelField(string text, int width, bool errorFlag) {
		GUIStyle textStyle = new GUIStyle(GUI.skin.textField);
		textStyle.normal.textColor = (errorFlag)? Color.yellow : Color.white;
		EditorGUILayout.LabelField(text, textStyle, GUILayout.Width(width));
	}

	void ShowResult() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Button("Texture", GUILayout.Width(200));
		GUILayout.Button("Path", GUILayout.Width(700));
		if (GUILayout.Button("Size", GUILayout.Width(100))) {
			m_SizeOrderAscend = !m_SizeOrderAscend;

			if (m_SizeOrderAscend)
				m_DependencyFiles = (from items in m_DependencyFiles orderby (items.fileInfo.Length) ascending select items).ToList();
			else
				m_DependencyFiles = (from items in m_DependencyFiles orderby (items.fileInfo.Length) descending select items).ToList();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();

		int height = Mathf.Min(m_DependencyFiles.Count * 20, Screen.height - 70);
		m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.Width (position.width), GUILayout.Height(height));

		for (int i=0; i<m_DependencyFiles.Count; ++i) {
			MyFileInfo info = m_DependencyFiles[i];

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.ObjectField(info.prefab, typeof(Object), GUILayout.Width(200));

			AddLabelField(info.path, 700, false);

			long size = info.fileInfo.Length;
			AddLabelField(size.ToString(), 100, (size > Mathf.Pow(2, 20)));

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		EditorGUILayout.EndHorizontal();
	}
}
