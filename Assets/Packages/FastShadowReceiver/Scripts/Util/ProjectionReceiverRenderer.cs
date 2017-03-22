//
// ProjectionReceiverRenderer.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;

namespace FastShadowReceiver {
	/// <summary>
	/// The objective of this component is to setup projection matrix to renderer.material.
	/// The material should have one of "FastShadowReceiver/Projector/XXXXX" shaders.
	/// </summary>
	//[ExecuteInEditMode] // this component cannot be executable in Edit Mode, because this component needs to set a shader keyword to non-shared renderer.material.
	public class ProjectionReceiverRenderer : MonoBehaviour {
		[SerializeField][HideInInspector]
		private Component m_projectorComponent;
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

		private Material m_material;
		private IProjector m_projector;
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
		private Transform m_transform;
#endif
		private Renderer m_renderer;
		private MaterialPropertyBlock m_propertyBlock;
		void Awake()
		{
			m_renderer = GetComponent<Renderer>();
			if (m_renderer == null) {
				if (Debug.isDebugBuild || Application.isEditor) {
					Debug.LogError("No renderer was found!", this);
				}
				return;
			}
			m_renderer.material.EnableKeyword("FSR_RECEIVER");
			if (m_projectorComponent != null) {
				if (m_projectorComponent is IProjector) {
					m_projector = m_projectorComponent as IProjector;
				}
				else if (m_projectorComponent is Projector) {
					m_projector = new UnityProjector(m_projectorComponent as Projector);
				}
			}
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
			m_transform = transform;
#endif
			m_propertyBlock = new MaterialPropertyBlock();
		}
		void Start()
		{
			if (m_projector == null) {
				ReceiverBase receiver = GetComponent<ReceiverBase>();
				if (receiver != null) {
					m_projector = receiver.projectorInterface;
				}
				else if (Debug.isDebugBuild || Application.isEditor) {
					Debug.LogError("No projector was found!", this);
				}
			}
		}
		
		void OnWillRenderObject()
		{
			if (m_projector == null) {
				return;
			}
			Matrix4x4 m = m_renderer.localToWorldMatrix;
			if (IsVertexPrescaled()) {
				m.SetColumn(0, m.GetColumn(0).normalized);
				m.SetColumn(1, m.GetColumn(1).normalized);
				m.SetColumn(2, m.GetColumn(2).normalized);
			}
			Vector4 dir = m.inverse.MultiplyVector(m_projector.direction).normalized;
			m = m_projector.uvProjectionMatrix * m;
			m_renderer.GetPropertyBlock(m_propertyBlock);
			m_propertyBlock.SetMatrix("_FSRProjector", m);
			m_propertyBlock.SetVector("_FSRProjectDir", dir);
			m_renderer.SetPropertyBlock(m_propertyBlock);
		}
		bool IsVertexPrescaled()
		{
#if (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6) // support Unity 4.3 or later
			Vector3 scale = m_transform.localScale;
			float max = Mathf.Max (scale.x, Mathf.Max (scale.y, scale.z));
			float min = Mathf.Min (scale.x, Mathf.Min (scale.y, scale.z));
			return 0.00001f * max < (max - min);
#else
			return false;
#endif
		}
	}
}
