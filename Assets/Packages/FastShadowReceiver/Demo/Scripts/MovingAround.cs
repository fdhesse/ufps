using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class MovingAround : MonoBehaviour, ITransformPredictor {
		public Vector3 m_rotateCenter;
		public float   m_rotationSpeed = 180;

		private Transform m_transform;
		private Vector3   m_nextPosition;
		// Use this for initialization
		void Start () {
			m_transform = transform;
			m_nextPosition = m_transform.position;
		}

		private Vector3 CalculateMoveVector(float dt)
		{
			return Quaternion.AngleAxis(m_rotationSpeed*dt, Vector3.up) * (m_transform.position - m_rotateCenter);
		}
		// Update is called once per frame
		void Update () {
			m_transform.position = m_nextPosition;
			m_nextPosition = m_rotateCenter + CalculateMoveVector(Time.deltaTime);
		}

		// ITransformPredictor interface
		public Bounds PredictNextFramePositionChanges()
		{
			Bounds move = new Bounds();
#if UNITY_EDITOR
			if (!Application.isPlaying) {
				m_transform = transform;
				return move;
			}
#endif
			move.center = m_transform.InverseTransformPoint(m_nextPosition);
			move.extents = Vector3.zero;
			return move;
		}
		public Bounds PredictNextFrameEulerAngleChanges()
		{
			return new Bounds();
		}
	}
}
