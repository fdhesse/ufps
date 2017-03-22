#if ASYNC_MODE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using LuaInterface;
using UObject = UnityEngine.Object;

public class AssetBundleInfo {
    public AssetBundle m_AssetBundle;
    public int m_ReferencedCount;

    public AssetBundleInfo(AssetBundle assetBundle) {
        m_AssetBundle = assetBundle;
        //m_ReferencedCount = 0;
        m_ReferencedCount = 1;
    }
}

namespace LuaFramework {
#if UNITY_EDITOR
    public static class BundleManager
    {
        static List<UnityEditor.AssetBundleBuild> maps = new List<UnityEditor.AssetBundleBuild>();
        static Dictionary<string, string> mAssetDabase = new Dictionary<string, string>();

        static int m_SimulateAssetBundleInEditor = -1;
        const string kSimulateAssetBundles = "SimulateAssetBundles";

        public static bool SimulateAssetBundleInEditor
        {
            get
            {
                if (m_SimulateAssetBundleInEditor == -1)
                    m_SimulateAssetBundleInEditor = UnityEditor.EditorPrefs.GetBool(kSimulateAssetBundles, true) ? 1 : 0;

                return m_SimulateAssetBundleInEditor != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != m_SimulateAssetBundleInEditor)
                {
                    m_SimulateAssetBundleInEditor = newValue;
                    UnityEditor.EditorPrefs.SetBool(kSimulateAssetBundles, value);
                }
            }
        }

        private static string AppDataPath
        {
            get { return Application.dataPath.ToLower(); }
        }

        private static void HandleExampleBundle()
        {
            string resPath = AppDataPath + "/" + AppConst.AssetDir + "/";
            if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);

            //AddBuildMap("prompt" + AppConst.ExtName, "*.prefab", "Assets/Packages/LuaFramework/Examples/Builds/Prompt");
            //AddBuildMap("message" + AppConst.ExtName, "*.prefab", "Assets/Packages/LuaFramework/Examples/Builds/Message");

            AddBuildMap("prompt_asset" + AppConst.ExtName, "*.png", "Assets/Packages/LuaFramework/Examples/Textures/Prompt");

            //AddBuildMap("shared_asset" + AppConst.ExtName, "*.png", "Assets/LuaFramework/Examples/Textures/Shared");

