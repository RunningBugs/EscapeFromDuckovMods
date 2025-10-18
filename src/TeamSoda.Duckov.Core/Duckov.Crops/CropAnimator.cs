using System;
using UnityEngine;

namespace Duckov.Crops;

public class CropAnimator : MonoBehaviour
{
	[Serializable]
	private struct Stage
	{
		public float progress;

		public float position;

		public Stage(float progress, float position)
		{
			this.progress = progress;
			this.position = position;
		}
	}

	[SerializeField]
	private Crop crop;

	[SerializeField]
	private Transform displayParent;

	[SerializeField]
	private ParticleSystem plantFX;

	[SerializeField]
	private ParticleSystem stageChangeFX;

	[SerializeField]
	private ParticleSystem ripenFX;

	[SerializeField]
	private ParticleSystem waterFX;

	[SerializeField]
	private ParticleSystem harvestFX;

	[SerializeField]
	private ParticleSystem destroyFX;

	[SerializeField]
	private Stage[] stages = new Stage[3]
	{
		new Stage(0.333f, -0.4f),
		new Stage(0.666f, -0.2f),
		new Stage(0.999f, -0.1f)
	};

	private int? cachedStage;

	private ParticleSystem PlantFX => plantFX;

	private ParticleSystem StageChangeFX => stageChangeFX;

	private ParticleSystem RipenFX => ripenFX;

	private ParticleSystem WaterFX => waterFX;

	private ParticleSystem HarvestFX => harvestFX;

	private ParticleSystem DestroyFX => destroyFX;

	private void Awake()
	{
		if (crop == null)
		{
			crop = GetComponent<Crop>();
		}
		Crop obj = crop;
		obj.onPlant = (Action<Crop>)Delegate.Combine(obj.onPlant, new Action<Crop>(OnPlant));
		Crop obj2 = crop;
		obj2.onRipen = (Action<Crop>)Delegate.Combine(obj2.onRipen, new Action<Crop>(OnRipen));
		Crop obj3 = crop;
		obj3.onWater = (Action<Crop>)Delegate.Combine(obj3.onWater, new Action<Crop>(OnWater));
		Crop obj4 = crop;
		obj4.onHarvest = (Action<Crop>)Delegate.Combine(obj4.onHarvest, new Action<Crop>(OnHarvest));
		Crop obj5 = crop;
		obj5.onBeforeDestroy = (Action<Crop>)Delegate.Combine(obj5.onBeforeDestroy, new Action<Crop>(OnBeforeDestroy));
	}

	private void Update()
	{
		RefreshPosition();
	}

	private void RefreshPosition(bool notifyStageChange = true)
	{
		float progress = crop.Progress;
		Stage stage = default(Stage);
		int? num = cachedStage;
		for (int i = 0; i < stages.Length; i++)
		{
			Stage stage2 = stages[i];
			if (progress < stages[i].progress)
			{
				stage = stage2;
				cachedStage = i;
				break;
			}
		}
		displayParent.localPosition = Vector3.up * stage.position;
		if (notifyStageChange && num.HasValue && num.Value != cachedStage)
		{
			OnStageChange();
		}
	}

	private void OnStageChange()
	{
		FXPool.Play(StageChangeFX, base.transform.position, base.transform.rotation);
	}

	private void OnWater(Crop crop)
	{
		FXPool.Play(WaterFX, base.transform.position, base.transform.rotation);
	}

	private void OnRipen(Crop crop)
	{
		FXPool.Play(RipenFX, base.transform.position, base.transform.rotation);
	}

	private void OnHarvest(Crop crop)
	{
		FXPool.Play(HarvestFX, base.transform.position, base.transform.rotation);
	}

	private void OnPlant(Crop crop)
	{
		FXPool.Play(PlantFX, base.transform.position, base.transform.rotation);
	}

	private void OnBeforeDestroy(Crop crop)
	{
		FXPool.Play(DestroyFX, base.transform.position, base.transform.rotation);
	}
}
