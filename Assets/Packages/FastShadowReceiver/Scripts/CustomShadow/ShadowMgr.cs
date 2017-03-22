using UnityEngine;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FastShadowReceiver
{  
	/// <summary>
	///
	/// </summary>
	[RequireComponent(typeof(MeshRenderer))]
	public class ShadowMgr: MonoBehaviour {
		public MeshTreeBase m_meshTree;
		public Transform    m_meshTransform;
		public float        m_scissorMargin = 0.0f;
		public bool         m_allowDelay = true;

		class SearchInstance {
			public MeshTreeSearch search;
			public Matrix4x4      uvProjection;
            public Vector4 tileParams;
            public float lightScale;
            public float brightness;
		}
		private Stack<SearchInstance> m_freeSearch;
		private Queue<SearchInstance> m_activeSearch;
		private Transform             m_transform;        


        private void _AddShot(IProjector bulletProjector, Vector3 pos, Vector3 dir, Vector4 tileParams, float lightScale, float brightness)
        {
            m_transform.position = m_meshTransform.position;
            m_transform.rotation = m_meshTransform.rotation;

            SearchInstance searchInstance;
            if (m_freeSearch.Count > 0)
            {
                searchInstance = m_freeSearch.Pop();
            }
            else
            {
                searchInstance = new SearchInstance();
                searchInstance.search = m_meshTree.CreateSearch();
                searchInstance.search.m_bBackfaceCulling = true;
                searchInstance.search.m_bScissor = true;
                searchInstance.search.m_bOutputNormals = true;
            }
            bulletProjector.GetClipPlanes(ref searchInstance.search.m_clipPlanes, m_meshTransform);

            // Usually, far clip plane is not used for scissoring. Scissoring by far clip plane will just waste CPU time.
            // To remove far clip plane from scissoring, "scissorPlaneCount" is less than "clipPlaneCount".
            // However, in this case, far clip plane is not so far. It is better to scissor polygons by far clip plane.
            // Also, if m_allowDelay is true, we don't need to care about CPU time.
            searchInstance.search.m_clipPlanes.scissorPlaneCount = searchInstance.search.m_clipPlanes.clipPlaneCount;

            searchInstance.search.m_scissorMargin = m_scissorMargin;



            searchInstance.search.SetProjectionDir(bulletProjector.isOrthographic, m_meshTransform.InverseTransformDirection(dir), m_meshTransform.InverseTransformPoint(pos));
            searchInstance.search.AsyncStart(m_meshTree);
            searchInstance.uvProjection = bulletProjector.uvProjectionMatrix;

            searchInstance.tileParams = tileParams;
            searchInstance.lightScale = lightScale;
            searchInstance.brightness = brightness;
#if UNITY_EDITOR
            searchInstance.search.Wait();
            if (searchInstance.search.IsDone())
            {
                if (AddBulletMark(searchInstance))
                {
                    SwapMesh();
                }
                m_freeSearch.Push(searchInstance);
            }           
#else
            m_activeSearch.Enqueue(searchInstance);            
#endif
        }

        public void Rebuild(IProjector bulletProjector)
        {            
            if (!m_meshTree.IsBuildFinished())
            {
                m_meshTree.WaitForBuild();
            }
            if (m_activeSearch == null) 
                Init();

            while (m_activeSearch.Count > 0)
            {
                Debug.LogWarning("m_activeSearch.Count is not zero!");
                return;
            }
            
            InitMesh();
            GameObject[] decalObjs = GameObject.FindGameObjectsWithTag("DecalObj");
            if (decalObjs == null || decalObjs.Length == 0)
            {
                Debug.LogWarning("DecalObj not found!");                
            }
            

            for (int i = 0; i < decalObjs.Length; i++ )
            {
                GameObject obj = decalObjs[i];

                DecalObj decalObj = new DecalObj();
                decalObj.mPos = obj.transform.position;
                decalObj.mRot = obj.transform.rotation;
                DecalParam param = obj.GetComponent<DecalParam>();
                decalObj.mTileParams = param.TileParams;
                decalObj.mCustomProjector = param.mCustomProjector;
                decalObj.mLightScale = param.LightScale;
                decalObj.mBrightness = param.Brightness;
#if UNITY_EDITOR                
                decalObj.mObj = obj;
                if (EditorApplication.isPlaying)
                {
                    obj.SetActive(false);
                }
#else
                obj.SetActive(false);
#endif
                
            }

            BuildDecals(bulletProjector);
        }

        void BuildDecals(IProjector bulletProjector)
        {
            CustomProjector projector = (CustomProjector)bulletProjector;

            //projector.transform.position = obj.mObj.transform.position;
            //projector.transform.rotation = obj.mObj.transform.rotation;


            //_AddShot(projector, projector.position, projector.direction, obj.mTileParams, obj.mLightScale, obj.mBrightness);
            
        }

		void Awake()
		{
            Init();
            SceneManager.sceneLoaded += OnSceneLoaded;
		}

        void Init()
        {
#if !NETFX_CORE
            Nyahoon.ThreadPool.InitInstance(); // required for MeshTreeSearch.AsyncStart()
#endif
            m_freeSearch = new Stack<SearchInstance>();
            m_activeSearch = new Queue<SearchInstance>();
            m_transform = transform;
            if (!m_meshTree.IsBuildFinished())
            {
                if (m_meshTree.IsPrebuilt())
                {
                    m_meshTree.BuildFromPrebuiltData();
                }
                else
                {
                    m_meshTree.AsyncBuild();
                }
            }
            InitMesh();
        }

		void Start()
		{
			if (!m_meshTree.IsBuildFinished()) {
				m_meshTree.WaitForBuild();
			}           
		}

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {            
            //StartCoroutine(DelayBuildDecals());
            //RebuildDecal();
        }

		void LateUpdate()
		{
			bool bAdded = false;
			while (0 < m_activeSearch.Count) {
				SearchInstance searchInstance = m_activeSearch.Peek();
				if (!m_allowDelay) {
					searchInstance.search.Wait();
				}
				if (searchInstance.search.IsDone()) {
					if (AddBulletMark(searchInstance)) {
						bAdded = true;
                        SwapMesh();
					}
					m_freeSearch.Push(m_activeSearch.Dequeue());
				}
				else {
					break;
				}
			}
			if (bAdded) {
//				SwapMesh();
			}
			m_transform.position = m_meshTransform.position;
			m_transform.rotation = m_meshTransform.rotation;
		}
		struct UVBuffer {
			public Vector2[] uv1;
			public Vector2[] uv2;
            public Vector2[] uv3;
            public Vector2[] uv4;
            public Color[] colors;
		}
		static Dictionary<int, UVBuffer> s_uvBuffer = new Dictionary<int, UVBuffer>();
		bool AddBulletMark(SearchInstance searchInstance)
		{
			Vector3[] vertices = searchInstance.search.vertices;
			if (vertices == null) {
				return false;
			}
			int vertexCount = vertices.Length;
			if (vertexCount == 0) {
				return false;
			}
			UVBuffer buffer;
			if (!s_uvBuffer.TryGetValue(vertexCount, out buffer)) {
				buffer = new UVBuffer();
				buffer.uv1 = new Vector2[vertexCount];
				buffer.uv2 = new Vector2[vertexCount];
                buffer.uv3 = new Vector2[vertexCount];
                buffer.uv4 = new Vector2[vertexCount];
                buffer.colors = new Color[vertexCount];
				s_uvBuffer.Add(vertexCount, buffer);
			}
			Matrix4x4 mat = searchInstance.uvProjection * m_meshTransform.localToWorldMatrix;
			for (int i = 0; i < vertexCount; ++i) {
				Vector4 v = vertices[i]; v.w = 1.0f;
				v = mat * v;
				buffer.uv1[i].x = v.x;
				buffer.uv1[i].y = v.y;
				buffer.uv2[i].x = v.z;
				buffer.uv2[i].y = v.w;
                buffer.uv3[i].x = searchInstance.tileParams.x;
                buffer.uv3[i].y = searchInstance.tileParams.y;
                buffer.uv4[i].x = searchInstance.tileParams.z;
                buffer.uv4[i].y = searchInstance.tileParams.w;
                buffer.colors[i].r = searchInstance.brightness;//0.2f; //brightness
                buffer.colors[i].g = searchInstance.lightScale * 0.01f;//5.0f * 0.01f; //lightscale
			}
			Mesh mesh;
			if (m_meshCombine[0].mesh == null) {
				mesh = nextMesh;
			}
			else {
				mesh = m_tempMesh;
			}
			mesh.Clear();
			mesh.vertices = vertices;
			mesh.normals = searchInstance.search.normals;
			mesh.uv = buffer.uv1;
			mesh.uv2 = buffer.uv2;
            mesh.uv3 = buffer.uv3;
            mesh.uv4 = buffer.uv4;
            mesh.colors = buffer.colors;

			mesh.triangles = searchInstance.search.triangles;
			if (m_meshCombine[0].mesh != null) {
				m_meshCombine[1].mesh = mesh;
				nextMesh.CombineMeshes(m_meshCombine, true, false);
			}
			m_meshCombine[0].mesh = nextMesh; // now we created a new mesh. so, next time we add bullet mark, it should be combined with nextMesh.
			return true;
		}

		// for mesh rendering
		const int BUFFER_COUNT = 2;
		private int m_nCurrentBuffer;
		private Mesh[] m_meshes;
		private Mesh m_tempMesh;
		private MeshFilter m_meshFilter;
		private Renderer m_renderer;
		private CombineInstance[] m_meshCombine;

		void InitMesh()
		{
			m_meshes = new Mesh[BUFFER_COUNT];
			for (int i = 0; i < BUFFER_COUNT; ++i) {
				m_meshes[i] = new Mesh();
			}
			m_tempMesh = new Mesh();
			m_nCurrentBuffer = 0;
			m_meshFilter = GetComponent<MeshFilter>();
			if (m_meshFilter == null) {
				m_meshFilter = gameObject.AddComponent<MeshFilter>();
			}
			m_meshFilter.mesh = m_meshes[0];
			m_meshCombine = new CombineInstance[2];
			m_meshCombine[0].mesh = null;
			m_meshCombine[0].subMeshIndex = 0;
			m_meshCombine[1].subMeshIndex = 0;
		}

		Mesh currentMesh
		{
			get { return m_meshes[m_nCurrentBuffer]; }
		}

		Mesh nextMesh
		{
			get { return m_meshes[(m_nCurrentBuffer + 1) % BUFFER_COUNT]; }
		}

		void SwapMesh()
		{
			m_nCurrentBuffer = (m_nCurrentBuffer + 1) % BUFFER_COUNT;
			m_meshFilter.mesh = currentMesh;
		}
	}
}
