using UnityEngine;

public class CustomFaceData : ScriptableObject
{
	public string prefabsPath = "Assets/CustomFace/CustomFacePrefabs";

	public string info;

	[SerializeField]
	private CustomFacePartCollection hairs;

	[SerializeField]
	private CustomFacePartCollection eyes;

	[SerializeField]
	private CustomFacePartCollection mouths;

	[SerializeField]
	private CustomFacePartCollection eyebrows;

	[SerializeField]
	private CustomFacePartCollection decorations;

	[SerializeField]
	private CustomFacePartCollection tails;

	[SerializeField]
	private CustomFacePartCollection foots;

	[SerializeField]
	private CustomFacePartCollection wings;

	[SerializeField]
	private CustomFacePreset defaultPreset;

	public CustomFacePartCollection Hairs => hairs;

	public CustomFacePartCollection Eyes => eyes;

	public CustomFacePartCollection Mouths => mouths;

	public CustomFacePartCollection Eyebrows => eyebrows;

	public CustomFacePartCollection Decorations => decorations;

	public CustomFacePartCollection Tails => tails;

	public CustomFacePartCollection Foots => foots;

	public CustomFacePartCollection Wings => wings;

	public CustomFacePreset DefaultPreset => defaultPreset;
}
