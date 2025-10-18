using UnityEngine;

public class PlayerPositionBackupProxy : MonoBehaviour
{
	public void StartRecoverInteract()
	{
		PauseMenu.Instance.Close();
		PlayerPositionBackupManager.StartRecover();
	}
}
