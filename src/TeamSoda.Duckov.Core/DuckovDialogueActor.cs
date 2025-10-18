using System.Collections.Generic;
using NodeCanvas.DialogueTrees;
using UnityEngine;

public class DuckovDialogueActor : MonoBehaviour, IDialogueActor
{
	private static List<DuckovDialogueActor> _activeActors;

	[SerializeField]
	private string id;

	[SerializeField]
	private Sprite _portraitSprite;

	[SerializeField]
	[LocalizationKey("Default")]
	private string nameKey;

	[SerializeField]
	private Vector3 offset;

	private static List<DuckovDialogueActor> ActiveActors
	{
		get
		{
			if (_activeActors == null)
			{
				_activeActors = new List<DuckovDialogueActor>();
			}
			return _activeActors;
		}
	}

	public string ID => id;

	public Vector3 Offset => offset;

	public string NameKey => nameKey;

	public Texture2D portrait => null;

	public Sprite portraitSprite => _portraitSprite;

	public Color dialogueColor => default(Color);

	public Vector3 dialoguePosition => default(Vector3);

	public static void Register(DuckovDialogueActor actor)
	{
		if (ActiveActors.Contains(actor))
		{
			Debug.Log("Actor " + actor.nameKey + " 在重复注册", actor);
		}
		else
		{
			ActiveActors.Add(actor);
		}
	}

	public static void Unregister(DuckovDialogueActor actor)
	{
		ActiveActors.Remove(actor);
	}

	public static DuckovDialogueActor Get(string id)
	{
		return ActiveActors.Find((DuckovDialogueActor e) => e.ID == id);
	}

	private void OnEnable()
	{
		Register(this);
	}

	private void OnDisable()
	{
		Unregister(this);
	}

	string IDialogueActor.get_name()
	{
		return base.name;
	}

	Transform IDialogueActor.get_transform()
	{
		return base.transform;
	}
}
