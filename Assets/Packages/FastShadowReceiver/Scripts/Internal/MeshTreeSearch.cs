//
// MeshTreeSearch.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

namespace FastShadowReceiver {
	public class MeshTreeSearch {
		public ClipPlanes m_clipPlanes;
		public float      m_scissorMargin = 0.0f; // applicable for BinaryMeshTreeSearch
		public bool       m_bScissor;
		public bool       m_bOutputNormals = false;
		public bool       m_bBackfaceCulling = false;

		protected class ResultBuffer {
			public Vector3[] vertices;
			public Vector3[] normals;
			public int[]     indices;
		}
		Dictionary<int, ResultBuffer> m_resultBuffer;
		protected Vector3[] m_clipMetric;
		protected Vector3[] m_result;
		protected Vector3[] m_resultNormal;
		protected int[]     m_resultIndices;
		protected Vector3   m_minBounds;
		protected Vector3   m_maxBounds;
		
		private ManualResetEvent m_event;
		private MeshTreeBase     m_tree;

		protected Vector3[][] m_scissoredTriangles;
		protected Vector3[][] m_scissoredTriangleNormals;
		protected int[]       m_scissoredTriangleVertexCount;
		protected Vector3[]   m_tempScissorBuffer;
		protected Vector3[]   m_tempScissorNormalBuffer;

		private Vector3 m_projectionDir;
		private Vector3 m_projectionPos;
		private bool    m_isOrthographic;
		public void SetProjectionDir(bool isOrtho, Vector3 direction, Vector3 position)
		{
			m_projectionDir = direction;
			m_projectionPos = position;
			m_isOrthographic = isOrtho;
		}
		public bool isFrontFaceTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
		{
			Vector3 triNormal = Vector3.Cross(v1 - v0, v2 - v0);
			if (Vector3.Dot(triNormal, m_projectionDir) < 0) {
				return true;
			}
			return m_isOrthographic && Vector3.Dot(triNormal, v0 - m_projectionPos) < 0;
		}
		public Vector3[] vertices
		{
			get { return m_result; }
		}
		public Vector3[] normals
		{
			get { return m_resultNormal; }
		}
		public int[] triangles
		{
			get { return m_resultIndices; }
		}
		
		public Bounds bounds
		{
			get {
				Bounds b = new Bounds();
				b.SetMinMax(m_minBounds, m_maxBounds);
				return b;
			}
		}

#if NETFX_CORE
		private Windows.System.Threading.WorkItemHandler m_workItemHandler;
		public MeshTreeSearch()
		{
			m_workItemHandler = (source) => this.Search();
		}
		public void AsyncStart(MeshTreeBase tree)
		{
			if (m_event == null) {
				m_event = new ManualResetEvent(false);
			}
			m_event.Reset();
			m_tree = tree;
			// we don't need to wait the following async function, because we are using m_event to wait for completion.
			// of course, we can use the return value of the following function call to wait, but we don't want to change code for Windows Store App.
			var suppressWarning = Windows.System.Threading.ThreadPool.RunAsync(m_workItemHandler);
		}
#else
		private static WaitCallback s_searchCallback = (arg => ((MeshTreeSearch)arg).Search());
		public void AsyncStart(MeshTreeBase tree)
		{
			if (m_event == null) {
				m_event = new ManualResetEvent(false);
			}
			m_event.Reset();
			m_tree = tree;
			Nyahoon.ThreadPool.QueueUserWorkItem(s_searchCallback, this);
		}
#endif

