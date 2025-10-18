using System.Collections.Generic;
using UnityEngine;

public class OcclusionFadeManager : MonoBehaviour
{
	private static OcclusionFadeManager instance;

	public OcclusionFadeChecker aimOcclusionFadeChecker;

	public OcclusionFadeChecker characterOcclusionFadeChecker;

	private CharacterMainControl character;

	private Camera cam;

	public Dictionary<Material, Material> materialDic;

	public List<Shader> supportedShaders;

	public Shader maskedShader;

	public Material testMat;

	[Range(0f, 4f)]
	public float viewRange;

	[Range(0f, 8f)]
	public float viewFadeRange;

	public Texture2D fadeNoiseTexture;

	public float heightFadeRange;

	private int aimViewDirHash = Shader.PropertyToID("OC_AimViewDir");

	private int aimPosHash = Shader.PropertyToID("OC_AimPos");

	private int characterViewDirHash = Shader.PropertyToID("OC_CharacterViewDir");

	private int charactetrPosHash = Shader.PropertyToID("OC_CharacterPos");

	private int viewRangeHash = Shader.PropertyToID("ViewRange");

	private int viewFadeRangeHash = Shader.PropertyToID("ViewFadeRange");

	private int startFadeHeightHash = Shader.PropertyToID("StartFadeHeight");

	private int heightFadeRangeHash = Shader.PropertyToID("HeightFadeRange");

	public static OcclusionFadeManager Instance
	{
		get
		{
			if (!instance)
			{
				instance = Object.FindFirstObjectByType<OcclusionFadeManager>();
			}
			return instance;
		}
	}

	public float startFadeHeight
	{
		get
		{
			CharacterMainControl main = CharacterMainControl.Main;
			if (!main || !main.gameObject.activeInHierarchy)
			{
				return 0.25f;
			}
			return main.transform.position.y + 0.25f;
		}
	}

	private void Awake()
	{
		materialDic = new Dictionary<Material, Material>();
		aimOcclusionFadeChecker.gameObject.layer = LayerMask.NameToLayer("VisualOcclusion");
		characterOcclusionFadeChecker.gameObject.layer = LayerMask.NameToLayer("VisualOcclusion");
		SetShader();
		Shader.SetGlobalTexture("FadeNoiseTexture", fadeNoiseTexture);
	}

	private void OnValidate()
	{
		SetShader();
	}

	private void SetShader()
	{
		Shader.SetGlobalFloat(viewRangeHash, viewRange);
		Shader.SetGlobalFloat(viewFadeRangeHash, viewFadeRange);
		Shader.SetGlobalFloat(startFadeHeightHash, startFadeHeight);
		Shader.SetGlobalFloat(heightFadeRangeHash, heightFadeRange);
	}

	private void Update()
	{
		if (!character)
		{
			if ((bool)LevelManager.Instance)
			{
				character = LevelManager.Instance.MainCharacter;
				cam = LevelManager.Instance.GameCamera.renderCamera;
			}
			return;
		}
		aimOcclusionFadeChecker.transform.position = LevelManager.Instance.InputManager.InputAimPoint;
		Vector3 normalized = (aimOcclusionFadeChecker.transform.position - cam.transform.position).normalized;
		aimOcclusionFadeChecker.transform.rotation = Quaternion.LookRotation(-normalized);
		Shader.SetGlobalVector(aimViewDirHash, normalized);
		Shader.SetGlobalVector(aimPosHash, aimOcclusionFadeChecker.transform.position);
		characterOcclusionFadeChecker.transform.position = character.transform.position;
		Vector3 normalized2 = (characterOcclusionFadeChecker.transform.position - cam.transform.position).normalized;
		characterOcclusionFadeChecker.transform.rotation = Quaternion.LookRotation(-normalized2);
		Shader.SetGlobalVector(characterViewDirHash, normalized2);
		Shader.SetGlobalFloat(startFadeHeightHash, startFadeHeight);
		Shader.SetGlobalVector(charactetrPosHash, character.transform.position);
	}

	public Material GetMaskedMaterial(Material mat)
	{
		if (mat == null)
		{
			return null;
		}
		if (!supportedShaders.Contains(mat.shader))
		{
			return mat;
		}
		if (materialDic.ContainsKey(mat))
		{
			return materialDic[mat];
		}
		Material material = new Material(maskedShader);
		material.CopyPropertiesFromMaterial(mat);
		materialDic.Add(mat, material);
		return material;
	}
}
