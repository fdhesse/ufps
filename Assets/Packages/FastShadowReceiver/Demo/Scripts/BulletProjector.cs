using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class BulletProjector : MonoBehaviour {
		public BulletMarkReceiver m_bulletReceiver;
		private IProjector m_projector;
		void Awake()
		{
			m_projector = GetComponent<ProjectorBase>();
			if (m_projector == null && (Debug.isDebugBuild || Application.isEditor)) {
				Debug.LogError("No projector component!", this);
			}
		}
        
		void Update()
		{
			if (m_bulletReceiver != null && Input.GetMouseButtonDown(0)) {
				float x = Input.mousePosition.x - 0.5f * Screen.width;
				float y = Input.mousePosition.y - 0.5f * Screen.height;
				if (x*x + y*y < 0.15f*0.15f*Screen.height*Screen.height) {
					m_bulletReceiver.AddShot(m_projector);
				}                
			}
		}
	}
}
