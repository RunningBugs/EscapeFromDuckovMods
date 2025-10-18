using System;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Duckov.UI;

public class Tooltips : MonoBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform contents;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI text;

	private static Action<ITooltipsProvider> OnEnterProvider;

	private static Action<ITooltipsProvider> OnExitProvider;

	public static ITooltipsProvider CurrentProvider { get; private set; }

	public static void NotifyEnterTooltipsProvider(ITooltipsProvider provider)
	{
		CurrentProvider = provider;
		OnEnterProvider?.Invoke(provider);
	}

	public static void NotifyExitTooltipsProvider(ITooltipsProvider provider)
	{
		if (CurrentProvider == provider)
		{
			CurrentProvider = null;
			OnExitProvider?.Invoke(provider);
		}
	}

	private void Awake()
	{
		if (rectTransform == null)
		{
			rectTransform = GetComponent<RectTransform>();
		}
		OnEnterProvider = (Action<ITooltipsProvider>)Delegate.Combine(OnEnterProvider, new Action<ITooltipsProvider>(DoOnEnterProvider));
		OnExitProvider = (Action<ITooltipsProvider>)Delegate.Combine(OnExitProvider, new Action<ITooltipsProvider>(DoOnExitProvider));
	}

	private void OnDestroy()
	{
		OnEnterProvider = (Action<ITooltipsProvider>)Delegate.Remove(OnEnterProvider, new Action<ITooltipsProvider>(DoOnEnterProvider));
		OnExitProvider = (Action<ITooltipsProvider>)Delegate.Remove(OnExitProvider, new Action<ITooltipsProvider>(DoOnExitProvider));
	}

	private void Update()
	{
		if (contents.gameObject.activeSelf)
		{
			RefreshPosition();
		}
	}

	private void DoOnExitProvider(ITooltipsProvider provider)
	{
		fadeGroup.Hide();
	}

	private void DoOnEnterProvider(ITooltipsProvider provider)
	{
		text.text = provider.GetTooltipsText();
		fadeGroup.Show();
	}

	private void RefreshPosition()
	{
		Vector2 value = Mouse.current.position.value;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, value, null, out var localPoint);
		contents.localPosition = localPoint;
	}
}
