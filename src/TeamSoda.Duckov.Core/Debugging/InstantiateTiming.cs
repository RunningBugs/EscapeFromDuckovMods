using UnityEngine;

namespace Debugging;

public class InstantiateTiming : MonoBehaviour
{
	public GameObject prefab;

	public void InstantiatePrefab()
	{
		Debug.Log("Start Instantiate");
		Object.Instantiate(prefab);
		Debug.Log("Instantiated");
	}

	private void Awake()
	{
		Debug.Log("Awake");
	}

	private void Start()
	{
		Debug.Log("Start");
	}
}
