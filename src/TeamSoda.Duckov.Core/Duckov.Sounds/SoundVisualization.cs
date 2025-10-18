using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.Sounds;

public class SoundVisualization : MonoBehaviour
{
	[SerializeField]
	private RectTransform layoutCenter;

	[SerializeField]
	private SoundDisplay displayTemplate;

	[SerializeField]
	private float retriggerDistanceThreshold = 1f;

	[SerializeField]
	private float displayOffset = 400f;

	private PrefabPool<SoundDisplay> _displayPool;

	private Queue<SoundDisplay> releaseBuffer = new Queue<SoundDisplay>();

	private PrefabPool<SoundDisplay> DisplayPool
	{
		get
		{
			if (_displayPool == null)
			{
				_displayPool = new PrefabPool<SoundDisplay>(displayTemplate);
			}
			return _displayPool;
		}
	}

	private void Awake()
	{
		AIMainBrain.OnPlayerHearSound += OnHeardSound;
		if (layoutCenter == null)
		{
			layoutCenter = base.transform as RectTransform;
		}
	}

	private void OnDestroy()
	{
		AIMainBrain.OnPlayerHearSound -= OnHeardSound;
	}

	private void Update()
	{
		foreach (SoundDisplay activeEntry in DisplayPool.ActiveEntries)
		{
			if (activeEntry.Value <= 0f)
			{
				releaseBuffer.Enqueue(activeEntry);
			}
			else
			{
				RefreshEntryPosition(activeEntry);
			}
		}
		while (releaseBuffer.Count > 0)
		{
			SoundDisplay soundDisplay = releaseBuffer.Dequeue();
			if (!(soundDisplay == null))
			{
				DisplayPool.Release(soundDisplay);
			}
		}
	}

	private void OnHeardSound(AISound sound)
	{
		Trigger(sound);
	}

	private void Trigger(AISound sound)
	{
		if (GameCamera.Instance == null)
		{
			return;
		}
		SoundDisplay soundDisplay = null;
		if (sound.fromCharacter != null)
		{
			foreach (SoundDisplay activeEntry in DisplayPool.ActiveEntries)
			{
				AISound currentSount = activeEntry.CurrentSount;
				if (!(currentSount.fromCharacter != sound.fromCharacter) && currentSount.soundType == sound.soundType && Vector3.Distance(currentSount.pos, sound.pos) < retriggerDistanceThreshold)
				{
					soundDisplay = activeEntry;
				}
			}
		}
		if (soundDisplay == null)
		{
			soundDisplay = DisplayPool.Get();
		}
		RefreshEntryPosition(soundDisplay);
		soundDisplay.Trigger(sound);
	}

	private void RefreshEntryPosition(SoundDisplay e)
	{
		Vector3 pos = e.CurrentSount.pos;
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GameCamera.Instance.renderCamera, pos);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(layoutCenter, screenPoint, null, out var localPoint);
		Vector2 normalized = localPoint.normalized;
		e.transform.localPosition = normalized * displayOffset;
		e.transform.rotation = Quaternion.FromToRotation(Vector2.up, normalized);
	}
}
