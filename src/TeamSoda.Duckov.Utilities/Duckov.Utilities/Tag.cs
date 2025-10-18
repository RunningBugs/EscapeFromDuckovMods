using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Utilities;

[CreateAssetMenu(menuName = "Items/Tag")]
public class Tag : ScriptableObject
{
	[SerializeField]
	private bool show;

	[SerializeField]
	private bool showDescription;

	[SerializeField]
	private Color color = Color.black;

	[SerializeField]
	private int priority;

	private int? _hash;

	[LocalizationKey("Tags")]
	private string displayNameKey
	{
		get
		{
			return "Tag_" + base.name;
		}
		set
		{
		}
	}

	[LocalizationKey("Tags")]
	private string descriptionKey
	{
		get
		{
			return "Tag_" + base.name + "_Desc";
		}
		set
		{
		}
	}

	public bool Show => show;

	public bool ShowDescription => showDescription;

	public int Priority => priority;

	public string DisplayName => displayNameKey.ToPlainText();

	public string Description => descriptionKey.ToPlainText();

	public int Hash
	{
		get
		{
			if (!_hash.HasValue)
			{
				_hash = GetHash(base.name);
			}
			return _hash.Value;
		}
	}

	public Color Color => color;

	public override string ToString()
	{
		return base.name;
	}

	private static int GetHash(string name)
	{
		return name.GetHashCode();
	}

	public static bool Match(Tag tag, string name)
	{
		if (tag == null)
		{
			return false;
		}
		return tag.Hash == GetHash(name);
	}
}
