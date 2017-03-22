//
// MeshTreeRaycast.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Threading;

namespace FastShadowReceiver {
	public class MeshTreeRaycast {
		/// <summary>
		/// Gets a value indicating whether the raycast hit the mesh object. 
		/// </summary>
		/// <value><c>true</c> if raycast hit; otherwise, <c>false</c>.</value>
		public bool isHit
		{
			get { return m_hitDistance < m_distance; }
		}

		/// <summary>
		/// Gets the position where the raycast hit.
		/// </summary>
		/// <value>The hit position.</value>
		public Vector3 hitPosition
		{
			get { return m_origin + m_hitDistance * m_direction; }
		}

		/// <summary>
		/// Gets the normal vector of the surface where the raycast hit.
		/// </summary>
		/// <value>The hit normal.</value>
		public Vector3 hitNormal
		{
			get { return m_hitNormal; }
		}

		public float hitDistance
		{
			get { return m_hitDistance; }
		}
		public Vector3 origin
		{
			get { return m_origin; }
		}

		public Vector3 direction
		{
			get { return m_direction; }
		}

		public void Wait()
		{
			if (m_event != null) {
				m_event.WaitOne();
			}
		}
		
		public bool IsDone()
		{
			if (m_event != null) {
				return m_event.WaitOne(0);
			}
			return false;
		}

		private Vector3 m_origin;
		private Vector3 m_direction;
		private float   m_distance;
		private bool    m_cullBackFace;
		private float   m_hitDistance;
		private Vector3 m_hitNormal;

		private ManualResetEvent m_event;
		private MeshTreeBase     m_tree;

		public struct TemporaryParam {
			public Vector3 m_dirsign;
			public Vector3 m_invdir;
		}

#if NETFX_CORE
		private Windows.System.Threading.WorkItemHandler m_workItemHandler;
		public MeshTreeRaycast()
		{
			m_workItemHandler = (source) => this.Raycast();
		}
#else
		private static WaitCallback s_raycastCallback = (arg => ((MeshTreeRaycast)arg).Raycast());
#endif

		/// <summary>
		/// Cast a ray against a mesh tree object in a background thread.
		/// You can wait for the raycast to be done by calling Wait() function.
		/// Also, you can check if the raycast is done or not by calling IsDone() function.
		/// Please be noted that "isHit", "hitPosition" and "hitNormal" properties are invalid until the raycast is done
		/// </summary>
		/// <param name="tree">A MeshTree object.</param>
		/// <param name="origin">The origin point of the ray in the local space of the mesh object.</param>
		/// <param name="direction">The direction of the ray in the local space of the mesh object.</param>
		/// <param name="distance">The length of the ray.</param>
		/// <param name="cullBackFace">If set to <c>true</c> cull back face.</param>
		public void AsyncRaycast(MeshTreeBase tree, Vector3 origin, Vector3 direction, float distance, bool cullBackFace)
		{
			if (m_event == null) {
				m_event = new ManualResetEvent(false);
			}
			m_event.Reset();
			m_tree = tree;
			distance *= direction.magnitude;
			direction.Normalize();
			m_origin = origin;
			m_direction = direction;
			m_distance = distance;
			m_hitDistance = distance;
			m_cullBackFace = cullBackFace;
#if NETFX_CORE
			// we don't need to wait the following async function, because we are using m_event to wait for completion.
			// of course, we can use the return value of the following function call to wait, but we don't want to change code for Windows Store App.
			var suppressWarning = Windows.System.Threading.ThreadPool.RunAsync(m_workItemHandler);
#else
			Nyahoon.ThreadPool.QueueUserWorkItem(s_raycastCallback, this);
#endif
		}
		/// <summary>
		/// Cast a ray against a mesh tree object.
		/// The return value indicates whether the raycast hit or not.
		/// After this function is called, you can access "isHit", "hitPosition" and "hitNormal" properties.
		/// </summary>
		/// <param name="tree">A MeshTree object.</param>
		/// <param name="origin">The origin point of the ray in the local space of the mesh object.</param>
		/// <param name="direction">The direction of the ray in the local space of the mesh object.</param>
		/// <param name="distance">The length of the ray.</param>
		/// <param name="cullBackFace">If set to <c>true</c> cull back face.</param>
		public bool Raycast(MeshTreeBase tree, Vector3 origin, Vector3 direction, float distance, bool cullBackFace)
		{
			m_origin = origin;
			distance *= direction.magnitude;
			direction.Normalize();
			m_direction = direction;
			m_distance = distance;
			m_hitDistance = distance;
			m_cullBackFace = cullBackFace;
			tree.Raycast(this);
			return isHit;
		}
		
