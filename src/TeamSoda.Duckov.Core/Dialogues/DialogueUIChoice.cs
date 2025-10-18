using System;
using System.Collections.Generic;
using DG.Tweening;
using NodeCanvas.DialogueTrees;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dialogues;

public class DialogueUIChoice : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler
{
	[SerializeField]
	private MenuItem menuItem;

	[SerializeField]
	private GameObject selectionIndicator;

	[SerializeField]
	private Image confirmIndicator;

	[SerializeField]
	private Gradient confirmAnimationColor;

	[SerializeField]
	private float confirmAnimationDuration = 0.2f;

	[SerializeField]
	private TextMeshProUGUI text;

	private DialogueUI master;

	private int index;

	public int Index => index;

	private void Awake()
	{
		MenuItem obj = menuItem;
		obj.onSelected = (Action<MenuItem>)Delegate.Combine(obj.onSelected, new Action<MenuItem>(Refresh));
		MenuItem obj2 = menuItem;
		obj2.onDeselected = (Action<MenuItem>)Delegate.Combine(obj2.onDeselected, new Action<MenuItem>(Refresh));
		MenuItem obj3 = menuItem;
		obj3.onFocusStatusChanged = (Action<MenuItem, bool>)Delegate.Combine(obj3.onFocusStatusChanged, new Action<MenuItem, bool>(Refresh));
		MenuItem obj4 = menuItem;
		obj4.onConfirmed = (Action<MenuItem>)Delegate.Combine(obj4.onConfirmed, new Action<MenuItem>(OnConfirm));
	}

	private void OnConfirm(MenuItem item)
	{
		Confirm();
	}

	private void AnimateConfirm()
	{
		confirmIndicator.DOKill();
		confirmIndicator.DOGradientColor(confirmAnimationColor, confirmAnimationDuration).OnComplete(delegate
		{
			confirmIndicator.color = Color.clear;
		}).OnKill(delegate
		{
			confirmIndicator.color = Color.clear;
		});
	}

	private void Refresh(MenuItem item, bool focus)
	{
		selectionIndicator.SetActive(menuItem.IsSelected);
	}

	private void Refresh(MenuItem item)
	{
		selectionIndicator.SetActive(menuItem.IsSelected);
	}

	private void Confirm()
	{
		master.NotifyChoiceConfirmed(this);
		AnimateConfirm();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Confirm();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		menuItem.Select();
	}

	internal void Setup(DialogueUI master, KeyValuePair<IStatement, int> cur)
	{
		this.master = master;
		index = cur.Value;
		text.text = cur.Key.text;
		confirmIndicator.color = Color.clear;
		Refresh(menuItem);
	}
}
