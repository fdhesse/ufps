//
// ReceiverBase.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections;
using System.Reflection;

namespace FastShadowReceiver {
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	public abstract class ReceiverBase : MonoBehaviour {
		[SerializeField][HideInInspector]
		private Component m_projectorComponent;
		[SerializeField]
		private bool m_manualUpdate = false;

		public Projector unityProjector
		{
			get { return m_projectorComponent as Projector; }
			set {
				if (m_projectorComponent != value) {
					m_projectorComponent = value;
					if (value != null) {
						if (m_projector is UnityProjector) {
							(m_projector as UnityProjector).projector = value;
						}
						else {
							m_projector = new UnityProjector(value);
						}
					}
					else {
						m_projector = null;
					}
				}
			}
		}
		public ProjectorBase customProjector
		{
			get { return m_projectorComponent as ProjectorBase; }
			set {
				if (m_projectorComponent != value) {
					m_projectorComponent = value;
					m_projector = value;
				}
			}
		}
		public IProjector projectorInterface
		{
			get { return m_projector; }
			set {
				m_projector = value;
				if (value is UnityProjector) {
					m_projectorComponent = ((UnityProjector)value).projector;
				}
				else if (value is Component) {
					m_projectorComponent = value as Component;
				}
			}
		}
		public bool manualUpdate
		{
			get { return m_manualUpdate; }
			set { m_manualUpdate = value; }
		}

		private IProjector m_projector = null;

		public void UpdateReceiver()
		{
#if UNITY_EDITOR
			if (!m_bAwoken || m_projector == null) {
				Awake();
			}
			if (m_projector == null) {
				return;
			}
			if (!m_bStarted) {
				Start();
			}
#endif
			if (m_projector != null && (Debug.isDebugBuild || Application.isEditor)) {
				if (m_projector is UnityProjector) {
					UnityProjector projector = m_projector as UnityProjector;
					if ((projector.projector.ignoreLayers & (1 << gameObject.layer)) != 0) {
						Debug.LogWarning("Ignore layers of the projector contains the layer of this shadow receiver. Shadow won't be rendered.", this);
					}
				}
			}
			OnUpdate();
		}
		protected IProjector projector
		{
			get { return m_projector; }
		}
		protected Mesh[] meshes
		{
			get { return m_meshes; }
		}
		protected Mesh currentMesh
		{
			get { return m_meshes[m_nCurrentBuffer]; }
		}
		protected void SwapMesh()
		{
			m_nCurrentBuffer = (m_nCurrentBuffer + 1) % BUFFER_COUNT;
			m_meshFilter.mesh = currentMesh;
		}
		protected void Hide(bool bHide)
		{
			m_renderer.enabled = !bHide;
		}
		protected virtual bool IsReady()
		{
			return true;//m_projector != null || (m_projectorComponent != null && (m_projectorComponent is Projector || m_projectorComponent is IProjector));
		}
		protected virtual void OnAwake() {}
		protected virtual void OnStart() {}
		protected abstract void OnUpdate();

		
		const int BUFFER_COUNT = 2;
		private int m_nCurrentBuffer;
		private Mesh[] m_meshes;
		private MeshFilter m_meshFilter;
		private Renderer m_renderer;
#if UNITY_EDITOR
		private bool m_bAwoken = false;
		private bool m_bStarted = false;
#endif

		void Awake () {
			m_renderer = GetComponent<Renderer>();
#if UNITY_EDITOR
			if (!IsReady()) {
				return;
			}
#endif
			if (m_projectorComponent is IProjector) {
				m_projector = m_projectorComponent as IProjector;
			}
			else if (m_projectorComponent is Projector) {
				m_projector = new UnityProjector(m_projectorComponent as Projector);
			}
			if (m_projectorComponent != null) {
				// check if the components of projector object have UpdateTransform methods
				Component[] components = m_projectorComponent.GetComponents<Component>();
				for (int i = 0; i < components.Length; ++i) {
					System.Type type = components[i].GetType();
#if NETFX_CORE
					MethodInfo method = type.GetTypeInfo().GetDeclaredMethod("UpdateTransform");
					if (method != null) {
						m_projector.updateTransform += method.CreateDelegate(typeof(System.Action), components[i]) as System.Action;
					}
#else
					MethodInfo method = type.GetMethod("UpdateTransform");
					if (method != null) {
						m_projector.updateTransform += System.Action.CreateDelegate(typeof(System.Action), components[i], method) as System.Action;
					}
#endif
				}
			}
			m_meshes = new Mesh[BUFFER_COUNT];
			for (int i = 0; i < BUFFER_COUNT; ++i) {
				m_meshes[i] = new Mesh();
				m_meshes[i].hideFlags = HideFlags.HideAndDontSave;
			}
			m_nCurrentBuffer = 0;
			m_meshFilter = GetComponent<MeshFilter>();
			if (m_meshFilter == null) {
				m_meshFilter = gameObject.AddComponent<MeshFilter>();
			}
			m_meshFilter.mesh = m_meshes[0];
			OnAwake();
#if UNITY_EDITOR
			m_bAwoken = true;
#endif
		}

		void Start () {
#if UNITY_EDITOR
			if (!m_bAwoken) {
				return;
			}
#endif
			OnStart();
#if UNITY_EDITOR
			m_bStarted = true;
#endif
		}

		void LateUpdate () {
#if UNITY_EDITOR
			if (!Application.isPlaying || !m_manualUpdate)
#else
			if (!m_manualUpdate)
#endif
			{
				if (projector != null) {
					projector.InvokeUpdateTransform();
				}
				UpdateReceiver();
			}
		}

		void OnDestroy()
		{
			if (m_meshFilter != null) {
				m_meshFilter.mesh = null;
			}
			if (m_meshes != null) {
				for (int i = 0; i < m_meshes.Length; ++i) {
#if UNITY_EDITOR
					DestroyImmediate(m_meshes[i]);
#else
					Destroy(m_meshes[i]);
#endif
					m_meshes[i] = null;
				}
				m_meshes = null;
			}
		}

#if UNITY_EDITOR
		static Material s_gizmoMaterial = null;
		void OnDrawGizmosSelected()
		{
			if (m_projector != null) {
				ProjectorBase.DrawFrustum(m_projector, new Color(1.0f, 1.0f, 1.0f, 0.25f));
				if (m_meshes != null) {
					Mesh mesh = currentMesh;
					if (0 < mesh.vertexCount) {
						if (s_gizmoMaterial == null) {
							s_gizmoMaterial = new Material(Shader.Find("FastShadowReceiver/Receiver/Gizmo"));
							s_gizmoMaterial.hideFlags = HideFlags.HideAndDontSave;
							s_gizmoMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
							s_gizmoMaterial.color = new Color(0.0f, 1.0f, 0.0f, 0.125f);
						}
						s_gizmoMaterial.SetPass(0);
						Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix);
					}
				}
			}
		}
#endif
	}
}
