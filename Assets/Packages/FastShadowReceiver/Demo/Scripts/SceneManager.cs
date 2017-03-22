using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver.Demo {
	public class SceneManager : MonoBehaviour {
		public GameObject m_environment;
		public GameObject[] m_objectsToSpawn;
		public float m_environmentTileSize = 10.0f;
		public float m_objectLifeSpan = 5.0f;
		public float m_objectSpawnRate = 5.0f;
		public Bounds m_spawnBounds = new Bounds(Vector3.zero, new Vector3(20, 20, 20));
		public Transform m_playerTransform;
		public LayerMask m_spawnedProjectorLayers;

		[System.Serializable] // make it serializable so that it can be restored when script files are rebuilt while scene is running
		struct SpawnedObject {
			public float m_time;
			public GameObject m_object;
		}
		private List<SpawnedObject> m_objectInstances;
		private List<GameObject> m_freeObjectInstances;
		private float m_spawnCount = 0.0f;
		private int m_currentProjectorLayer = -1;

		void Awake()
		{
			m_objectInstances = new List<SpawnedObject>();
			m_freeObjectInstances = new List<GameObject>();
			for (int i = 0; i < m_objectsToSpawn.Length; ++i) {
				m_objectsToSpawn[i].SetActive(false);
				AssignProjectorLayer(m_objectsToSpawn[i]);
			}
			m_freeObjectInstances.AddRange(m_objectsToSpawn);

			// Put environment objects at runtime as an example of dynamically generated level.
			// If you don't need to generate levels at runtime, you can register receivers in Inspector View.
			RegisterReceiversInChildren(m_environment);
			Vector3 envPos = m_environment.transform.position;
			GameObject env = Instantiate(m_environment) as GameObject;
			CopyLightmapScaleOffset(env.transform, m_environment.transform);
			envPos.z += m_environmentTileSize;
			env.transform.position = envPos;
			RegisterReceiversInChildren(env);
			env = Instantiate(m_environment) as GameObject;
			CopyLightmapScaleOffset(env.transform, m_environment.transform);
			envPos.z += m_environmentTileSize;
			env.transform.position = envPos;
			RegisterReceiversInChildren(env);
		}

		// Update is called once per frame
		void Update()
		{
			// update timer and kill old objects.
			for (int i = 0; i < m_objectInstances.Count; ++i) {
				SpawnedObject obj = m_objectInstances[i];
				obj.m_time += Time.deltaTime;
				m_objectInstances[i] = obj;
			}
			KillObjects();
			// spawn new objects
			m_spawnCount += Time.deltaTime * m_objectSpawnRate;
			while (1.0f < m_spawnCount) {
				SpawnObject();
				m_spawnCount -= 1.0f;
			}
		}

		void RegisterReceiversInChildren(GameObject env)
		{
			ReceiverBase[] receivers = env.GetComponentsInChildren<ReceiverBase>(true);
			if (receivers != null && 0 < receivers.Length) {
				for (int i = 0; i < receivers.Length; ++i) {
					receivers[i].gameObject.SetActive(true);
					ProjectorManager.Instance.AddReceiver(receivers[i]);
				}
			}
		}

		void CopyLightmapScaleOffset(Transform dst, Transform src)
		{
			Renderer srcRenderer = src.GetComponent<Renderer>();
			if (srcRenderer != null) {
				Renderer dstRenderer = dst.GetComponent<Renderer>();
				if (dstRenderer != null) {
					dstRenderer.lightmapIndex = srcRenderer.lightmapIndex;
					dstRenderer.lightmapScaleOffset = srcRenderer.lightmapScaleOffset;
				}
			}
			for (int i = 0; i < src.childCount && i < dst.childCount; ++i) {
				CopyLightmapScaleOffset(dst.GetChild(i), src.GetChild(i));
			}
		}
		void SpawnObject()
		{
			// randomly spawn object. if the object has AutoProjector component, the projector will be automatically added into ProjectorManager.
			Vector3 min = m_spawnBounds.min;
			Vector3 max = m_spawnBounds.max;
			Vector3 randomPos = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
			GameObject newObject;
			if (0 < m_freeObjectInstances.Count) {
				newObject = m_freeObjectInstances[m_freeObjectInstances.Count-1];
				m_freeObjectInstances.RemoveAt(m_freeObjectInstances.Count - 1);
			}
			else {
				newObject = Instantiate(m_objectsToSpawn[Random.Range(0, m_objectsToSpawn.Length)]) as GameObject;
				AssignProjectorLayer(newObject);
			}
			newObject.transform.position = randomPos;
			newObject.SetActive(true);
			SpawnedObject obj = new SpawnedObject();
			obj.m_time = 0.0f;
			obj.m_object = newObject;
			m_objectInstances.Add(obj);
		}
		void KillObjects()
		{
			while (0 < m_objectInstances.Count && m_objectLifeSpan < m_objectInstances[0].m_time) {
				m_objectInstances[0].m_object.SetActive(false);
				m_freeObjectInstances.Add(m_objectInstances[0].m_object);
				m_objectInstances.RemoveAt(0);
			}
		}
		void AssignProjectorLayer(GameObject obj)
		{
			if (m_spawnedProjectorLayers != 0) {
				do {
					++m_currentProjectorLayer;
					if (32 < m_currentProjectorLayer || m_spawnedProjectorLayers < (1 << m_currentProjectorLayer)) {
						m_currentProjectorLayer = 0;
					}
				} while ((m_spawnedProjectorLayers & (1 << m_currentProjectorLayer)) == 0);
				obj.layer = m_currentProjectorLayer;
				Projector[] projectors = obj.GetComponentsInChildren<Projector>(true);
				if (projectors != null) {
					for (int i = 0; i < projectors.Length; ++i) {
						projectors[i].ignoreLayers &= ~m_spawnedProjectorLayers;
						projectors[i].ignoreLayers |= (1 << m_currentProjectorLayer); // do not project shadow on its self.
					}
				}
			}
		}
		// called from RandomSpawnTest.cs, just for test
		public void SetProjectorMaterial(Material material)
		{
			for (int i = 0; i < m_objectInstances.Count; ++i) {
				Projector[] projectors = m_objectInstances[i].m_object.GetComponentsInChildren<Projector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].material = new Material(material);
					}
				}
			}
			for (int i = 0; i < m_freeObjectInstances.Count; ++i) {
				Projector[] projectors = m_freeObjectInstances[i].GetComponentsInChildren<Projector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].material = new Material(material);
					}
				}
			}
		}
		public void SetProjectorIgnoreLayers(int layerMask)
		{
			for (int i = 0; i < m_objectInstances.Count; ++i) {
				Projector[] projectors = m_objectInstances[i].m_object.GetComponentsInChildren<Projector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].ignoreLayers |= layerMask;
					}
				}
			}
			for (int i = 0; i < m_freeObjectInstances.Count; ++i) {
				Projector[] projectors = m_freeObjectInstances[i].GetComponentsInChildren<Projector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].ignoreLayers |= layerMask;
					}
				}
			}
		}
		public void ResetProjectorIgnoreLayers(int layerMask)
		{
			for (int i = 0; i < m_objectInstances.Count; ++i) {
				Projector[] projectors = m_objectInstances[i].m_object.GetComponentsInChildren<Projector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].ignoreLayers &= ~layerMask;
					}
				}
			}
			for (int i = 0; i < m_freeObjectInstances.Count; ++i) {
				Projector[] projectors = m_freeObjectInstances[i].GetComponentsInChildren<Projector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].ignoreLayers &= ~layerMask;
					}
				}
			}
		}
		public void ForceEnableAutoProjectors()
		{
			for (int i = 0; i < m_objectInstances.Count; ++i) {
				AutoProjector[] projectors = m_objectInstances[i].m_object.GetComponentsInChildren<AutoProjector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].enabled = true;
					}
				}
			}
			for (int i = 0; i < m_freeObjectInstances.Count; ++i) {
				AutoProjector[] projectors = m_freeObjectInstances[i].GetComponentsInChildren<AutoProjector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].enabled = true;
					}
				}
			}
		}
		public void ForceDisableAutoProjectors()
		{
			for (int i = 0; i < m_objectInstances.Count; ++i) {
				AutoProjector[] projectors = m_objectInstances[i].m_object.GetComponentsInChildren<AutoProjector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].enabled = false;
						projectors[j].projector.projector.material.SetFloat("_Alpha", 1.0f);
						projectors[j].projector.projector.enabled = true;
					}
				}
			}
			for (int i = 0; i < m_freeObjectInstances.Count; ++i) {
				AutoProjector[] projectors = m_freeObjectInstances[i].GetComponentsInChildren<AutoProjector>(true);
				if (projectors != null) {
					for (int j = 0; j < projectors.Length; ++j) {
						projectors[j].enabled = false;
						projectors[j].projector.projector.material.SetFloat("_Alpha", 1.0f);
						projectors[j].projector.projector.enabled = true;
					}
				}
			}
		}
	}
}
