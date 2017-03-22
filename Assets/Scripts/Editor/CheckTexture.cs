using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class CheckTexture : EditorWindow {
	enum Flags {
		Type = 1,
		Size = 2,
		Mipmap = 4,
		Filter = 8,
		Wrap = 16,
		Format = 32,
		FileLength = 64,
	}

	struct ErrorFileInfo {
		public FileInfo fileInfo;
		public TextureImporter texInfo;
		public Texture texture;
		public int flags;
	}
	List<ErrorFileInfo> m_ErrorFiles = new List<ErrorFileInfo> ();
	Vector2 m_ScrollPos = Vector2.zero;

	bool m_SizeOrderAscend = false;
	bool m_TypeOrderAscend = false;
	bool m_MipmapOrderAscend = false;
	bool m_FilterOrderAscend = false;
	bool m_WrapOrderAscend = false;
	bool m_FormatOrderAscend = false;
	bool m_FileSizeOrderAscend = false;

	[MenuItem ("Tools/CheckTextures")]
	static void Init () {
		CheckTexture window = EditorWindow.GetWindow(typeof(CheckTexture)) as CheckTexture;
		window.Show();
	}

	void OnGUI() {
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button("Check UI", GUILayout.Width(100))) {
			m_ErrorFiles.Clear();
			CheckUI("Assets/Resources/Prefabs/UI");
		}
		if (GUILayout.Button("Check Character", GUILayout.Width(120))) {
			m_ErrorFiles.Clear();
			Check3D("Assets/3D");
		}
		if (GUILayout.Button("Check Story", GUILayout.Width(100))) {
			m_ErrorFiles.Clear();
			Check3D("Assets/Resources/Prefabs/Story");
		}
		if (GUILayout.Button("Check Effects", GUILayout.Width(100))) {
			m_ErrorFiles.Clear();
			Check3D("Assets/Resources/Prefabs/Effects");
		}
		if (GUILayout.Button("Check Equips", GUILayout.Width(100))) {
			m_ErrorFiles.Clear();
			Check3D("Assets/Resources/Prefabs/Equips");
		}
		if (GUILayout.Button("Check Scenes", GUILayout.Width(100))) {
			m_ErrorFiles.Clear();
			Check3D("Assets/3D");
		}
		if (GUILayout.Button("Fix Format (UI)", GUILayout.Width(150))) {
			FixTextures("Default", "Assets/BundleResources/UI");
		}
		if (GUILayout.Button("Fix Format (3D)", GUILayout.Width(150))) {
			FixTextures("Default", "Assets/BundleResources/UI");
		}
		if (GUILayout.Button("CompressTextures", GUILayout.Width(150))) {
			CompressTextures("Assets/BundleResources/UI");
		}

		GUILayout.Label("Count: " + m_ErrorFiles.Count, GUILayout.Width(100));

		EditorGUILayout.EndHorizontal();

		ShowResult();
	}

	void CompressTextures(string rootPath) {
		DirectoryInfo dir = new DirectoryInfo(rootPath);
		List<FileInfo> files = new List<FileInfo>(dir.GetFiles("*.png", SearchOption.AllDirectories));

		for (int i=0; i<files.Count; ++i) {
			string filePath = files[i].FullName;
			filePath = filePath.Substring(filePath.IndexOf(rootPath));
			CompressPng(filePath);
		}
	}

	public static void CompressPng(string path) {
		if (!path.Contains ("_cp"))
			return;

		System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
		myProcess.StartInfo.FileName = System.Environment.CurrentDirectory + "/Tools/pngquant/conv_one.bat";
		myProcess.StartInfo.Arguments = System.Environment.CurrentDirectory + "/" + path;
		myProcess.StartInfo.UseShellExecute = false;
		myProcess.StartInfo.CreateNoWindow = true;//是否显示DOS窗口，true代表隐藏;
		myProcess.StartInfo.RedirectStandardInput = true;
		myProcess.StartInfo.RedirectStandardOutput = true;
		myProcess.StartInfo.RedirectStandardError = true;
		myProcess.Start();

		StreamReader sOut = myProcess.StandardOutput;//标准输入流 
		StreamReader sErr = myProcess.StandardError;//标准错误流 

		string s = sOut.ReadToEnd();//读取执行DOS命令后输出信息 
		string er = sErr.ReadToEnd();//读取执行DOS命令后错误信息 

		if (myProcess.HasExited == false) {
			myProcess.Kill();
			Debug.Log("cmd err" + er);
		}
		else {
			Debug.Log("cmd log" + s);
		}

		sOut.Close();
		sErr.Close();
		myProcess.Close();
	}

	void FixTextures(string platform, string rootPath) {
		for (int i=0; i<m_ErrorFiles.Count; ++i) {
			ErrorFileInfo info = m_ErrorFiles[i];

			bool changed = false;

			if (platform == "Default") {
				if (info.texInfo.textureFormat != TextureImporterFormat.AutomaticTruecolor) {
					info.texInfo.textureFormat = TextureImporterFormat.AutomaticTruecolor;
					info.texInfo.ClearPlatformTextureSettings("Android");
					changed = true;
				}
			}
			//if (platform == "Android" || platform == "All") {
			//	TextureImporterFormat textureFormatAndroid = TextureImporterFormat.AutomaticTruecolor;
			//	int maxTextureSize = 0;
			//	info.texInfo.GetPlatformTextureSettings("Android", out maxTextureSize, out textureFormatAndroid);

			//	if (textureFormatAndroid != TextureImporterFormat.AutomaticTruecolor) {
			//		textureFormatAndroid = TextureImporterFormat.AutomaticTruecolor;
			//		info.texInfo.SetPlatformTextureSettings("Android", maxTextureSize, textureFormatAndroid);
			//		changed = true;
			//	}
			//}

			if (changed) {
				// 保存更改
				string filePath = info.fileInfo.FullName;
				filePath = filePath.Substring(filePath.IndexOf(rootPath));
				AssetDatabase.ImportAsset(filePath);
			}
		}
	}

	void CheckUI(string rootPath) {
		List<string> filePathList = DependencyUtil.FindUsedFiles(rootPath, ".png");

		for (int i=0; i<filePathList.Count; ++i) {
			string filePath = filePathList[i];
			//Debug.LogError(filePath);
			if (filePath.IndexOf("UI/Icons") != -1) {
				// icons目录跳过
				continue;
			}

			TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;

			FileInfo fileInfo = new FileInfo(filePath);

			int flags = 0;
			if (textureImporter.textureType != TextureImporterType.Default)
				flags |= (int)Flags.Type;
			if (textureImporter.mipmapEnabled)
				flags |= (int)Flags.Mipmap;
			if (textureImporter.filterMode == FilterMode.Point)
				flags |= (int)Flags.Filter;
			if (textureImporter.wrapMode != TextureWrapMode.Clamp)
				flags |= (int)Flags.Wrap;
			if (textureImporter.textureFormat != TextureImporterFormat.AutomaticTruecolor)
				flags |= (int)Flags.Format;

			if (fileInfo.Length > 1024*512)
				flags |= (int)Flags.FileLength;

			Texture texture = AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture)) as Texture;
			if (texture.width > 1024 || texture.height > 1024)
				flags |= (int)Flags.Size;

			if (texture.width == texture.height) {
				// ios下，只有长宽相等的图片才使用pvrtc格式
				TextureImporterFormat oldFormat = TextureImporterFormat.PVRTC_RGBA4;
				int maxSize = 1024;
				//textureImporter.GetPlatformTextureSettings("iPhone", out maxSize, out oldFormat);
				//textureImporter.SetPlatformTextureSettings("iPhone", maxSize, TextureImporterFormat.PVRTC_RGBA4, 100);
			}

			//if (flags != 0) {
				ErrorFileInfo info;
				info.fileInfo = fileInfo;
				info.texInfo = textureImporter;
				info.texture = texture;
				info.flags = flags;
				m_ErrorFiles.Add(info);
			//}
		}
		AssetDatabase.Refresh();
	}

	void Check3D(string rootPath) {
		List<string> filePathList = DependencyUtil.FindUsedFiles(rootPath, ".tga");

		for (int i=0; i<filePathList.Count; ++i) {
			string filePath = filePathList[i];
			//Debug.LogError(filePath);
			TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;

			FileInfo fileInfo = new FileInfo(filePath);

			int flags = 0;
			//if (textureImporter.textureType != TextureImporterType.Advanced)
			//	flags |= (int)Flags.Type;
			//if (textureImporter.mipmapEnabled == false)
			//	flags |= (int)Flags.Mipmap;
			if (textureImporter.filterMode == FilterMode.Point)
				flags |= (int)Flags.Filter;
			//if (textureImporter.wrapMode != TextureWrapMode.Clamp)
			//	flags |= (int)Flags.Wrap;
			if (textureImporter.textureFormat == TextureImporterFormat.AutomaticTruecolor ||
			    //textureImporter.textureFormat == TextureImporterFormat.AutomaticCompressed ||
			    textureImporter.textureFormat == TextureImporterFormat.Automatic16bit ||
			    textureImporter.textureFormat == TextureImporterFormat.RGB16 ||
			    textureImporter.textureFormat == TextureImporterFormat.RGBA16)
				flags |= (int)Flags.Format;

			Texture texture = AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture)) as Texture;
			if (texture.width > 1024 || texture.height > 1024)
				flags |= (int)Flags.Size;

			if (fileInfo.Length > texture.width * texture.height * 1.34f + 1000)
				flags |= (int)Flags.FileLength;

			//if (flags != 0) {
			ErrorFileInfo info;
			info.fileInfo = fileInfo;
			info.texInfo = textureImporter;
			info.texture = texture;
			info.flags = flags;
			m_ErrorFiles.Add(info);
			//}
		}
	}

	void AddLabelField(string text, int width, bool errorFlag) {
		GUIStyle textStyle = new GUIStyle(GUI.skin.textField);
		if (errorFlag)
			textStyle.normal.textColor = Color.yellow;
		else
			textStyle.normal.textColor = Color.white;

		EditorGUILayout.LabelField(text, textStyle, GUILayout.Width(width));
	}

	void ShowResult() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.Button ("Texture", GUILayout.Width(200));

		// 标题按钮可以点击排序
		if (GUILayout.Button ("Size", GUILayout.Width(80))) {
			m_SizeOrderAscend = !m_SizeOrderAscend;

			if (m_SizeOrderAscend)
				m_ErrorFiles = (from items in m_ErrorFiles orderby (items.texture.width + items.texture.height) ascending select items).ToList();
			else
				m_ErrorFiles = (from items in m_ErrorFiles orderby (items.texture.width + items.texture.height) descending select items).ToList();
		}
		if (GUILayout.Button ("Type", GUILayout.Width(80))) {
			m_TypeOrderAscend = !m_TypeOrderAscend;

			if (m_TypeOrderAscend)
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.textureType ascending select items).ToList();
			else
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.textureType descending select items).ToList();
		}
		if (GUILayout.Button ("Mipmap", GUILayout.Width(80))) {
			m_MipmapOrderAscend = !m_MipmapOrderAscend;

			if (m_MipmapOrderAscend)
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.mipmapEnabled ascending select items).ToList();
			else
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.mipmapEnabled descending select items).ToList();
		}
		if (GUILayout.Button ("Filter", GUILayout.Width(80))) {
			m_FilterOrderAscend = !m_FilterOrderAscend;

			if (m_FilterOrderAscend)
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.filterMode ascending select items).ToList();
			else
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.filterMode descending select items).ToList();
		}
		if (GUILayout.Button ("Wrap", GUILayout.Width(80))) {
			m_WrapOrderAscend = !m_WrapOrderAscend;

			if (m_WrapOrderAscend)
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.wrapMode ascending select items).ToList();
			else
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.wrapMode descending select items).ToList();
		}
		if (GUILayout.Button ("Format", GUILayout.Width(150))) {
			m_FormatOrderAscend = !m_FormatOrderAscend;

			if (m_FormatOrderAscend)
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.textureFormat ascending select items).ToList();
			else
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.texInfo.textureFormat descending select items).ToList();
		}
		if (GUILayout.Button ("AndroidFormat", GUILayout.Width(150))) {
		}
		if (GUILayout.Button ("iPhoneFormat", GUILayout.Width(150))) {
		}
		if (GUILayout.Button ("FileLength", GUILayout.Width (80))) {
			m_FileSizeOrderAscend = !m_FileSizeOrderAscend;

			if (m_FileSizeOrderAscend)
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.fileInfo.Length ascending select items).ToList();
			else
				m_ErrorFiles = (from items in m_ErrorFiles orderby items.fileInfo.Length descending select items).ToList();
		}
		GUILayout.Button ("NPO", GUILayout.Width(80));
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		int height = Mathf.Min(m_ErrorFiles.Count * 20, Screen.height - 70);
		m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.Width(position.width), GUILayout.Height(height));

		for (int i=0; i<m_ErrorFiles.Count; ++i) {
			ErrorFileInfo info = m_ErrorFiles[i];

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.ObjectField(info.texture, typeof(Texture), GUILayout.Width(200));

			AddLabelField(string.Format("{0}x{1}", info.texture.width, info.texture.height), 80, ((info.flags & (int)Flags.Size) != 0));
			AddLabelField(info.texInfo.textureType.ToString(), 80, ((info.flags & (int)Flags.Type) != 0));
			AddLabelField(info.texInfo.mipmapEnabled.ToString(), 80, ((info.flags & (int)Flags.Mipmap) != 0));
			AddLabelField(info.texInfo.filterMode.ToString(), 80, ((info.flags & (int)Flags.Filter) != 0));
			AddLabelField(info.texInfo.wrapMode.ToString(), 80, ((info.flags & (int)Flags.Wrap) != 0));
			AddLabelField(info.texInfo.textureFormat.ToString(), 150, ((info.flags & (int)Flags.Format) != 0));

			// 显示android下的图片格式
			TextureImporterFormat textureFormatOld = TextureImporterFormat.AutomaticTruecolor;
			int maxTextureSize = 0;
			info.texInfo.GetPlatformTextureSettings("Android", out maxTextureSize, out textureFormatOld);
			AddLabelField(textureFormatOld.ToString(), 150, ((info.flags & (int)Flags.Format) != 0));

			// 显示iOS下的图片格式
			info.texInfo.GetPlatformTextureSettings("iPhone", out maxTextureSize, out textureFormatOld);
			AddLabelField(textureFormatOld.ToString(), 150, ((info.flags & (int)Flags.Format) != 0));

			// 对于压缩纹理，需要重新计算大小
			long fileLength = info.fileInfo.Length;
			if (info.texInfo.textureFormat == TextureImporterFormat.ETC_RGB4)
				fileLength = info.texture.width * info.texture.height * 4 / 8;
			AddLabelField(fileLength.ToString(), 80, (fileLength > 1024*512));
			AddLabelField(info.texInfo.npotScale.ToString(), 80, false);

			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndScrollView();

		EditorGUILayout.EndHorizontal();
	}
}
