using UnityEngine;
using System.Collections;

namespace FastShadowReceiver.Demo {
	public class ShadowmapTest : MonoBehaviour {
		public Material m_visualizeShadow;
		public Material m_normalShadow;
		public Material m_debugDiffuse;
		public Material m_normalDiffuse;
		public GameObject m_rootNoShadow;
		public GameObject m_rootEnvironment;
		public ReceiverBase m_shadowReceiver;
		private Renderer[] m_allShadowReceivers;
		private Renderer[] m_noShadowRenderers;
		private Renderer m_shadowReceiverRenderer;

		void Start () {
			m_noShadowRenderers = m_rootNoShadow.GetComponentsInChildren<Renderer>();
			m_shadowReceiverRenderer = m_shadowReceiver.GetComponent<Renderer>();
			m_allShadowReceivers = m_rootEnvironment.GetComponentsInChildren<Renderer>();
		}
		
		void OnGUI()
		{
			GUILayoutOption height = GUILayout.Height(50);
			GUILayout.BeginArea(new Rect(0, 150, 200, Screen.height - 150));
			bool bVisualize = m_shadowReceiverRenderer.sharedMaterial == m_visualizeShadow;
			if (GUILayout.Button("Visualize: " + (bVisualize ? "ON" : "OFF"), height)) {
				m_shadowReceiverRenderer.sharedMaterial = bVisualize ? m_normalShadow : m_visualizeShadow;
				Material material = bVisualize ? m_normalDiffuse : m_debugDiffuse;
				for (int i = 0; i < m_allShadowReceivers.Length; ++i) {
					m_allShadowReceivers[i].sharedMaterial = material;
				}
			}
			bool bMinimalReceiver = m_shadowReceiver.gameObject.activeSelf;
			if (GUILayout.Button("Fast Receiver: " + (bMinimalReceiver ? "ON" : "OFF"), height)) {
				if (bMinimalReceiver) {
					m_shadowReceiver.gameObject.SetActive(false);
					for (int i = 0; i < m_noShadowRenderers.Length; ++i) {
						m_noShadowRenderers[i].receiveShadows = true;
					}
				}
				else {
					m_shadowReceiver.gameObject.SetActive(true);
					for (int i = 0; i < m_noShadowRenderers.Length; ++i) {
						m_noShadowRenderers[i].receiveShadows = false;
					}
				}
			}
			GUILayout.EndArea();
		}
	}
}
