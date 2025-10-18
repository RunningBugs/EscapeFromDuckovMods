using UnityEngine;

namespace Duckov.Rules;

[CreateAssetMenu(menuName = "Duckov/Ruleset")]
public class RulesetFile : ScriptableObject
{
	[SerializeField]
	private Ruleset data;

	public Ruleset Data => data;
}
