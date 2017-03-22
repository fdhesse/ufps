using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver.Demo {
	/// <summary>
	/// This class demonstrates how to use MeshTreeRaycast.
	/// Actually, MeshTreeRaycast is not useful for making bullet marks, because you might need to cast a ray not only against a mesh object but also against character objects.
	/// So, please consult the following code just to understand how to cast rays against a mesh object in background threads.
	/// It might be useful if you can cast rays in background threads especially when you are implementing AI.
	/// For example, you can cast a ray to check whether a NPC can see the player character or not.
	/// </summary>
	[RequireComponent(typeof(MeshRenderer))]
	public class RaycastBulletMarkReceiver : MonoBehaviour {
		public MeshTreeBase m_meshTree;
		public Transform    m_meshTransform;
		public bool         m_allowDelay = true;
		public bool         m_cullBackFace = true;
		public float        m_bulletMarkRadius = 0.05f;

		private Stack<MeshTreeRaycast> m_freeRays;
		private Queue<MeshTreeRaycast> m_activeRays;
		private Transform              m_transform;
		private List<MeshTreeRaycast>  m_hitRays;

		public void AddShot(Vector3 origin, Vector3 dir, float distance)
		{
			MeshTreeRaycast raycast;
			if (0 < m_freeRays.Count) {
				raycast = m_freeRays.Pop();
			}
			else {
				raycast = new MeshTreeRaycast();
			}
			if (m_meshTransform != null) {
				origin = m_meshTransform.InverseTransformPoint(origin);
				dir = m_meshTransform.InverseTransformDirection(dir);
			}
			raycast.AsyncRaycast(m_meshTree, origin, dir, distance, m_cullBackFace);
			m_activeRays.Enqueue(raycast);
		}

		void Awake()
		{
#if !NETFX_CORE
			Nyahoon.ThreadPool.InitInstance(); // required for MeshTreeRaycast.AsyncRaycast()
#endif
			m_freeRays = new Stack<MeshTreeRaycast>();
			m_activeRays = new Queue<MeshTreeRaycast>();
			m_hitRays = new List<MeshTreeRaycast>();
			m_meshData = new Dictionary<int, MeshData>();
			m_transform = transform;
			if (!m_meshTree.IsBuildFinished()) {
				if (m_meshTree.IsPrebuilt()) {
					m_meshTree.BuildFromPrebuiltData();
				}
				else {
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
		
		void LateUpdate()
		{
			m_hitRays.Clear(); // Do not create new List<MeshTreeRaycast> every frame so as not to invoke garbage collections.
			while (0 < m_activeRays.Count) {
				MeshTreeRaycast raycast = m_activeRays.Peek();
				if (!m_allowDelay) {
					raycast.Wait();
				}
				if (raycast.IsDone()) {
					if (raycast.isHit) {
						m_hitRays.Add(raycast);
					}
					m_freeRays.Push(m_activeRays.Dequeue());
				}
				else {
					break;
				}
			}
			if (0 < m_hitRays.Count) {
				AddBulletMarks(m_hitRays);
				SwapMesh();
			}
			m_transform.position = m_meshTransform.position;
			m_transform.rotation = m_meshTransform.rotation;
		}

		struct MeshData {
			public Vector3[] m_vertices;
			public Vector3[] m_normals;
			public Vector2[] m_uvs;
			public int[] m_triangles;
		}
		Dictionary<int, MeshData> m_meshData;
		void AddBulletMarks(List<MeshTreeRaycast> rays)
		{
			MeshData data;
			int vertexCount = 4*rays.Count;
			if (!m_meshData.TryGetValue(vertexCount, out data)) {
				data = new MeshData();
				data.m_vertices = new Vector3[vertexCount];
				data.m_normals = new Vector3[vertexCount];
				data.m_uvs = new Vector2[vertexCount];
				data.m_triangles = new int[6*vertexCount/4];
				for (int i = 0; i < rays.Count; ++i) {
					data.m_uvs[4*i + 0] = new Vector2(0,0);
					data.m_uvs[4*i + 1] = new Vector2(0,1);
					data.m_uvs[4*i + 2] = new Vector2(1,0);
					data.m_uvs[4*i + 3] = new Vector2(1,1);
					data.m_triangles[6*i + 0] = 4*i;
					data.m_triangles[6*i + 1] = 4*i + 1;
					data.m_triangles[6*i + 2] = 4*i + 2;
					data.m_triangles[6*i + 3] = 4*i + 2;
					data.m_triangles[6*i + 4] = 4*i + 1;
					data.m_triangles[6*i + 5] = 4*i + 3;
				}
				m_meshData.Add(vertexCount, data);
			}
			for (int i = 0; i < rays.Count; ++i) {
				Vector3 v = rays[i].hitPosition;
				Vector3 n = rays[i].hitNormal;
				Vector3 up, right;
				if (0.7071f < Mathf.Abs(n.y)) {
					up = m_bulletMarkRadius * Vector3.Cross(n, Vector3.right).normalized;
				}
				else if (Mathf.Abs(n.x) < Mathf.Abs(n.z)) {
					up = m_bulletMarkRadius * Vector3.Cross(n, Vector3.right).normalized;
				}
				else {
					up = m_bulletMarkRadius * Vector3.Cross(n, Vector3.forward).normalized;
				}
				right = Vector3.Cross(up, n);
				data.m_vertices[4*i + 0] = v + up - right;
				data.m_vertices[4*i + 1] = v - up - right;
				data.m_vertices[4*i + 2] = v + up + right;
				data.m_vertices[4*i + 3] = v - up + right;
				data.m_normals[4*i + 0] = n;
				data.m_normals[4*i + 1] = n;
				data.m_normals[4*i + 2] = n;
				data.m_normals[4*i + 3] = n;
			}
			Mesh mesh;
			if (m_meshCombine[0].mesh == null) {
				mesh = nextMesh;
			}
			else {
				mesh = m_tempMesh;
			}
			mesh.Clear();
			mesh.vertices = data.m_vertices;
			mesh.normals = data.m_normals;
			mesh.uv = data.m_uvs;
			mesh.triangles = data.m_triangles;
			if (m_meshCombine[0].mesh != null) {
				m_meshCombine[1].mesh = mesh;
				nextMesh.CombineMeshes(m_meshCombine, true, false);
			}
			m_meshCombine[0].mesh = nextMesh; // now we created a new mesh. so, next time we add bullet mark, it should be combined with nextMesh.
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
