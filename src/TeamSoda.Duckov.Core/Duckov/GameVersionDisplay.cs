using TMPro;
using UnityEngine;

namespace Duckov;

public class GameVersionDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	private void Start()
	{
		text.text = $"v{GameMetaData.Instance.Version}";
	}
}
