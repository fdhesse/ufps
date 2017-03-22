//
// OctMeshTree.cs
//
// Fast Shadow Receiver
//
// Copyright 2014 NYAHOON GAMES PTE. LTD. All Rights Reserved.
//
using UnityEngine;
using System.Collections.Generic;

namespace FastShadowReceiver {
	public class OctMeshTree : MeshTree {
		public float m_minNodeSize = 1.0f;

		[System.Serializable]
		private struct TreeNode {
			public int     m_childIndex;
			public int[]   m_triangles;
			public void SetChildIndex(int index, uint collapseFlags)
			{
				m_childIndex = (int)(index << 3 | (byte)(collapseFlags & 0x7));
			}
			public int childIndex {
				get { return m_childIndex >> 3; }
			}
			public uint collapseFlags {
				get { return (uint)(m_childIndex & 0x7); }
			}
		}
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3)
		[SerializeField][HideInInspector] // from Unity 4.5, struct can be serialized!
#endif
		private TreeNode[] m_treeNodes;

#if UNITY_EDITOR
		public override int GetMemoryUsage ()
		{
			int size = base.GetMemoryUsage ();
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3)
			if (m_treeNodes != null && m_treeNodes.Length != 0 && (m_childNodes == null || m_childNodes.Length == 0)) {
				size += m_treeNodes.Length * (sizeof(int) + System.IntPtr.Size);
				for (int i = 0; i < m_treeNodes.Length; ++i) {
					if (m_treeNodes[i].m_triangles != null) {
						size += m_treeNodes[i].m_triangles.Length * sizeof(int) + sizeof(int);
					}
				}
			}
#endif
			return size;
		}
		public override int GetNodeCount ()
		{
			if (m_treeNodes != null && m_treeNodes.Length != 0) {
				return m_treeNodes.Length;
			}
			return base.GetNodeCount();
		}
		private float m_totalVolume;
		private float m_leafNodesVolume;
		public override float GetBuildProgress ()
		{
			if (m_treeNodes != null) {
				return 1.0f;
			}
			if (m_totalVolume == 0) {
				return 0;
			}
			return m_leafNodesVolume/m_totalVolume;
		}
