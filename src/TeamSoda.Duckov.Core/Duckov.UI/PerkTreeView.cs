using System;
using System.Collections.ObjectModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.PerkTrees;
using Duckov.UI.Animations;
using Duckov.Utilities;
using NodeCanvas.Framework;
using TMPro;
using UI_Spline_Renderer;
using UnityEngine;
using UnityEngine.Splines;

namespace Duckov.UI;

public class PerkTreeView : View, ISingleSelectionMenu<PerkEntry>
{
	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private RectTransform contentParent;

	[SerializeField]
	private PerkDetails details;

	[SerializeField]
	private PerkEntry perkEntryPrefab;

	[SerializeField]
	private PerkLineEntry perkLinePrefab;

	[SerializeField]
	private UISplineRenderer activeConnectionsRenderer;

	[SerializeField]
	private UISplineRenderer inactiveConnectionsRenderer;

	[SerializeField]
	private float splineTangent = 100f;

	[SerializeField]
	private PerkTree target;

	private PrefabPool<PerkEntry> _perkEntryPool;

	private PrefabPool<PerkLineEntry> _perkLinePool;

	private PerkEntry selectedPerkEntry;

	[SerializeField]
	private float layoutFactor = 10f;

	[SerializeField]
	private Vector2 padding = Vector2.one;

	public static PerkTreeView Instance => View.GetViewInstance<PerkTreeView>();

	private PrefabPool<PerkEntry> PerkEntryPool
	{
		get
		{
			if (_perkEntryPool == null)
			{
				_perkEntryPool = new PrefabPool<PerkEntry>(perkEntryPrefab, contentParent);
			}
			return _perkEntryPool;
		}
	}

	private PrefabPool<PerkLineEntry> PerkLinePool
	{
		get
		{
			if (_perkLinePool == null)
			{
				_perkLinePool = new PrefabPool<PerkLineEntry>(perkLinePrefab, contentParent);
			}
			return _perkLinePool;
		}
	}

	protected override bool ShowOpenCloseButtons => false;

	internal event Action<PerkEntry> onSelectionChanged;

	private void PopulatePerks()
	{
		contentParent.ForceUpdateRectTransforms();
		PerkEntryPool.ReleaseAll();
		PerkLinePool.ReleaseAll();
		bool isDemo = GameMetaData.Instance.IsDemo;
		foreach (Perk perk in target.Perks)
		{
			if ((!isDemo || !perk.LockInDemo) && target.RelationGraphOwner.GetRelatedNode(perk) != null)
			{
				PerkEntryPool.Get(contentParent).Setup(this, perk);
			}
		}
		foreach (PerkLevelLineNode item in target.RelationGraphOwner.graph.GetAllNodesOfType<PerkLevelLineNode>())
		{
			PerkLinePool.Get(contentParent).Setup(this, item);
		}
		FitChildren();
		RefreshConnections();
	}

	private void RefreshConnections()
	{
		bool isDemo = GameMetaData.Instance.IsDemo;
		activeConnectionsRenderer.enabled = false;
		inactiveConnectionsRenderer.enabled = false;
		SplineContainer splineContainer = activeConnectionsRenderer.splineContainer;
		SplineContainer splineContainer2 = inactiveConnectionsRenderer.splineContainer;
		ClearSplines(splineContainer);
		ClearSplines(splineContainer2);
		bool horizontal = target.Horizontal;
		Vector3 splineTangentVector = (horizontal ? Vector3.left : Vector3.up) * splineTangent;
		foreach (Perk perk in target.Perks)
		{
			if (isDemo && perk.LockInDemo)
			{
				continue;
			}
			PerkRelationNode relatedNode = target.RelationGraphOwner.GetRelatedNode(perk);
			PerkEntry perkEntry = GetPerkEntry(perk);
			if (perkEntry == null || relatedNode == null)
			{
				continue;
			}
			SplineContainer container = (perk.Unlocked ? splineContainer : splineContainer2);
			foreach (Connection outConnection in relatedNode.outConnections)
			{
				PerkRelationNode perkRelationNode = outConnection.targetNode as PerkRelationNode;
				Perk relatedNode2 = perkRelationNode.relatedNode;
				if (relatedNode2 == null)
				{
					Debug.Log("Target Perk is Null (Connection from " + relatedNode.name + " to " + perkRelationNode.name + ")");
				}
				else if (!isDemo || !relatedNode2.LockInDemo)
				{
					PerkEntry perkEntry2 = GetPerkEntry(relatedNode2);
					if (perkEntry2 == null)
					{
						Debug.Log("Target Perk Entry is Null (Connection from " + relatedNode.name + " to " + perkRelationNode.name + ")");
					}
					else
					{
						AddConnection(container, perkEntry.transform.localPosition, perkEntry2.transform.localPosition);
					}
				}
			}
		}
		activeConnectionsRenderer.enabled = true;
		inactiveConnectionsRenderer.enabled = true;
		void AddConnection(SplineContainer container2, Vector2 from, Vector2 to)
		{
			if (horizontal)
			{
				container2.AddSpline(new Spline(new BezierKnot[4]
				{
					new BezierKnot((Vector3)from, splineTangentVector, -splineTangentVector),
					new BezierKnot((Vector3)from - splineTangentVector, splineTangentVector, -splineTangentVector),
					new BezierKnot(new Vector3(from.x, to.y) - 2f * splineTangentVector, splineTangentVector, -splineTangentVector),
					new BezierKnot((Vector3)to, splineTangentVector, -splineTangentVector)
				}));
			}
			else
			{
				container2.AddSpline(new Spline(new BezierKnot[4]
				{
					new BezierKnot((Vector3)from, splineTangentVector, -splineTangentVector),
					new BezierKnot((Vector3)from - splineTangentVector, splineTangentVector, -splineTangentVector),
					new BezierKnot(new Vector3(to.x, from.y) - 2f * splineTangentVector, splineTangentVector, -splineTangentVector),
					new BezierKnot((Vector3)to, splineTangentVector, -splineTangentVector)
				}));
			}
		}
		static void ClearSplines(SplineContainer splineContainer3)
		{
			while (splineContainer3.Splines.Count > 0)
			{
				splineContainer3.RemoveSplineAt(0);
			}
		}
	}

