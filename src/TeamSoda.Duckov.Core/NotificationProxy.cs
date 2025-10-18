using Duckov.UI;
using SodaCraft.Localizations;
using UnityEngine;

public class NotificationProxy : MonoBehaviour
{
	[LocalizationKey("Default")]
	public string notification;

	public void Notify()
	{
		NotificationText.Push(notification.ToPlainText());
	}
}
