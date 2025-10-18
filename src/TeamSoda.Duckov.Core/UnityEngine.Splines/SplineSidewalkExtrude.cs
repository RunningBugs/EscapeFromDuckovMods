using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[AddComponentMenu("Splines/Spline Sidewalk Extrude")]
public class SplineSidewalkExtrude : MonoBehaviour
{
	[Flags]
	public enum Sides
	{
		None = 0,
		Left = 1,
		Right = 2,
		Both = 3
	}

	private struct ProfileLine
	{
		public Vector3 start;

		public Vector3 end;

		public float u0;

		public float u1;

		public ProfileLine(Vector3 start, Vector3 end, float u0, float u1)
		{
			this.start = start;
			this.end = end;
			this.u0 = u0;
			this.u1 = u1;
		}
	}

	[SerializeField]
	[Tooltip("The Spline to extrude.")]
	private SplineContainer m_Container;

	[SerializeField]
	private float offset;

	[SerializeField]
	private float height;

	[SerializeField]
	private float width;

	[SerializeField]
	private float bevel;

	[SerializeField]
	private Sides sides = Sides.Both;

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
			Extrude(Splines[0], m_Mesh, m_SegmentsPerUnit, m_Range);
			m_NextScheduledRebuild = Time.time + 1f / (float)m_RebuildFrequency;
		}
	}

	private void Extrude<T>(T spline, Mesh mesh, float segmentsPerUnit, float2 range) where T : ISpline
	{
		mesh.Clear();
		if (sides == Sides.None)
		{
			return;
		}
		float num = Mathf.Abs(range.y - range.x);
		int num2 = Mathf.Max((int)Mathf.Ceil(spline.GetLength() * num * segmentsPerUnit), 1);
		float v = 0f;
		List<Vector3> verts = new List<Vector3>();
		List<Vector3> n = new List<Vector3>();
		List<Vector2> uv = new List<Vector2>();
		List<int> triangles = new List<int>();
		Vector3 vector = Vector3.zero;
		ProfileLine[] array = GenerateProfile();
		int profileVertexCount = array.Length * 2;
		float3 center;
		Vector3 forward;
		Vector3 up;
		Vector3 right;
		for (int i = 0; i < num2; i++)
		{
			bool isLastSegment = i == num2 - 1;
			float num3 = math.lerp(range.x, range.y, (float)i / ((float)num2 - 1f));
			if (num3 > 1f)
			{
				num3 = 1f;
			}
			if (num3 < 1E-07f)
			{
				num3 = 1E-07f;
			}
			spline.Evaluate(num3, out center, out var tangent, out var upVector);
			forward = ((Vector3)tangent).normalized;
			up = ((Vector3)upVector).normalized;
			right = Vector3.Cross(forward, up);
			if (i > 0)
			{
				v += ((Vector3)center - vector).magnitude;
			}
			ProfileLine[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				ProfileLine profileLine = array2[j];
				DrawLine(profileLine.start, profileLine.end, profileLine.u0, profileLine.u1);
			}
			vector = center;
			void DrawLine(Vector3 p0, Vector3 p1, float u0, float u1)
			{
				Vector3 vector2 = ProfileToObject(p0);
				Vector3 vector3 = ProfileToObject(p1);
				Vector3 item = Vector3.Cross(vector3 - vector2, forward);
				int count = verts.Count;
				verts.Add(vector2);
				verts.Add(vector3);
				n.Add(item);
				n.Add(item);
				uv.Add(new Vector2(u0 * uFactor, v * vFactor));
				uv.Add(new Vector2(u1 * uFactor, v * vFactor));
				if (!isLastSegment)
				{
					AddTriangles(new int[3]
					{
						count,
						count + 1,
						count + profileVertexCount
					});
					AddTriangles(new int[3]
					{
						count + 1,
						count + 1 + profileVertexCount,
						count + profileVertexCount
					});
				}
			}
		}
		mesh.vertices = verts.ToArray();
		mesh.uv = uv.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		void AddTriangles(int[] indicies)
		{
			triangles.AddRange(indicies);
		}
		Vector3 ProfileToObject(Vector3 profilePos)
		{
			return (Vector3)center + profilePos.x * right + profilePos.y * up + profilePos.z * forward;
		}
	}

	private ProfileLine[] GenerateProfile()
	{
		List<ProfileLine> lines = new List<ProfileLine>();
		float num = height - bevel;
		float num2 = Mathf.Sqrt(2f * bevel * bevel);
		float num3 = width - 2f * bevel;
		float uFactor = num + num2 + num3 + num2 + num;
		if ((sides | Sides.Left) == sides)
		{
			Add(0f - offset - width, 0f, 0f - offset - width, height - bevel, 0f, num);
			Add(0f - offset - width + bevel, height, 0f - offset - bevel, height, num + num2, num + num2 + num3);
			Add(0f - offset, height - bevel, 0f - offset, 0f, num + num2 + num3 + num2, num + num2 + num3 + num2 + num);
			if (bevel > 0f)
			{
				Add(0f - offset - width, height - bevel, 0f - offset - width + bevel, height, num, num + num2);
				Add(0f - offset - bevel, height, 0f - offset, height - bevel, num + num2 + num3, num + num2 + num3 + num2);
			}
		}
		if ((sides | Sides.Right) == sides)
		{
			Add(offset, 0f, offset, height - bevel, num + num2 + num3 + num2 + num, num + num2 + num3 + num2);
			Add(offset + bevel, height, offset + width - bevel, height, num + num2 + num3, num + num2);
			Add(offset + width, height - bevel, offset + width, 0f, num, 0f);
			if (bevel > 0f)
			{
				Add(offset, height - bevel, offset + bevel, height, num + num2 + num3 + num2, num + num2 + num3);
				Add(offset + width - bevel, height, offset + width, height - bevel, num + num2, num);
			}
		}
		return lines.ToArray();
		void Add(float x0, float y0, float x1, float y1, float u0, float u1)
		{
			lines.Add(new ProfileLine(new Vector3(x0, y0), new Vector3(x1, y1), u0 / uFactor, u1 / uFactor));
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
