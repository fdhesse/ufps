//
// RaycastPlaneShadowReceiver.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections;

namespace FastShadowReceiver {
	public class RaycastPlaneShadowReceiver : PlaneShadowReceiverBase {
		public LayerMask m_raycastMask = -1;
		
		protected override void OnUpdate ()
		{
			if (projector == null) {
				Hide(true);
				return;
			}
			Vector3 origin = projector.position;
			Vector3 dir = projector.direction;
			RaycastHit hit;
			if (Physics.Raycast(origin, dir, out hit, projector.farClipPlane, m_raycastMask)) {
				Hide(false);
				Plane plane = new Plane(hit.normal, hit.point);
				UpdatePlane(plane);
			}
			else {
				Hide(true);
			}
		}
	}
}
