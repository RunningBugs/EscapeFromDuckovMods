using System;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[Serializable]
public struct TimeOfDayPhase
{
	[FormerlySerializedAs("phaseTag")]
	public TimePhaseTags timePhaseTag;

	public VolumeProfile volumeProfile;
}
