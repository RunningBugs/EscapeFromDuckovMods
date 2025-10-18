using System;
using System.Collections.Generic;
using Duckov.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionBackupManager : MonoBehaviour
{
	private struct PlayerPositionBackupEntry
	{
		public int sceneID;

		public Vector3 position;
	}

	private List<PlayerPositionBackupEntry> backups;

	private CharacterMainControl mainCharacter;

	public float backupTimeSpace = 3f;

	public float minBackupDistance = 3f;

	private float backupTimer = 3f;

	public InteractableBase backupInteract;

	public int listSize = 20;

	private static event Action OnStartRecoverEvent;

	private void Awake()
	{
		backups = new List<PlayerPositionBackupEntry>();
		MultiSceneCore.OnSubSceneLoaded += OnSubSceneLoaded;
		OnStartRecoverEvent += OnStartRecover;
	}

	private void OnDestroy()
	{
		MultiSceneCore.OnSubSceneLoaded -= OnSubSceneLoaded;
		OnStartRecoverEvent -= OnStartRecover;
	}

	private void Update()
	{
		if (!LevelManager.LevelInited)
		{
			return;
		}
		if (!mainCharacter)
		{
			mainCharacter = CharacterMainControl.Main;
		}
		if ((bool)mainCharacter)
		{
			backupTimer -= Time.deltaTime;
			if (backupTimer < 0f && CheckCanBackup())
			{
				BackupCurrentPos();
			}
		}
	}

	private bool CheckCanBackup()
	{
		if (!mainCharacter)
		{
			return false;
		}
		if (!mainCharacter.IsOnGround)
		{
			return false;
		}
		if (Mathf.Abs(mainCharacter.Velocity.y) > 2f)
		{
			return false;
		}
		int count = backups.Count;
		if (count > 0)
		{
			Vector3 position = backups[count - 1].position;
			if (Vector3.Distance(mainCharacter.transform.position, position) < minBackupDistance)
			{
				return false;
			}
		}
		return true;
	}

	private void OnSubSceneLoaded(MultiSceneCore multiSceneCore, Scene scene)
	{
		backups.Clear();
		backupTimer = backupTimeSpace;
	}

	public void BackupCurrentPos()
	{
		if (LevelManager.LevelInited && (bool)mainCharacter)
		{
			backupTimer = backupTimeSpace;
			PlayerPositionBackupEntry item = new PlayerPositionBackupEntry
			{
				position = mainCharacter.transform.position,
				sceneID = SceneManager.GetActiveScene().buildIndex
			};
			backups.Add(item);
			if (backups.Count > listSize)
			{
				backups.RemoveAt(0);
			}
		}
	}

	public static void StartRecover()
	{
		PlayerPositionBackupManager.OnStartRecoverEvent?.Invoke();
	}

	private void OnStartRecover()
	{
		if (mainCharacter.CurrentAction != null && mainCharacter.CurrentAction.Running)
		{
			mainCharacter.CurrentAction.StopAction();
		}
		mainCharacter.Interact(backupInteract);
	}

	public void SetPlayerToBackupPos()
	{
		if (backups.Count > 0)
		{
			_ = SceneManager.GetActiveScene().buildIndex;
			Vector3 position = mainCharacter.transform.position;
			PlayerPositionBackupEntry playerPositionBackupEntry = backups[backups.Count - 1];
			backups.RemoveAt(backups.Count - 1);
			Vector3 position2 = playerPositionBackupEntry.position;
			if (Vector3.Distance(position, position2) > minBackupDistance)
			{
				mainCharacter.SetPosition(position2);
			}
			else
			{
				SetPlayerToBackupPos();
			}
		}
	}
}
