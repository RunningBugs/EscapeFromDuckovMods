using System;
using Cysharp.Threading.Tasks;
using Duckov.Buffs;
using Duckov.UI.Animations;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class BuffsDisplayEntry : MonoBehaviour, IPoolable, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI remainingTimeText;

	[SerializeField]
	private TextMeshProUGUI layersText;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private PunchReceiver punchReceiver;

	[SerializeField]
	private string timeFormat = "{0:0}s";

	private BuffsDisplay master;

	private Buff target;

	private bool releasing;

	private bool pooled;

	public Image Icon => icon;

	public Buff Target => target;

	public static event Action<BuffsDisplayEntry, PointerEventData> OnBuffsDisplayEntryClicked;

	public void Setup(BuffsDisplay master, Buff target)
	{
		this.master = master;
		this.target = target;
		icon.sprite = target.Icon;
		if ((bool)displayName)
		{
			displayName.text = target.DisplayName;
		}
		fadeGroup.Show();
	}

	private void Update()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (target == null)
		{
			Release();
			return;
		}
		if (target.LimitedLifeTime)
		{
			remainingTimeText.text = string.Format(timeFormat, target.RemainingTime);
		}
		else
		{
			remainingTimeText.text = "";
		}
		if (target.MaxLayers > 1)
		{
			layersText.text = target.CurrentLayers.ToString();
		}
		else
		{
			layersText.text = "";
		}
	}

	public void Release()
	{
		if (!releasing)
		{
			releasing = true;
			ReleaseTask().Forget();
		}
	}

	private async UniTask ReleaseTask()
	{
		await fadeGroup.HideAndReturnTask();
		if (pooled)
		{
			master.ReleaseEntry(this);
		}
	}

	public void NotifyPooled()
	{
		pooled = true;
		releasing = false;
	}

	public void NotifyReleased()
	{
		pooled = false;
		target = null;
		releasing = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		punchReceiver?.Punch();
		BuffsDisplayEntry.OnBuffsDisplayEntryClicked?.Invoke(this, eventData);
	}
}
