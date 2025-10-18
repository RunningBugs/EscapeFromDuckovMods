using System;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace Duckov;

public class AudioObject : MonoBehaviour
{
	private Dictionary<string, float> parameters = new Dictionary<string, float>();

	private Dictionary<string, string> strParameters = new Dictionary<string, string>();

	private AudioManager.VoiceType voiceType;

	public List<EventInstance> events = new List<EventInstance>();

	private bool needCleanup;

	public AudioManager.VoiceType VoiceType
	{
		get
		{
			return voiceType;
		}
		set
		{
			voiceType = value;
		}
	}

	internal static AudioObject GetOrCreate(GameObject from)
	{
		AudioObject component = from.GetComponent<AudioObject>();
		if (component != null)
		{
			return component;
		}
		return from.AddComponent<AudioObject>();
	}

	public EventInstance? PostQuak(string soundKey)
	{
		string eventName = "Char/Voice/vo_" + voiceType.ToString().ToLower() + "_" + soundKey;
		return Post(eventName);
	}

	public EventInstance? Post(string eventName, bool doRelease = true)
	{
		if (!AudioManager.TryCreateEventInstance(eventName ?? "", out var eventInstance))
		{
			return null;
		}
		eventInstance.setCallback(EventCallback);
		events.Add(eventInstance);
		eventInstance.set3DAttributes(base.gameObject.transform.position.To3DAttributes());
		ApplyParameters(eventInstance);
		eventInstance.start();
		if (doRelease)
		{
			eventInstance.release();
		}
		return eventInstance;
	}

	public void Stop(string eventName, FMOD.Studio.STOP_MODE mode)
	{
		foreach (EventInstance @event in events)
		{
			if (@event.getDescription(out var description) == RESULT.OK && description.getPath(out var path) == RESULT.OK && !("event:/" + path != eventName))
			{
				@event.stop(mode);
				break;
			}
		}
	}

	private static RESULT EventCallback(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
	{
		if (type <= EVENT_CALLBACK_TYPE.PLUGIN_DESTROYED)
		{
			switch (type)
			{
			}
		}
		else if (type <= EVENT_CALLBACK_TYPE.SOUND_STOPPED)
		{
			switch (type)
			{
			}
		}
		else if (type <= EVENT_CALLBACK_TYPE.VIRTUAL_TO_REAL)
		{
			switch (type)
			{
			}
		}
		else if (type != EVENT_CALLBACK_TYPE.START_EVENT_COMMAND && type != EVENT_CALLBACK_TYPE.NESTED_TIMELINE_BEAT)
		{
			_ = -1;
		}
		return RESULT.OK;
	}

	private void FixedUpdate()
	{
		if (this == null || base.transform == null || events == null)
		{
			return;
		}
		foreach (EventInstance @event in events)
		{
			if (!@event.isValid())
			{
				needCleanup = true;
			}
			else
			{
				@event.set3DAttributes(base.transform.position.To3DAttributes());
			}
		}
		if (needCleanup)
		{
			events.RemoveAll((EventInstance e) => !e.isValid());
			needCleanup = false;
		}
	}

	internal void SetParameterByName(string parameter, float value)
	{
		parameters[parameter] = value;
		foreach (EventInstance @event in events)
		{
			if (!@event.isValid())
			{
				needCleanup = true;
			}
			else
			{
				@event.setParameterByName(parameter, value);
			}
		}
	}

	internal void SetParameterByNameWithLabel(string parameter, string label)
	{
		strParameters[parameter] = label;
		foreach (EventInstance @event in events)
		{
			if (!@event.isValid())
			{
				needCleanup = true;
			}
			else
			{
				@event.setParameterByNameWithLabel(parameter, label);
			}
		}
	}

	private void ApplyParameters(EventInstance eventInstance)
	{
		foreach (KeyValuePair<string, float> parameter in parameters)
		{
			eventInstance.setParameterByName(parameter.Key, parameter.Value);
		}
		foreach (KeyValuePair<string, string> strParameter in strParameters)
		{
			eventInstance.setParameterByNameWithLabel(strParameter.Key, strParameter.Value);
		}
	}

	internal void StopAll(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
	{
		foreach (EventInstance @event in events)
		{
			if (!@event.isValid())
			{
				needCleanup = true;
			}
			else
			{
				@event.stop(mode);
			}
		}
	}
}
