using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class ProjectorTest : MonoBehaviour {
		public Material m_visualizeShadow;
		public Material m_normalShadow;
		public Projector m_projector;
		public ReceiverBase m_shadowReceiver;
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
			bool bVisualize = m_projector.material == m_visualizeShadow;
			if (GUILayout.Button("Visualize: " + (bVisualize ? "ON" : "OFF"), height)) {
				if (m_normalShadow.shader == null || m_normalShadow.shader.name == "Hidden/InternalErrorShader") {
					Debug.LogError("Projector/Multiply shader is missing! Please import 'Projectors' assets from 'Assets > ImportPackage > Projectors' menu.");
				}
				else {
					m_projector.material = bVisualize ? m_normalShadow : m_visualizeShadow;
				}
			}
			bool bMinimalReceiver = (m_projector.ignoreLayers & (1 << m_ignoreProjectorLayer)) != 0;
			if (GUILayout.Button("Fast Receiver: " + (bMinimalReceiver ? "ON" : "OFF"), height)) {
				if (bMinimalReceiver) {
					m_projector.ignoreLayers &= ~(1 << m_ignoreProjectorLayer);
					m_shadowReceiver.gameObject.SetActive(false);
				}
				else {
					m_projector.ignoreLayers |= (1 << m_ignoreProjectorLayer);
					m_shadowReceiver.gameObject.SetActive(true);
				}
			}
			GUILayout.EndArea();
		}
	}
}
