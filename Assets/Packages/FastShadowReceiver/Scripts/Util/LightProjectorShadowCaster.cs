//
// LightProjectorShadowCaster.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver {
	/// <summary>
	/// Attach this component to a caster object which will be lit by light projector.
	/// </summary>
	public class LightProjectorShadowCaster : MonoBehaviour {
		public enum ProjectionType {
			Sprite,
			Billboard,
			Plane,
		}
		[SerializeField]
		private Texture m_shadowTexture;
		[SerializeField]
		private Vector2 m_extension = Vector2.one;
		[SerializeField]
		private ProjectionType m_projectionType = ProjectionType.Sprite;
		[SerializeField]
		private float m_nearClipPlane = 0.0f;
		[SerializeField]
		private float m_nearClipSharpness = 100.0f;
		[SerializeField]
		private bool m_manualUpdate = false;

		public new Transform transform { get; private set; }
		public Texture shadowTexture
		{
			get { return m_shadowTexture; }
			set { m_shadowTexture = value; }
		}
		public Vector2 extension
		{
			get { return m_extension; }
			set { m_extension = value; }
		}
		public ProjectionType projectionType
		{
			get { return m_projectionType; }
			set { m_projectionType = value; }
		}
		public float nearClipPlane
		{
			get { return m_nearClipPlane; }
			set { m_nearClipPlane = value; }
		}
		public float nearClipSharpness
		{
			get { return m_nearClipSharpness; }
			set { m_nearClipSharpness = value; }
		}
		public bool manualUpdate
		{
			get { return m_manualUpdate; }
			set { m_manualUpdate = value; }
		}
		public Matrix4x4 GetProjectionMatrix(Vector3 lightPos)
		{
			Vector3 pos = transform.position;
			Vector3 x = transform.right;
			Vector3 y = transform.up;
			Vector3 z = transform.forward;
			Vector3 epsilon = 0.001f * z;
			switch (m_projectionType) {
			case ProjectionType.Sprite:
				z = (pos - lightPos).normalized;
				x = x - Vector3.Dot(x, z) * (z + epsilon); // add epsilon to make sure that x never become zero.
				x.Normalize();
				y = y - Vector3.Dot(y, z) * (z + epsilon);
				y.Normalize();
				break;
			case ProjectionType.Billboard:
				z = pos - lightPos;
				z = (z - Vector3.Dot(y, z) * y) + epsilon;
				z.Normalize();
				x = x - Vector3.Dot(x, z) * (z + epsilon);
				x.Normalize();
				break;
			case ProjectionType.Plane:
				break;
			}
			Vector4 rowX = x;
			Vector4 rowY = y;
			Vector4 rowZ = z;
			Vector4 rowW;
			rowX.w = -Vector3.Dot(x, lightPos);
			rowY.w = -Vector3.Dot(y, lightPos);
			rowZ.w = -Vector3.Dot(z, pos) - m_nearClipPlane;
			rowZ *= m_nearClipSharpness;
			rowW = z; rowW.w = -Vector3.Dot(z, lightPos);
			Vector4 origin = pos; origin.w = 1.0f;
			float ow = Vector4.Dot(rowW, origin);
			rowX *= 0.5f*ow/m_extension.x;
			rowY *= 0.5f*ow/m_extension.y;
			float ox = Vector4.Dot(rowX, origin);
			float oy = Vector4.Dot(rowY, origin);
			float invW = 1.0f/ow;
			float transX = 0.5f - ox*invW;
			float transY = 0.5f - oy*invW;
			rowX += transX * rowW;
			rowY += transY * rowW;
			Matrix4x4 mat = Matrix4x4.zero;
			mat.SetRow(0, rowX);
			mat.SetRow(1, rowY);
			mat.SetRow(2, rowZ);
			mat.SetRow(3, rowW);
			return mat;
		}
		public Matrix4x4 GetOrthoProjectionMatrix(Vector3 lightDir)
		{
			Vector3 pos = transform.position;
			Vector3 x = transform.right;
			Vector3 y = transform.up;
			Vector3 z = lightDir;
			Vector3 epsilon = 0.001f * transform.forward;
			switch (m_projectionType) {
			case ProjectionType.Sprite:
				x = x - Vector3.Dot(x, z) * (z + epsilon); // add epsilon to make sure that x never become zero.
				x.Normalize();
				y = y - Vector3.Dot(y, z) * (z + epsilon);
				y.Normalize();
				break;
			case ProjectionType.Billboard:
				x = x - Vector3.Dot(x, z) * (z + epsilon);
				x.Normalize();
				{
					Vector3 n = Vector3.Cross(x, y);
					y = y - (Vector3.Dot(y, z)/Vector3.Dot(z, n)) * n;
				}
				break;
			case ProjectionType.Plane:
				{
					Vector3 n = Vector3.Cross(x, y);
					float a = 1.0f/Vector3.Dot(z, n);
					x = x - (a * Vector3.Dot(x, z)) * n;
					y = y - (a * Vector3.Dot(y, z)) * n;
				}
				break;
			}
			Vector4 rowX = x;
			Vector4 rowY = y;
			Vector4 rowZ = z;
			Vector4 rowW;
			rowX.w = -Vector3.Dot(x, pos);
			rowY.w = -Vector3.Dot(y, pos);
			rowZ.w = -Vector3.Dot(z, pos);
			rowW = Vector4.zero; rowW.w = 1.0f;
			rowX *= 0.5f/m_extension.x;
			rowY *= 0.5f/m_extension.y;
			rowX.w += 0.5f;
			rowY.w += 0.5f;
			Matrix4x4 mat = Matrix4x4.zero;
			mat.SetRow(0, rowX);
			mat.SetRow(1, rowY);
			mat.SetRow(2, rowZ);
			mat.SetRow(3, rowW);
			return mat;
		}
		public void GetShadowPlaneAxes(Vector3 lightDir, out Vector3 x, out Vector3 y)
		{
			x = transform.right;
			y = transform.up;
			Vector3 epsilon = 0.001f * transform.forward;
			switch (m_projectionType) {
			case ProjectionType.Sprite:
				x = x - Vector3.Dot(x, lightDir) * (lightDir + epsilon); // add epsilon to make sure that x never become zero.
				x.Normalize();
				y = y - Vector3.Dot(y, lightDir) * (lightDir + epsilon);
				y.Normalize();
				break;
			case ProjectionType.Billboard:
				lightDir = (lightDir - Vector3.Dot(y, lightDir) * y) + epsilon;
				lightDir.Normalize();
				x = x - Vector3.Dot(x, lightDir) * (lightDir + epsilon);
				x.Normalize();
				break;
			case ProjectionType.Plane:
				break;
			}
		}
		public static List<LightProjectorShadowCaster> GetAllCasters()
		{
			return s_listCasters;
		}
		private static List<LightProjectorShadowCaster> s_listCasters = new List<LightProjectorShadowCaster>();
		void Awake()
		{
			transform = base.transform;
		}
		void OnEnable()
		{
			if (!s_listCasters.Contains(this)) {
				s_listCasters.Add(this);
			}
		}
		void OnDisable()
		{
			s_listCasters.Remove(this);
		}
	}
}
