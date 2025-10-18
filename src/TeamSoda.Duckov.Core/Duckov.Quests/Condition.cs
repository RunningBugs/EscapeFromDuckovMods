using UnityEngine;

namespace Duckov.Quests;

public class Condition : MonoBehaviour
{
	public virtual string DisplayText => "";

	public virtual bool Evaluate()
	{
		return false;
	}
}
