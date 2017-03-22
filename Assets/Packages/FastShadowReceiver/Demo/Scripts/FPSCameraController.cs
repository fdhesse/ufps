using UnityEngine;
using System.Collections;

namespace FastShadowReceiver.Demo {
	public class FPSCameraController : MonoBehaviour {
		public float m_speed = 10.0f;
		private float m_cameraAngleX;
		private float m_cameraAngleY;
		private Vector3 m_mousePosition;
		enum MouseButtonState {
			Normal,
			MouseButtonDown,
		}
		private MouseButtonState m_mouseButtonState = MouseButtonState.Normal;
		private Transform m_transform;
		
		// Use this for initialization
		void Start () {
			m_transform = transform;
			Vector3 eulerAngles = m_transform.eulerAngles;
			m_cameraAngleX = eulerAngles.y;
			m_cameraAngleY = eulerAngles.x;
			m_mouseButtonState = MouseButtonState.Normal;
			m_mousePosition = Input.mousePosition;
		}
		
		// Update is called once per frame
		void Update () {
			if (m_mouseButtonState == MouseButtonState.Normal) {
				m_mousePosition = Input.mousePosition;
			}
			if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && Input.touchCount < 2) {
				m_mouseButtonState = MouseButtonState.MouseButtonDown;
			}
			else {
				m_mouseButtonState = MouseButtonState.Normal;
			}
			if (m_mouseButtonState == MouseButtonState.MouseButtonDown) {
				Vector3 deltaPos = Input.mousePosition - m_mousePosition;
				m_cameraAngleX -= deltaPos.x * 180.0f/Screen.width;
				m_cameraAngleY += deltaPos.y * 180.0f/Screen.width;
				if (180.0f < m_cameraAngleX) {
					m_cameraAngleX -= 360.0f;
				}
				else if (m_cameraAngleX < -180.0f) {
					m_cameraAngleX += 360.0f;
				}
				if (180.0f < m_cameraAngleY) {
					m_cameraAngleY -= -360.0f;
				}
				else if (m_cameraAngleY < -180.0f) {
					m_cameraAngleY += 360.0f;
				}
				m_transform.rotation = Quaternion.Euler(m_cameraAngleY, m_cameraAngleX, 0);
			}
			float dz = Input.GetAxis("Mouse ScrollWheel");
			if (Input.touchCount == 2) {
				Vector2 p1 = (Input.touches[0].position - Input.touches[1].position);
				Vector2 p2 = p1 - (Input.touches[0].deltaPosition - Input.touches[1].deltaPosition);
				float zoom = p2.magnitude/p1.magnitude;
				dz = 5.0f * Mathf.Log(zoom);
			}
			if (dz != 0.0f) {
				Vector3 dp = m_transform.forward * dz;
				m_transform.position += dp;
			}
			float x = Input.GetAxis("Horizontal");
			if (x != 0.0f) {
				Vector3 v = m_transform.right;
				v *= m_speed * x * Time.deltaTime;
				m_transform.position += v;
			}
			float y = Input.GetAxis("Vertical");
			if (y != 0.0f) {
				Vector3 v = m_transform.forward;
				v *= m_speed * y * Time.deltaTime;
				m_transform.position += v;
			}
			m_mousePosition = Input.mousePosition;
		}

	}
}
