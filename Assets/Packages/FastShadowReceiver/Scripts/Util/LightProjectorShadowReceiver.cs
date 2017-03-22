//
// LightProjectorShadowReceiver.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver {
	/// <summary>
	/// Receives shadows from LightProjectorShadowCaster objects.
	/// Attach this component to light projector(UnityEngine.Projector) or shadow receiver object to render shadows.
	/// The shadow receiver object should have ProjectionReceiverRenderer component as well.
	/// 
	/// If this component is attached to a light projector object, all the objects lit by the light projector will receive shadows.
	/// If this component is attached to a shadow receiver object, only the shadow receiver object will receive shadows.
	/// </summary>
	public class LightProjectorShadowReceiver : MonoBehaviour {
		public bool m_manualUpdate = false;
		public LayerMask m_shadowCasterMask = -1; // default: everything

		private static int[] s_shadowProjectionMatrixPropertyIDs;
		private static int[] s_shadowTexturePropertyIDs;
		private static string[] s_shadowNumKeyword = null;

		private int m_shadowCasterCount;
		private int m_prevShadowCasterCount;
		private LightProjectorShadowCaster[] m_shadowCasters;
		private LightProjectorShadowCaster[] m_prevShadowCasters;
		private Material m_material;
		private IProjector m_projectorInterface;
		private ClipPlanes m_clipPlanes;
		private int m_maxShadowNum;
		void Awake () {
			m_shadowCasterCount = 0;
			m_prevShadowCasterCount = 0;
			m_shadowCasters = null;
			m_prevShadowCasters = null;
			m_material = null;
			m_maxShadowNum = 0;
		}
		void Start() {
			Component lightProjector = GetComponent<Projector>();
			ReceiverBase receiver = null;
			if (lightProjector == null) {
				receiver = GetComponent<ReceiverBase>();
				if (receiver != null) {
					m_projectorInterface = receiver.projectorInterface;
					lightProjector = receiver.unityProjector;
					if (lightProjector == null) {
						lightProjector = receiver.customProjector;
					}
				}
			}
			Renderer r = GetComponent<Renderer>();
			if (r != null && (lightProjector == null || lightProjector.gameObject != gameObject)) {
				// if light projector is not this game object, cast shadows on only this game object.
				m_material = r.material;
			}
			if (m_material == null) {
				if (lightProjector is Projector) {
					Projector projector = (Projector)lightProjector;
					m_material = new Material(projector.material);
					projector.material = m_material;
				}
				if (m_material == null && (Debug.isDebugBuild || Application.isEditor)) {
					Debug.LogError("No material was found!", this);
				}
			}
			m_maxShadowNum = int.Parse(m_material.GetTag("FSRMaxShadowNum", false, "0"));
			if (m_projectorInterface == null) {
				if (lightProjector is IProjector) {
					m_projectorInterface = lightProjector as IProjector;
				}
				else if (lightProjector is Projector) {
					m_projectorInterface = new UnityProjector(lightProjector as Projector);
				}
				else if (Debug.isDebugBuild || Application.isEditor) {
					Debug.LogError("No light projector!", this);
				}
			}
			if (s_shadowNumKeyword == null || s_shadowNumKeyword.Length <= m_maxShadowNum) {
				s_shadowNumKeyword = new string[m_maxShadowNum + 1];
				for (int i = 0; i < m_maxShadowNum + 1; ++i) {
					s_shadowNumKeyword[i] = "FSR_SHADOW_PROJECTOR_COUNT_" + i;
				}
			}
			if (s_shadowProjectionMatrixPropertyIDs == null || s_shadowProjectionMatrixPropertyIDs.Length < m_maxShadowNum) {
				s_shadowProjectionMatrixPropertyIDs = new int[m_maxShadowNum];
				s_shadowTexturePropertyIDs = new int[m_maxShadowNum];
				for (int i = 0; i < m_maxShadowNum; ++i) {
					s_shadowProjectionMatrixPropertyIDs[i] = Shader.PropertyToID("_ShadowProjector" + (i+1).ToString());
					s_shadowTexturePropertyIDs[i] = Shader.PropertyToID("_ShadowTex" + (i+1).ToString());
				}
			}
			if (m_shadowCasters == null || m_shadowCasters.Length < m_maxShadowNum) {
				m_shadowCasters = new LightProjectorShadowCaster[m_maxShadowNum];
			}
			if (m_prevShadowCasters == null || m_prevShadowCasters.Length < m_maxShadowNum) {
				m_prevShadowCasters = new LightProjectorShadowCaster[m_maxShadowNum];
			}
		}
		void LateUpdate () {
			if (!m_manualUpdate) {
				UpdateShadows();
			}
		}
		public void UpdateShadows() {
			m_projectorInterface.GetClipPlanes(ref m_clipPlanes, null);
			for (int i = 0; i < m_shadowCasterCount; ++i) {
				m_prevShadowCasters[i] = m_shadowCasters[i];
			}
			for (int i = 0; i < m_shadowCasterCount; ++i) {
				LightProjectorShadowCaster caster = m_shadowCasters[i];
				if ((m_shadowCasterMask.value & (1 << caster.gameObject.layer)) == 0) {
					RemoveCaster(m_shadowCasters[i]);
					--i;
				}
				if (!IsInProjectionVolume(caster)) {
					RemoveCaster(caster);
					--i;
				}
			}
			List<LightProjectorShadowCaster> casters = LightProjectorShadowCaster.GetAllCasters();
			for (int i = 0; i < casters.Count && m_shadowCasterCount < m_maxShadowNum; ++i) {
				LightProjectorShadowCaster caster = casters[i];
				if ((m_shadowCasterMask.value & (1 << caster.gameObject.layer)) == 0) {
					continue;
				}
				bool alreadyExist = false;
				for (int j = 0; j < m_prevShadowCasterCount; ++j) {
					if (caster == m_prevShadowCasters[j]) {
						alreadyExist = true;
						break;
					}
				}
				if ((!alreadyExist) && IsInProjectionVolume(caster)) {
					AddCaster(caster);
				}
			}
			Vector3 lightPos;
			if (m_projectorInterface.isOrthographic) {
				lightPos = m_projectorInterface.direction;
			}
			else {
				lightPos = m_projectorInterface.position;
			}
			for (int i = 0; i < m_shadowCasterCount; ++i) {
				Matrix4x4 mat;
				if (m_projectorInterface.isOrthographic) {
					mat = m_shadowCasters[i].GetOrthoProjectionMatrix(lightPos);
				}
				else {
					mat = m_shadowCasters[i].GetProjectionMatrix(lightPos);
				}
				m_material.SetMatrix(s_shadowProjectionMatrixPropertyIDs[i], mat);
				m_material.SetTexture(s_shadowTexturePropertyIDs[i], m_shadowCasters[i].shadowTexture);
			}
			if (m_prevShadowCasterCount != m_shadowCasterCount) {
				m_material.DisableKeyword(s_shadowNumKeyword[m_prevShadowCasterCount]);
				m_material.EnableKeyword(s_shadowNumKeyword[m_shadowCasterCount]);
				m_prevShadowCasterCount = m_shadowCasterCount;
			}
		}
		private bool IsInProjectionVolume(LightProjectorShadowCaster caster)
		{
			Transform shadowTransform = caster.transform;
			Vector3 shadowPos = shadowTransform.position;
			Vector3 x;
			Vector3 y;
			Vector3 lightDir;
			if (m_projectorInterface.isOrthographic) {
				lightDir = m_projectorInterface.direction;
			}
			else {
				lightDir = (shadowPos - m_projectorInterface.position).normalized;
			}
			caster.GetShadowPlaneAxes(lightDir, out x, out y);
			float maxExtension = caster.extension.magnitude;
			for (int plane = 0; plane < m_clipPlanes.clipPlaneCount; ++plane) {
				Plane p = m_clipPlanes.clipPlanes[plane];
				float d = p.GetDistanceToPoint(shadowPos);
				if (d < 0) {
					if (d < - maxExtension) {
						return false;
					}
					float ext = Mathf.Abs(caster.extension.x * Vector3.Dot(x, p.normal)) + Mathf.Abs(caster.extension.y * Vector3.Dot(y, p.normal));
					if (d < -ext) {
						return false;
					}
				}
				else if (m_clipPlanes.twoSideClipping && m_clipPlanes.maxDistance[plane] < d) {
					if (m_clipPlanes.maxDistance[plane] + maxExtension < d) {
						return false;
					}
					float ext = Mathf.Abs(caster.extension.x * Vector3.Dot(x, p.normal)) + Mathf.Abs(caster.extension.y * Vector3.Dot(y, p.normal));
					if (m_clipPlanes.maxDistance[plane] + ext < d) {
						return false;
					}
				}
			}
			return true;
		}
		private void AddCaster(LightProjectorShadowCaster caster)
		{
			if (m_shadowCasterCount < m_maxShadowNum) {
				for (int i = 0; i < m_shadowCasterCount; ++i) {
					if (m_shadowCasters[i] == caster) {
						return;
					}
				}
				m_shadowCasters[m_shadowCasterCount++] = caster;
			}
		}
		private void RemoveCaster(LightProjectorShadowCaster caster)
		{
			for (int i = 0; i < m_shadowCasterCount; ++i) {
				if (m_shadowCasters[i] == caster) {
					m_shadowCasters[i] = m_shadowCasters[--m_shadowCasterCount];
					m_shadowCasters[m_shadowCasterCount] = null;
					break;
				}
			}
		}
	}
}