		private void Search()
		{
			try {
				m_tree.Search(this);
				m_tree = null;
			}
			catch (System.Exception e) {
				Debug.LogException(e);
				m_event.Set();
				throw e;
			}
			finally {
				m_event.Set();
			}
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
		
		protected void InitClipMetrics()
		{
			int numClipPlanes = m_clipPlanes.clipPlaneCount;
			if (m_clipMetric == null || m_clipMetric.Length < numClipPlanes) {
				m_clipMetric = new Vector3[numClipPlanes];
			}
			for (int i = 0; i < numClipPlanes; ++i) {
				m_clipMetric[i].x = Mathf.Abs(m_clipPlanes.clipPlanes[i].normal.x);
				m_clipMetric[i].y = Mathf.Abs(m_clipPlanes.clipPlanes[i].normal.y);
				m_clipMetric[i].z = Mathf.Abs(m_clipPlanes.clipPlanes[i].normal.z);
			}
		}
		
		protected void InitScissorBuffer(int maxTriangleNum)
		{
			if (m_bScissor && 0 < maxTriangleNum) {
				int scissorePlaneNum = m_clipPlanes.twoSideClipping ? 2*m_clipPlanes.scissorPlaneCount : m_clipPlanes.scissorPlaneCount;
				if (m_tempScissorBuffer == null || m_tempScissorBuffer.Length < 3 + scissorePlaneNum) {
					m_tempScissorBuffer = new Vector3[3 + scissorePlaneNum];
				}
				if (m_scissoredTriangles == null || m_scissoredTriangles.Length < maxTriangleNum || m_scissoredTriangles[0].Length < 3 + scissorePlaneNum) {
					Vector3[][] scissoredTriangles = new Vector3[maxTriangleNum][];
					int i = 0;
					if (m_scissoredTriangles != null) {
						for (; i < m_scissoredTriangles.Length; ++i) {
							scissoredTriangles[i] = m_scissoredTriangles[i];
						}
					}
					for (; i < scissoredTriangles.Length; ++i) {
						scissoredTriangles[i] = new Vector3[3 + scissorePlaneNum];
					}
					m_scissoredTriangles = scissoredTriangles;
					m_scissoredTriangleVertexCount = new int[maxTriangleNum];
				}
			}
		}

		protected void InitScissorNormalBuffer(int maxTriangleNum)
		{
			if (m_bScissor && 0 < maxTriangleNum) {
				int scissorePlaneNum = m_clipPlanes.twoSideClipping ? 2*m_clipPlanes.scissorPlaneCount : m_clipPlanes.scissorPlaneCount;
				if (m_tempScissorNormalBuffer == null || m_tempScissorNormalBuffer.Length < 3 + scissorePlaneNum) {
					m_tempScissorNormalBuffer = new Vector3[3 + scissorePlaneNum];
				}
				if (m_scissoredTriangleNormals == null || m_scissoredTriangleNormals.Length < maxTriangleNum || m_scissoredTriangleNormals[0].Length < 3 + scissorePlaneNum) {
					Vector3[][] scissoredTriangleNormals = new Vector3[maxTriangleNum][];
					int i = 0;
					if (m_scissoredTriangleNormals != null) {
						for (; i < m_scissoredTriangleNormals.Length; ++i) {
							scissoredTriangleNormals[i] = m_scissoredTriangleNormals[i];
						}
					}
					for (; i < scissoredTriangleNormals.Length; ++i) {
						scissoredTriangleNormals[i] = new Vector3[3 + scissorePlaneNum];
					}
					m_scissoredTriangleNormals = scissoredTriangleNormals;
				}
			}
		}

		private int GetResultBufferIndex(int count)
		{
			int n = count >> 2;
			int i = 0;
			while (0 < n && i < 8) {
				++i;
				n >>= 1;
			}
			return i + n;
		}
		protected void InitResultBuffer(int vertexCount, int indexCount, bool bNormal)
		{
			if (m_resultBuffer == null) {
				m_resultBuffer = new Dictionary<int, ResultBuffer>();
			}
			int n = GetResultBufferIndex(vertexCount);
			ResultBuffer buffer;
			if (m_resultBuffer.TryGetValue(n, out buffer)) {
				if (buffer.vertices.Length < vertexCount) {
					buffer.vertices = new Vector3[vertexCount];
					if (bNormal) {
						buffer.normals = new Vector3[vertexCount];
					}
				}
				else if (bNormal && buffer.normals == null) {
					buffer.normals = new Vector3[vertexCount];
				}
				if (buffer.indices.Length < indexCount) {
					buffer.indices = new int[indexCount];
				}
			}
			else {
				buffer = new ResultBuffer();
				buffer.vertices = new Vector3[vertexCount];
				if (bNormal) {
					buffer.normals = new Vector3[vertexCount];
				}
				else {
					buffer.normals = null;
				}
				buffer.indices = new int[indexCount];
				m_resultBuffer.Add(n, buffer);
			}
			m_result = buffer.vertices;
			m_resultNormal = buffer.normals;
			m_resultIndices = buffer.indices;
		}

		protected struct ScissoredTriangleCount {
			public int m_nTriangleCount;
			public int m_nVertexCount;
			public int m_nIndexCount;
		}
		protected int ScissorTriangle(Vector3 v0, Vector3 v1, Vector3 v2, ref ScissoredTriangleCount triCount, bool bResetBounds)
		{
			int numVertices = 3;
			Vector3[] vertices = m_scissoredTriangles[triCount.m_nTriangleCount];
			vertices[0] = v0;
			vertices[1] = v1;
			vertices[2] = v2;
			if (m_clipPlanes.twoSideClipping) {
				for (int j = 0; j < m_clipPlanes.scissorPlaneCount; ++j) {
					Plane clipPlane = m_clipPlanes.clipPlanes[j];
					float maxDistance = m_clipPlanes.maxDistance[j];
					float firstDistance = clipPlane.GetDistanceToPoint(vertices[0]);
					float firstDistance2 = maxDistance - firstDistance;
					float lastDistance = firstDistance;
					float lastDistance2 = firstDistance2;
					int newVertexCount = 0;
					for (int k = 1; k < numVertices; ++k) {
						float distance = clipPlane.GetDistanceToPoint(vertices[k]);
						float distance2 = maxDistance - distance;
						if (distance <= 0) {
							if (lastDistance2 <= 0) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k], vertices[k-1], distance2/(distance2 - lastDistance2));
							}
							if (0 < lastDistance) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k-1], vertices[k], lastDistance/(lastDistance - distance));
							}
						}
						else {
							if (lastDistance <= 0) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k], vertices[k-1], distance/(distance - lastDistance));
								if (distance2 <= 0) {
									m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k-1], vertices[k], lastDistance2/(lastDistance2 - distance2));
								}
							}
							else if (distance2 <= 0) {
								if (0 < lastDistance2) {
									m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k-1], vertices[k], lastDistance2/(lastDistance2 - distance2));
								}
							}
							else {
								if (lastDistance2 <= 0) {
									m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k], vertices[k-1], distance2/(distance2 - lastDistance2));
								}
							}
						}
						if (0 < distance && 0 < distance2) {
							m_tempScissorBuffer[newVertexCount++] = vertices[k];
						}
						lastDistance = distance;
						lastDistance2 = distance2;
					}
					if (firstDistance <= 0) {
						if (lastDistance2 <= 0) {
							m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[0], vertices[numVertices-1], firstDistance2/(firstDistance2 - lastDistance2));
						}
						if (0 < lastDistance) {
							m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[numVertices-1], vertices[0], lastDistance/(lastDistance - firstDistance));
						}
					}
					else {
						if (lastDistance <= 0) {
							m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[0], vertices[numVertices-1], firstDistance/(firstDistance - lastDistance));
							if (firstDistance2 <= 0) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[numVertices-1], vertices[0], lastDistance2/(lastDistance2 - firstDistance2));
							}
						}
						else if (firstDistance2 <= 0) {
							if (0 < lastDistance2) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[numVertices-1], vertices[0], lastDistance2/(lastDistance2 - firstDistance2));
							}
						}
						else {
							if (lastDistance2 <= 0) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[0], vertices[numVertices-1], firstDistance2/(firstDistance2 - lastDistance2));
							}
						}
					}
					if (0 < firstDistance && 0 < firstDistance2) {
						m_tempScissorBuffer[newVertexCount++] = vertices[0];
					}
					numVertices = newVertexCount;
					vertices = m_tempScissorBuffer;
					m_tempScissorBuffer = m_scissoredTriangles[triCount.m_nTriangleCount];
					m_scissoredTriangles[triCount.m_nTriangleCount] = vertices;
					if (numVertices < 3) {
						return 0;
					}
				}
			}
			else {
				for (int j = 0; j < m_clipPlanes.scissorPlaneCount; ++j) {
					Plane clipPlane = m_clipPlanes.clipPlanes[j];
					float firstDistance = clipPlane.GetDistanceToPoint(vertices[0]);
					float lastDistance = firstDistance;
					int newVertexCount = 0;
					for (int k = 1; k < numVertices; ++k) {
						float distance = clipPlane.GetDistanceToPoint(vertices[k]);
						if (distance <= 0) {
							if (0 < lastDistance) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k-1], vertices[k], lastDistance/(lastDistance - distance));
							}
						}
						else {
							if (lastDistance <= 0) {
								m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[k], vertices[k-1], distance/(distance - lastDistance));
							}
							m_tempScissorBuffer[newVertexCount++] = vertices[k];
						}
						lastDistance = distance;
					}
					if (firstDistance <= 0) {
						if (0 < lastDistance) {
							m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[numVertices-1], vertices[0], lastDistance/(lastDistance - firstDistance));
						}
					}
					else {
						if (lastDistance <= 0) {
							m_tempScissorBuffer[newVertexCount++] = Vector3.Lerp(vertices[0], vertices[numVertices-1], firstDistance/(firstDistance - lastDistance));
						}
						m_tempScissorBuffer[newVertexCount++] = vertices[0];
					}
					numVertices = newVertexCount;
					vertices = m_tempScissorBuffer;
					m_tempScissorBuffer = m_scissoredTriangles[triCount.m_nTriangleCount];
					m_scissoredTriangles[triCount.m_nTriangleCount] = vertices;
					if (numVertices < 3) {
						return 0;
					}
				}
			}
			if (bResetBounds && triCount.m_nTriangleCount == 0) {
				m_maxBounds = m_minBounds = m_scissoredTriangles[triCount.m_nTriangleCount][0];
			}
			for (int j = 0; j < numVertices; ++j) {
				Vector3 v = m_scissoredTriangles[triCount.m_nTriangleCount][j];
				m_minBounds.x = Mathf.Min (m_minBounds.x, v.x);
				m_minBounds.y = Mathf.Min (m_minBounds.y, v.y);
				m_minBounds.z = Mathf.Min (m_minBounds.z, v.z);
				m_maxBounds.x = Mathf.Max (m_maxBounds.x, v.x);
				m_maxBounds.y = Mathf.Max (m_maxBounds.y, v.y);
				m_maxBounds.z = Mathf.Max (m_maxBounds.z, v.z);
			}
			m_scissoredTriangleVertexCount[triCount.m_nTriangleCount++] = numVertices;
			triCount.m_nVertexCount += numVertices;
			triCount.m_nIndexCount += (numVertices - 2) * 3;
			return numVertices;
		}
		protected int ScissorTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 n0, Vector3 n1, Vector3 n2, ref ScissoredTriangleCount triCount, bool bResetBounds)
		{
			int numVertices = 3;
			Vector3[] vertices = m_scissoredTriangles[triCount.m_nTriangleCount];
			Vector3[] normals = m_scissoredTriangleNormals[triCount.m_nTriangleCount];
			vertices[0] = v0;
			vertices[1] = v1;
			vertices[2] = v2;
			normals[0] = n0;
			normals[1] = n1;
			normals[2] = n2;
			if (m_clipPlanes.twoSideClipping) {
				for (int j = 0; j < m_clipPlanes.scissorPlaneCount; ++j) {
					Plane clipPlane = m_clipPlanes.clipPlanes[j];
					float maxDistance = m_clipPlanes.maxDistance[j];
					float firstDistance = clipPlane.GetDistanceToPoint(vertices[0]);
					float firstDistance2 = maxDistance - firstDistance;
					float lastDistance = firstDistance;
					float lastDistance2 = firstDistance2;
					int newVertexCount = 0;
					for (int k = 1; k < numVertices; ++k) {
						float distance = clipPlane.GetDistanceToPoint(vertices[k]);
						float distance2 = maxDistance - distance;
						if (distance <= 0) {
							if (lastDistance2 <= 0) {
								float w = distance2/(distance2 - lastDistance2);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k], vertices[k-1], w);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k], normals[k-1], w);
								++newVertexCount;
							}
							if (0 < lastDistance) {
								float w = lastDistance/(lastDistance - distance);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k-1], vertices[k], w);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k-1], normals[k], w);
								++newVertexCount;
							}
						}
						else {
							if (lastDistance <= 0) {
								float w = distance/(distance - lastDistance);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k], vertices[k-1], w);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k], normals[k-1], w);
								++newVertexCount;
								if (distance2 <= 0) {
									float w2 = lastDistance2/(lastDistance2 - distance2);
									m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k-1], vertices[k], w2);
									m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k-1], normals[k], w2);
									++newVertexCount;
								}
							}
							else if (distance2 <= 0) {
								if (0 < lastDistance2) {
									float w = lastDistance2/(lastDistance2 - distance2);
									m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k-1], vertices[k], w);
									m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k-1], normals[k], w);
									++newVertexCount;
								}
							}
							else {
								if (lastDistance2 <= 0) {
									float w = distance2/(distance2 - lastDistance2);
									m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k], vertices[k-1], w);
									m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k], normals[k-1], w);
									++newVertexCount;
								}
							}
						}
						if (0 < distance && 0 < distance2) {
							m_tempScissorBuffer[newVertexCount] = vertices[k];
							m_tempScissorNormalBuffer[newVertexCount] = normals[k];
							++newVertexCount;
						}
						lastDistance = distance;
						lastDistance2 = distance2;
					}
					if (firstDistance <= 0) {
						if (lastDistance2 <= 0) {
							float w = firstDistance2/(firstDistance2 - lastDistance2);
							m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[0], vertices[numVertices-1], w);
							m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[0], normals[numVertices-1], w);
							++newVertexCount;
						}
						if (0 < lastDistance) {
							float w = lastDistance/(lastDistance - firstDistance);
							m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[numVertices-1], vertices[0], w);
							m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[numVertices-1], normals[0], w);
							++newVertexCount;
						}
					}
					else {
						if (lastDistance <= 0) {
							float w = firstDistance/(firstDistance - lastDistance);
							m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[0], vertices[numVertices-1], w);
							m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[0], normals[numVertices-1], w);
							++newVertexCount;
							if (firstDistance2 <= 0) {
								float w2 = lastDistance2/(lastDistance2 - firstDistance2);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[numVertices-1], vertices[0], w2);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[numVertices-1], normals[0], w2);
								++newVertexCount;
							}
						}
						else if (firstDistance2 <= 0) {
							if (0 < lastDistance2) {
								float w = lastDistance2/(lastDistance2 - firstDistance2);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[numVertices-1], vertices[0], w);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[numVertices-1], normals[0], w);
								++newVertexCount;
							}
						}
						else {
							if (lastDistance2 <= 0) {
								float w = firstDistance2/(firstDistance2 - lastDistance2);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[0], vertices[numVertices-1], w);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[0], normals[numVertices-1], w);
								++newVertexCount;
							}
						}
					}
					if (0 < firstDistance && 0 < firstDistance2) {
						m_tempScissorBuffer[newVertexCount] = vertices[0];
						m_tempScissorNormalBuffer[newVertexCount] = normals[0];
						++newVertexCount;
					}
					numVertices = newVertexCount;
					vertices = m_tempScissorBuffer;
					normals = m_tempScissorNormalBuffer;
					m_tempScissorBuffer = m_scissoredTriangles[triCount.m_nTriangleCount];
					m_tempScissorNormalBuffer = m_scissoredTriangleNormals[triCount.m_nTriangleCount];
					m_scissoredTriangles[triCount.m_nTriangleCount] = vertices;
					m_scissoredTriangleNormals[triCount.m_nTriangleCount] = normals;
					if (numVertices < 3) {
						return 0;
					}
				}
			}
			else {
				for (int j = 0; j < m_clipPlanes.scissorPlaneCount; ++j) {
					Plane clipPlane = m_clipPlanes.clipPlanes[j];
					float firstDistance = clipPlane.GetDistanceToPoint(vertices[0]);
					float lastDistance = firstDistance;
					int newVertexCount = 0;
					for (int k = 1; k < numVertices; ++k) {
						float distance = clipPlane.GetDistanceToPoint(vertices[k]);
						if (distance <= 0) {
							if (0 < lastDistance) {
								float w = lastDistance/(lastDistance - distance);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k-1], vertices[k], w);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k-1], normals[k], w);
								++newVertexCount;
							}
						}
						else {
							if (lastDistance <= 0) {
								float w = distance/(distance - lastDistance);
								m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[k], vertices[k-1], w);
								m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[k], normals[k-1], w);
								++newVertexCount;
							}
							m_tempScissorBuffer[newVertexCount] = vertices[k];
							m_tempScissorNormalBuffer[newVertexCount] = normals[k];
							++newVertexCount;
						}
						lastDistance = distance;
					}
					if (firstDistance <= 0) {
						if (0 < lastDistance) {
							float w = lastDistance/(lastDistance - firstDistance);
							m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[numVertices-1], vertices[0], w);
							m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[numVertices-1], normals[0], w);
							++newVertexCount;
						}
					}
					else {
						if (lastDistance <= 0) {
							float w = firstDistance/(firstDistance - lastDistance);
							m_tempScissorBuffer[newVertexCount] = Vector3.Lerp(vertices[0], vertices[numVertices-1], w);
							m_tempScissorNormalBuffer[newVertexCount] = Vector3.Lerp(normals[0], normals[numVertices-1], w);
							++newVertexCount;
						}
						m_tempScissorBuffer[newVertexCount] = vertices[0];
						m_tempScissorNormalBuffer[newVertexCount] = normals[0];
						++newVertexCount;
					}
					numVertices = newVertexCount;
					vertices = m_tempScissorBuffer;
					normals = m_tempScissorNormalBuffer;
					m_tempScissorBuffer = m_scissoredTriangles[triCount.m_nTriangleCount];
					m_tempScissorNormalBuffer = m_scissoredTriangleNormals[triCount.m_nTriangleCount];
					m_scissoredTriangles[triCount.m_nTriangleCount] = vertices;
					m_scissoredTriangleNormals[triCount.m_nTriangleCount] = normals;
					if (numVertices < 3) {
						return 0;
					}
				}
			}
			if (bResetBounds && triCount.m_nTriangleCount == 0) {
				m_maxBounds = m_minBounds = m_scissoredTriangles[triCount.m_nTriangleCount][0];
			}
			for (int j = 0; j < numVertices; ++j) {
				Vector3 v = m_scissoredTriangles[triCount.m_nTriangleCount][j];
				m_minBounds.x = Mathf.Min (m_minBounds.x, v.x);
				m_minBounds.y = Mathf.Min (m_minBounds.y, v.y);
				m_minBounds.z = Mathf.Min (m_minBounds.z, v.z);
				m_maxBounds.x = Mathf.Max (m_maxBounds.x, v.x);
				m_maxBounds.y = Mathf.Max (m_maxBounds.y, v.y);
				m_maxBounds.z = Mathf.Max (m_maxBounds.z, v.z);
			}
			m_scissoredTriangleVertexCount[triCount.m_nTriangleCount++] = numVertices;
			triCount.m_nVertexCount += numVertices;
			triCount.m_nIndexCount += (numVertices - 2) * 3;
			return numVertices;
		}
		protected void FillResultWithScissoredTriangles(int scissoredTriangleCount, ref int vertexOffset, ref int indexOffset)
		{
			int nVertex = vertexOffset;
			int nIndex = indexOffset;
			for (int i = 0; i < scissoredTriangleCount; ++i) {
				Vector3[] scissoredVertices = m_scissoredTriangles[i];
				int firstIndex = nVertex;
				m_resultIndices[nIndex++] = nVertex;
				m_result[nVertex++] = scissoredVertices[0];
				m_resultIndices[nIndex++] = nVertex;
				m_result[nVertex++] = scissoredVertices[1];
				m_resultIndices[nIndex++] = nVertex;
				m_result[nVertex++] = scissoredVertices[2];
				for (int j = 3; j < m_scissoredTriangleVertexCount[i]; ++j) {
					m_resultIndices[nIndex++] = firstIndex;
					m_resultIndices[nIndex++] = nVertex - 1;
					m_resultIndices[nIndex++] = nVertex;
					m_result[nVertex++] = scissoredVertices[j];
				}
			}
			vertexOffset = nVertex;
			indexOffset = nIndex;
		}
		protected void FillResultWithScissoredTrianglesWithNormals(int scissoredTriangleCount, ref int vertexOffset, ref int indexOffset)
		{
			int nVertex = vertexOffset;
			int nIndex = indexOffset;
			for (int i = 0; i < scissoredTriangleCount; ++i) {
				Vector3[] scissoredVertices = m_scissoredTriangles[i];
				Vector3[] scissoredNormals = m_scissoredTriangleNormals[i];
				int firstIndex = nVertex;
				m_resultIndices[nIndex++] = nVertex;
				m_resultNormal[nVertex] = scissoredNormals[0];
				m_result[nVertex++] = scissoredVertices[0];
				m_resultIndices[nIndex++] = nVertex;
				m_resultNormal[nVertex] = scissoredNormals[1];
				m_result[nVertex++] = scissoredVertices[1];
				m_resultIndices[nIndex++] = nVertex;
				m_resultNormal[nVertex] = scissoredNormals[2];
				m_result[nVertex++] = scissoredVertices[2];
				for (int j = 3; j < m_scissoredTriangleVertexCount[i]; ++j) {
					m_resultIndices[nIndex++] = firstIndex;
					m_resultIndices[nIndex++] = nVertex - 1;
					m_resultIndices[nIndex++] = nVertex;
					m_resultNormal[nVertex] = scissoredNormals[j];
					m_result[nVertex++] = scissoredVertices[j];
				}
			}
			vertexOffset = nVertex;
			indexOffset = nIndex;
		}
	}
}
