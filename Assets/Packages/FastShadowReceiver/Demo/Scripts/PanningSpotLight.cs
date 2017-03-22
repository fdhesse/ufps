using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class PanningSpotLight : MonoBehaviour {
		public float m_minAngle = 0.0f;
		public float m_maxAngle = 90.0f;
		public float m_frequency = 0.25f;

		private Transform  m_transform;
		private Quaternion m_initialRotation;
		private float      m_time;
		void Awake()
		{
			m_transform = transform;
			m_initialRotation = m_transform.rotation;
			m_time = 0.0f;
		}
		void Update ()
		{
			m_time += Time.deltaTime;
			float a = Mathf.Sin(2.0f * Mathf.PI * m_frequency * m_time);
			a = m_minAngle + (m_maxAngle - m_minAngle) * 0.5f * (1.0f + a);
			m_transform.rotation = Quaternion.AngleAxis(a, Vector3.up) * m_initialRotation;
		}
	}
}