		private void Raycast()
		{
			try {
				m_tree.Raycast(this);
				m_tree = null;
			}
			catch (System.Exception e) {
				if (Debug.isDebugBuild || Application.isEditor) {
					Debug.LogException(e);
				}
				throw e;
			}
			finally {
				m_event.Set();
			}
		}

		public TemporaryParam CreateTemporaryParam()
		{
			TemporaryParam param;
			param.m_dirsign.x = m_direction.x < 0.0f ? -1.0f : 1.0f;
			param.m_dirsign.y = m_direction.y < 0.0f ? -1.0f : 1.0f;
			param.m_dirsign.z = m_direction.z < 0.0f ? -1.0f : 1.0f;
			param.m_invdir.x = Mathf.Epsilon < Mathf.Abs(m_direction.x) ? 1.0f/m_direction.x : m_direction.x < 0.0f ? -1.0f/Mathf.Epsilon : 1.0f/Mathf.Epsilon;
			param.m_invdir.y = Mathf.Epsilon < Mathf.Abs(m_direction.y) ? 1.0f/m_direction.y : m_direction.y < 0.0f ? -1.0f/Mathf.Epsilon : 1.0f/Mathf.Epsilon;
			param.m_invdir.z = Mathf.Epsilon < Mathf.Abs(m_direction.z) ? 1.0f/m_direction.z : m_direction.z < 0.0f ? -1.0f/Mathf.Epsilon : 1.0f/Mathf.Epsilon;
			return param;
		}

		public bool BoundsHitTest(Vector3 center, Vector3 extents, TemporaryParam param, out float distance)
		{
			center -= m_origin;
			extents = Vector3.Scale(param.m_dirsign, extents);
			Vector3 min = Vector3.Scale(param.m_invdir, center - extents);
			Vector3 max = Vector3.Scale(param.m_invdir, center + extents);
			float m = Mathf.Max(Mathf.Max(min.x, min.y), min.z);
			float M = Mathf.Min(Mathf.Min(max.x, max.y), max.z);
			distance = m;
			return m <= M && 0.0f <= M && m < m_hitDistance;
		}

		public bool TriangleHitTest(Vector3 v0, Vector3 v1, Vector3 v2)
		{
			Vector3 p = m_origin - v0;
			Vector3 a = v1 - v0;
			Vector3 b = v2 - v0;
			Vector3 axb = Vector3.Cross(a, b);
			float dot = Vector3.Dot(m_direction, axb);
			if (-Mathf.Epsilon < dot && (dot < Mathf.Epsilon || m_cullBackFace)) {
				return false;
			}
			float rdot = 1.0f/dot;
			float distance = -Vector3.Dot(p, axb)*rdot;
			if (distance < 0.0f || m_hitDistance <= distance) {
				return false;
			}
			float u = Vector3.Dot(p, Vector3.Cross(b, m_direction))*rdot;
			float v = Vector3.Dot(p, Vector3.Cross(m_direction, a))*rdot;
			if (u < 0.0f || v < 0.0f || 1.0f < u + v) {
				return false;
			}
			m_hitDistance = distance;
			m_hitNormal = axb.normalized;
			return true;
		}
	}
}