#endif
		public override bool IsBuildFinished()
		{
			return m_treeNodes != null && 0 < m_treeNodes.Length;
		}
		public override System.Type GetSearchType ()
		{
			return typeof(OctMeshTreeSearch);
		}
		public override MeshTreeSearch CreateSearch()
		{
			return new OctMeshTreeSearch();
		}
		public override void Search (MeshTreeSearch search)
		{
			if (!(search is OctMeshTreeSearch)) {
				Debug.LogError("Invalid MeshTreeSearch class!");
				return;
			}
			OctMeshTreeSearch octSearch = (OctMeshTreeSearch)search;
			octSearch.Initialize();
			Search(octSearch, ref m_treeNodes[m_treeNodes.Length - 1], m_bounds.center, m_bounds.extents);
			if (search.m_bOutputNormals && m_normals != null && 0 < m_normals.Length) {
				octSearch.Finalize(m_vertices, m_indices, m_normals);
			}
			else {
				octSearch.Finalize(m_vertices, m_indices);
			}
		}
		public override void Raycast(MeshTreeRaycast raycast)
		{
			MeshTreeRaycast.TemporaryParam param = raycast.CreateTemporaryParam();
			uint order = 0x76543210u;
			if (raycast.direction.x < 0.0f) {
				order ^= 0x11111111u;
			}
			if (raycast.direction.y < 0.0f) {
				order ^= 0x22222222u;
			}
			if (raycast.direction.z < 0.0f) {
				order ^= 0x44444444u;
			}
			Raycast(raycast, ref m_treeNodes[m_treeNodes.Length - 1], m_bounds.center, m_bounds.extents, ref param, order);
		}
		public override void BuildFromPrebuiltData ()
		{
			if (m_treeNodes == null || m_treeNodes.Length == 0) {
				int numNodes = m_childNodes.Length;
				TreeNode[] treeNodes = new TreeNode[numNodes];
				for (int i = 0; i < numNodes; ++i) {
					treeNodes[i].m_childIndex = m_childNodes[i];
					if (m_triangleLists[i] != null && m_triangleLists[i].triangles != null && 0 < m_triangleLists[i].triangles.Length) {
						treeNodes[i].m_triangles = m_triangleLists[i].triangles;
					}
					else {
						treeNodes[i].m_triangles = null;
					}
				}
				m_treeNodes = treeNodes;
			}
#if !(UNITY_EDITOR || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3)
			// release memory
			m_childNodes = null;
			m_triangleLists = null;
#endif
		}

		private void AddAllTrianglesInChildren(OctMeshTreeSearch search, ref TreeNode node, Vector3 center, Vector3 extents)
		{
			if (node.m_triangles != null && 0 < node.m_triangles.Length) {
				search.AddTriangles(node.m_triangles, center, extents);
			}
			else if (0 <= node.m_childIndex) {
				int childIndex = node.childIndex;
				uint collapseFlags = node.collapseFlags;
				for (int i = 0; i < 8; ++i) {
					if ((collapseFlags & i) == 0) {
						AddAllTrianglesInChildren(search, ref m_treeNodes[childIndex++], center, extents);
					}
				}
			}
		}

		private void Search(OctMeshTreeSearch search, ref TreeNode node, Vector3 center, Vector3 extents)
		{
			bool noTriangles = (node.m_triangles == null || node.m_triangles.Length == 0);
			if (node.childIndex < 0 && noTriangles) {
				return;
			}
			bool isPartial;
			uint flags = search.IsInView(center, extents, out isPartial);
			if (flags == 0) {
				return;
			}
			if (isPartial && 0 <= node.m_childIndex) {
				int childIndex = node.childIndex;
				uint collapseFlags = node.collapseFlags;
				Vector3 halfExtents = extents;
				if (collapseFlags != 0) {
					if ((collapseFlags & 0x1) != 0) {
						flags = (flags | (flags >> 1)) & OctMeshTreeSearch.FLAGS_LEFT;
					}
					else {
						halfExtents.x *= 0.5f;
					}
					if ((collapseFlags & 0x2) != 0) {
						flags = (flags | (flags >> 2)) & OctMeshTreeSearch.FLAGS_BOTTOM;
					}
					else {
						halfExtents.y *= 0.5f;
					}
					if ((collapseFlags & 0x4) != 0) {
						flags = (flags | (flags >> 4)) & OctMeshTreeSearch.FLAGS_FRONT;
					}
					else {
						halfExtents.z *= 0.5f;
					}
				}
				else {
					halfExtents *= 0.5f;
				}
				center = center - extents + halfExtents;
				for (int i = 0; i < 8 && flags != 0; ++i, flags >>= 1) {
					if ((collapseFlags & i) == 0) {
						if ((flags & 1) != 0) {
							Vector3 p = center;
							if ((i & 1) != 0) {
								p.x += extents.x;
							}
							if ((i & 2) != 0) {
								p.y += extents.y;
							}
							if ((i & 4) != 0) {
								p.z += extents.z;
							}
							if (0 <= m_treeNodes[childIndex].m_childIndex) {
								Search(search, ref m_treeNodes[childIndex], p, halfExtents);
							}
							else if (m_treeNodes[childIndex].m_triangles != null) {
								search.AddTriangles(m_treeNodes[childIndex].m_triangles, p, halfExtents);
							}
						}
						++childIndex;
					}
				}
			}
			else {
				if (noTriangles) {
					int childIndex = node.childIndex;
					uint collapseFlags = node.collapseFlags;
					for (int i = 0; i < 8; ++i) {
						if ((collapseFlags & i) == 0) {
							AddAllTrianglesInChildren(search, ref m_treeNodes[childIndex++], center, extents);
						}
					}
				}
				else {
					search.AddTriangles(node.m_triangles, center, extents);
				}
			}
		}

		private bool Raycast(MeshTreeRaycast raycast, ref TreeNode node, Vector3 center, Vector3 extents, ref MeshTreeRaycast.TemporaryParam param, uint order)
		{
			float distance;
			if (!raycast.BoundsHitTest(center, extents, param, out distance)) {
				return false;
			}
			if (node.m_childIndex < 0) {
				if (node.m_triangles == null || node.m_triangles.Length == 0) {
					return false;
				}
				bool hit = false;
				for (int i = 0; i < node.m_triangles.Length; ++i) {
					int tri = node.m_triangles[i];
					if (raycast.TriangleHitTest(m_vertices[m_indices[tri]], m_vertices[m_indices[tri+1]], m_vertices[m_indices[tri+2]])) {
						hit = true;
					}
				}
				return hit;
			}
			uint collapseFlags = node.collapseFlags;
			Vector3 halfExtents = extents;
			if (collapseFlags != 0) {
				uint flags = 0xff;
				uint collapsedOrder = order;
				int indexShift = 0;
				uint shiftMask = 0;
				if ((collapseFlags & 0x1) != 0) {
					collapsedOrder &= ~0x11111111u;
					flags &= 0x55;
					indexShift = 1;
				}
				else {
					halfExtents.x *= 0.5f;
				}
				if ((collapseFlags & 0x2) != 0) {
					collapsedOrder &= ~0x22222222u;
					flags &= 0x33;
					shiftMask = 1u >> indexShift;
					++indexShift;
				}
				else {
					halfExtents.y *= 0.5f;
				}
				if ((collapseFlags & 0x4) != 0) {
					collapsedOrder &= ~0x44444444u;
					flags &= 0x0F;
				}
				else {
					halfExtents.z *= 0.5f;
				}
				center = center - extents + halfExtents;
				for (int i = 0; i < 8 && flags != 0; ++i, flags >>= 1) {
					Vector3 p = center;
					if ((flags & 1) != 0) {
						uint n = (collapsedOrder >> (4*i)) & 0xF;
						if ((n & 1) != 0) {
							p.x += extents.x;
						}
						if ((n & 2) != 0) {
							p.y += extents.y;
						}
						if ((n & 4) != 0) {
							p.z += extents.z;
						}
						int index = (int)(node.childIndex + ((n >> indexShift) | (n & shiftMask)));
						if (Raycast(raycast, ref m_treeNodes[index], p, halfExtents, ref param, order)) {
							return true;
						}
					}
				}
			}
			else {
				halfExtents *= 0.5f;
				center = center - extents + halfExtents;
				for (int i = 0; i < 8; ++i) {
					Vector3 p = center;
					uint n = (order >> (4*i)) & 0xF;
					if ((n & 1) != 0) {
						p.x += extents.x;
					}
					if ((n & 2) != 0) {
						p.y += extents.y;
					}
					if ((n & 4) != 0) {
						p.z += extents.z;
					}
					if (Raycast(raycast, ref m_treeNodes[node.childIndex+n], p, halfExtents, ref param, order)) {
						return true;
					}
				}
			}
			return false;
		}

		protected override void Clear ()
		{
			base.Clear ();
			m_treeNodes = null;
		}

		protected override void PrepareForBuild ()
		{
			base.PrepareForBuild ();
			m_treeNodes = null;
#if UNITY_EDITOR
			m_totalVolume = 1.0f;
			if (Mathf.Epsilon < m_bounds.extents.x) {
				m_totalVolume *= m_bounds.extents.x;
			}
			if (Mathf.Epsilon < m_bounds.extents.y) {
				m_totalVolume *= m_bounds.extents.y;
			}
			if (Mathf.Epsilon < m_bounds.extents.z) {
				m_totalVolume *= m_bounds.extents.z;
			}
			m_leafNodesVolume = 0.0f;
#endif
		}