	private PerkEntry GetPerkEntry(Perk ofPerk)
	{
		return PerkEntryPool.ActiveEntries.FirstOrDefault((PerkEntry e) => e != null && e.Target == ofPerk);
	}

	private void FitChildren()
	{
		contentParent.ForceUpdateRectTransforms();
		ReadOnlyCollection<PerkEntry> activeEntries = PerkEntryPool.ActiveEntries;
		float num2;
		float num = (num2 = float.MaxValue);
		float num4;
		float num3 = (num4 = float.MinValue);
		foreach (PerkEntry item in activeEntries)
		{
			RectTransform rectTransform = item.RectTransform;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.zero;
			Vector2 layoutPosition = item.GetLayoutPosition();
			layoutPosition.y *= -1f;
			Vector2 vector = (rectTransform.anchoredPosition = layoutPosition * layoutFactor);
			if (vector.x < num)
			{
				num = vector.x;
			}
			if (vector.y < num2)
			{
				num2 = vector.y;
			}
			if (vector.x > num3)
			{
				num3 = vector.x;
			}
			if (vector.y > num4)
			{
				num4 = vector.y;
			}
		}
		float num5 = num3 - num;
		float num6 = num4 - num2;
		Vector2 vector3 = -new Vector2(num, num2);
		RectTransform rectTransform2 = contentParent;
		Vector2 sizeDelta = rectTransform2.sizeDelta;
		sizeDelta.y = num6 + padding.y * 2f;
		rectTransform2.sizeDelta = sizeDelta;
		foreach (PerkEntry item2 in activeEntries)
		{
			RectTransform rectTransform3 = item2.RectTransform;
			Vector2 anchoredPosition = rectTransform3.anchoredPosition + vector3;
			if (num5 == 0f)
			{
				anchoredPosition.x = (rectTransform2.rect.width - padding.x * 2f) / 2f;
			}
			else
			{
				float num7 = (rectTransform2.rect.width - padding.x * 2f) / num5;
				anchoredPosition.x *= num7;
			}
			anchoredPosition += padding;
			rectTransform3.anchoredPosition = anchoredPosition;
		}
		foreach (PerkLineEntry activeEntry in PerkLinePool.ActiveEntries)
		{
			RectTransform rectTransform4 = activeEntry.RectTransform;
			Vector2 layoutPosition2 = activeEntry.GetLayoutPosition();
			layoutPosition2.y *= -1f;
			Vector2 anchoredPosition2 = layoutPosition2 * layoutFactor;
			anchoredPosition2 += padding;
			anchoredPosition2.x = rectTransform4.anchoredPosition.x;
			rectTransform4.anchoredPosition = anchoredPosition2;
			rectTransform4.SetAsFirstSibling();
		}
		contentParent.anchoredPosition = Vector2.zero;
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	public PerkEntry GetSelection()
	{
		return selectedPerkEntry;
	}

	public bool SetSelection(PerkEntry selection)
	{
		selectedPerkEntry = selection;
		OnSelectionChanged();
		return true;
	}

	private void OnSelectionChanged()
	{
		this.onSelectionChanged?.Invoke(selectedPerkEntry);
		RefreshDetails();
	}

	private void RefreshDetails()
	{
		details.Setup(selectedPerkEntry?.Target, editable: true);
	}

	private void Show_Local(PerkTree target)
	{
		UnregisterEvents();
		SetSelection(null);
		this.target = target;
		title.text = target.DisplayName;
		ShowTask().Forget();
		RegisterEvents();
	}

	public static void Show(PerkTree target)
	{
		if (!(Instance == null))
		{
			Instance.Show_Local(target);
		}
	}

	private void RegisterEvents()
	{
		if (target != null)
		{
			target.onPerkTreeStatusChanged += Refresh;
		}
	}

	private void UnregisterEvents()
	{
		if (target != null)
		{
			target.onPerkTreeStatusChanged -= Refresh;
		}
	}

	private void Refresh(PerkTree tree)
	{
		RefreshConnections();
	}

	private async UniTask ShowTask()
	{
		if (target == null)
		{
			Close();
			return;
		}
		Open();
		await UniTask.WaitForEndOfFrame(this);
		await UniTask.WaitForEndOfFrame(this);
		await UniTask.WaitForEndOfFrame(this);
		PopulatePerks();
	}

	public void Hide()
	{
		Close();
	}

	protected override void Awake()
	{
		base.Awake();
	}
}
