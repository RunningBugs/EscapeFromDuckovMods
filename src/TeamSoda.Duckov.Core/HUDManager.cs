using System;
using System.Collections.Generic;
using System.Linq;
using Dialogues;
using Duckov.UI;
using Duckov.UI.Animations;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private static List<UnityEngine.Object> hideTokens = new List<UnityEngine.Object>();

	private bool ShouldDisplay
	{
		get
		{
			bool num = hideTokens.Any((UnityEngine.Object e) => e != null);
			bool flag = View.ActiveView != null;
			bool active = DialogueUI.Active;
			bool flag2 = CustomFaceUI.ActiveView != null;
			bool active2 = CameraMode.Active;
			if (!num && !flag && !active && !flag2)
			{
				return !active2;
			}
			return false;
		}
	}

	private static event Action onHideTokensChanged;

	private void Awake()
	{
		View.OnActiveViewChanged += OnActiveViewChanged;
		DialogueUI.OnDialogueStatusChanged += OnDialogueStatusChanged;
		CustomFaceUI.OnCustomUIViewChanged += OnCustomFaceViewChange;
		CameraMode.OnCameraModeChanged = (Action<bool>)Delegate.Combine(CameraMode.OnCameraModeChanged, new Action<bool>(OnCameraModeChanged));
		onHideTokensChanged += OnHideTokensChanged;
	}

	private void OnDestroy()
	{
		View.OnActiveViewChanged -= OnActiveViewChanged;
		DialogueUI.OnDialogueStatusChanged -= OnDialogueStatusChanged;
		CustomFaceUI.OnCustomUIViewChanged -= OnCustomFaceViewChange;
		CameraMode.OnCameraModeChanged = (Action<bool>)Delegate.Remove(CameraMode.OnCameraModeChanged, new Action<bool>(OnCameraModeChanged));
		onHideTokensChanged -= OnHideTokensChanged;
	}

	private void OnHideTokensChanged()
	{
		Refresh();
	}

	private void OnCameraModeChanged(bool value)
	{
		Refresh();
	}

	private void OnDialogueStatusChanged()
	{
		Refresh();
	}

	private void OnActiveViewChanged()
	{
		Refresh();
	}

	private void OnCustomFaceViewChange()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (ShouldDisplay)
		{
			canvasGroup.blocksRaycasts = true;
			if (!fadeGroup.IsShown)
			{
				fadeGroup.Show();
			}
		}
		else
		{
			canvasGroup.blocksRaycasts = false;
			if (!fadeGroup.IsHidden)
			{
				fadeGroup.Hide();
			}
		}
	}

	public static void RegisterHideToken(UnityEngine.Object obj)
	{
		hideTokens.Add(obj);
		HUDManager.onHideTokensChanged?.Invoke();
	}

	public static void UnregisterHideToken(UnityEngine.Object obj)
	{
		hideTokens.Remove(obj);
		HUDManager.onHideTokensChanged?.Invoke();
	}
}
