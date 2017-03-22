//
// ProjectorManager.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver {
	[RequireComponent(typeof(MeshRenderer))]
	public class ProjectorManager : MonoBehaviour {
		// singleton instance
		static ProjectorManager s_instance = null;
		public static ProjectorManager Instance
		{
			get { return s_instance; }
		}
		public ProjectorManager()
		{
			s_instance = this;
			if (m_receivers == null) {
				m_receivers = new List<ReceiverBase>();
			}
			if (m_projectors == null) {
				m_projectors = new List<AutoProjector>();
			}
			m_receiverInstances = new List<List<ReceiverBase>>();
			m_freeReceiverInstances = new Stack<List<ReceiverBase>>();
		}

		// serialize field
		[SerializeField]
		private LayerMask   m_receiverLayerMask;
		[SerializeField]
		private LayerMask   m_environmentLayers = 0;
		[SerializeField]
		private List<ReceiverBase> m_receivers;
		[SerializeField]
		private Camera      m_mainCamera;
		[SerializeField]
		private float       m_projectorFadeoutDistance = 20.0f;
		[SerializeField]
		private float       m_fadeDuration = 0.5f;
		[SerializeField]
		private LayerMask   m_raycastPlaneMask = 0;
		[SerializeField]
		private bool        m_useInfinitePlane = false;
		[SerializeField]
		private Transform   m_infinitePlaneTransform;
		[SerializeField]
		private Vector3     m_infinitePlaneNormal = Vector3.up;
		[SerializeField]
		private float       m_infinitePlaneHeight = 0.0f;
		[SerializeField]
		private Texture2D[] m_blobShadowTextures;
		[SerializeField]
		private Texture2D   m_packedBlobShadowTexture;
		[SerializeField]
		private int         m_packedTexturePadding;
		[SerializeField]
		private int         m_packedTextureMaxSize = 1024;
		[SerializeField]
		private Rect[]      m_blobShadowTextureRects;
		[SerializeField]
		private string      m_shadowTexName = "_ShadowTex";
		[SerializeField]
		private bool        m_manualUpdate = false;

		// public properties
		public LayerMask receiverLayerMask
		{
			get { return m_receiverLayerMask; }
			set {
				if (m_receiverLayerMask.value != value.value) {
					m_receiverLayerMask = value;
					UpdateReceiverLayer();
				}
			}
		}
		public LayerMask raycastPlaneMask
		{
			get { return m_raycastPlaneMask; }
			set { m_raycastPlaneMask = value; }
		}
		public List<ReceiverBase> receivers
		{
			get { return m_receivers; }
		}
		public LayerMask environmentLayers
		{
			get { return m_environmentLayers; }
			set { m_environmentLayers = value; }
		}
		public bool useInfinitPlane
		{
			get { return m_useInfinitePlane; }
			set { m_useInfinitePlane = value; }
		}
		public Transform infinitPlaneTransform
		{
			get { return m_infinitePlaneTransform; }
			set { m_infinitePlaneTransform = value; }
		}
		public Vector3 infinitePlaneNormal
		{
			get { return m_infinitePlaneNormal; }
			set { m_infinitePlaneNormal = value; }
		}
		public float infinitePlaneHeight
		{
			get { return m_infinitePlaneHeight; }
			set { m_infinitePlaneHeight = value; }
		}
		public Camera mainCamera
		{
			get { return m_mainCamera; }
			set { m_mainCamera = value; }
		}
		public float projectorFadeoutDistance
		{
			get { return m_projectorFadeoutDistance; }
			set { m_projectorFadeoutDistance = value; }
		}
		public float fadeDuration
		{
			get { return m_fadeDuration; }
			set { m_fadeDuration = value; }
		}
		public bool manualUpdate
		{
			get { return m_manualUpdate; }
			set { m_manualUpdate = value; }
		}
		public Texture2D[] blobShadowTextures
		{
			get { return m_blobShadowTextures; }
		}
		public Texture2D packedBlobShadowTexture
		{
			get { return m_packedBlobShadowTexture; }
		}
		public Rect[] blobShadowTextureRects
		{
			get { return m_blobShadowTextureRects; }
		}
		public string blobShadowTextureName
		{
			get { return m_shadowTexName; }
		}
		public int projectorCount
		{
			get { return m_projectors.Count; }
		}
		public int activeProjectorCount
		{
			get {
				int count = 0;
				for (int i = 0; i < m_projectors.Count; ++i) {
					if (m_projectors[i].projector.projector.enabled) {
						++count;
					}
				}
				return count;
			}
		}
		// public methods
		public void AddProjector(AutoProjector projector)
		{
			if (m_projectors.Contains(projector)) {
				return;
			}
			m_projectors.Add(projector);
			projector.uvIndex = 0;
			UpdateUVIndex(projector);
			projector.projector.projector.ignoreLayers |= m_environmentLayers;
		}
		public void UpdateUVIndex(AutoProjector projector)
		{
			if (m_blobShadowTextures != null && 1 < m_blobShadowTextures.Length && projector.projector.projector.material.HasProperty(m_shadowTexName)) {
				Texture2D tex = projector.projector.projector.material.GetTexture(m_shadowTexName) as Texture2D;
				for (int i = 0; i < m_blobShadowTextures.Length; ++i) {
					if (tex == m_blobShadowTextures[i]) {
						projector.uvIndex = i;
						break;
					}
				}
			}
		}
		public void RemoveProjector(AutoProjector projector)
		{
			List<ReceiverBase> receivers = projector.GetReceivers();
			if (receivers != null) {
				projector.ClearReceivers();
				FreeReceivers(receivers);
			}
			m_projectors.Remove(projector);
		}
		public void AddReceiver(ReceiverBase receiver)
		{
			if (m_receivers.Contains(receiver)) {
				return;
			}
			receiver.gameObject.SetActive(false);
			m_receivers.Add(receiver);
			for (int i = 0; i < m_receiverInstances.Count; ++i) {
				ReceiverBase clone = Instantiate(receiver) as ReceiverBase;
				clone.transform.parent = transform;
				if (0 < m_receiverInstances[i].Count) {
					clone.gameObject.layer = m_receiverInstances[i][0].gameObject.layer;
					clone.gameObject.SetActive(m_receiverInstances[i][0].gameObject.activeSelf);
				}
				else {
					clone.gameObject.SetActive(false);
				}
				m_receiverInstances[i].Add(clone);
			}
		}
		public void RemoveReceiver(ReceiverBase receiver)
		{
			int index = m_receivers.IndexOf(receiver);
			if (0 <= index) {
				m_receivers.RemoveAt(index);
			}
			for (int i = 0; i < m_receiverInstances.Count; ++i) {
				Destroy(m_receiverInstances[i][index].gameObject);
				m_receiverInstances[i].RemoveAt(index);
			}
		}

		public void UpdateScene()
		{
			// setup camera clip planes
			Vector3 cameraZ = m_cameraTransform.forward;
			Vector3 cameraPos = m_cameraTransform.position;
			Plane cameraZPlane = new Plane(cameraZ, cameraPos);
			Vector3 cameraX = m_cameraTransform.right;
			Vector3 cameraY = m_cameraTransform.up;
			if (m_mainCamera.orthographic) {
				float ySize = m_mainCamera.orthographicSize;
				float xSize = m_mainCamera.aspect * ySize;
				m_cameraClipPlanes[0] = new Plane(cameraX, cameraPos);
				m_cameraClipPlanes[0].distance += xSize;
				m_cameraClipPlanes[1] = new Plane(-cameraX, cameraPos);
				m_cameraClipPlanes[1].distance += xSize;
				m_cameraClipPlanes[2] = new Plane(cameraY, cameraPos);
				m_cameraClipPlanes[2].distance += ySize;
				m_cameraClipPlanes[3] = new Plane(-cameraY, cameraPos);
				m_cameraClipPlanes[3].distance += ySize;
			}
			else {
				float ySize = Mathf.Tan(0.5f*Mathf.Deg2Rad*m_mainCamera.fieldOfView);
				float xSize = m_mainCamera.aspect * ySize;
				Vector3 x0 = (cameraX + xSize*cameraZ).normalized;
				Vector3 x1 = (-cameraX + xSize*cameraZ).normalized;
				Vector3 y0 = (cameraY + ySize*cameraZ).normalized;
				Vector3 y1 = (-cameraY + ySize*cameraZ).normalized;
				m_cameraClipPlanes[0] = new Plane(x0, cameraPos);
				m_cameraClipPlanes[1] = new Plane(x1, cameraPos);
				m_cameraClipPlanes[2] = new Plane(y0, cameraPos);
				m_cameraClipPlanes[3] = new Plane(y1, cameraPos);
			}
			// check visibility of each projector
			int receiverLayer = -1;
			int quadShadowCount = 0;
			float fadeStep = 0.0f < m_fadeDuration ? Time.deltaTime / m_fadeDuration : 1;
			bool bAlwaysShowQuadShadows = m_receivers == null || m_receivers.Count == 0;
			for (int i = 0; i < m_projectors.Count; ++i) {
				AutoProjector projector = m_projectors[i];
				projector.projector.InvokeUpdateTransform();
				// find a plane on which the shadow of the projector is projected.
				bool foundPlane = false;
				Plane plane = new Plane();
				if (m_raycastPlaneMask != 0) {
					Vector3 origin = projector.projector.position;
					Vector3 dir = projector.projector.direction;
					RaycastHit hit;
					if (Physics.Raycast(origin, dir, out hit, projector.projector.farClipPlane, m_raycastPlaneMask)) {
						plane = new Plane(hit.normal, hit.point);
						foundPlane = true;
					}
				}
				if (!foundPlane) {
					if (m_useInfinitePlane) {
						if (m_infinitePlaneTransform != null) {
							plane = new Plane(m_infinitePlaneTransform.TransformDirection(m_infinitePlaneNormal).normalized, m_infinitePlaneTransform.position);
						}
						else {
							plane = new Plane(m_infinitePlaneNormal, Vector3.zero);
						}
						plane.distance -= m_infinitePlaneHeight;
						foundPlane = true;
					}
				}
				// check visibility and create a quadrangle shadow polygon on the plane.
				List<ReceiverBase> receivers = projector.GetReceivers();
				bool isVisible;
				if (foundPlane) {
					isVisible = projector.UpdateVisibility(plane, cameraZPlane, m_cameraClipPlanes, m_mainCamera.nearClipPlane, m_mainCamera.farClipPlane);
				}
				else {
					isVisible = projector.UpdateVisibility(cameraZPlane, m_cameraClipPlanes, m_mainCamera.nearClipPlane, m_mainCamera.farClipPlane);
				}
				if (isVisible) {
					// if projector is far from the main camera, disable projection.
					if (projector.distanceFromCamera < m_projectorFadeoutDistance) {
						projector.EnableProjector();
					}
					else {
						projector.DisableProjector();
					}
					projector.UpdateWeights(fadeStep, bAlwaysShowQuadShadows);
					if (projector.isProjectorActive && 0 < m_receivers.Count) {
						// if projection is enabled, assign a set of shadow receivers.
						if (receivers == null) {
							receivers = GetFreeReceivers();
							projector.SetReceivers(receivers);
						}
						receiverLayer = GetNextReceiverLayer(receiverLayer);
						projector.SetReceiverLayer(receiverLayer, m_receiverLayerMask);
						for (int j = 0; j < receivers.Count; ++j) {
							receivers[j].UpdateReceiver();
						}
					}
					else if (receivers != null) {
						projector.ClearReceivers();
						FreeReceivers(receivers);
					}
					if (0.0f < projector.quadShadowWeight) {
						++quadShadowCount;
					}
				}
				else if (receivers != null) {
					projector.ClearReceivers();
					FreeReceivers(receivers);
				}
			}
			if (m_meshes != null) {
				// update quad shadows mesh
				int numVertices = 4*quadShadowCount;
				int index = 0;
				int indexCount = 0;
				Color32 color = new Color32(255, 255, 255, 255);
				if (0 < quadShadowCount) {
					if (m_vertices == null || m_vertices.Length < numVertices) {
						m_vertices = new Vector3[numVertices];
						m_colors = new Color32[numVertices];
						m_uvs = new Vector2[numVertices];
						m_triangles = new int[6*quadShadowCount];
					}
					for (int i = 0; i < m_projectors.Count; ++i) {
						AutoProjector projector = m_projectors[i];
						if (projector.isVisible && 0.0f < projector.quadShadowWeight) {
							m_triangles[indexCount++] = index;
							m_triangles[indexCount++] = index + 1;
							m_triangles[indexCount++] = index + 2;
							m_triangles[indexCount++] = index + 2;
							m_triangles[indexCount++] = index + 1;
							m_triangles[indexCount++] = index + 3;
							projector.GetQuadShadowVertices(m_vertices, index);
							color.a = (byte)Mathf.FloorToInt(255*projector.quadShadowAlpha);
							Rect rect = m_blobShadowTextureRects[projector.uvIndex];
							m_colors[index] = color;
							m_uvs[index] = new Vector2(rect.x, rect.y);
							index++;
							m_colors[index] = color;
							m_uvs[index] = new Vector2(rect.x, rect.y + rect.height);
							index++;
							m_colors[index] = color;
							m_uvs[index] = new Vector2(rect.x + rect.width, rect.y);
							index++;
							m_colors[index] = color;
							m_uvs[index] = new Vector2(rect.x + rect.width, rect.y + rect.height);
							index++;
						}
					}
					while (indexCount < m_triangles.Length) {
						m_triangles[indexCount++] = 0;
					}
					m_nCurrentBuffer = (m_nCurrentBuffer + 1) % BUFFER_COUNT;
					Mesh currentMesh = m_meshes[m_nCurrentBuffer];
					currentMesh.Clear();
					currentMesh.vertices = m_vertices;
					currentMesh.colors32 = m_colors;
					currentMesh.uv = m_uvs;
					currentMesh.triangles = m_triangles;
					m_meshFilter.mesh = currentMesh;
					m_renderer.enabled = true;
				}
				else {
					m_renderer.enabled = false;
				}
			}
		}
		public void PackBlobShadowTextures(Texture2D packedTexture)
		{
			m_blobShadowTextureRects = packedTexture.PackTextures(m_blobShadowTextures, m_packedTexturePadding, m_packedTextureMaxSize, false);
			m_packedBlobShadowTexture = packedTexture;
			/*if (0 < m_packedBlobShadowTexturePadding)*/ {
				// fill padding area with white color
				int w = m_packedBlobShadowTexture.width;
				int h = m_packedBlobShadowTexture.height;
				System.Collections.BitArray bits = new System.Collections.BitArray(w*h, false);
				for (int i = 0; i < m_blobShadowTextureRects.Length; ++i) {
					Rect rect = m_blobShadowTextureRects[i];
					int startX = Mathf.RoundToInt(rect.x * w);
					int startY = Mathf.RoundToInt(rect.y * h);
					int endX = Mathf.RoundToInt((rect.x + rect.width)*w);
					int endY = Mathf.RoundToInt((rect.y + rect.height)*h);
					for (int y = startY; y < endY; ++y) {
						for (int x = startX; x < endX; ++x) {
							bits[x + y * w] = true;
						}
					}
				}
				Color32[] pixels = m_packedBlobShadowTexture.GetPixels32();
				Color32 white = new Color32(255, 255, 255, 255);
				int index = 0;
				for (int y = 0; y < h; ++y) {
					for (int x = 0; x < w; ++x) {
						if (!bits[index]) {
							pixels[index] = white;
						}
						++index;
					}
				}
				m_packedBlobShadowTexture.SetPixels32(pixels);
			}
		}

		// private fields
		private List<AutoProjector> m_projectors;
		private List< List<ReceiverBase> > m_receiverInstances;
		private Stack< List<ReceiverBase> > m_freeReceiverInstances;
		private Transform m_cameraTransform;
		private int m_firstReceiverLayer;
		private int m_lastReceiverLayer;

		const int BUFFER_COUNT = 2;
		private int m_nCurrentBuffer;
		private Vector3[] m_vertices;
		private Color32[] m_colors;
		private Vector2[] m_uvs;
		private int[]     m_triangles;
		private Mesh[] m_meshes;
		private MeshFilter m_meshFilter;
		private Renderer m_renderer;
		private Plane[] m_cameraClipPlanes;

		// private methods
		void Awake()
		{
			if (s_instance != this && s_instance != null) {
				if (Application.isEditor || Debug.isDebugBuild) {
					Debug.LogWarning("There is aother Projector Manager. Projector Manager should be singleton.", s_instance);
				}
				m_projectors = s_instance.m_projectors;
				s_instance = this;
			}
			UpdateReceiverLayer();
			if (m_mainCamera == null) {
				m_mainCamera = Camera.main;
			}
			m_cameraTransform = m_mainCamera.transform;
			m_cameraClipPlanes = new Plane[4];
			for (int i = 0; i < m_receivers.Count; ++i) {
				m_receivers[i].gameObject.SetActive(false);
				m_receivers[i].manualUpdate = true;
			}
			m_renderer = GetComponent<Renderer>();
			if (m_blobShadowTextures != null && 0 < m_blobShadowTextures.Length && (m_blobShadowTextures.Length == 1 || m_packedBlobShadowTexture != null)) {
				m_meshes = new Mesh[BUFFER_COUNT];
				for (int i = 0; i < BUFFER_COUNT; ++i) {
					m_meshes[i] = new Mesh();
				}
				m_meshFilter = GetComponent<MeshFilter>();
				if (m_meshFilter == null) {
					m_meshFilter = gameObject.AddComponent<MeshFilter>();
				}
				m_nCurrentBuffer = 0;
				if (m_packedBlobShadowTexture != null) {
					m_renderer.material.SetTexture(m_shadowTexName, m_packedBlobShadowTexture);
				}
				else {
					m_renderer.material.SetTexture(m_shadowTexName, m_blobShadowTextures[0]);
				}
			}
			else {
				m_meshes = null;
				m_renderer.enabled = false;
			}
			if (m_blobShadowTextureRects == null || m_blobShadowTextureRects.Length == 0 || m_blobShadowTextures.Length <= 1) {
				m_blobShadowTextureRects = new Rect[1];
				m_blobShadowTextureRects[0] = new Rect(0,0,1,1);
			}
			else if ((m_blobShadowTextureRects.Length != m_blobShadowTextures.Length) && (Debug.isDebugBuild || Application.isEditor)) {
				Debug.LogError("Combined Blob Shadow Texture has not been updated since the array of Blob Shadow Textures was changed.");
			}
			m_infinitePlaneNormal.Normalize();
			transform.position = Vector3.zero;
			transform.rotation = Quaternion.identity;
	    }

		void OnValidate()
		{
			if (m_cameraClipPlanes == null || m_cameraClipPlanes.Length != 4) {
				m_cameraClipPlanes = new Plane[4];
			}
		}
		void OnEnable()
		{
			if (m_freeReceiverInstances == null) {
				m_freeReceiverInstances = new Stack<List<ReceiverBase>>();
			}
		}
		void OnDisable()
		{
			for (int i = 0; i < m_projectors.Count; ++i) {
				List<ReceiverBase> receivers = m_projectors[i].GetReceivers();
				if (receivers != null) {
					m_projectors[i].ClearReceivers();
					FreeReceivers(receivers);
				}
			}
			if (m_freeReceiverInstances != null) {
				while (0 < m_freeReceiverInstances.Count) {
					List<ReceiverBase> receivers = m_freeReceiverInstances.Pop();
					for (int i = 0; i < receivers.Count; ++i) {
						if (receivers[i] != null) {
							Destroy(receivers[i].gameObject);
						}
					}
				}
				m_freeReceiverInstances.Clear();
				m_freeReceiverInstances = null;
			}
			if (m_receiverInstances != null) {
				for (int j = 0; j < m_receiverInstances.Count; ++j) {
					List<ReceiverBase> receivers = m_receiverInstances[j];
					for (int i = 0; i < receivers.Count; ++i) {
						if (receivers[i] != null) {
							Destroy(receivers[i].gameObject);
						}
					}
				}
				m_receiverInstances.Clear();
			}
		}

		void OnDestroy()
		{
			if (s_instance == this) {
				s_instance = null;
			}
		}

		void LateUpdate()
		{
			if (!m_manualUpdate) {
				UpdateScene();
			}
		}

		int GetNextReceiverLayer(int layer)
		{
			if (layer < m_firstReceiverLayer) {
				return m_firstReceiverLayer;
			}
			while (++layer <= m_lastReceiverLayer) {
				if ((m_receiverLayerMask & (1 << layer)) != 0) {
					return layer;
				}
			}
			return m_firstReceiverLayer;
		}
		void UpdateReceiverLayer()
		{
			if (m_receiverLayerMask == 0) {
				for (int i = 0; i < 32; ++i) {
					if (string.IsNullOrEmpty(LayerMask.LayerToName(i))) {
						m_receiverLayerMask |= 1 << i;
					}
				}
			}
			m_firstReceiverLayer = -1;
			m_lastReceiverLayer = -1;
			for (int i = 0; i < 32; ++i) {
				if ((m_receiverLayerMask & (1 << i)) != 0) {
					if (m_firstReceiverLayer == -1) {
						m_firstReceiverLayer = i;
					}
					m_lastReceiverLayer = i;
				}
			}
		}
		List<ReceiverBase> GetFreeReceivers()
		{
			List<ReceiverBase> receivers;
			if (0 < m_freeReceiverInstances.Count) {
				receivers = m_freeReceiverInstances.Pop();
				for (int j = 0; j < m_receivers.Count; ++j) {
					receivers[j].gameObject.SetActive(true);
				}
			}
			else {
				receivers = new List<ReceiverBase>(m_receivers.Count);
				for (int j = 0; j < m_receivers.Count; ++j) {
					ReceiverBase clone = Instantiate(m_receivers[j]) as ReceiverBase;
					clone.transform.parent = transform;
					clone.gameObject.SetActive(true);
					receivers.Add(clone);
				}
				m_receiverInstances.Add(receivers);
			}
			return receivers;
		}
		void FreeReceivers(List<ReceiverBase> receivers)
		{
			for (int j = 0; j < m_receivers.Count; ++j) {
				receivers[j].gameObject.SetActive(false);
			}
			if (m_freeReceiverInstances != null) {
				m_freeReceiverInstances.Push(receivers);
			}
			else {
				for (int j = 0; j < m_receivers.Count; ++j) {
					if (receivers[j] != null) {
						Destroy(receivers[j].gameObject);
					}
				}
			}
		}
		// this function is public for Editor
		public void AddBlobShadowTextureIfNotExist(Texture2D tex)
		{
			int texCount = 0;
			if (m_blobShadowTextures != null) {
				texCount = m_blobShadowTextures.Length;
				for (int i = 0; i < texCount; ++i) {
					if (m_blobShadowTextures[i] == tex) {
						return;
					}
				}
			}
			Texture2D[] textures = new Texture2D[texCount + 1];
			for (int i = 0; i < texCount; ++i) {
				textures[i] = m_blobShadowTextures[i];
			}
			textures[texCount] = tex;
			m_blobShadowTextures = textures;
		}
	}
}
