using UnityEngine;
using System.Collections;

namespace FastShadowReceiver.Demo {
	public class SelfRotation : MonoBehaviour {
		public float m_rotationSpeed = 180.0f;
		private Transform m_transform;
		void Awake () {
			m_transform = transform;
		}
		
		void Update () {
			m_transform.rotation = Quaternion.AngleAxis(m_rotationSpeed*Time.deltaTime, Vector3.up) * m_transform.rotation;
		}
	}
}
