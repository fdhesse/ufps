using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class MovingToward : MonoBehaviour, ITransformPredictor {
		public Transform m_target;
		public Bounds    m_randomTargetRange;
		public float     m_speed;

		private Transform m_transform;
		private Vector3   m_nextPosition;
		private Vector3   m_velocity;

		void Reset () {
			Vector3 target = m_target.position;
			Vector3 min = m_randomTargetRange.min;
			Vector3 max = m_randomTargetRange.max;
			target.x += Random.Range(min.x, max.x);
			target.z += Random.Range(min.z, max.z);
			m_transform = transform;
			m_velocity = target - m_transform.position;
			m_velocity.y = 0.0f;
			m_velocity.Normalize();
			m_velocity *= m_speed;
			m_nextPosition = m_transform.position + m_velocity * Time.deltaTime;
		}

		void Start () {
			Reset();
		}

		void OnEnable() {
			Reset();
		}
		
		void Update () {
			m_transform.position = m_nextPosition;
			m_nextPosition = m_transform.position + m_velocity * Time.deltaTime;
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
