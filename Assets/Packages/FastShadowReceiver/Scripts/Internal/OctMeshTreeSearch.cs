//
// OctMeshTreeSearch.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver {
	public class OctMeshTreeSearch : MeshTreeSearch {
		struct ClipFlags {
			public byte all;
			public byte xFlags;
			public byte yFlags;
			public byte zFlags;
		}
		internal const byte FLAGS_LEFT   = (1 << 0) | (1 << 2) | (1 << 4) | (1 << 6);
		internal const byte FLAGS_RIGHT  = (1 << 1) | (1 << 3) | (1 << 5) | (1 << 7);
		internal const byte FLAGS_BOTTOM = (1 << 0) | (1 << 1) | (1 << 4) | (1 << 5);
		internal const byte FLAGS_TOP    = (1 << 2) | (1 << 3) | (1 << 6) | (1 << 7);
		internal const byte FLAGS_FRONT  = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3);
		internal const byte FLAGS_BACK   = (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7);
		private ClipFlags[]    m_clipFlags;
		private ClipFlags[]    m_clipFlags2;

		private HashSet<int> m_triangleSet;
		private List<int> m_triangleList;
		
		internal void Initialize()
		{
			int numClipPlanes = m_clipPlanes.clipPlaneCount;
			if (m_clipMetric == null || m_clipMetric.Length < numClipPlanes) {
				m_clipMetric = new Vector3[numClipPlanes];
			}
			if (m_clipFlags == null || m_clipFlags.Length < numClipPlanes) {
				m_clipFlags = new ClipFlags[numClipPlanes];
			}
			if (m_clipPlanes.twoSideClipping && (m_clipFlags2 == null || m_clipFlags2.Length < numClipPlanes)) {
				m_clipFlags2 = new ClipFlags[numClipPlanes];
			}
			for (int i = 0; i < numClipPlanes; ++i) {
				Plane clipPlane = m_clipPlanes.clipPlanes[i];
				m_clipMetric[i].x = Mathf.Abs(clipPlane.normal.x);
				m_clipMetric[i].y = Mathf.Abs(clipPlane.normal.y);
				m_clipMetric[i].z = Mathf.Abs(clipPlane.normal.z);
				if (clipPlane.normal.x < 0) {
					m_clipFlags[i].xFlags = FLAGS_LEFT;
				}
				else {
					m_clipFlags[i].xFlags = FLAGS_RIGHT;
				}
				if (clipPlane.normal.y < 0) {
					m_clipFlags[i].yFlags = FLAGS_BOTTOM;
				}
				else {
					m_clipFlags[i].yFlags = FLAGS_TOP;
				}
				if (clipPlane.normal.z < 0) {
					m_clipFlags[i].zFlags = FLAGS_FRONT;
				}
				else {
					m_clipFlags[i].zFlags = FLAGS_BACK;
				}
				m_clipFlags[i].all = (byte)(m_clipFlags[i].xFlags|m_clipFlags[i].yFlags|m_clipFlags[i].zFlags);
				if (m_clipPlanes.twoSideClipping) {
					m_clipFlags2[i].xFlags = (byte)~m_clipFlags[i].xFlags;
					m_clipFlags2[i].yFlags = (byte)~m_clipFlags[i].yFlags;
					m_clipFlags2[i].zFlags = (byte)~m_clipFlags[i].zFlags;
					m_clipFlags2[i].all = (byte)(m_clipFlags2[i].xFlags|m_clipFlags2[i].yFlags|m_clipFlags2[i].zFlags);
				}
			}
			if (m_triangleList == null) {
				m_triangleList = new List<int>();
			}
			if (m_triangleSet == null) {
				m_triangleSet = new HashSet<int>();
			}
			m_triangleList.Clear();
			m_triangleSet.Clear();
			m_minBounds = Vector3.zero;
			m_maxBounds = Vector3.zero;
		}
		internal void Finalize(Vector3[] vertices, int[] indices)
		{
			int nVertexCount = 0;
			int nIndexCount = 0;
			int numTriangles = m_triangleList.Count;
			int scissoredTriangleCount = 0;
			if (m_bScissor) {
				InitScissorBuffer(numTriangles);
				ScissoredTriangleCount scissorCount = new ScissoredTriangleCount();
				if (m_bBackfaceCulling) {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						Vector3 v0 = vertices[indices[index++]];
						Vector3 v1 = vertices[indices[index++]];
						Vector3 v2 = vertices[indices[index++]];
						if (isFrontFaceTriangle(v0, v1, v2)) {
							ScissorTriangle(v0, v1, v2, ref scissorCount, true);
						}
					}
				}
				else {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						Vector3 v0 = vertices[indices[index++]];
						Vector3 v1 = vertices[indices[index++]];
						Vector3 v2 = vertices[indices[index++]];
						ScissorTriangle(v0, v1, v2, ref scissorCount, true);
					}
				}
				nVertexCount = scissorCount.m_nVertexCount;
				nIndexCount = scissorCount.m_nIndexCount;
				scissoredTriangleCount = scissorCount.m_nTriangleCount;
			}
			else {
				nVertexCount = 3*numTriangles;
				nIndexCount = 3*numTriangles;
			}
			// create result buffer
			InitResultBuffer(nVertexCount, nIndexCount, false);
			m_bOutputNormals = false;
			// fill result buffer
			int nVertex = 0;
			int nIndex = 0;
			if (m_bScissor) {
				FillResultWithScissoredTriangles(scissoredTriangleCount, ref nVertex, ref nIndex);
			}
			else {
				if (m_bBackfaceCulling) {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						Vector3 v0 = vertices[indices[index++]];
						Vector3 v1 = vertices[indices[index++]];
						Vector3 v2 = vertices[indices[index++]];
						if (isFrontFaceTriangle(v0, v1, v2)) {
							m_resultIndices[nIndex++] = nVertex;
							m_result[nVertex++] = v0;
							m_resultIndices[nIndex++] = nVertex;
							m_result[nVertex++] = v1;
							m_resultIndices[nIndex++] = nVertex;
							m_result[nVertex++] = v2;
						}
					}
				}
				else {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						m_resultIndices[nIndex++] = nVertex;
						m_result[nVertex++] = vertices[indices[index++]];
						m_resultIndices[nIndex++] = nVertex;
						m_result[nVertex++] = vertices[indices[index++]];
						m_resultIndices[nIndex++] = nVertex;
						m_result[nVertex++] = vertices[indices[index++]];
					}
				}
			}
			int lastIndex = 0 < nIndex ? m_resultIndices[nIndex-1] : 0;
			while (nIndex < m_resultIndices.Length) {
				m_resultIndices[nIndex++] = lastIndex;
			}
		}
		internal void Finalize(Vector3[] vertices, int[] indices, Vector3[] normals)
		{
			int nVertexCount = 0;
			int nIndexCount = 0;
			int numTriangles = m_triangleList.Count;
			int scissoredTriangleCount = 0;
			if (m_bScissor) {
				InitScissorBuffer(numTriangles);
				InitScissorNormalBuffer(numTriangles);
				ScissoredTriangleCount scissorCount = new ScissoredTriangleCount();
				if (m_bBackfaceCulling) {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						Vector3 v0 = vertices[indices[index]];
						Vector3 v1 = vertices[indices[index+1]];
						Vector3 v2 = vertices[indices[index+2]];
						if (isFrontFaceTriangle(v0, v1, v2)) {
							Vector3 n0 = normals[indices[index++]];
							Vector3 n1 = normals[indices[index++]];
							Vector3 n2 = normals[indices[index++]];
							ScissorTriangle(v0, v1, v2, n0, n1, n2, ref scissorCount, true);
						}
					}
				}
				else {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						Vector3 n0 = normals[indices[index]];
						Vector3 v0 = vertices[indices[index++]];
						Vector3 n1 = normals[indices[index]];
						Vector3 v1 = vertices[indices[index++]];
						Vector3 n2 = normals[indices[index]];
						Vector3 v2 = vertices[indices[index++]];
						ScissorTriangle(v0, v1, v2, n0, n1, n2, ref scissorCount, true);
					}
				}
				nVertexCount = scissorCount.m_nVertexCount;
				nIndexCount = scissorCount.m_nIndexCount;
				scissoredTriangleCount = scissorCount.m_nTriangleCount;
			}
			else {
				nVertexCount = 3*numTriangles;
				nIndexCount = 3*numTriangles;
			}
			// create result buffer
			InitResultBuffer(nVertexCount, nIndexCount, true);
			// fill result buffer
			int nVertex = 0;
			int nIndex = 0;
			if (m_bScissor) {
				FillResultWithScissoredTrianglesWithNormals(scissoredTriangleCount, ref nVertex, ref nIndex);
			}
			else {
				if (m_bBackfaceCulling) {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						Vector3 v0 = vertices[indices[index]];
						Vector3 v1 = vertices[indices[index+1]];
						Vector3 v2 = vertices[indices[index+2]];
						if (isFrontFaceTriangle(v0, v1, v2)) {
							m_resultIndices[nIndex++] = nVertex;
							m_resultNormal[nVertex] = normals[indices[index++]];
							m_result[nVertex++] = v0;
							m_resultIndices[nIndex++] = nVertex;
							m_resultNormal[nVertex] = normals[indices[index++]];
							m_result[nVertex++] = v1;
							m_resultIndices[nIndex++] = nVertex;
							m_resultNormal[nVertex] = normals[indices[index++]];
							m_result[nVertex++] = v2;
						}
					}
				}
				else {
					for (int i = 0; i < numTriangles; ++i) {
						int index = m_triangleList[i];
						m_resultIndices[nIndex++] = nVertex;
						m_resultNormal[nVertex] = normals[indices[index]];
						m_result[nVertex++] = vertices[indices[index++]];
						m_resultIndices[nIndex++] = nVertex;
						m_resultNormal[nVertex] = normals[indices[index]];
						m_result[nVertex++] = vertices[indices[index++]];
						m_resultIndices[nIndex++] = nVertex;
						m_resultNormal[nVertex] = normals[indices[index]];
						m_result[nVertex++] = vertices[indices[index++]];
					}
				}
			}
			int lastIndex = 0 < nIndex ? m_resultIndices[nIndex-1] : 0;
			while (nIndex < m_resultIndices.Length) {
				m_resultIndices[nIndex++] = lastIndex;
			}
		}
		private static uint UpdateClipFlags(uint flags, ClipFlags clipFlags, float distance, float xExtent, float yExtent, float zExtent)
		{
			flags &= clipFlags.all;
			float d = distance + xExtent;
			if (d < 0) {
				flags &= (uint)(clipFlags.yFlags | clipFlags.zFlags);
				if (d + yExtent < 0) {
					flags &= clipFlags.zFlags;
				}
				if (d + zExtent < 0) {
					flags &= clipFlags.yFlags;
				}
			}
			d = distance + yExtent;
			if (d < 0) {
				flags &= (uint)(clipFlags.zFlags | clipFlags.xFlags);
				if (d + zExtent < 0) {
					flags &= clipFlags.xFlags;
				}
			}
			d = distance + zExtent;
			if (d < 0) {
				flags &= (uint)(clipFlags.xFlags | clipFlags.yFlags);
			}
			return flags;
		}
		internal uint IsInView(Vector3 center, Vector3 extents, out bool isPartial)
		{
			int numClipPlanes = m_clipPlanes.clipPlaneCount;
			uint flags = 0xff;
			isPartial = false;
			for (int i = 0; i < numClipPlanes; ++i) {
				float distance = m_clipPlanes.clipPlanes[i].GetDistanceToPoint(center);
				float xExtent = extents.x * m_clipMetric[i].x;
				float yExtent = extents.y * m_clipMetric[i].y;
				float zExtent = extents.z * m_clipMetric[i].z;
				float extent = xExtent + yExtent + zExtent;
				float maxDistance = distance + extent;
				float minDistance = distance - extent;
				float clipDistance = m_clipPlanes.maxDistance[i];
				if (maxDistance < 0 || clipDistance < minDistance) {
					return 0U;
				}
				if (minDistance < 0) {
					isPartial = true;
					if (distance < 0) {
						flags = UpdateClipFlags(flags, m_clipFlags[i], distance, xExtent, yExtent, zExtent);
					}
				}
				if (m_clipPlanes.twoSideClipping && clipDistance < maxDistance) {
					// in case of orthographic
					isPartial = true;
					distance = clipDistance - distance;
					if (distance < 0) {
						flags = UpdateClipFlags(flags, m_clipFlags2[i], distance, xExtent, yExtent, zExtent);
					}
				}
			}
			return flags;
		}
		
		internal void AddTriangles(int[] trianglesToAdd, Vector3 center, Vector3 extents)
		{
			for (int i = 0; i < trianglesToAdd.Length; ++i) {
				int tri = trianglesToAdd[i];
				if (!m_triangleSet.Contains(tri)) {
					m_triangleList.Add(tri);
					m_triangleSet.Add(tri);
				}
			}
			if (!m_bScissor) {
				Vector3 min = center - extents;
				Vector3 max = center + extents;
				if (m_triangleList.Count == 0) {
					m_minBounds = min;
					m_maxBounds = max;
				}
				else {
					m_minBounds.x = Mathf.Min (m_minBounds.x, min.x);
					m_minBounds.y = Mathf.Min (m_minBounds.y, min.y);
					m_minBounds.z = Mathf.Min (m_minBounds.z, min.z);
					m_maxBounds.x = Mathf.Max (m_maxBounds.x, max.x);
					m_maxBounds.y = Mathf.Max (m_maxBounds.y, max.y);
					m_maxBounds.z = Mathf.Max (m_maxBounds.z, max.z);
				}
			}
		}

	}
}
