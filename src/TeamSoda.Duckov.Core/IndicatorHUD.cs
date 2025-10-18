using UnityEngine;

public class IndicatorHUD : MonoBehaviour
{
	public GameObject mapIndicator;

	public GameObject toggleParent;

	public bool startActive;

	private void Start()
	{
		if ((LevelManager.Instance == null || LevelManager.Instance.IsBaseLevel) && (bool)mapIndicator)
		{
			mapIndicator.SetActive(value: false);
		}
		toggleParent.SetActive(startActive);
	}

	private void Awake()
	{
		UIInputManager.OnToggleIndicatorHUD += Toggle;
	}

	private void OnDestroy()
	{
		UIInputManager.OnToggleIndicatorHUD -= Toggle;
	}

	private void Toggle(UIInputEventData data)
	{
		if (base.gameObject.activeInHierarchy)
		{
			toggleParent.SetActive(!toggleParent.activeInHierarchy);
		}
	}
}
