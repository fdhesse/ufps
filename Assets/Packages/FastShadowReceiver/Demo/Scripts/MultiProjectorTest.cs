using UnityEngine;
using System.Collections;

namespace FastShadowReceiver.Demo {
	public class MultiProjectorTest : MonoBehaviour {
		public Material m_visualizeShadow;
		public Material m_normalShadow;
		public Projector[] m_projectors;
		public ReceiverBase[] m_shadowReceivers;
		private int m_ignoreProjectorLayer;
		
		void Start () {
			m_ignoreProjectorLayer = LayerMask.NameToLayer("Ignore Projector");
			if (m_ignoreProjectorLayer <= 0) {
				m_ignoreProjectorLayer = 8;
			}
		}
		
		void OnGUI()
		{
			GUILayoutOption height = GUILayout.Height(50);
			GUILayout.BeginArea(new Rect(0, 150, 200, Screen.height - 150));
			bool bVisualize = m_projectors[0].material == m_visualizeShadow;
			if (GUILayout.Button("Visualize: " + (bVisualize ? "ON" : "OFF"), height)) {
				if (m_normalShadow.shader == null || m_normalShadow.shader.name == "Hidden/InternalErrorShader") {
					Debug.LogError("Projector/Multiply shader is missing! Please import 'Projectors' assets from 'Assets > ImportPackage > Projectors' menu.");
				}
				else {
					Material material = bVisualize ? m_normalShadow : m_visualizeShadow;
					for (int i = 0; i < m_projectors.Length; ++i) {
						m_projectors[i].material = material;
					}
				}
			}
			bool bMinimalReceiver = (m_projectors[0].ignoreLayers & (1 << m_ignoreProjectorLayer)) != 0;
			if (GUILayout.Button("Fast Receiver: " + (bMinimalReceiver ? "ON" : "OFF"), height)) {
				if (bMinimalReceiver) {
					for (int i = 0; i < m_projectors.Length; ++i) {
						m_projectors[i].ignoreLayers &= ~(1 << m_ignoreProjectorLayer);
					}
					for (int i = 0; i < m_shadowReceivers.Length; ++i) {
						m_shadowReceivers[i].gameObject.SetActive(false);
					}
				}
				else {
					for (int i = 0; i < m_projectors.Length; ++i) {
						m_projectors[i].ignoreLayers |= (1 << m_ignoreProjectorLayer);
					}
					for (int i = 0; i < m_shadowReceivers.Length; ++i) {
						m_shadowReceivers[i].gameObject.SetActive(true);
					}
				}
			}
			GUILayout.EndArea();
		}
	}
}
