using UnityEngine;

public class RectStructRefTest : MonoBehaviour
{
	private void Test()
	{
		Rect rect = new Rect(Vector2.up, Vector2.one);
		Debug.Log("original: " + rect.size.ToString());
		Rect rect2 = rect;
		rect2.xMax = 20f;
		Debug.Log("Changed");
		Debug.Log("rect: " + rect.size.ToString());
		Debug.Log("rect2: " + rect2.size.ToString());
	}
}
