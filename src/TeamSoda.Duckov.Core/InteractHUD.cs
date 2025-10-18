using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

public class InteractHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	public RectTransform master;

	private InteractableBase interactableMaster;

	private InteractableBase interactableMasterTemp;

	private List<InteractableBase> interactableGroup;

	private List<InteractSelectionHUD> selectionsHUD;

	private int interactableIndexTemp;

	private bool interactable;

	private Camera camera;

	public bool syncPosToTarget;

	public InteractSelectionHUD selectionPrefab;

	private int interactableHash = Shader.PropertyToID("Interactable");

	private PrefabPool<InteractSelectionHUD> _selectionsCache;

	private PrefabPool<InteractSelectionHUD> Selections
	{
		get
		{
			if (_selectionsCache == null)
			{
				_selectionsCache = new PrefabPool<InteractSelectionHUD>(selectionPrefab);
			}
			return _selectionsCache;
		}
	}

	private void Awake()
	{
		interactableGroup = new List<InteractableBase>();
		selectionsHUD = new List<InteractSelectionHUD>();
		selectionPrefab.gameObject.SetActive(value: false);
		master.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (characterMainControl == null)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if (characterMainControl == null)
			{
				return;
			}
		}
		if (camera == null)
		{
			camera = Camera.main;
			if (camera == null)
			{
				return;
			}
		}
		bool flag = false;
		bool flag2 = false;
		interactableMaster = characterMainControl.interactAction.MasterInteractableAround;
		bool flag3 = InputManager.InputActived && (!characterMainControl.CurrentAction || !characterMainControl.CurrentAction.Running);
		Shader.SetGlobalFloat(interactableHash, flag3 ? 1f : 0f);
		interactable = interactableMaster != null && flag3;
		if (interactable)
		{
			if (interactableMaster != interactableMasterTemp)
			{
				interactableMasterTemp = interactableMaster;
				flag = true;
				flag2 = true;
			}
			if (interactableIndexTemp != characterMainControl.interactAction.InteractIndexInGroup)
			{
				interactableIndexTemp = characterMainControl.interactAction.InteractIndexInGroup;
				flag2 = true;
			}
		}
		else
		{
			interactableMasterTemp = null;
		}
		if (interactable != master.gameObject.activeInHierarchy)
		{
			master.gameObject.SetActive(interactable);
		}
		if (flag)
		{
			RefreshContent();
			SyncPos();
		}
		if (flag2)
		{
			RefreshSelection();
		}
	}

	private void LateUpdate()
	{
		if (!(characterMainControl == null) && !(camera == null))
		{
			SyncPos();
			UpdateInteractLine();
		}
	}

	private void SyncPos()
	{
		if (syncPosToTarget && (bool)interactableMaster)
		{
			Vector3 position = interactableMaster.transform.TransformPoint(interactableMaster.interactMarkerOffset);
			Vector3 vector = LevelManager.Instance.GameCamera.renderCamera.WorldToScreenPoint(position);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform.parent as RectTransform, vector, null, out var localPoint);
			base.transform.localPosition = localPoint;
		}
	}

	private void RefreshContent()
	{
		if (interactableMaster == null)
		{
			return;
		}
		selectionsHUD.Clear();
		interactableGroup.Clear();
		foreach (InteractableBase interactable in interactableMaster.GetInteractableList())
		{
			if (interactable != null)
			{
				interactableGroup.Add(interactable);
			}
		}
		Selections.ReleaseAll();
		foreach (InteractableBase item in interactableGroup)
		{
			InteractSelectionHUD interactSelectionHUD = Selections.Get();
			interactSelectionHUD.transform.SetAsLastSibling();
			interactSelectionHUD.SetInteractable(item, interactableGroup.Count > 1);
			selectionsHUD.Add(interactSelectionHUD);
		}
		master.ForceUpdateRectTransforms();
	}

	private void RefreshSelection()
	{
		InteractableBase interactTarget = characterMainControl.interactAction.InteractTarget;
		foreach (InteractSelectionHUD item in selectionsHUD)
		{
			if (item.InteractTarget == interactTarget)
			{
				item.SetSelection(_select: true);
			}
			else
			{
				item.SetSelection(_select: false);
			}
		}
		master.ForceUpdateRectTransforms();
	}

	private void UpdateInteractLine()
	{
	}
}
