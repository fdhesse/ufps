//
// TerrainMeshTreeSearch.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver {
	public class TerrainMeshTreeSearch : MeshTreeSearch {
		private uint[]    m_clipFlags;
		private int       m_minX, m_maxX;
		private int       m_minZ, m_maxZ;
		
		struct Patch {
			public Patch(int posX, int posZ, int width)
			{
				m_posX = posX; m_posZ = posZ; m_width = width;
			}
			public int m_posX;
			public int m_posZ;
			public int m_width;
		}
		private List<Patch> m_patchList;
		private List<Patch> m_patchListToScissor;
		private int         m_indexCount;

		private const uint CLIP_FLAGS_NXNZ = 7 | (3 << 4) | (5 << 8) | (1 << 12);
		private const uint CLIP_FLAGS_NXPZ = 13 | (12 << 4) | (5 << 8) | (4 << 12);
		private const uint CLIP_FLAGS_PXNZ = 11 | (3 << 4) | (10 << 8) | (2 << 12);
		private const uint CLIP_FLAGS_PXPZ = 14 | (12 << 4) | (10 << 8) | (8 << 12);
		internal void Initialize()
		{
			int numClipPlanes = m_clipPlanes.clipPlaneCount;
			if (m_clipMetric == null || m_clipMetric.Length < numClipPlanes) {
				m_clipMetric = new Vector3[numClipPlanes];
			}
			if (m_clipFlags == null || m_clipFlags.Length < numClipPlanes) {
				m_clipFlags = new uint[numClipPlanes];
			}
			for (int i = 0; i < numClipPlanes; ++i) {
				Plane clipPlane = m_clipPlanes.clipPlanes[i];
				m_clipMetric[i].x = Mathf.Abs(clipPlane.normal.x);
				m_clipMetric[i].y = Mathf.Abs(clipPlane.normal.y);
				m_clipMetric[i].z = Mathf.Abs(clipPlane.normal.z);
				if (clipPlane.normal.x < 0) {
					if (clipPlane.normal.z < 0) {
						m_clipFlags[i] = CLIP_FLAGS_NXNZ | (CLIP_FLAGS_PXPZ << 16);
					}
					else {
						m_clipFlags[i] = CLIP_FLAGS_NXPZ | (CLIP_FLAGS_PXNZ << 16);
					}
				}
				else {
					if (clipPlane.normal.z < 0) {
						m_clipFlags[i] = CLIP_FLAGS_PXNZ | (CLIP_FLAGS_NXPZ << 16);
					}
					else {
						m_clipFlags[i] = CLIP_FLAGS_PXPZ | (CLIP_FLAGS_NXNZ << 16);
					}
				}
			}
			
			if (m_patchList == null) {
				m_patchList = new List<Patch>();
			}
			m_patchList.Clear();
			if (m_bScissor) {
				if (m_patchListToScissor == null) {
					m_patchListToScissor = new List<Patch>();
				}
				m_patchListToScissor.Clear();
			}
			m_indexCount = 0;
			m_minX = m_maxX = 0;
			m_minZ = m_maxZ = 0;
			m_minBounds = Vector3.zero;
			m_maxBounds = Vector3.zero;
		}
		internal void Finalize(float[,] heightMap, float scaleX, float scaleZ)
		{
			m_minBounds.x = scaleX * m_minX;
			m_minBounds.z = scaleZ * m_minZ;
			m_maxBounds.x = scaleX * m_maxX;
			m_maxBounds.z = scaleZ * m_maxZ;

			ScissoredTriangleCount scissoredTriangleCount = new ScissoredTriangleCount();
			// scissor triangles if any
			if (m_bScissor && 0 < m_patchListToScissor.Count) {
				int numTrianglesToScissor = 0;
				for (int i = 0; i < m_patchListToScissor.Count; ++i) {
					int w = m_patchListToScissor[i].m_width;
					numTrianglesToScissor += 2 * w * w;
				}
				InitScissorBuffer(numTrianglesToScissor);
				for (int i = 0; i < m_patchListToScissor.Count; ++i) {
					Patch patch = m_patchListToScissor[i];
					for (int z = patch.m_posZ, zEnd = patch.m_posZ + patch.m_width; z < zEnd; ++z) {
						for (int x = patch.m_posX, xEnd = patch.m_posX + patch.m_width; x < xEnd; ++x) {
							Vector3 v0 = new Vector3(scaleX*x, heightMap[z, x], scaleZ*z);
							Vector3 v1 = new Vector3(scaleX*(x+1), heightMap[z, x+1], scaleZ*z);
							Vector3 v2 = new Vector3(scaleX*x, heightMap[z+1, x], scaleZ*(z+1));
							Vector3 v3 = new Vector3(scaleX*(x+1), heightMap[z+1, x+1], scaleZ*(z+1));
							ScissorTriangle(v0, v2, v3, ref scissoredTriangleCount, m_patchList.Count == 0);
							ScissorTriangle(v0, v3, v1, ref scissoredTriangleCount, m_patchList.Count == 0);
						}
					}
				}
			}
			// create result buffer
			int vertexCount = scissoredTriangleCount.m_nVertexCount + (m_maxX - m_minX + 1)*(m_maxZ - m_minZ + 1);
			int indexCount = scissoredTriangleCount.m_nIndexCount + m_indexCount;
			InitResultBuffer(vertexCount, indexCount, false);
			m_bOutputNormals = false;
			// fill result buffer
			int nVertex = 0;
			int nIndex = 0;
			for (int z = m_minZ; z <= m_maxZ; ++z) {
				for (int x = m_minX; x <= m_maxX; ++x) {
					m_result[nVertex++] = new Vector3(scaleX * x, heightMap[z,x], scaleZ * z);
				}
			}
			int width = m_maxX - m_minX + 1;
			for (int i = 0; i < m_patchList.Count; ++i) {
				Patch patch = m_patchList[i];
				int offset = (patch.m_posZ - m_minZ) * width + (patch.m_posX - m_minX);
				for (int z = 0; z < patch.m_width; ++z) {
					int vtx = offset;
					for (int x = 0; x < patch.m_width; ++x) {
						m_resultIndices[nIndex++] = vtx;
						m_resultIndices[nIndex++] = vtx + width;
						m_resultIndices[nIndex++] = vtx + width + 1;
						m_resultIndices[nIndex++] = vtx;
						m_resultIndices[nIndex++] = vtx + width + 1;
						m_resultIndices[nIndex++] = ++vtx;
					}
					offset += width;
				}
			}
			FillResultWithScissoredTriangles(scissoredTriangleCount.m_nTriangleCount, ref nVertex, ref nIndex);
			int lastIndex = 0 < nIndex ? m_resultIndices[nIndex-1] : 0;
			while (nIndex < m_resultIndices.Length) {
				m_resultIndices[nIndex++] = lastIndex;
			}
		}
		private Vector3 CalculateNormal(float[,] heightMap, float invScaleX, float invScaleZ, int z, int x)
		{
			float dx, dz;
			if (0 < x) {
				dx = invScaleX*(heightMap[z, x+1] - heightMap[z, x-1]);
			}
			else {
				dx = 2.0f*invScaleX*(heightMap[z, x+1] - heightMap[z, 0]);
			}
			if (0 < z) {
				dz = invScaleZ*(heightMap[z+1, x] - heightMap[z-1, x]);
			}
			else {
				dz = 2.0f*invScaleZ*(heightMap[z+1, x] - heightMap[0, x]);
			}
			return new Vector3(dx, 1.0f, dz).normalized;
		}
		internal void FinalizeWithNormal(float[,] heightMap, float scaleX, float scaleZ)
		{
			m_minBounds.x = scaleX * m_minX;
			m_minBounds.z = scaleZ * m_minZ;
			m_maxBounds.x = scaleX * m_maxX;
			m_maxBounds.z = scaleZ * m_maxZ;

			float invScaleX = 0.5f/scaleX;
			float invScaleZ = 0.5f/scaleZ;
			ScissoredTriangleCount scissoredTriangleCount = new ScissoredTriangleCount();
			// scissor triangles if any
			if (m_bScissor && 0 < m_patchListToScissor.Count) {
				int numTrianglesToScissor = 0;
				for (int i = 0; i < m_patchListToScissor.Count; ++i) {
					int w = m_patchListToScissor[i].m_width;
					numTrianglesToScissor += 2 * w * w;
				}
				InitScissorBuffer(numTrianglesToScissor);
				InitScissorNormalBuffer(numTrianglesToScissor);
				for (int i = 0; i < m_patchListToScissor.Count; ++i) {
					Patch patch = m_patchListToScissor[i];
					for (int z = patch.m_posZ, zEnd = patch.m_posZ + patch.m_width; z < zEnd; ++z) {
						for (int x = patch.m_posX, xEnd = patch.m_posX + patch.m_width; x < xEnd; ++x) {
							Vector3 v0 = new Vector3(scaleX*x, heightMap[z, x], scaleZ*z);
							Vector3 n0 = CalculateNormal(heightMap, invScaleX, invScaleZ, z, x);
							Vector3 v1 = new Vector3(scaleX*(x+1), heightMap[z, x+1], scaleZ*z);
							Vector3 n1 = CalculateNormal(heightMap, invScaleX, invScaleZ, z, x+1);
							Vector3 v2 = new Vector3(scaleX*x, heightMap[z+1, x], scaleZ*(z+1));
							Vector3 n2 = CalculateNormal(heightMap, invScaleX, invScaleZ, z+1, x);
							Vector3 v3 = new Vector3(scaleX*(x+1), heightMap[z+1, x+1], scaleZ*(z+1));
							Vector3 n3 = CalculateNormal(heightMap, invScaleX, invScaleZ, z+1, x+1);
							ScissorTriangle(v0, v2, v3, n0, n2, n3, ref scissoredTriangleCount, m_patchList.Count == 0);
							ScissorTriangle(v0, v3, v1, n0, n3, n1, ref scissoredTriangleCount, m_patchList.Count == 0);
						}
					}
				}
			}
			// create result buffer
			int vertexCount = scissoredTriangleCount.m_nVertexCount + (m_maxX - m_minX + 1)*(m_maxZ - m_minZ + 1);
			int indexCount = scissoredTriangleCount.m_nIndexCount + m_indexCount;
			InitResultBuffer(vertexCount, indexCount, true);
			// fill result buffer
			int nVertex = 0;
			int nIndex = 0;
			for (int z = m_minZ; z <= m_maxZ; ++z) {
				for (int x = m_minX; x <= m_maxX; ++x) {
					m_resultNormal[nVertex] = CalculateNormal(heightMap, invScaleX, invScaleZ, z, x);
					m_result[nVertex++] = new Vector3(scaleX * x, heightMap[z,x], scaleZ * z);
				}
			}
			int width = m_maxX - m_minX + 1;
			for (int i = 0; i < m_patchList.Count; ++i) {
				Patch patch = m_patchList[i];
				int offset = (patch.m_posZ - m_minZ) * width + (patch.m_posX - m_minX);
				for (int z = 0; z < patch.m_width; ++z) {
					int vtx = offset;
					for (int x = 0; x < patch.m_width; ++x) {
						m_resultIndices[nIndex++] = vtx;
						m_resultIndices[nIndex++] = vtx + width;
						m_resultIndices[nIndex++] = vtx + width + 1;
						m_resultIndices[nIndex++] = vtx;
						m_resultIndices[nIndex++] = vtx + width + 1;
						m_resultIndices[nIndex++] = ++vtx;
					}
					offset += width;
				}
			}
			FillResultWithScissoredTrianglesWithNormals(scissoredTriangleCount.m_nTriangleCount, ref nVertex, ref nIndex);
			int lastIndex = 0 < nIndex ? m_resultIndices[nIndex-1] : 0;
			while (nIndex < m_resultIndices.Length) {
				m_resultIndices[nIndex++] = lastIndex;
			}
		}
		internal uint IsInView(Vector3 center, Vector3 extents, out bool isPartial)
		{
			int numClipPlanes = m_clipPlanes.clipPlaneCount;
			uint flags = 0xf;
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
				uint clipFlags = m_clipFlags[i];
				if (maxDistance < 0 || clipDistance < minDistance) {
					return 0U;
				}
				if (minDistance < 0) {
					isPartial = true;
					float d = distance + yExtent;
					if (d < 0) {
						flags &= clipFlags;
						if (d + xExtent < 0) {
							flags &= (clipFlags >> 4);
						}
						if (d + zExtent < 0) {
							flags &= (clipFlags >> 8);
						}
					}
				}
				if (m_clipPlanes.twoSideClipping && clipDistance < maxDistance) {
					isPartial = true;
					float d = clipDistance - distance + yExtent;
					if (d < 0) {
						flags &= (clipFlags >> 16);
						if (d + xExtent < 0) {
							flags &= (clipFlags >> 20);
						}
						if (d + zExtent < 0) {
							flags &= (clipFlags >> 24);
						}
					}
				}
			}
			return flags;
		}
		private static uint UpdateClipFlags(uint flags, uint clipFlags, float distance, float xExtent, float yExtent, float zExtent)
		{
			float d1 = distance + yExtent;
			float d2 = distance - yExtent;
			if (d1 < 0) {
				flags &= clipFlags & 0xf;
				if (d1 + xExtent < 0) {
					flags &= (clipFlags >> 4);
				}
				if (d1 + zExtent < 0) {
					flags &= (clipFlags >> 8);
				}
			}
			else if (0 < d2) {
				uint f = clipFlags | 0xf;
				if (0 < d2 - xExtent) {
					f |= (clipFlags << 8);
				}
				if (0 < d2 - zExtent) {
					f |= (clipFlags << 4);
				}
				flags &= f;
			}
			else {
				flags &= 0xf;
			}
			return flags;
		}
		internal uint IsInView(Vector3 center, Vector3 extents)
		{
			int numClipPlanes = m_clipPlanes.clipPlaneCount;
			uint flags = 0xf00f; // clipFlags | (partialFlags << 12)
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
					flags = UpdateClipFlags(flags, m_clipFlags[i], distance, xExtent, yExtent, zExtent);
				}
				if (m_clipPlanes.twoSideClipping && clipDistance < maxDistance) {
					flags = UpdateClipFlags(flags, m_clipFlags[i] >> 16, clipDistance - distance, xExtent, yExtent, zExtent);
				}
			}
			return flags;
		}
		public void AddPatch(int posX, int posZ, int width, float minY, float maxY, bool isPartial)
		{
			if (isPartial && m_bScissor) {
				m_patchListToScissor.Add(new Patch(posX, posZ, width));
				return;
			}
			if (m_patchList.Count == 0) {
				m_minX = posX;
				m_minBounds.y = minY;
				m_minZ = posZ;
				m_maxX = posX + width;
				m_maxBounds.y = maxY;
				m_maxZ = posZ + width;
			}
			else {
				m_minX = Mathf.Min(m_minX, posX);
				m_minZ = Mathf.Min(m_minZ, posZ);
				m_maxX = Mathf.Max(m_maxX, posX + width);
				m_maxZ = Mathf.Max(m_maxZ, posZ + width);
				m_minBounds.y = Mathf.Min(m_minBounds.y, minY);
				m_maxBounds.y = Mathf.Max(m_maxBounds.y, maxY);
			}
			m_patchList.Add(new Patch(posX, posZ, width));
			m_indexCount += 6 * width * width;
		}
	}
}
