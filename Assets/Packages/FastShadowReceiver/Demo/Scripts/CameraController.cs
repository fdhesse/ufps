using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class CameraController : MonoBehaviour {
		public Transform m_lookAt;
		public float m_speed = 10.0f;
		private float m_distance;
		private float m_cameraAngleX;
		private float m_cameraAngleY;
		private Vector3 m_mousePosition;
		enum MouseButtonState {
			Normal,
			LeftButtonDown,
			RightButtonDown
		}
		private MouseButtonState m_mouseButtonState = MouseButtonState.Normal;
		private Transform m_transform;

		// Use this for initialization
		void Start () {
			m_transform = transform;
			m_transform.LookAt(m_lookAt);
			m_distance = Vector3.Distance(m_lookAt.position, m_transform.position);
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
			if (Input.GetMouseButton(0)) {
				m_mouseButtonState = MouseButtonState.LeftButtonDown;
			}
			else if (Input.GetMouseButton(1)) {
				m_mouseButtonState = MouseButtonState.RightButtonDown;
			}
			else {
				m_mouseButtonState = MouseButtonState.Normal;
			}
			if (m_mouseButtonState != MouseButtonState.Normal) {
				if (Input.GetKey(KeyCode.LeftShift)) {
					m_mouseButtonState = MouseButtonState.RightButtonDown;
				}
				if (Input.touchCount == 3) {
					m_mouseButtonState = MouseButtonState.RightButtonDown;
				}
				else if (Input.touchCount == 2) {
					m_mouseButtonState = MouseButtonState.Normal;
				}
			}
			if (m_mouseButtonState == MouseButtonState.RightButtonDown) {
				Vector3 deltaPos = Input.mousePosition - m_mousePosition;
				Camera cam = GetComponent<Camera>();
	    		float fov = cam.fieldOfView;
				float d = -2.0f * m_distance * Mathf.Tan(0.5f*fov*Mathf.Deg2Rad)/cam.pixelHeight;
				Vector3 dp = m_transform.rotation * deltaPos * d;
				dp.y = 0.0f;
				m_lookAt.position += dp;
			}
			else if (m_mouseButtonState == MouseButtonState.LeftButtonDown) {
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
				zoom = Mathf.Max(0.5f, zoom);
				zoom = Mathf.Min(2.0f, zoom);
				dz = Mathf.Log(zoom);
			}
	    	if (dz != 0.0f) {
	    		m_distance *= Mathf.Exp(dz);
				if (m_distance < 0.01f) {
					m_distance = 0.01f;
				}
	    	}
			float x = Input.GetAxis("Horizontal");
			if (x != 0.0f) {
				Vector3 v = m_transform.right;
				v.y = 0.0f;
				if (0.0001f < v.sqrMagnitude) {
					v.Normalize();
				}
				v *= m_speed * x * Time.deltaTime;
				m_lookAt.position += v;
			}
			float y = Input.GetAxis("Vertical");
			if (y != 0.0f) {
				Vector3 v = Vector3.Cross(m_transform.right, Vector3.up);
				v.y = 0.0f;
				if (0.0001f < v.sqrMagnitude) {
					v.Normalize();
				}
				v *= m_speed * y * Time.deltaTime;
				m_lookAt.position += v;
			}
			m_transform.position = m_transform.rotation * (new Vector3(0.0f, 0.0f, -m_distance)) + m_lookAt.position;
			m_mousePosition = Input.mousePosition;
		}
	}
}
