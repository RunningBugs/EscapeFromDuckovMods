using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("Splines/Spline Profile Extrude")]
public class SplineProfileExtrude : MonoBehaviour
{
	[Serializable]
	private struct Vertex
	{
		public Vector3 position;

		public Vector3 normal;

		public float u;
	}

	[SerializeField]
	[Tooltip("The Spline to extrude.")]
	private SplineContainer m_Container;

	[SerializeField]
	private Vertex[] profile;

	[SerializeField]
	[Tooltip("Enable to regenerate the extruded mesh when the target Spline is modified. Disable this option if the Spline will not be modified at runtime.")]
	private bool m_RebuildOnSplineChange;

	[SerializeField]
	[Tooltip("The maximum number of times per-second that the mesh will be rebuilt.")]
	private int m_RebuildFrequency = 30;

	[SerializeField]
	[Tooltip("Automatically update any Mesh, Box, or Sphere collider components when the mesh is extruded.")]
	private bool m_UpdateColliders = true;

	[SerializeField]
	[Tooltip("The number of edge loops that comprise the length of one unit of the mesh. The total number of sections is equal to \"Spline.GetLength() * segmentsPerUnit\".")]
	private float m_SegmentsPerUnit = 4f;

	[SerializeField]
	[Tooltip("The radius of the extruded mesh.")]
	private float m_Width = 0.25f;

	[SerializeField]
	private float m_Height = 0.05f;

	[SerializeField]
	[Tooltip("The section of the Spline to extrude.")]
	private Vector2 m_Range = new Vector2(0f, 1f);

	[SerializeField]
	private float uFactor = 1f;

	[SerializeField]
	private float vFactor = 1f;

	private Mesh m_Mesh;

	private bool m_RebuildRequested;

	private float m_NextScheduledRebuild;

	[Obsolete("Use Container instead.", false)]
	public SplineContainer container => Container;

	public SplineContainer Container
	{
		get
		{
			return m_Container;
		}
		set
		{
			m_Container = value;
		}
	}

	[Obsolete("Use RebuildOnSplineChange instead.", false)]
	public bool rebuildOnSplineChange => RebuildOnSplineChange;

	public bool RebuildOnSplineChange
	{
		get
		{
			return m_RebuildOnSplineChange;
		}
		set
		{
			m_RebuildOnSplineChange = value;
		}
	}

	public int RebuildFrequency
	{
		get
		{
			return m_RebuildFrequency;
		}
		set
		{
			m_RebuildFrequency = Mathf.Max(value, 1);
		}
	}

	public float SegmentsPerUnit
	{
		get
		{
			return m_SegmentsPerUnit;
		}
		set
		{
			m_SegmentsPerUnit = Mathf.Max(value, 0.0001f);
		}
	}

	public float Width
	{
		get
		{
			return m_Width;
		}
		set
		{
			m_Width = Mathf.Max(value, 1E-05f);
		}
	}

	public int ProfileSeg => profile.Length;

	public float Height
	{
		get
		{
			return m_Height;
		}
		set
		{
			m_Height = value;
		}
	}

	public Vector2 Range
	{
		get
		{
			return m_Range;
		}
		set
		{
			m_Range = new Vector2(Mathf.Min(value.x, value.y), Mathf.Max(value.x, value.y));
		}
	}

	public Spline Spline => m_Container?.Spline;

	public IReadOnlyList<Spline> Splines => m_Container?.Splines;

	internal void Reset()
	{
		TryGetComponent<SplineContainer>(out m_Container);
		if (TryGetComponent<MeshFilter>(out var component))
		{
			component.sharedMesh = (m_Mesh = CreateMeshAsset());
		}
		if (TryGetComponent<MeshRenderer>(out var component2) && component2.sharedMaterial == null)
		{
			GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
			Material sharedMaterial = obj.GetComponent<MeshRenderer>().sharedMaterial;
			Object.DestroyImmediate(obj);
			component2.sharedMaterial = sharedMaterial;
		}
		Rebuild();
	}

