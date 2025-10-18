using System;
using System.Diagnostics;
using SodaCraft.StringUtilities;
using UnityEngine;

public class NamedFormatTest : MonoBehaviour
{
	[Serializable]
	public struct Content
	{
		public string textA;

		public string textB;
	}

	public string format = "Displaying {textA} {textB}";

	public string format2 = "Displaying {0} {1}";

	public Content content;

	[SerializeField]
	private int loopCount = 100;

	private void Test()
	{
		string message = "";
		Stopwatch stopwatch = Stopwatch.StartNew();
		for (int i = 0; i < loopCount; i++)
		{
			message = format.Format(content);
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("Time Consumed 1:" + stopwatch.ElapsedMilliseconds);
		stopwatch = Stopwatch.StartNew();
		for (int j = 0; j < loopCount; j++)
		{
			message = string.Format(format2, content.textA, content.textB);
		}
		stopwatch.Stop();
		UnityEngine.Debug.Log("Time Consumed 2:" + stopwatch.ElapsedMilliseconds);
		UnityEngine.Debug.Log(message);
	}

	private void Test2()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		string message = format.Format(new { content.textA, content.textB });
		stopwatch.Stop();
		UnityEngine.Debug.Log("Time Consumed:" + stopwatch.ElapsedMilliseconds);
		UnityEngine.Debug.Log(message);
	}
}
