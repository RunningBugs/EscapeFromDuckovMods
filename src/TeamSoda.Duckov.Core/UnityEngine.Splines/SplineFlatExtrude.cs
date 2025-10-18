using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("Splines/Spline Flat Extrude")]
public class SplineFlatExtrude : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The Spline to extrude.")]
	private SplineContainer m_Container;

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
	private int m_ProfileSeg = 2;

	[SerializeField]
	private float m_Height = 0.05f;

	[SerializeField]
	[Tooltip("The section of the Spline to extrude.")]
	private Vector2 m_Range = new Vector2(0f, 0.999f);

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

	public int ProfileSeg
	{
		get
		{
			return m_ProfileSeg;
		}
		set
		{
			m_ProfileSeg = value;
		}
	}

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
			Extrude(Splines[0], m_Mesh, m_Width, m_ProfileSeg, m_Height, m_SegmentsPerUnit, m_Range);
			m_NextScheduledRebuild = Time.time + 1f / (float)m_RebuildFrequency;
		}
	}

	private void Extrude<T>(T spline, Mesh mesh, float width, int profileSegments, float height, float segmentsPerUnit, float2 range) where T : ISpline
	{
		if (profileSegments < 2)
		{
			return;
		}
		float num = Mathf.Abs(range.y - range.x);
		int num2 = Mathf.Max((int)Mathf.Ceil(spline.GetLength() * num * segmentsPerUnit), 1);
		float num3 = 0f;
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < num2; i++)
		{
			float num4 = math.lerp(range.x, range.y, (float)i / ((float)num2 - 1f));
			if (num4 > 1f)
			{
				num4 = 1f;
			}
			spline.Evaluate(num4, out var position, out var tangent, out var upVector);
			Vector3 normalized = ((Vector3)tangent).normalized;
			Vector3 normalized2 = ((Vector3)upVector).normalized;
			Vector3 vector2 = Vector3.Cross(normalized, normalized2);
			float num5 = 1f / (float)(profileSegments - 1);
			if (i > 0)
			{
				num3 += ((Vector3)position - vector).magnitude;
			}
			for (int j = 0; j < profileSegments; j++)
			{
				float num6 = num5 * (float)j;
				float num7 = (num6 - 0.5f) * 2f;
				float num8 = Mathf.Cos(num7 * MathF.PI * 0.5f) * height;
				float num9 = num7 * width;
				Vector3 item = (Vector3)position + num9 * vector2 + num8 * normalized2;
				list.Add(item);
				list3.Add(new Vector2(num6 * uFactor, num3 * vFactor));
				list2.Add(normalized2);
			}
			vector = position;
		}
		List<int> triangles = new List<int>();
		for (int k = 0; k < num2 - 1; k++)
		{
			int num10 = k * profileSegments;
			for (int l = 0; l < profileSegments - 1; l++)
			{
				int num11 = num10 + l;
				AddTriangles(new int[3]
				{
					num11,
					num11 + 1,
					num11 + profileSegments
				});
				AddTriangles(new int[3]
				{
					num11 + 1,
					num11 + 1 + profileSegments,
					num11 + profileSegments
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
