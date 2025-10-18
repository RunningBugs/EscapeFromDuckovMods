using UnityEngine;

public class ShowLocationInMap : MonoBehaviour
{
	[SerializeField]
	private string displayName;

	public string DisplayName => displayName;

	public string DisplayNameRaw => displayName;
}
