//
// CustomProjector.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections;

namespace FastShadowReceiver {
	public class CustomProjector : ProjectorBase {
		[SerializeField]
		private bool m_isOrthographic = false;
		[SerializeField]
		private float m_nearClipPlane = 0.1f;
		[SerializeField]
		private float m_farClipPlane = 50.0f;
		[SerializeField]
		private float m_fieldOfView = 30.0f;
		[SerializeField]
		private float m_orthographicSize = 2;
		[SerializeField]
		private float m_aspectRatio = 1;

		public override bool isOrthographic {
			get { return m_isOrthographic; }
		}
		public void SetOrthographic(bool bOrthographic) {
			if (m_isOrthographic != bOrthographic) {
				m_isOrthographic = bOrthographic;
			}
		}
		public override float nearClipPlane {
			get { return m_nearClipPlane; }
		}
		public void SetNearClipPlane(float value)
		{
			m_nearClipPlane = value;
		}
		public override float farClipPlane {
			get { return m_farClipPlane; }
		}
		public void SetFarClipPlane(float value)
		{
			m_farClipPlane = value;
		}
		public float fieldOfView {
			get { return m_fieldOfView; }
			set {
				m_fieldOfView = value;
			}
		}
		public float orthographicSize {
			get { return m_orthographicSize; }
			set {
				m_orthographicSize = value;
			}
		}
		public float aspectRatio {
			get { return m_aspectRatio; }
			set {
				m_aspectRatio = value;
			}
		}
		public override Vector3 position {
			get {
#if UNITY_EDITOR
				if (m_transform == null) {
					m_transform = transform;
				}
#endif
				return m_transform.position;
			}
		}
		public override Vector3 direction {
			get {
#if UNITY_EDITOR
				if (m_transform == null) {
					m_transform = transform;
				}
#endif
				return m_transform.forward;
			}
		}
		public override Quaternion rotation {
			get {
				#if UNITY_EDITOR
				if (m_transform == null) {
					m_transform = transform;
				}
				#endif
				return m_transform.rotation;
			}
		}
		public override Matrix4x4 uvProjectionMatrix
		{
			get {
				#if UNITY_EDITOR
				if (m_transform == null) {
					m_transform = transform;
				}
				#endif
				Matrix4x4 matProjection;
				if (isOrthographic) {
					float x = aspectRatio * orthographicSize;
					float y = orthographicSize;
					matProjection = Matrix4x4.Ortho(-x, x, -y, y, nearClipPlane, farClipPlane);
				}
				else {
					matProjection = Matrix4x4.Perspective(fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
				}
				matProjection.m00 *= 0.5f;
				matProjection.m02 += 0.5f*matProjection.m32;
				matProjection.m03 += 0.5f*matProjection.m33;
				matProjection.m11 *= 0.5f;
				matProjection.m12 += 0.5f*matProjection.m32;
				matProjection.m13 += 0.5f*matProjection.m33;
				float zScale = 1.0f/(m_farClipPlane - m_nearClipPlane);
				matProjection.m22 = zScale;
				matProjection.m23 = -zScale * m_nearClipPlane;
				matProjection = matProjection * m_transform.worldToLocalMatrix;
				return matProjection;
			}
		}
		public override void GetPlaneIntersection (Vector3[] vertices, Plane plane)
		{
#if UNITY_EDITOR
			if (m_transform == null) {
				m_transform = transform;
			}
#endif
			if (m_isOrthographic) {
				float xSize = m_orthographicSize * m_aspectRatio;
				float ySize = m_orthographicSize;
				Vector3 x = xSize * m_transform.right;
				Vector3 y = ySize * m_transform.up;
				Vector3 z = m_transform.forward;
				Vector3 o = m_transform.position;
				Vector3 v0 = o - x - y;
				Vector3 v1 = o - x + y;
				Vector3 v2 = o + x - y;
				Vector3 v3 = o + x + y;
				float invZdotN = 1.0f/Vector3.Dot(z, plane.normal);
				vertices[0] = v0 - (invZdotN * (plane.distance + Vector3.Dot(v0, plane.normal))) * z;
				vertices[1] = v1 - (invZdotN * (plane.distance + Vector3.Dot(v1, plane.normal))) * z;
				vertices[2] = v2 - (invZdotN * (plane.distance + Vector3.Dot(v2, plane.normal))) * z;
				vertices[3] = v3 - (invZdotN * (plane.distance + Vector3.Dot(v3, plane.normal))) * z;
			}
			else {
				float ySize = Mathf.Tan(0.5f*Mathf.Deg2Rad*m_fieldOfView);
				float xSize = ySize * m_aspectRatio;
				Vector3 x = xSize * m_transform.right;
				Vector3 y = ySize * m_transform.up;
				Vector3 z = m_transform.forward;
				Vector3 o = m_transform.position;
				Vector3 v0 = z - x - y;
				Vector3 v1 = z - x + y;
				Vector3 v2 = z + x - y;
				Vector3 v3 = z + x + y;
				float d = Vector3.Dot(plane.normal, o) + plane.distance;
				vertices[0] = o - (d/Vector3.Dot(plane.normal, v0))*v0;
				vertices[1] = o - (d/Vector3.Dot(plane.normal, v1))*v1;
				vertices[2] = o - (d/Vector3.Dot(plane.normal, v2))*v2;
				vertices[3] = o - (d/Vector3.Dot(plane.normal, v3))*v3;
			}
		}
		public override void GetClipPlanes (ref ClipPlanes clipPlanes, Transform clipPlaneTransform)
		{
#if UNITY_EDITOR
			if (m_transform == null) {
				m_transform = transform;
			}
#endif
			Vector3 x = m_transform.right;
			Vector3 y = m_transform.up;
			Vector3 z = m_transform.forward;
			Vector3 o = m_transform.position;
			if (m_isOrthographic) {
				clipPlanes.SetClipPlaneNum(3, 2, true);
				float xSize = m_orthographicSize * m_aspectRatio;
				float ySize = m_orthographicSize;
				clipPlanes.clipPlanes[0] = new Plane(x, o);
				clipPlanes.clipPlanes[0].distance += xSize;
				clipPlanes.maxDistance[0] = 2*xSize;
				clipPlanes.clipPlanes[1] = new Plane(y, o);
				clipPlanes.clipPlanes[1].distance += ySize;
				clipPlanes.maxDistance[1] = 2*ySize;
				clipPlanes.clipPlanes[2] = new Plane(z, o);
				clipPlanes.clipPlanes[2].distance -= m_nearClipPlane;
				clipPlanes.maxDistance[2] = m_farClipPlane - m_nearClipPlane;
			}
			else {
				clipPlanes.SetClipPlaneNum(5, 4, false);
				float ySize = Mathf.Tan(0.5f*Mathf.Deg2Rad*m_fieldOfView);
				float xSize = ySize * m_aspectRatio;
				Vector3 x0 = (x + xSize*z).normalized;
				Vector3 x1 = (-x + xSize*z).normalized;
				Vector3 y0 = (y + ySize*z).normalized;
				Vector3 y1 = (-y + ySize*z).normalized;
				clipPlanes.clipPlanes[0] = new Plane(x0, o);
				clipPlanes.clipPlanes[1] = new Plane(y0, o);
				clipPlanes.clipPlanes[2] = new Plane(x1, o);
				clipPlanes.clipPlanes[3] = new Plane(y1, o);
				clipPlanes.clipPlanes[4] = new Plane(-z, o);
				clipPlanes.clipPlanes[4].distance += m_farClipPlane;
				clipPlanes.maxDistance[0] = clipPlanes.maxDistance[2] = 2.0f * xSize * m_farClipPlane;
				clipPlanes.maxDistance[1] = clipPlanes.maxDistance[3] = 2.0f * ySize * m_farClipPlane;
				clipPlanes.maxDistance[4] = m_farClipPlane - m_nearClipPlane;
			}
			if (clipPlaneTransform != null) {
				Matrix4x4 m = clipPlaneTransform.localToWorldMatrix.transpose;
				Vector3 t = m.GetRow(3);
				for (int i = 0; i < clipPlanes.clipPlaneCount; ++i) {
					float d = Vector3.Dot (clipPlanes.clipPlanes[i].normal, t);
					clipPlanes.clipPlanes[i].distance += d;
					clipPlanes.clipPlanes[i].normal = m.MultiplyVector(clipPlanes.clipPlanes[i].normal);
				}
			}
		}
		public override void GetClipPlanes(ref ClipPlanes clipPlanes, Transform clipPlaneTransform, ITransformPredictor predictor)
		{
#if UNITY_EDITOR
			if (m_transform == null) {
				m_transform = transform;
			}
#endif
			Vector3 x = m_transform.right;
			Vector3 y = m_transform.up;
			Vector3 z = m_transform.forward;
			Bounds angleBounds = predictor.PredictNextFrameEulerAngleChanges();
			if (angleBounds.center != Vector3.zero) {
				Quaternion rot = m_transform.rotation * Quaternion.Euler(angleBounds.center) * Quaternion.Inverse(m_transform.rotation);
				x = rot * x;
				y = rot * y;
				z = rot * z;
			}
			Vector3 o = m_transform.position;
			Bounds moveBounds = predictor.PredictNextFramePositionChanges();
			o += m_transform.TransformDirection(moveBounds.center);
			if (m_isOrthographic) {
				clipPlanes.SetClipPlaneNum(3, 2, true);
				float farExtentX = Mathf.Tan(Mathf.Deg2Rad * Mathf.Min(80, angleBounds.extents.y)) * m_farClipPlane;
				float farExtentY = Mathf.Tan(Mathf.Deg2Rad * Mathf.Min(80, angleBounds.extents.x)) * m_farClipPlane;
				float xSize = m_orthographicSize * m_aspectRatio + farExtentX + moveBounds.extents.x;
				float ySize = m_orthographicSize + farExtentY + moveBounds.extents.y;
				float cosZ = Mathf.Cos(Mathf.Deg2Rad * Mathf.Min(90, angleBounds.extents.z));
				float rcpLen = 1.0f/Mathf.Sqrt(xSize*xSize + ySize*ySize);
				float cosZ_x = Mathf.Max(cosZ, xSize*rcpLen);
				float cosZ_y = Mathf.Max(cosZ, ySize*rcpLen);
				float xSin = Mathf.Sqrt(1.0f - cosZ_y*cosZ_y) * xSize;
				float ySin = Mathf.Sqrt(1.0f - cosZ_x*cosZ_x) * ySize;
				xSize = cosZ_x * xSize + ySin;
				ySize = cosZ_y * ySize + xSin;
				clipPlanes.clipPlanes[0] = new Plane(x, o);
				clipPlanes.clipPlanes[0].distance += xSize;
				clipPlanes.maxDistance[0] = 2*xSize;
				clipPlanes.clipPlanes[1] = new Plane(y, o);
				clipPlanes.clipPlanes[1].distance += ySize;
				clipPlanes.maxDistance[1] = 2*ySize;
				clipPlanes.clipPlanes[2] = new Plane(z, o);
				clipPlanes.clipPlanes[2].distance -= m_nearClipPlane - moveBounds.extents.z;
				clipPlanes.maxDistance[2] = m_farClipPlane - m_nearClipPlane + 2.0f*moveBounds.extents.z;
			}
			else {
				clipPlanes.SetClipPlaneNum(5, 4, false);
				float ySize = Mathf.Tan(Mathf.Deg2Rad*Mathf.Min(0.5f*m_fieldOfView + angleBounds.extents.x, 80)) + moveBounds.extents.y;
				float xSize = Mathf.Tan(0.5f*Mathf.Deg2Rad*m_fieldOfView) * m_aspectRatio;
				float extentX = Mathf.Tan(Mathf.Deg2Rad * Mathf.Min (80, angleBounds.extents.y));
				xSize = (xSize + extentX)/Mathf.Max (0.1f, 1.0f - xSize * extentX) + moveBounds.extents.x;
				float cosZ = Mathf.Cos(Mathf.Deg2Rad * Mathf.Min(90, angleBounds.extents.z));
				float rcpLen = 1.0f/Mathf.Sqrt(xSize*xSize + ySize*ySize);
				float cosZ_x = Mathf.Max(cosZ, xSize*rcpLen);
				float cosZ_y = Mathf.Max(cosZ, ySize*rcpLen);
				float xSin = Mathf.Sqrt(1.0f - cosZ_y*cosZ_y) * xSize;
				float ySin = Mathf.Sqrt(1.0f - cosZ_x*cosZ_x) * ySize;
				xSize = cosZ_x * xSize + ySin;
				ySize = cosZ_y * ySize + xSin;
				o = o - moveBounds.extents.z * z;
				float far = m_farClipPlane + 2.0f*moveBounds.extents.z;
				Vector3 x0 = (x + xSize*z).normalized;
				Vector3 x1 = (-x + xSize*z).normalized;
				Vector3 y0 = (y + ySize*z).normalized;
				Vector3 y1 = (-y + ySize*z).normalized;
				clipPlanes.clipPlanes[0] = new Plane(x0, o);
				clipPlanes.clipPlanes[1] = new Plane(y0, o);
				clipPlanes.clipPlanes[2] = new Plane(x1, o);
				clipPlanes.clipPlanes[3] = new Plane(y1, o);
				clipPlanes.clipPlanes[4] = new Plane(-z, o);
				clipPlanes.clipPlanes[4].distance += far;
				clipPlanes.maxDistance[0] = clipPlanes.maxDistance[2] = 2.0f * xSize * far;
				clipPlanes.maxDistance[1] = clipPlanes.maxDistance[3] = 2.0f * ySize * far;
				clipPlanes.maxDistance[4] = far - m_nearClipPlane;
			}
			if (clipPlaneTransform != null) {
				Matrix4x4 m = clipPlaneTransform.localToWorldMatrix.transpose;
				Vector3 t = m.GetRow(3);
				for (int i = 0; i < clipPlanes.clipPlaneCount; ++i) {
					float d = Vector3.Dot (clipPlanes.clipPlanes[i].normal, t);
					clipPlanes.clipPlanes[i].distance += d;
					clipPlanes.clipPlanes[i].normal = m.MultiplyVector(clipPlanes.clipPlanes[i].normal);
				}
			}
		}
		private Transform m_transform;

		void Awake () {
			m_transform = transform;
		}
	}
}
