using UnityEngine;
using System.Collections;

namespace FastShadowReceiver.Demo {
	public class PerformanceCounter : MonoBehaviour {
		public MeshShadowReceiver   m_meshShadowReceiver;
		public MeshShadowReceiver[] m_meshShadowReceivers;
		public BinaryMeshTree       m_binaryMeshTree;
		public OctMeshTree          m_octMeshTree;
		public ProjectorManager     m_projectorManager;
		private float m_fps;
		private float m_fpsUpdateInterval = 0.5f;
		private float m_accumTime = 0.0f;
		private float m_timeLeft = 0.0f;
		private int m_frameCount = 20;
		private float m_meshSearchAccumTime = 0.0f;
		private float m_meshSearchTime = 0.0f;

		// Use this for initialization
		void Start () {
			Application.targetFrameRate = 300;
			if (m_meshShadowReceiver != null) {
				// Enable manual update so that LateUpdate of this component can measure mesh update time.
				m_meshShadowReceiver.manualUpdate = true;
				// Disable multithreading to measure mesh update time correctly.
				m_meshShadowReceiver.multiThreadEnabled = false;
			}
		}
		
		// Update is called once per frame
		void LateUpdate () {
			if (m_meshShadowReceiver != null && m_meshShadowReceiver.gameObject.activeSelf) {
				float searchStart = Time.realtimeSinceStartup;
				m_meshShadowReceiver.UpdateReceiver();
				m_meshSearchAccumTime += Time.realtimeSinceStartup - searchStart;
			}
			float dt = Time.deltaTime/Time.timeScale;
			m_accumTime += dt;
			m_timeLeft -= dt;
			++m_frameCount;
			if (m_timeLeft <= 0.0f) {
				m_fps = m_frameCount/m_accumTime;
				m_accumTime = 0.0f;
				m_meshSearchTime = m_meshSearchAccumTime/m_frameCount;
				m_meshSearchAccumTime = 0.0f;
				m_timeLeft = m_fpsUpdateInterval;
				m_frameCount = 0;
			}
		}
		void OnGUI()
		{
			GUILayoutOption height = GUILayout.Height(50);
			GUILayout.BeginArea(new Rect(0, 0, 200, 150));
			GUILayout.Label("FPS: " + m_fps.ToString("f2"));
			if (m_meshShadowReceiver != null) {
				GUILayout.Label("Mesh Update: " + (1000.0f*m_meshSearchTime).ToString("f3") + "msec");
			}
			if (m_projectorManager != null) {
				GUILayout.Label("Active Projector: " + m_projectorManager.activeProjectorCount + " / " + m_projectorManager.projectorCount);
			}
			if (m_meshShadowReceiver != null || (m_meshShadowReceivers != null && 0 < m_meshShadowReceivers.Length)) {
				if (m_binaryMeshTree != null && m_octMeshTree != null) {
					bool bOctree;
					if (m_meshShadowReceiver != null) {
						bOctree = (m_meshShadowReceiver.meshTree == m_octMeshTree);
					}
					else {
						bOctree = (m_meshShadowReceivers[0].meshTree == m_octMeshTree);
					}
					if (GUILayout.Button("Tree: " + (bOctree ? "Octree" : "BinaryTree"), height)) {
						MeshTree meshTree = bOctree ? (MeshTree)m_binaryMeshTree : (MeshTree)m_octMeshTree;
						if (m_meshShadowReceiver != null) {
							m_meshShadowReceiver.meshTree = meshTree;
						}
						if (m_meshShadowReceivers != null) {
							for (int i = 0; i < m_meshShadowReceivers.Length; ++i) {
								m_meshShadowReceivers[i].meshTree = meshTree;
							}
						}
					}
				}
				bool bScissor;
				if (m_meshShadowReceiver != null) {
					bScissor = m_meshShadowReceiver.scissorEnabled;
				}
				else {
					bScissor = m_meshShadowReceivers[0].scissorEnabled;
				}
				if (GUILayout.Button("Scissoring: " + (bScissor ? "ON" : "OFF"), height)) {
					bScissor = !bScissor;
					if (m_meshShadowReceiver != null) {
						m_meshShadowReceiver.scissorEnabled = bScissor;
					}
					if (m_meshShadowReceivers != null) {
						for (int i = 0; i < m_meshShadowReceivers.Length; ++i) {
							m_meshShadowReceivers[i].scissorEnabled = bScissor;
						}
					}
				}
			}
			GUILayout.EndArea();
		}
	}
}
