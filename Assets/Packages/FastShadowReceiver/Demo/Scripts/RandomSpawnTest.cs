using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class RandomSpawnTest : MonoBehaviour {
		public Material         m_visualizeShadow;
		public Material         m_normalShadow;
		public Projector        m_projector;
		public SceneManager     m_sceneManager;

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
			bool bVisualize = m_projector.material.name == m_visualizeShadow.name;
			if (GUILayout.Button("Visualize: " + (bVisualize ? "ON" : "OFF"), height)) {
				if (m_normalShadow.shader == null || m_normalShadow.shader.name == "Hidden/InternalErrorShader") {
					Debug.LogError("Projector/Multiply shader is missing! Please import 'Projectors' assets from 'Assets > ImportPackage > Projectors' menu.");
				}
				else {
					m_projector.material = new Material(bVisualize ? m_normalShadow : m_visualizeShadow);
					m_sceneManager.SetProjectorMaterial(m_projector.material);
				}
			}
			bool bMinimalReceiver = (m_projector.ignoreLayers & (1 << m_ignoreProjectorLayer)) != 0;
			if (GUILayout.Button("Fast Receiver: " + (bMinimalReceiver ? "ON" : "OFF"), height)) {
				if (bMinimalReceiver) {
					m_projector.ignoreLayers &= ~(1 << m_ignoreProjectorLayer);
					m_sceneManager.ResetProjectorIgnoreLayers(1 << m_ignoreProjectorLayer);
					m_sceneManager.ForceDisableAutoProjectors();
					ProjectorManager.Instance.enabled = false;
					ProjectorManager.Instance.GetComponent<Renderer>().enabled = false;
				}
				else {
					m_projector.ignoreLayers |= (1 << m_ignoreProjectorLayer);
					m_sceneManager.ForceEnableAutoProjectors();
					m_sceneManager.SetProjectorIgnoreLayers(1 << m_ignoreProjectorLayer);
					ProjectorManager.Instance.enabled = true;
					ProjectorManager.Instance.GetComponent<Renderer>().enabled = true;
				}
			}
			GUILayout.EndArea();
		}
	}
}
