using System.Collections.Generic;
using UnityEngine;

public class BlueNoiseSetter : MonoBehaviour
{
	public List<Texture2D> blueNoises;

	private int index;

	private void Update()
	{
		Shader.SetGlobalTexture("GlobalBlueNoise", blueNoises[index]);
		index++;
		if (index >= blueNoises.Count)
		{
			index = 0;
		}
	}
}
