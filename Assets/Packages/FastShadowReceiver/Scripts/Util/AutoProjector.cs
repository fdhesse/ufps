//
// AutoProjector.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace FastShadowReceiver {
	[RequireComponent(typeof(Projector))]
	public class AutoProjector : MonoBehaviour {
		public const string SHADOW_ALPHA_PROPERTY_NAME = "_Alpha";
		[SerializeField][HideInInspector]
		private Component m_predictor;
		public ITransformPredictor predictor
		{
			get {
				return m_predictor as ITransformPredictor;
			}
			set {
				if (value == null || value is Component) {
					m_predictor = value as Component;
				}
				else if (Debug.isDebugBuild || Application.isEditor) {
					Debug.LogError("predictor must be a Component!");
				}
			}
		}
		// call this function manually when blob shadow texture is changed
		public void OnTextureChanged()
		{
			ProjectorManager.Instance.UpdateUVIndex(this);
		}
		private UnityProjector m_projector;
		private Transform m_transform;
		private bool m_canFadeout;
		void Awake()
		{
			m_projector = new UnityProjector(GetComponent<Projector>());
			// check if the components of this game object have UpdateTransform methods
			Component[] components = gameObject.GetComponents<Component>();
			for (int i = 0; i < components.Length; ++i) {
				System.Type type = components[i].GetType();
#if NETFX_CORE
				MethodInfo method = type.GetTypeInfo().GetDeclaredMethod("UpdateTransform");
				if (method != null) {
					m_projector.updateTransform += method.CreateDelegate(typeof(System.Action), components[i]) as System.Action;
				}
#else
				System.Reflection.MethodInfo method = type.GetMethod("UpdateTransform");
				if (method != null) {
					m_projector.updateTransform += System.Action.CreateDelegate(typeof(System.Action), components[i], method) as System.Action;
				}
#endif
			}
			m_transform = transform;
			m_vertices = new Vector3[4];
			if (m_projector.projector.material.HasProperty(SHADOW_ALPHA_PROPERTY_NAME)) {
				m_projector.projector.material = new Material(m_projector.projector.material); // make a copy of material so that we can change alpha later.
				m_originalAlpha = m_projector.projector.material.GetFloat(SHADOW_ALPHA_PROPERTY_NAME);
				m_canFadeout = true;
			}
			else {
				m_originalAlpha = 1.0f;
				m_canFadeout = false;
			}
		}
		void OnValidate()
		{
			// when script files are rebuilt while scene running, unity tries to restore m_receivers and make it empty list rather than null.
			if (m_receivers != null && m_receivers.Count == 0) {
				m_receivers = null;
			}
		}
		void OnEnable()
		{
			// register projector to ProjectorManager, and reset visibility.
			if (ProjectorManager.Instance != null) {
				ProjectorManager.Instance.AddProjector(this);
				m_projectorWeight = 0.0f;
				m_quadShadowWeight = 0.0f;
				m_projector.projector.enabled = false;
				m_enableProjector = false;
			}
			else if (Debug.isDebugBuild || Application.isEditor) {
				Debug.LogWarning("There is no Projector Manager instance.", this);
			}
		}
		void OnDisable()
		{
			// check if ProjectorManager.Instance is null or not, in case that ProjectorManager is already destroyed.
			if (ProjectorManager.Instance != null) {
				ProjectorManager.Instance.RemoveProjector(this);
			}
		}
		// called from ProjectorManager
		public UnityProjector projector
		{
			get
			{
#if UNITY_EDITOR
				if (m_projector == null) {
					m_projector = new UnityProjector(GetComponent<Projector>());
				}
#endif
				return m_projector;
			}
		}
		private float m_projectorWeight = 0.0f;
		private float m_quadShadowWeight = 0.0f;
		private float m_originalAlpha;
		private int m_receiverLayer = -1;
		private List<ReceiverBase> m_receivers = null;
		private bool m_enableProjector = false;
		private float m_distanceFromCamera;
		private bool  m_isVisible = false;
		private Vector3[] m_vertices;
		private int m_uvIndex = 0;
		private bool CheckVisibility(Plane targetPlane, Plane zPlane, Plane[] cameraClipPlanes, float cameraNear, float cameraFar)
		{
			m_projector.GetPlaneIntersection(m_vertices, targetPlane);
			Vector3 o = m_transform.position;
			float z0 = zPlane.GetDistanceToPoint(o);
			float z1 = zPlane.GetDistanceToPoint(m_vertices[0]);
			float z2 = zPlane.GetDistanceToPoint(m_vertices[1]);
			float z3 = zPlane.GetDistanceToPoint(m_vertices[2]);
			float z4 = zPlane.GetDistanceToPoint(m_vertices[3]);
			m_distanceFromCamera = 0.5f*(z1 + z3);
			if (z0 < cameraNear && z1 < cameraNear && z3 < cameraNear && z4 < cameraNear) {
				return false;
			}
			else if (cameraFar < z0 && cameraFar < z1 && cameraFar < z2 && cameraFar < z4) {
				return false;
			}
			else {
				for (int i = 0; i < cameraClipPlanes.Length; ++i) {
					if (0.0f < cameraClipPlanes[i].GetDistanceToPoint(o)) {
						continue;
					}
					if (0.0f < cameraClipPlanes[i].GetDistanceToPoint(m_vertices[0])) {
						continue;
					}
					if (0.0f < cameraClipPlanes[i].GetDistanceToPoint(m_vertices[1])) {
						continue;
					}
					if (0.0f < cameraClipPlanes[i].GetDistanceToPoint(m_vertices[2])) {
						continue;
					}
					if (0.0f < cameraClipPlanes[i].GetDistanceToPoint(m_vertices[3])) {
						continue;
					}
					return false;
				}
			}
			return true;
		}
		public bool UpdateVisibility(Plane targetPlane, Plane zPlane, Plane[] cameraClipPlanes, float cameraNear, float cameraFar)
		{
			// calculate quadrangle shadow vertex positions, and check visibility.
			m_isVisible = CheckVisibility(targetPlane, zPlane, cameraClipPlanes, cameraNear, cameraFar);
			if (!m_isVisible) {
				m_projectorWeight = 0.0f;
				m_quadShadowWeight = 0.0f;
				m_projector.projector.enabled = false;
				m_enableProjector = false;
			}
			return m_isVisible;
		}
		public bool UpdateVisibility(Plane zPlane, Plane[] cameraClipPlanes, float cameraNear, float cameraFar)
		{
			// calculate quadrangle shadow vertex positions, and check visibility.
			Vector3 o = m_transform.position;
			Vector3 z = m_transform.forward;
			m_isVisible = false;
			for (int i = 0; i < cameraClipPlanes.Length; ++i) {
				if (0.0f < cameraClipPlanes[i].GetDistanceToPoint(o) && Vector3.Dot(cameraClipPlanes[i].normal, z) < 0) {
					Plane targetPlane = cameraClipPlanes[i];
					targetPlane.distance += 0.01f;
					m_isVisible = CheckVisibility(targetPlane, zPlane, cameraClipPlanes, cameraNear, cameraFar);
					if (m_isVisible) {
						break;
					}
				}
			}
			m_distanceFromCamera = zPlane.GetDistanceToPoint(o);
			if (!m_isVisible) {
				m_projectorWeight = 0.0f;
				m_quadShadowWeight = 0.0f;
				m_projector.projector.enabled = false;
				m_enableProjector = false;
			}
			return m_isVisible;
		}
		public void EnableProjector()
		{
			m_projector.projector.enabled = true;
			m_enableProjector = true;
		}
		public void DisableProjector()
		{
			m_enableProjector = false;
		}
		public void SetReceivers(List<ReceiverBase> receivers)
		{
			m_receivers = receivers;
			for (int j = 0; j < receivers.Count; ++j) {
				ReceiverBase receiver = receivers[j];
				receiver.projectorInterface = m_projector;
				if (m_predictor != null) {
					MeshShadowReceiver meshReceiver = receiver as MeshShadowReceiver;
					if (meshReceiver != null) {
						meshReceiver.predictor = predictor;
					}
				}
				receiver.gameObject.SetActive(true);
			}
		}
		public void SetReceiverLayer(int receiverLayer, int receiverLayerMask)
		{
			if (m_receiverLayer != receiverLayer && 0 <= receiverLayer) {
				m_projector.projector.ignoreLayers |= receiverLayerMask;
				m_projector.projector.ignoreLayers &= ~(1 << receiverLayer);
			}
			m_receiverLayer = receiverLayer;
			if (0 <= receiverLayer) {
				for (int j = 0; j < m_receivers.Count; ++j) {
					m_receivers[j].gameObject.layer = receiverLayer;
				}
			}
		}
		public List<ReceiverBase> GetReceivers()
		{
			return m_receivers;
		}
		public void ClearReceivers()
		{
			m_receivers = null;
		}
		public bool isProjectorActive
		{
			get { return m_projector.projector.enabled; }
		}
		public bool isVisible
		{
			get { return m_isVisible; }
		}
		public float quadShadowWeight
		{
			get { return m_quadShadowWeight; }
		}
		public float quadShadowAlpha
		{
			get { return m_originalAlpha * m_quadShadowWeight; }
		}
		public float projectorShadowAlpha
		{
			get { return m_originalAlpha * m_projectorWeight; }
		}
		public float distanceFromCamera
		{
			get { return m_distanceFromCamera; }
		}
		public int uvIndex
		{
			get { return m_uvIndex; }
			set { m_uvIndex = value; }
		}
		public void GetQuadShadowVertices(Vector3[] vertices, int offset)
		{
			vertices[offset++] = m_vertices[0];
			vertices[offset++] = m_vertices[1];
			vertices[offset++] = m_vertices[2];
			vertices[offset++] = m_vertices[3];
		}
		public void UpdateWeights(float fadeStep, bool alwaysShowQuadShadow)
		{
			if (m_isVisible) {
				bool bShowQuadShadow = alwaysShowQuadShadow || !m_enableProjector;
				bool wasVisible = (0.0f < m_quadShadowWeight || 0.0f < m_projectorWeight);
				if (m_enableProjector) {
					if (m_projectorWeight < 1.0f) {
						if (m_canFadeout && wasVisible) {
							m_projectorWeight = Mathf.Min(1.0f, m_projectorWeight + fadeStep);
						}
						else {
							m_projectorWeight = 1.0f;
						}
						m_projector.projector.material.SetFloat(SHADOW_ALPHA_PROPERTY_NAME, projectorShadowAlpha);
					}
				}
				else {
					if (0.0f < m_projectorWeight) {
						m_projectorWeight = Mathf.Max(0.0f, m_projectorWeight - fadeStep);
						m_projector.projector.material.SetFloat(SHADOW_ALPHA_PROPERTY_NAME, projectorShadowAlpha);
						if (m_projectorWeight <= 0.0f) {
							m_projector.projector.enabled = false;
						}
					}
				}
				if (bShowQuadShadow) {
					if (m_canFadeout && wasVisible) {
						m_quadShadowWeight = Mathf.Min(1.0f, m_quadShadowWeight + fadeStep);
					}
					else {
						m_quadShadowWeight = 1.0f;
					}
				}
				else {
					m_quadShadowWeight = Mathf.Max(0.0f, m_quadShadowWeight - fadeStep);
				}
			}
		}
	}
}
