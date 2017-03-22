using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class RollingLight : MonoBehaviour {
		public float m_rollSpeed = 90.0f;

		private Transform m_transform;
		void Awake()
		{
			m_transform = transform;
		}
		
		void Update()
		{
			m_transform.rotation = m_transform.rotation * Quaternion.AngleAxis(m_rollSpeed * Time.deltaTime, Vector3.forward);
		}
	}
}