            AddBuildMap("ui" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/");
            //AddBuildMap("main" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/UI/Main");
            AddBuildMap("login" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/Login");
            AddBuildMap("combat" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/Combat");
            //AddBuildMap("player" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/Player");
            AddBuildMap("mainmenu" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/MainMenu");
            AddBuildMap("inventory" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/Inventory");
            AddBuildMap("inventorydetailinfo" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/InventoryDetailInfo");
            AddBuildMap("worldmap" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/WorldMap");
            AddBuildMap("message" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/Message");
            AddBuildMap("loading" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/Loading");
            AddBuildMap("lobbysceneui" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/LobbySceneUI");
            AddBuildMap("backpack" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/BackPack");
            AddBuildMap("heroattribute" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/HeroAttribute");
            AddBuildMap("lobbyloading" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/UI/LobbyLoading");

            //AddBuildMap("weaponui" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Geometry/Textures/UI/Weapon/BulletHud");
            //AddBuildMap("weaponui" + AppConst.ExtName, "*.png", "Assets/IntenseTPS/Geometry/Textures/Fx Textures");
            //AddBuildMap("materials" + AppConst.ExtName, "*.mat", "Assets/IntenseTPS/Geometry/Materials/Fx Materials");
            //AddBuildMap("weaponbase" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/Weapon/Other");
            //AddBuildMap("weaponbase" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/Weapon/Projectiles");

            //AddBuildMap("weaponbase" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/Weapon/MuzzleFlashes");
            //AddBuildMap("weapons" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/Weapon/Weapons/Rpg");
            //AddBuildMap("weapons" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/Weapon/Weapons/Pistol");
            //AddBuildMap("weapons2" + AppConst.ExtName, "*.prefab", "Assets/IntenseTPS/Prefabs/Weapon/Weapons/Ak47");

            //AddBuildMap("pvpscenes" + AppConst.ExtName, "pvp.unity", "Assets/Scenes/");
            AddBuildMap("pvpscenes" + AppConst.ExtName, "TWD_pvp_warehouse.unity", "Assets/Scenes/TWD_pvp_warehouse");
            AddBuildMap("pvescenes" + AppConst.ExtName, "TWD_coop_warehouse.unity", "Assets/Scenes/TWD_coop_warehouse");
            //AddBuildMap("scenes" + AppConst.ExtName, "PvpServer.unity", "Assets/Scenes/");
            AddBuildMap("lobbyscenes" + AppConst.ExtName, "lobby.unity", "Assets/Scenes/");

            AddBuildMap("monsters" + AppConst.ExtName, "*.prefab", "Assets/Prefabs/Gameplay/AI");
        }

        private static void AddBuildMap(string bundleName, string pattern, string path)
        {
            string[] files = Directory.GetFiles(path, pattern);
            if (files.Length == 0) return;

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace('\\', '/');
            }
            UnityEditor.AssetBundleBuild build = new UnityEditor.AssetBundleBuild();
            build.assetBundleName = bundleName;
            build.assetNames = files;
            maps.Add(build);

            foreach (var file in files)
            {
                //UnityEngine.Debug.Log(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var key = bundleName + "_" + fileName;
                AddAsset(key, file);
            }
        }

        private static void AddAsset(string abName, string path)
        {
            mAssetDabase.Add(abName, path);
        }

        private static void ClearAssetDatabase()
        {
            mAssetDabase.Clear();
            maps.Clear();
        }

        public static string[] GetAssetPath(string abName, string[] assetNames)
        {
            abName = abName + AppConst.ExtName;

            List<string> paths = new List<string>();
            foreach (var assetName in assetNames)
            {
                string path;
                string key = abName + "_" + assetName;
                if (mAssetDabase.TryGetValue(key, out path))
                {
                    paths.Add(path);
                }
                else
                {
                    Debug.LogError("There is no asset with name \"" + assetName + "\" in " + abName);
                }
            }
            return paths.ToArray();
        }

        public static void DefineBundles()
        {
            ClearAssetDatabase();
            HandleExampleBundle();
        }

        public static void BuildBundles(UnityEditor.BuildTarget target)
        {
            string resPath = "Assets/" + AppConst.AssetDir;
            UnityEditor.BuildAssetBundleOptions options = UnityEditor.BuildAssetBundleOptions.DeterministicAssetBundle
                                             | UnityEditor.BuildAssetBundleOptions.ChunkBasedCompression;
            //|  BuildAssetBundleOptions.UncompressedAssetBundle;
            UnityEditor.BuildPipeline.BuildAssetBundles(resPath, maps.ToArray(), options, target);
        }
    }

#endif
    public class ResourceManager : Manager {

        string m_BaseDownloadingURL = "";
        string[] m_AllManifest = null;
        AssetBundleManifest m_AssetBundleManifest = null;
        Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>>();

        class LoadAssetRequest {
            public Type assetType;
            public string[] assetNames;
            public LuaFunction luaFunc;
            public Action<UObject[]> sharpFunc;
        }

#if UNITY_EDITOR
        private static void LoadAssetInEditor<T>(string abName, string[] assetNames, Action<UObject[]> action = null, LuaFunction func = null) where T : UnityEngine.Object
        {
            string[] assetPaths = BundleManager.GetAssetPath(abName, assetNames);
            if (assetPaths.Length == 0)
            {
                Debug.LogError("There is no asset with name \"" + assetNames + "\" in " + abName);
                return;
            }

            List<UObject> result = new List<UObject>();
            // @TODO: Now we only get the main object from the first asset. Should consider type also.
            foreach (var path in assetPaths)
            {
                var target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                result.Add(target);
            }

            if (action != null)
            {
                action(result.ToArray());
            }
            if (func != null)
            {
                func.Call((object)result.ToArray());
                func.Dispose();
            }

            for (int i = 0; i < result.Count; ++i)
            {
                Debug.Log("unload " + abName + assetNames[i]);
                //Resources.UnloadAsset(result[i]);
                //GameObject.Destroy(result[i]);
            }
            Resources.UnloadUnusedAssets();
        }
#endif
        // Load AssetBundleManifest.
        public void Initialize(string manifestName, Action initOK) {
            m_BaseDownloadingURL = Util.GetRelativePath();

#if UNITY_EDITOR
            if (BundleManager.SimulateAssetBundleInEditor)
            {
                if (initOK != null) initOK();
                return;
            }
#endif
            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate(UObject[] objs) {
                if (objs.Length > 0) {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                    m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();
                }
                if (initOK != null) initOK();
            });
        }

        public void LoadLevel(string abName, string assetName, LuaFunction stepFunc, LuaFunction endFunc)
        {
#if UNITY_EDITOR
            if (BundleManager.SimulateAssetBundleInEditor)
            {
                var levelPaths = BundleManager.GetAssetPath(abName, new string[] { assetName });
                var op = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPaths[0]);
                StartCoroutine(_LoadLevel(op, stepFunc, endFunc));
            }
            else
#endif
            {
                LoadAsset<GameObject>(abName, new string[] { assetName }, (ob) =>
                {
                    var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(assetName);
                    StartCoroutine(_LoadLevel(op, stepFunc, endFunc));
                },
                null,
                true
                );
            }
        }

        private System.Collections.IEnumerator _LoadLevel(AsyncOperation op, LuaFunction stepFunc, LuaFunction endFunc)
        {
            while (!op.isDone)
            {
                if (stepFunc != null) stepFunc.Call(op.progress);
                yield return new WaitForEndOfFrame();
            }
            if (endFunc != null) endFunc.Call();
        }

        public void LoadPrefab(string abName, string assetName, Action<UObject[]> func) {
            LoadAsset<GameObject>(abName, new string[] { assetName }, func);
        }

        public void LoadPrefab(string abName, string[] assetNames, Action<UObject[]> func) {
            LoadAsset<GameObject>(abName, assetNames, func);
        }

        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func) {
            LoadAsset<GameObject>(abName, assetNames, null, func);
        }

        string GetRealAssetPath(string abName) {
            if (abName.Equals(AppConst.AssetDir)) {
                return abName;
            }
            abName = abName.ToLower();
            if (!abName.EndsWith(AppConst.ExtName)) {
                abName += AppConst.ExtName;
            }
            if (abName.Contains("/")) {
                return abName;
            }
            //string[] paths = m_AssetBundleManifest.GetAllAssetBundles();  产生GC，需要缓存结果
            for (int i = 0; i < m_AllManifest.Length; i++) {
                int index = m_AllManifest[i].LastIndexOf('/');  
                string path = m_AllManifest[i].Remove(0, index + 1);    //字符串操作函数都会产生GC
                if (path.Equals(abName)) {
                    return m_AllManifest[i];
                }
            }
            Debug.LogError("GetRealAssetPath Error:>>" + abName);
            return null;
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        void LoadAsset<T>(string abName, string[] assetNames, Action<UObject[]> action = null, LuaFunction func = null, bool isScene = false) where T : UObject {
#if UNITY_EDITOR
            if (BundleManager.SimulateAssetBundleInEditor)
            {
                LoadAssetInEditor<T>(abName, assetNames, action, func);
                return;
            }
#endif

            abName = GetRealAssetPath(abName);

            LoadAssetRequest request = new LoadAssetRequest();
            request.assetType = typeof(T);
            request.assetNames = assetNames;
            request.luaFunc = func;
            request.sharpFunc = action;

            List<LoadAssetRequest> requests = null;
            if (!m_LoadRequests.TryGetValue(abName, out requests)) {
                requests = new List<LoadAssetRequest>();
                requests.Add(request);
                m_LoadRequests.Add(abName, requests);
                StartCoroutine(OnLoadAsset<T>(abName, isScene));
            } else {
                requests.Add(request);
            }
        }

        IEnumerator OnLoadAsset<T>(string abName, bool isScene) where T : UObject {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
            if (bundleInfo == null) {
                yield return StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));

                bundleInfo = GetLoadedAssetBundle(abName);
                if (bundleInfo == null) {
                    m_LoadRequests.Remove(abName);
                    Debug.LogError("OnLoadAsset--->>>" + abName);
                    yield break;
                }
            }
            List<LoadAssetRequest> list = null;
            if (!m_LoadRequests.TryGetValue(abName, out list)) {
                m_LoadRequests.Remove(abName);
                yield break;
            }
            for (int i = 0; i < list.Count; i++) {
                string[] assetNames = list[i].assetNames;
                List<UObject> result = new List<UObject>();

                AssetBundle ab = bundleInfo.m_AssetBundle;
                if (!isScene)
                {
                    for (int j = 0; j < assetNames.Length; j++)
                    {
                        string assetPath = assetNames[j];
                        AssetBundleRequest request = ab.LoadAssetAsync(assetPath, list[i].assetType);
                        yield return request;
                        result.Add(request.asset);

                        //T assetObj = ab.LoadAsset<T>(assetPath);
                        //result.Add(assetObj);
                    }
                }
                //try
                {
                    if (list[i].sharpFunc != null)
                    {
                        list[i].sharpFunc(result.ToArray());
                        list[i].sharpFunc = null;
                    }
                    if (list[i].luaFunc != null)
                    {
                        list[i].luaFunc.Call((object)result.ToArray());
                        list[i].luaFunc.Dispose();
                        list[i].luaFunc = null;
                    }
                }
                //catch(Exception ex)
                {
                   // Debug.Log(ex.Message);
                }
                

                //this not considered dependencies assetbundle and will cause m_ReferencedCount error
                //bundleInfo.m_ReferencedCount++;
                //Debug.Log("assetbundle loaded:" + abName);
            }
            m_LoadRequests.Remove(abName);

            if (!isScene) UnloadAssetBundle(abName, false);
        }

        IEnumerator OnLoadAssetBundle(string abName, Type type) {
            string url = m_BaseDownloadingURL + abName;

            AssetBundleCreateRequest request;
            //WWW download = null;
            if (type == typeof(AssetBundleManifest))
                //download = new WWW(url);
                request = AssetBundle.LoadFromFileAsync(url);
            else {
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                if (dependencies.Length > 0) {
                    m_Dependencies.Add(abName, dependencies);
                    for (int i = 0; i < dependencies.Length; i++) {
                        string depName = dependencies[i];
                        AssetBundleInfo bundleInfo = null;
                        if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo)) {
                            bundleInfo.m_ReferencedCount++;
                        } else if (!m_LoadRequests.ContainsKey(depName)) {
                            yield return StartCoroutine(OnLoadAssetBundle(depName, type));
                        }
                    }
                }
                //download = WWW.LoadFromCacheOrDownload(url, m_AssetBundleManifest.GetAssetBundleHash(abName), 0);
                request = AssetBundle.LoadFromFileAsync(url);
            }
            yield return request;

            AssetBundle assetObj = request.assetBundle;
            if (assetObj != null) {
                m_LoadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj));
                Debug.Log("assetbundle loaded:" + abName);
            }
        }

        AssetBundleInfo GetLoadedAssetBundle(string abName) {
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
            if (bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies) {
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null) return null;
            }
            return bundle;
        }

        /// <summary>
        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnloadAssetBundle(string abName, bool isThorough = false) {
#if UNITY_EDITOR
            if (BundleManager.SimulateAssetBundleInEditor)
                return;
#endif
            abName = GetRealAssetPath(abName);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
            UnloadAssetBundleInternal(abName, isThorough);
            UnloadDependencies(abName, isThorough);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);

            foreach(var ab in m_LoadedAssetBundles)
            {
                Debug.Log("left asset bundle:" + ab.Key);
            }
        }

        void UnloadDependencies(string abName, bool isThorough) {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies) {
                UnloadAssetBundleInternal(dependency, isThorough);
            }
            m_Dependencies.Remove(abName);
        }

