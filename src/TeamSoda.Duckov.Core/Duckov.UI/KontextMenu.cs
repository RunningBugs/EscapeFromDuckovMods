using System.Collections.Generic;
using Duckov.UI.Animations;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.UI;

public class KontextMenu : MonoBehaviour
{
	private static KontextMenu instance;

	private RectTransform rectTransform;

	[SerializeField]
	private KontextMenuEntry entryPrefab;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private float positionMoveCloseThreshold = 10f;

	private object target;

	private bool isWatchingRectTransform;

	private RectTransform watchRectTransform;

	private Vector3 cachedTransformPosition;

	private PrefabPool<KontextMenuEntry> _entryPool;

	private Transform ContentRoot => base.transform;

	private PrefabPool<KontextMenuEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<KontextMenuEntry>(entryPrefab, ContentRoot);
			}
			return _entryPool;
		}
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		rectTransform = base.transform as RectTransform;
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		if ((bool)watchRectTransform)
		{
			if ((cachedTransformPosition - watchRectTransform.position).magnitude > positionMoveCloseThreshold)
			{
				Hide(null);
			}
		}
		else if (isWatchingRectTransform)
		{
			Hide(null);
		}
	}

	public void InstanceShow(object target, RectTransform targetRectTransform, params KontextMenuDataEntry[] entries)
	{
		this.target = target;
		watchRectTransform = targetRectTransform;
		isWatchingRectTransform = true;
		cachedTransformPosition = watchRectTransform.position;
		Vector3[] array = new Vector3[4];
		targetRectTransform.GetWorldCorners(array);
		float num = Mathf.Min(array[0].x, array[1].x, array[2].x, array[3].x);
		float num2 = Mathf.Max(array[0].x, array[1].x, array[2].x, array[3].x);
		float num3 = Mathf.Min(array[0].y, array[1].y, array[2].y, array[3].y);
		float num4 = Mathf.Max(array[0].y, array[1].y, array[2].y, array[3].y);
		float num5 = num;
		float num6 = (float)Screen.width - num2;
		float num7 = (float)Screen.height - num4;
		float x = ((num5 > num6) ? num : num2);
		float y = ((num3 > num7) ? num3 : num4);
		Vector2 vector = new Vector2(x, y);
		if (entries.Length < 1)
		{
			InstanceHide();
			return;
		}
		Vector2 vector2 = new Vector2(vector.x / (float)Screen.width, vector.y / (float)Screen.height);
		float x2 = ((!(vector2.x < 0.5f)) ? 1 : 0);
		float y2 = ((!(vector2.y < 0.5f)) ? 1 : 0);
		rectTransform.pivot = new Vector2(x2, y2);
		base.gameObject.SetActive(value: true);
		fadeGroup.SkipHide();
		Setup(entries);
		fadeGroup.Show();
		base.transform.position = vector;
	}

	public void InstanceShow(object target, Vector2 screenPoint, params KontextMenuDataEntry[] entries)
	{
		this.target = target;
		watchRectTransform = null;
		isWatchingRectTransform = false;
		if (entries.Length < 1)
		{
			InstanceHide();
			return;
		}
		Vector2 vector = new Vector2(screenPoint.x / (float)Screen.width, screenPoint.y / (float)Screen.height);
		float x = ((!(vector.x < 0.5f)) ? 1 : 0);
		float y = ((!(vector.y < 0.5f)) ? 1 : 0);
		rectTransform.pivot = new Vector2(x, y);
		base.gameObject.SetActive(value: true);
		fadeGroup.SkipHide();
		Setup(entries);
		fadeGroup.Show();
		base.transform.position = screenPoint;
	}

	private void Clear()
	{
		EntryPool.ReleaseAll();
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < ContentRoot.childCount; i++)
		{
			Transform child = ContentRoot.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				list.Add(child.gameObject);
			}
		}
		foreach (GameObject item in list)
		{
			Object.Destroy(item);
		}
	}

	private void Setup(IEnumerable<KontextMenuDataEntry> entries)
	{
		Clear();
		int num = 0;
		foreach (KontextMenuDataEntry entry in entries)
		{
			if (entry != null)
			{
				KontextMenuEntry kontextMenuEntry = EntryPool.Get(ContentRoot);
				num++;
				kontextMenuEntry.Setup(this, num, entry);
				kontextMenuEntry.transform.SetAsLastSibling();
			}
		}
	}

	public void InstanceHide()
	{
		target = null;
		watchRectTransform = null;
		fadeGroup.Hide();
	}

	public static void Show(object target, RectTransform watchRectTransform, params KontextMenuDataEntry[] entries)
	{
		if (!(instance == null))
		{
			instance.InstanceShow(target, watchRectTransform, entries);
		}
	}

	public static void Show(object target, Vector2 position, params KontextMenuDataEntry[] entries)
	{
		if (!(instance == null))
		{
			instance.InstanceShow(target, position, entries);
		}
	}

	public static void Hide(object target)
	{
		if (!(instance == null) && (target == null || target == instance.target) && !instance.fadeGroup.IsHidingInProgress)
		{
			instance.InstanceHide();
		}
	}
}
