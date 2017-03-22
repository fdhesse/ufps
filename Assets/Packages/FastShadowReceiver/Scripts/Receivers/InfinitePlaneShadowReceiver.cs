//
// InfinitePlaneShadowReceiver.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;

namespace FastShadowReceiver {
	public class InfinitePlaneShadowReceiver : PlaneShadowReceiverBase {
		public Transform m_target;
		public Vector3   m_normal = Vector3.up;
		public float     m_height = 0.0f;
		
		protected override void OnUpdate ()
		{
			if (projector == null) {
				Hide(true);
				return;
			}
			else {
				Hide(false);
			}
			Plane plane;
			if (m_target != null) {
				plane = new Plane(m_target.TransformDirection(m_normal).normalized, m_target.position);
				plane.distance -= m_height;
			}
			else {
				plane = new Plane(m_normal, -m_height);
			}
			UpdatePlane(plane);
		}
	}
}