        void UnloadAssetBundleInternal(string abName, bool isThorough) {
            AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
            if (bundle == null) return;

            if (--bundle.m_ReferencedCount <= 0) {
                if (m_LoadRequests.ContainsKey(abName)) {
                    return;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
                }
                bundle.m_AssetBundle.Unload(isThorough);
                m_LoadedAssetBundles.Remove(abName);
                Debug.Log(abName + " has been unloaded successfully");
            }
        }
    }
}
#else

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LuaFramework;
using LuaInterface;
using UObject = UnityEngine.Object;

namespace LuaFramework {
    public class ResourceManager : Manager {
        private string[] m_Variants = { };
        private AssetBundleManifest manifest;
        private AssetBundle shared, assetbundle;
        private Dictionary<string, AssetBundle> bundles;

        void Awake() {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize() {
            byte[] stream = null;
            string uri = string.Empty;
            bundles = new Dictionary<string, AssetBundle>();
            uri = Util.DataPath + AppConst.AssetDir;
            if (!File.Exists(uri)) return;
            stream = File.ReadAllBytes(uri);
            assetbundle = AssetBundle.LoadFromMemory(stream);
            manifest = assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        public T LoadAsset<T>(string abname, string assetname) where T : UnityEngine.Object {
            abname = abname.ToLower();
            AssetBundle bundle = LoadAssetBundle(abname);
            return bundle.LoadAsset<T>(assetname);
        }

        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func) {
            abName = abName.ToLower();
            List<UObject> result = new List<UObject>();
            for (int i = 0; i < assetNames.Length; i++) {
                UObject go = LoadAsset<UObject>(abName, assetNames[i]);
                if (go != null) result.Add(go);
            }
            if (func != null) func.Call((object)result.ToArray());
        }

        /// <summary>
        /// 载入AssetBundle
        /// </summary>
        /// <param name="abname"></param>
        /// <returns></returns>
        public AssetBundle LoadAssetBundle(string abname) {
            if (!abname.EndsWith(AppConst.ExtName)) {
                abname += AppConst.ExtName;
            }
            AssetBundle bundle = null;
            if (!bundles.ContainsKey(abname)) {
                byte[] stream = null;
                string uri = Util.DataPath + abname;
                Debug.LogWarning("LoadFile::>> " + uri);
                LoadDependencies(abname);

                stream = File.ReadAllBytes(uri);
                bundle = AssetBundle.LoadFromMemory(stream); //关联数据的素材绑定
                bundles.Add(abname, bundle);
            } else {
                bundles.TryGetValue(abname, out bundle);
            }
            return bundle;
        }

        /// <summary>
        /// 载入依赖
        /// </summary>
        /// <param name="name"></param>
        void LoadDependencies(string name) {
            if (manifest == null) {
                Debug.LogError("Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }
            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = manifest.GetAllDependencies(name);
            if (dependencies.Length == 0) return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            for (int i = 0; i < dependencies.Length; i++) {
                LoadAssetBundle(dependencies[i]);
            }
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        string RemapVariantName(string assetBundleName) {
            string[] bundlesWithVariant = manifest.GetAllAssetBundlesWithVariant();

            // If the asset bundle doesn't have variant, simply return.
            if (System.Array.IndexOf(bundlesWithVariant, assetBundleName) < 0)
                return assetBundleName;

            string[] split = assetBundleName.Split('.');

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < bundlesWithVariant.Length; i++) {
                string[] curSplit = bundlesWithVariant[i].Split('.');
                if (curSplit[0] != split[0])
                    continue;

                int found = System.Array.IndexOf(m_Variants, curSplit[1]);
                if (found != -1 && found < bestFit) {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }
            if (bestFitIndex != -1)
                return bundlesWithVariant[bestFitIndex];
            else
                return assetBundleName;
        }

        /// <summary>
        /// 销毁资源
        /// </summary>
        void OnDestroy() {
            if (shared != null) shared.Unload(true);
            if (manifest != null) manifest = null;
            Debug.Log("~ResourceManager was destroy!");
        }
    }
}
#endif
