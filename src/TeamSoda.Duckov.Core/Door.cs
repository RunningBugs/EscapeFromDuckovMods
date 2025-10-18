using System;
using System.Collections.Generic;
using Duckov;
using Duckov.Scenes;
using Pathfinding;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
	[Serializable]
	public struct DoorTransformInfo
	{
		public Transform target;

		public Vector3 localPosition;

		public quaternion localRotation;

		public bool activation;
	}

	private bool closed = true;

	private float closedLerpValue;

	private float targetLerpValue;

	[SerializeField]
	private float lerpTime = 0.5f;

	[SerializeField]
	private List<Transform> doorParts;

	[SerializeField]
	private List<DoorTransformInfo> closeTransforms;

	[SerializeField]
	private List<DoorTransformInfo> openTransforms;

	[SerializeField]
	private DoorTrigger doorTrigger;

	[SerializeField]
	private Collider doorCollider;

	[SerializeField]
	private List<NavmeshCut> navmeshCuts = new List<NavmeshCut>();

	[SerializeField]
	private bool activeNavmeshCutWhenDoorIsOpen = true;

	[SerializeField]
	private bool ignoreInLevelData;

	private int _doorClosedDataKeyCached = -1;

	[SerializeField]
	private InteractableBase interact;

	public bool hasSound;

	public string openSound = "SFX/Actions/door_normal_open";

	public string closeSound = "SFX/Actions/door_normal_close";

	public UnityEvent OnOpenEvent;

	public UnityEvent OnCloseEvent;

	public bool IsOpen => !closed;

	public bool NoRequireItem
	{
		get
		{
			if (!interact)
			{
				return true;
			}
			return !interact.requireItem;
		}
	}

	public InteractableBase Interact => interact;

	private void Start()
	{
		if (_doorClosedDataKeyCached == -1)
		{
			_doorClosedDataKeyCached = GetKey();
		}
		if (!ignoreInLevelData && (bool)MultiSceneCore.Instance && MultiSceneCore.Instance.inLevelData.TryGetValue(_doorClosedDataKeyCached, out var value) && value is bool flag)
		{
			Debug.Log($"存在门存档信息：{flag}");
			closed = flag;
		}
		targetLerpValue = (closedLerpValue = (closed ? 1f : 0f));
		SyncNavmeshCut();
		SetPartsByLerpValue(setActivation: true);
	}

	private void OnEnable()
	{
		if ((bool)doorCollider)
		{
			doorCollider.isTrigger = true;
		}
	}

	private void OnDisable()
	{
		if ((bool)doorCollider)
		{
			doorCollider.isTrigger = false;
		}
	}

	private void SyncNavmeshCut()
	{
		bool flag = false;
		if (closed)
		{
			if (NoRequireItem)
			{
				flag = false;
			}
			else
			{
				flag = true;
			}
			return;
		}
		flag = activeNavmeshCutWhenDoorIsOpen;
		foreach (NavmeshCut navmeshCut in navmeshCuts)
		{
			if ((bool)(UnityEngine.Object)(object)navmeshCut)
			{
				((Behaviour)(object)navmeshCut).enabled = flag;
			}
		}
	}

	private void Update()
	{
		targetLerpValue = (closed ? 1f : 0f);
		if (targetLerpValue == closedLerpValue)
		{
			base.enabled = false;
		}
		closedLerpValue = Mathf.MoveTowards(closedLerpValue, targetLerpValue, Time.deltaTime / lerpTime);
		SetPartsByLerpValue(targetLerpValue == closedLerpValue);
	}

	public void Switch()
	{
		SetClosed(!closed);
	}

	public void Open()
	{
		SetClosed(_closed: false);
	}

	public void Close()
	{
		SetClosed(_closed: true);
	}

	public void ForceSetClosed(bool _closed, bool triggerEvent)
	{
		SetClosed(_closed, triggerEvent);
	}

	private void SetClosed(bool _closed, bool triggerEvent = true)
	{
		if (!LevelManager.LevelInited)
		{
			Debug.LogError("在关卡没有初始化时，不能对门进行设置");
			return;
		}
		if (triggerEvent)
		{
			if (_closed)
			{
				OnCloseEvent?.Invoke();
			}
			else
			{
				OnOpenEvent?.Invoke();
			}
		}
		Debug.Log($"Set Door Closed:{_closed}");
		if (_doorClosedDataKeyCached == -1)
		{
			_doorClosedDataKeyCached = GetKey();
		}
		closed = _closed;
		targetLerpValue = (closed ? 1f : 0f);
		if (closedLerpValue != targetLerpValue)
		{
			base.enabled = true;
		}
		if (hasSound)
		{
			AudioManager.Post(_closed ? closeSound : openSound, base.gameObject);
		}
		if ((bool)MultiSceneCore.Instance)
		{
			MultiSceneCore.Instance.inLevelData[_doorClosedDataKeyCached] = closed;
		}
		else
		{
			Debug.Log("没有MultiScene Core，无法存储data");
		}
		SyncNavmeshCut();
	}

	private List<DoorTransformInfo> GetCurrentTransformInfos()
	{
		List<DoorTransformInfo> list = new List<DoorTransformInfo>();
		foreach (Transform doorPart in doorParts)
		{
			DoorTransformInfo item = default(DoorTransformInfo);
			if (doorPart != null)
			{
				item.target = doorPart;
				item.localPosition = doorPart.localPosition;
				item.localRotation = doorPart.localRotation;
				item.activation = doorPart.gameObject.activeSelf;
			}
			list.Add(item);
		}
		return list;
	}

	public void SetParts(List<DoorTransformInfo> transforms)
	{
		for (int i = 0; i < transforms.Count; i++)
		{
			DoorTransformInfo doorTransformInfo = transforms[i];
			if (!(doorTransformInfo.target == null))
			{
				doorTransformInfo.target.localPosition = doorTransformInfo.localPosition;
				doorTransformInfo.target.localRotation = doorTransformInfo.localRotation;
				doorTransformInfo.target.gameObject.SetActive(doorTransformInfo.activation);
			}
		}
	}

	private void SetPartsByLerpValue(bool setActivation)
	{
		if (doorParts.Count != closeTransforms.Count || doorParts.Count != openTransforms.Count)
		{
			return;
		}
		for (int i = 0; i < openTransforms.Count; i++)
		{
			DoorTransformInfo doorTransformInfo = openTransforms[i];
			DoorTransformInfo doorTransformInfo2 = closeTransforms[i];
			if (doorTransformInfo.target == null || doorTransformInfo.target != doorTransformInfo2.target)
			{
				continue;
			}
			doorTransformInfo.target.localPosition = Vector3.Lerp(doorTransformInfo.localPosition, doorTransformInfo2.localPosition, closedLerpValue);
			doorTransformInfo.target.localRotation = Quaternion.Lerp(doorTransformInfo.localRotation, doorTransformInfo2.localRotation, closedLerpValue);
			if (setActivation)
			{
				if (closedLerpValue >= 1f)
				{
					doorTransformInfo.target.gameObject.SetActive(doorTransformInfo2.activation);
				}
				else
				{
					doorTransformInfo.target.gameObject.SetActive(doorTransformInfo.activation);
				}
			}
		}
	}

	private int GetKey()
	{
		Vector3 vector = base.transform.position * 10f;
		int x = Mathf.RoundToInt(vector.x);
		int y = Mathf.RoundToInt(vector.y);
		int z = Mathf.RoundToInt(vector.z);
		Vector3Int vector3Int = new Vector3Int(x, y, z);
		return $"Door_{vector3Int}".GetHashCode();
	}
}
