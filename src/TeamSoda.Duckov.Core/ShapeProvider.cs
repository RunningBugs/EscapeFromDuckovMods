using UnityEngine;

public abstract class ShapeProvider : MonoBehaviour
{
	public abstract PipeRenderer.OrientedPoint[] GenerateShape();
}
