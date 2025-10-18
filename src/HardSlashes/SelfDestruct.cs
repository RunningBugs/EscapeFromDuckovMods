using UnityEngine;

namespace HardSlashes;

public class SelfDestruct : MonoBehaviour
{
	public float selfdestruct_in = 4f;

	private void Start()
	{
		if (selfdestruct_in != 0f)
		{
			Object.Destroy(base.gameObject, selfdestruct_in);
		}
	}
}