#if UNITY_EDITOR
		private void AddLeafNodeVoluem(Vector3 extents)
		{
			float volume = 1.0f;
			if (Mathf.Epsilon < m_bounds.extents.x) {
				volume *= extents.x;
			}
			if (Mathf.Epsilon < m_bounds.extents.y) {
				volume *= extents.y;
			}
			if (Mathf.Epsilon < m_bounds.extents.z) {
				volume *= extents.z;
			}
			m_leafNodesVolume += volume;
		}
#endif

		struct TriangleBounds {
			public Vector3 m_min;
			public Vector3 m_max;
		}
		private const int LEAFNODE_TRIANGLE_COUNT = 2;

		protected override void Build ()
		{
			int numTriangles = m_indices.Length/3;
			TriangleBounds[] triangleBounds = new TriangleBounds[numTriangles];
			int index = 0;
			List<TreeNode> nodeList = new List<TreeNode>();
			TreeNode rootNode;
			rootNode.m_childIndex = -1;
			rootNode.m_triangles = null;
			List<int> triangleList = new List<int>();
			for (int i = 0; i < numTriangles; ++i) {
				triangleList.Add(index);
				Vector3 v0 = m_vertices[m_indices[index++]];
				Vector3 v1 = m_vertices[m_indices[index++]];
				Vector3 v2 = m_vertices[m_indices[index++]];
				Vector3 min, max;
				min.x = Mathf.Min(Mathf.Min(v0.x, v1.x), v2.x);
				min.y = Mathf.Min(Mathf.Min(v0.y, v1.y), v2.y);
				min.z = Mathf.Min(Mathf.Min(v0.z, v1.z), v2.z);
				max.x = Mathf.Max(Mathf.Max(v0.x, v1.x), v2.x);
				max.y = Mathf.Max(Mathf.Max(v0.y, v1.y), v2.y);
				max.z = Mathf.Max(Mathf.Max(v0.z, v1.z), v2.z);
				triangleBounds[i].m_min = min;
				triangleBounds[i].m_max = max;
			}
			BuildMeshTree(nodeList, ref rootNode, m_bounds.center, m_bounds.extents, triangleList, triangleBounds);
			nodeList.Add(rootNode);
#if UNITY_EDITOR && (UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3)
			int numNodes = nodeList.Count;
			int[] childNodes = new int[numNodes];
			TriangleList[] triangleLists = new TriangleList[numNodes];
			for (int i = 0; i < numNodes; ++i) {
				TreeNode node = nodeList[i];
				childNodes[i] = node.m_childIndex;
				if (node.m_triangles != null) {
					triangleLists[i] = new TriangleList();
					triangleLists[i].triangles = node.m_triangles;
				}
				else {
					triangleLists[i] = null;
				}
			}
			m_childNodes = childNodes;
			m_triangleLists = triangleLists;
#endif
			m_treeNodes = nodeList.ToArray();
		}
		private static void MergeSortedTriangleList(List<int> list1, List<int> list2)
		{
			foreach (int i in list2) {
				if (list1.BinarySearch(i) < 0) {
					list1.Add(i);
				}
			}
			list1.Sort();
			list2.Clear();
		}
		private void BuildMeshTree(List<TreeNode> nodeList, ref TreeNode parentNode, Vector3 center, Vector3 extents, List<int> triangleList, TriangleBounds[] triangleBounds)
		{
			int numTriangles = triangleList.Count;
			float maxExtent = Mathf.Max(Mathf.Max(extents.x, extents.y), extents.z);
			if (numTriangles <= LEAFNODE_TRIANGLE_COUNT || maxExtent < m_minNodeSize) {
				parentNode.m_triangles = triangleList.ToArray();
				parentNode.m_childIndex = -1;
#if UNITY_EDITOR
				AddLeafNodeVoluem(extents);
#endif
				return;
			}
			List<int> list0 = new List<int>();
			List<int> list1 = new List<int>();
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			List<int> list5 = new List<int>();
			List<int> list6 = new List<int>();
			List<int> list7 = new List<int>();
			uint collapseFlags = 0x7;
			for (int i = 0; i < numTriangles; ++i) {
				int tri = triangleList[i];
				TriangleBounds bounds = triangleBounds[tri/3];
				uint collapse = 0x07;
				uint xFlags = 0;
				if (bounds.m_min.x <= center.x) {
					xFlags |= OctMeshTreeSearch.FLAGS_LEFT;
					collapse ^= 0x1;
				}
				if (center.x < bounds.m_max.x) {
					xFlags |= OctMeshTreeSearch.FLAGS_RIGHT;
					collapse ^= 0x1;
				}
				uint yFlags = 0;
				if (bounds.m_min.y <= center.y) {
					yFlags |= OctMeshTreeSearch.FLAGS_BOTTOM;
					collapse ^= 0x2;
				}
				if (center.y < bounds.m_max.y) {
					yFlags |= OctMeshTreeSearch.FLAGS_TOP;
					collapse ^= 0x2;
				}
				uint zFlags = 0;
				if (bounds.m_min.z <= center.z) {
					zFlags |= OctMeshTreeSearch.FLAGS_FRONT;
					collapse ^= 0x4;
				}
				if (center.z < bounds.m_max.z) {
					zFlags |= OctMeshTreeSearch.FLAGS_BACK;
					collapse ^= 0x4;
				}
				uint flags = xFlags & yFlags & zFlags;
				if ((flags & (1 << 0)) != 0) {
					list0.Add(tri);
				}
				if ((flags & (1 << 1)) != 0) {
					list1.Add(tri);
				}
				if ((flags & (1 << 2)) != 0) {
					list2.Add(tri);
				}
				if ((flags & (1 << 3)) != 0) {
					list3.Add(tri);
				}
				if ((flags & (1 << 4)) != 0) {
					list4.Add(tri);
				}
				if ((flags & (1 << 5)) != 0) {
					list5.Add(tri);
				}
				if ((flags & (1 << 6)) != 0) {
					list6.Add(tri);
				}
				if ((flags & (1 << 7)) != 0) {
					list7.Add(tri);
				}
				collapseFlags &= collapse;
			}
			maxExtent = 0.5f*maxExtent;
			if (extents.x < maxExtent) {
				collapseFlags |= 0x1;
			}
			if (extents.y < maxExtent) {
				collapseFlags |= 0x2;
			}
			if (extents.z < maxExtent) {
				collapseFlags |= 0x4;
			}
			if (collapseFlags == 0x7) {
				parentNode.m_triangles = triangleList.ToArray();
				parentNode.m_childIndex = -1;
#if UNITY_EDITOR
				AddLeafNodeVoluem(extents);
#endif
				return;
			}
			triangleList.Clear();
			triangleList.Capacity = 0;
			Vector3 halfExtent = extents;
			if ((collapseFlags & 0x1) != 0) {
				MergeSortedTriangleList(list0, list1);
				MergeSortedTriangleList(list2, list3);
				MergeSortedTriangleList(list4, list5);
				MergeSortedTriangleList(list6, list7);
			}
			else {
				halfExtent.x *= 0.5f;
			}
			if ((collapseFlags & 0x2) != 0) {
				MergeSortedTriangleList(list0, list2);
				MergeSortedTriangleList(list1, list3);
				MergeSortedTriangleList(list4, list6);
				MergeSortedTriangleList(list5, list7);
			}
			else {
				halfExtent.y *= 0.5f;
			}
			if ((collapseFlags & 0x4) != 0) {
				MergeSortedTriangleList(list0, list4);
				MergeSortedTriangleList(list1, list5);
				MergeSortedTriangleList(list2, list6);
				MergeSortedTriangleList(list3, list7);
			}
			else {
				halfExtent.z *= 0.5f;
			}
			TreeNode child0 = new TreeNode();
			TreeNode child1 = new TreeNode();
			TreeNode child2 = new TreeNode();
			TreeNode child3 = new TreeNode();
			TreeNode child4 = new TreeNode();
			TreeNode child5 = new TreeNode();
			TreeNode child6 = new TreeNode();
			TreeNode child7 = new TreeNode();
			center = center - extents + halfExtent;
			extents = 2.0f*(extents - halfExtent);
			Vector3 p = center;
			BuildMeshTree(nodeList, ref child0, p, halfExtent, list0, triangleBounds);
			p.x += extents.x;
			BuildMeshTree(nodeList, ref child1, p, halfExtent, list1, triangleBounds);
			p.x = center.x;
			p.y += extents.y;
			BuildMeshTree(nodeList, ref child2, p, halfExtent, list2, triangleBounds);
			p.x += extents.x;
			BuildMeshTree(nodeList, ref child3, p, halfExtent, list3, triangleBounds);
			p = center;
			p.z += extents.z;
			BuildMeshTree(nodeList, ref child4, p, halfExtent, list4, triangleBounds);
			p.x += extents.x;
			BuildMeshTree(nodeList, ref child5, p, halfExtent, list5, triangleBounds);
			p.x = center.x;
			p.y += extents.y;
			BuildMeshTree(nodeList, ref child6, p, halfExtent, list6, triangleBounds);
			p.x += extents.x;
			BuildMeshTree(nodeList, ref child7, p, halfExtent, list7, triangleBounds);
			
			parentNode.SetChildIndex(nodeList.Count, collapseFlags);
			nodeList.Add(child0);
			if ((collapseFlags & 0x1) == 0) {
				nodeList.Add(child1);
			}
			if ((collapseFlags & 0x2) == 0) {
				nodeList.Add(child2);
			}
			if ((collapseFlags & 0x3) == 0) {
				nodeList.Add(child3);
			}
			if ((collapseFlags & 0x4) == 0) {
				nodeList.Add(child4);
				if ((collapseFlags & 0x1) == 0) {
					nodeList.Add(child5);
				}
				if ((collapseFlags & 0x2) == 0) {
					nodeList.Add(child6);
				}
				if ((collapseFlags & 0x3) == 0) {
					nodeList.Add(child7);
				}
			}
		}
	}
}
