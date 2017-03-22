using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class RaycastBulletProjector : MonoBehaviour {
		public RaycastBulletMarkReceiver m_bulletReceiver;
		public float m_range = 10.0f;
		public float m_scatteringAngle = 10.0f;
		public int m_numPellets = 10;

		private float m_probDensitiy;
		void Awake()
		{
			m_probDensitiy = 1.0f - Mathf.Cos(Mathf.Deg2Rad * m_scatteringAngle);
		}
		void Update()
		{
			if (m_bulletReceiver != null && Input.GetMouseButtonDown(0)) {
				float x = Input.mousePosition.x - 0.5f * Screen.width;
				float y = Input.mousePosition.y - 0.5f * Screen.height;
				if (x*x + y*y < 0.15f*0.15f*Screen.height*Screen.height) {
					for (int i = 0; i < m_numPellets; ++i) {
						float cosTheta = 1.0f - m_probDensitiy * Random.value;
						float sinTheta = Mathf.Sqrt(1.0f - cosTheta*cosTheta);
						float phi = Random.Range(-Mathf.PI, Mathf.PI);
						Vector3 dir = new Vector3(sinTheta*Mathf.Cos(phi), sinTheta*Mathf.Sin(phi), cosTheta);
						dir = transform.TransformDirection(dir);
						m_bulletReceiver.AddShot(transform.position, dir, m_range);
					}
				}
			}
		}
	}
}
