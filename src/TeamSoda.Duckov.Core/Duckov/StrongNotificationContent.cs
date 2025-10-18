using UnityEngine;

namespace Duckov;

public class StrongNotificationContent
{
	public string mainText;

	public string subText;

	public Sprite image;

	public StrongNotificationContent(string mainText, string subText = "", Sprite image = null)
	{
		this.mainText = mainText;
		this.subText = subText;
		this.image = image;
	}
}
