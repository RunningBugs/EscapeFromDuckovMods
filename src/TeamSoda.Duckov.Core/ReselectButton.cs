using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using UnityEngine;
using UnityEngine.UI;

public class ReselectButton : MonoBehaviour
{
	[SerializeField]
	private GameObject setActiveGroup;

	[SerializeField]
	private Button button;

	[SerializeField]
	[SceneID]
	private string prepareSceneID = "Prepare";

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClicked);
	}

	private void OnEnable()
	{
		setActiveGroup.SetActive((bool)LevelManager.Instance && LevelManager.Instance.IsBaseLevel);
	}

	private void OnDisable()
	{
	}

	private void OnButtonClicked()
	{
		SceneLoader.Instance.LoadScene(prepareSceneID).Forget();
		if ((bool)PauseMenu.Instance && PauseMenu.Instance.Shown)
		{
			PauseMenu.Hide();
		}
	}
}
