//
// VelocityTransformPredictor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections;

namespace FastShadowReceiver {
	public class VelocityTransformPredictor : MonoBehaviour, ITransformPredictor {
		public Vector3 averageVelocity;
		public Vector3 velocityRange;
		public Vector3 averageEulerAngleVelocity;
		public Vector3 eulerAngleVelocityRange;
		public Bounds PredictNextFramePositionChanges()
		{
			Bounds bounds = new Bounds();
			bounds.center = Time.deltaTime * averageVelocity;
			bounds.extents = Time.deltaTime * velocityRange;
			return bounds;
		}
		public Bounds PredictNextFrameEulerAngleChanges()
		{
			Bounds bounds = new Bounds();
			bounds.center = Time.deltaTime * averageEulerAngleVelocity;
			bounds.extents = Time.deltaTime * eulerAngleVelocityRange;
			return bounds;
		}
	}
}