	private void Start()
	{
		if (m_Container == null || m_Container.Spline == null)
		{
			Debug.LogError("Spline Extrude does not have a valid SplineContainer set.");
			return;
		}
		if ((m_Mesh = GetComponent<MeshFilter>().sharedMesh) == null)
		{
			Debug.LogError("SplineExtrude.createMeshInstance is disabled, but there is no valid mesh assigned. Please create or assign a writable mesh asset.");
		}
		Rebuild();
	}

	private void OnEnable()
	{
		Spline.Changed += OnSplineChanged;
	}

	private void OnDisable()
	{
		Spline.Changed -= OnSplineChanged;
	}

	private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
	{
		if (m_Container != null && Splines.Contains(spline) && m_RebuildOnSplineChange)
		{
			m_RebuildRequested = true;
		}
	}

	private void Update()
	{
		if (m_RebuildRequested && Time.time >= m_NextScheduledRebuild)
		{
			Rebuild();
		}
	}

	public void Rebuild()
	{
		if (!((m_Mesh = GetComponent<MeshFilter>().sharedMesh) == null))
		{
			Extrude(Splines[0], profile, m_Mesh, m_SegmentsPerUnit, m_Range);
			m_NextScheduledRebuild = Time.time + 1f / (float)m_RebuildFrequency;
		}
	}

	private void Extrude<T>(T spline, Vertex[] profile, Mesh mesh, float segmentsPerUnit, float2 range) where T : ISpline
	{
		int num = profile.Length;
		if (num < 2)
		{
			return;
		}
		float num2 = Mathf.Abs(range.y - range.x);
		int num3 = Mathf.Max((int)Mathf.Ceil(spline.GetLength() * num2 * segmentsPerUnit), 1);
		float num4 = 0f;
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < num3; i++)
		{
			float num5 = math.lerp(range.x, range.y, (float)i / ((float)num3 - 1f));
			if (num5 > 1f)
			{
				num5 = 1f;
			}
			if (num5 < 1E-07f)
			{
				num5 = 1E-07f;
			}
			spline.Evaluate(num5, out var position, out var tangent, out var upVector);
			Vector3 normalized = ((Vector3)tangent).normalized;
			Vector3 normalized2 = ((Vector3)upVector).normalized;
			Vector3 vector2 = Vector3.Cross(normalized, normalized2);
			_ = 1f / (float)(num - 1);
			if (i > 0)
			{
				num4 += ((Vector3)position - vector).magnitude;
			}
			for (int j = 0; j < num; j++)
			{
				Vertex vertex = profile[j];
				float u = vertex.u;
				float y = vertex.position.y;
				float x = vertex.position.x;
				float z = vertex.position.z;
				Vector3 item = Quaternion.FromToRotation(Vector3.up, normalized2) * vertex.normal;
				Vector3 item2 = (Vector3)position + x * vector2 + y * normalized2 + z * normalized;
				list.Add(item2);
				list3.Add(new Vector2(u * uFactor, num4 * vFactor));
				list2.Add(item);
			}
			vector = position;
		}
		List<int> triangles = new List<int>();
		for (int k = 0; k < num3 - 1; k++)
		{
			int num6 = k * num;
			for (int l = 0; l < num - 1; l++)
			{
				int num7 = num6 + l;
				AddTriangles(new int[3]
				{
					num7,
					num7 + 1,
					num7 + num
				});
				AddTriangles(new int[3]
				{
					num7 + 1,
					num7 + 1 + num,
					num7 + num
				});
			}
		}
		mesh.Clear();
		mesh.vertices = list.ToArray();
		mesh.uv = list3.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		void AddTriangles(int[] indicies)
		{
			triangles.AddRange(indicies);
		}
	}

	private void OnValidate()
	{
		Rebuild();
	}

	internal Mesh CreateMeshAsset()
	{
		return new Mesh
		{
			name = base.name
		};
	}

	private void FlattenSpline()
	{
	}
}
