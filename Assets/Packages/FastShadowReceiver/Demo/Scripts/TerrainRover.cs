using UnityEngine;

namespace FastShadowReceiver.Demo {
	public class TerrainRover : MonoBehaviour {
		public Terrain m_terrain;
		private Transform m_transform;
		private float m_hoverHeight;
		void Awake()
		{
			m_transform = transform;
			m_hoverHeight = m_transform.position.y;
		}
		public void LateUpdate()
		{
			Vector3 pos = m_transform.position;
			pos.y = m_terrain.SampleHeight(pos) + m_hoverHeight;
			m_transform.position = pos;
		}
	}
}