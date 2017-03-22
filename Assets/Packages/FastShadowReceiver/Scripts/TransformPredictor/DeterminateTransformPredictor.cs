//
// DeterminateTransformPredictor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;

namespace FastShadowReceiver {
	public class DeterminateTransformPredictor : MonoBehaviour, ITransformPredictor {
		public Vector3 averageMove;
		public Vector3 moveRange;
		public Vector3 averageEulerAngle;
		public Vector3 eulerAngleRange;
		public Bounds PredictNextFramePositionChanges()
		{
			Bounds bounds = new Bounds();
			bounds.center = averageMove;
			bounds.extents = moveRange;
			return bounds;
		}
		public Bounds PredictNextFrameEulerAngleChanges()
		{
			Bounds bounds = new Bounds();
			bounds.center = averageEulerAngle;
			bounds.extents = eulerAngleRange;
			return bounds;
		}
	}
}