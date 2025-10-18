using UnityEngine;

public class AISpecialAttachment_Shop : AISpecialAttachmentBase
{
	public GameObject shop;

	protected override void OnInited()
	{
		base.OnInited();
		aiCharacterController.hideIfFoundEnemy = shop;
	}
}
