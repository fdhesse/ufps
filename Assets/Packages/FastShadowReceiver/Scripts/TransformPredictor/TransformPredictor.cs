//
// TransformPredictor.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;

namespace FastShadowReceiver {
	public interface ITransformPredictor {
		/// <summary>
		/// Predicts the next frame position changes.
		/// </summary>
		/// <returns>The bounds of next frame move vector in local coordinates.</returns>
		Bounds PredictNextFramePositionChanges();
		/// <summary>
		/// Predicts the next frame euler angle changes.
		/// </summary>
		/// <returns>The bounds of next frame euler angle changes in local coordinates.</returns>
		Bounds PredictNextFrameEulerAngleChanges();
	}
}
