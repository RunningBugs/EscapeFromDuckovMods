using System.Collections.ObjectModel;
using Cysharp.Threading.Tasks;
using Duckov.Utilities;
using UnityEngine;

public class MultiInteractionMenu : MonoBehaviour
{
	[SerializeField]
	private MultiInteractionMenuButton buttonTemplate;

	[SerializeField]
	private float delayEachButton = 0.25f;

	private PrefabPool<MultiInteractionMenuButton> _buttonPool;

	private MultiInteraction target;

	private int currentTaskToken;

	public static MultiInteractionMenu Instance { get; private set; }

	private PrefabPool<MultiInteractionMenuButton> ButtonPool
	{
		get
		{
			if (_buttonPool == null)
			{
				_buttonPool = new PrefabPool<MultiInteractionMenuButton>(buttonTemplate, buttonTemplate.transform.parent);
				buttonTemplate.gameObject.SetActive(value: false);
			}
			return _buttonPool;
		}
	}

	public MultiInteraction Target => target;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		buttonTemplate.gameObject.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}

	private void Setup(MultiInteraction target)
	{
		this.target = target;
		ReadOnlyCollection<InteractableBase> interactables = target.Interactables;
		ButtonPool.ReleaseAll();
		foreach (InteractableBase item in interactables)
		{
			if (!(item == null))
			{
				MultiInteractionMenuButton multiInteractionMenuButton = ButtonPool.Get();
				multiInteractionMenuButton.Setup(item);
				multiInteractionMenuButton.transform.SetAsLastSibling();
			}
		}
	}

	private int CreateNewToken()
	{
		currentTaskToken = Random.Range(int.MinValue, int.MaxValue);
		return currentTaskToken;
	}

	private bool TokenChanged(int token)
	{
		return token != currentTaskToken;
	}

	public async UniTask SetupAndShow(MultiInteraction target)
	{
		base.gameObject.SetActive(value: true);
		int token = CreateNewToken();
		Setup(target);
		ReadOnlyCollection<MultiInteractionMenuButton> activeEntries = ButtonPool.ActiveEntries;
		foreach (MultiInteractionMenuButton item in activeEntries)
		{
			item.Show();
			await UniTask.WaitForSeconds(delayEachButton, ignoreTimeScale: true);
			if (TokenChanged(token))
			{
				return;
			}
		}
	}

	public async UniTask Hide()
	{
		int token = CreateNewToken();
		ReadOnlyCollection<MultiInteractionMenuButton> activeEntries = ButtonPool.ActiveEntries;
		foreach (MultiInteractionMenuButton item in activeEntries)
		{
			item.Hide();
			await UniTask.WaitForSeconds(delayEachButton, ignoreTimeScale: true);
			if (TokenChanged(token))
			{
				return;
			}
		}
		base.gameObject.SetActive(value: false);
	}
}
