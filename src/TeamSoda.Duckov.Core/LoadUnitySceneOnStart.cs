using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadUnitySceneOnStart : MonoBehaviour
{
	public int sceneIndex;

	private void Start()
	{
		SceneManager.LoadScene(sceneIndex);
	}
}
