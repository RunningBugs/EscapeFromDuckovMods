using System.Collections.Generic;
using KINEMATION.KAnimationCore.Runtime.Rig;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace KINEMATION.MagicBlend.Runtime;

[HelpURL("https://kinemation.gitbook.io/magic-blend-documentation/")]
public class MagicBlending : MonoBehaviour
{
	public PlayableGraph playableGraph;

	[Tooltip("This asset controls the blending weights.")]
	[SerializeField]
	private MagicBlendAsset blendAsset;

	[Tooltip("Will update weights every frame.")]
	[SerializeField]
	private bool forceUpdateWeights = true;

	[Tooltip("Will process the Overlay pose. Keep it on most of the time.")]
	[SerializeField]
	private bool alwaysAnimatePoses = true;

	private const ushort PlayableSortingPriority = 900;

	private Animator _animator;

	private KRigComponent _rigComponent;

	private AnimationLayerMixerPlayable _playableMixer;

	private NativeArray<BlendStreamAtom> _atoms;

	private PoseJob _poseJob;

	private OverlayJob _overlayJob;

	private LayeringJob _layeringJob;

	private AnimationScriptPlayable _poseJobPlayable;

	private AnimationScriptPlayable _overlayJobPlayable;

	private AnimationScriptPlayable _layeringJobPlayable;

	private bool _isInitialized;

	private float _blendPlayback = 1f;

	private float _blendTime;

	private AnimationCurve _blendCurve;

	private MagicBlendAsset _desiredBlendAsset;

	private float _desiredBlendTime;

	private bool _useBlendCurve;

	private List<int> _blendedIndexes = new List<int>();

	private Dictionary<string, int> _hierarchyMap;

	private RuntimeAnimatorController _cachedController;

	private AnimationPlayableOutput _magicBlendOutput;

	public MagicBlendAsset BlendAsset => blendAsset;

	public void UpdateMagicBlendAsset(MagicBlendAsset newAsset, bool useBlending = false, float blendTime = -1f, bool useCurve = false)
	{
		if (newAsset == null)
		{
			Debug.LogWarning("MagicBlending: input asset is NULL!");
			return;
		}
		_desiredBlendAsset = newAsset;
		_useBlendCurve = useCurve;
		_desiredBlendTime = blendTime;
		if (!useBlending)
		{
			SetNewAsset();
			if (!alwaysAnimatePoses)
			{
				_poseJob.readPose = true;
				_overlayJob.cachePose = true;
				_poseJobPlayable.SetJobData(_poseJob);
				_overlayJobPlayable.SetJobData(_overlayJob);
			}
		}
		else
		{
			_layeringJob.cachePose = true;
			_layeringJobPlayable.SetJobData(_layeringJob);
		}
	}

	public float GetOverlayTime(bool isNormalized = true)
	{
		Playable input = _overlayJobPlayable.GetInput(0);
		if (!input.IsValid() || !blendAsset.isAnimation)
		{
			return 0f;
		}
		float num = (float)input.GetDuration();
		if (Mathf.Approximately(num, 0f))
		{
			return 0f;
		}
		float num2 = (float)input.GetTime();
		if (!isNormalized)
		{
			return num2;
		}
		return Mathf.Clamp01(num2 / num);
	}

	protected virtual void SetNewAsset()
	{
		blendAsset = _desiredBlendAsset;
		_blendCurve = (_useBlendCurve ? blendAsset.blendCurve : null);
		_blendTime = ((_desiredBlendTime > 0f) ? _desiredBlendTime : blendAsset.blendTime);
		MagicBlendLibrary.ConnectPose(_poseJobPlayable, playableGraph, blendAsset.basePose);
		MagicBlendLibrary.ConnectPose(_overlayJobPlayable, playableGraph, blendAsset.overlayPose);
		if (blendAsset.isAnimation)
		{
			_overlayJobPlayable.GetInput(0).SetSpeed(1.0);
		}
		for (int i = 0; i < _hierarchyMap.Count; i++)
		{
			BlendStreamAtom value = _atoms[i];
			value.baseWeight = (value.additiveWeight = (value.localWeight = 0f));
			_atoms[i] = value;
		}
		_blendedIndexes.Clear();
		foreach (LayeredBlend layeredBlend in blendAsset.layeredBlends)
		{
			foreach (KRigElement item in layeredBlend.layer.elementChain)
			{
				_hierarchyMap.TryGetValue(item.name, out var value2);
				_blendedIndexes.Add(value2);
			}
		}
		UpdateBlendWeights();
	}

	protected virtual void BuildMagicMixer()
	{
		if (_playableMixer.IsValid())
		{
			_playableMixer.DisconnectInput(2);
		}
		else
		{
			_playableMixer = AnimationLayerMixerPlayable.Create(playableGraph, 3);
			InitializeJobs();
			_playableMixer.ConnectInput(0, _poseJobPlayable, 0, 1f);
			_playableMixer.ConnectInput(1, _overlayJobPlayable, 0, 1f);
			_magicBlendOutput.SetSourcePlayable(_playableMixer);
			_magicBlendOutput.SetSortingOrder(900);
		}
		_magicBlendOutput.SetSourcePlayable(_playableMixer);
		_magicBlendOutput.SetSortingOrder(900);
		int outputCount = playableGraph.GetOutputCount();
		int index = 0;
		for (int i = 0; i < outputCount; i++)
		{
			if (!(playableGraph.GetOutput(i).GetSourcePlayable().GetPlayableType() != typeof(AnimatorControllerPlayable)))
			{
				index = i;
			}
		}
		Playable sourcePlayable = playableGraph.GetOutput(index).GetSourcePlayable();
		_layeringJobPlayable.ConnectInput(0, sourcePlayable, 0, 1f);
		_playableMixer.ConnectInput(2, _layeringJobPlayable, 0, 1f);
		if (blendAsset != null)
		{
			UpdateMagicBlendAsset(blendAsset);
		}
	}

	protected virtual void InitializeMagicBlending()
	{
		playableGraph = _animator.playableGraph;
		_atoms = MagicBlendLibrary.SetupBlendAtoms(_animator, _rigComponent);
		_magicBlendOutput = AnimationPlayableOutput.Create(playableGraph, "MagicBlendOutput", _animator);
		BuildMagicMixer();
		playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
		playableGraph.Play();
		_isInitialized = true;
	}

	private void InitializeJobs()
	{
		_poseJob = new PoseJob
		{
			atoms = _atoms,
			alwaysAnimate = alwaysAnimatePoses,
			readPose = false
		};
		_poseJobPlayable = AnimationScriptPlayable.Create(playableGraph, _poseJob, 1);
		_overlayJob = new OverlayJob
		{
			atoms = _atoms,
			alwaysAnimate = alwaysAnimatePoses,
			cachePose = false
		};
		_overlayJobPlayable = AnimationScriptPlayable.Create(playableGraph, _overlayJob, 1);
		_layeringJob = new LayeringJob
		{
			atoms = _atoms,
			blendWeight = 1f,
			cachePose = false
		};
		_layeringJobPlayable = AnimationScriptPlayable.Create(playableGraph, _layeringJob, 1);
	}

	private void OnEnable()
	{
		if (_isInitialized)
		{
			BuildMagicMixer();
		}
	}

	private void Start()
	{
		_animator = GetComponent<Animator>();
		_cachedController = _animator.runtimeAnimatorController;
		_rigComponent = GetComponentInChildren<KRigComponent>();
		_hierarchyMap = new Dictionary<string, int>();
		Transform[] rigTransforms = _rigComponent.GetRigTransforms();
		for (int i = 0; i < rigTransforms.Length; i++)
		{
			_hierarchyMap.Add(rigTransforms[i].name, i);
		}
		InitializeMagicBlending();
	}

	protected virtual void UpdateBlendWeights()
	{
		int num = 0;
		foreach (LayeredBlend layeredBlend in blendAsset.layeredBlends)
		{
			foreach (KRigElement item in layeredBlend.layer.elementChain)
			{
				_ = item;
				int index = _blendedIndexes[num] + 1;
				BlendStreamAtom value = _atoms[index];
				value.baseWeight = layeredBlend.baseWeight * blendAsset.globalWeight;
				value.additiveWeight = layeredBlend.additiveWeight * blendAsset.globalWeight;
				value.localWeight = layeredBlend.localWeight * blendAsset.globalWeight;
				_atoms[index] = value;
				num++;
			}
		}
	}

	protected virtual void Update()
	{
		RuntimeAnimatorController runtimeAnimatorController = _animator.runtimeAnimatorController;
		if (_cachedController != runtimeAnimatorController)
		{
			BuildMagicMixer();
		}
		_cachedController = runtimeAnimatorController;
		if (blendAsset == null)
		{
			return;
		}
		if (blendAsset.isAnimation)
		{
			Playable input = _overlayJobPlayable.GetInput(0);
			if (blendAsset.overlayPose.isLooping && input.GetTime() > (double)blendAsset.overlayPose.length)
			{
				input.SetTime(0.0);
			}
		}
		if (forceUpdateWeights)
		{
			UpdateBlendWeights();
		}
		if (!Mathf.Approximately(_blendPlayback, 1f))
		{
			_blendPlayback = Mathf.Clamp01(_blendPlayback + Time.deltaTime / _blendTime);
			_layeringJob.blendWeight = _blendCurve?.Evaluate(_blendPlayback) ?? _blendPlayback;
			_layeringJobPlayable.SetJobData(_layeringJob);
		}
	}

	protected virtual void LateUpdate()
	{
		if (!alwaysAnimatePoses && _poseJob.readPose)
		{
			_poseJob.readPose = false;
			_overlayJob.cachePose = false;
			_poseJobPlayable.SetJobData(_poseJob);
			_overlayJobPlayable.SetJobData(_overlayJob);
		}
		if (_layeringJob.cachePose)
		{
			SetNewAsset();
			_blendPlayback = 0f;
			_layeringJob.cachePose = false;
			_layeringJob.blendWeight = 0f;
			_layeringJobPlayable.SetJobData(_layeringJob);
			if (!alwaysAnimatePoses)
			{
				_poseJob.readPose = true;
				_overlayJob.cachePose = true;
				_poseJobPlayable.SetJobData(_poseJob);
				_overlayJobPlayable.SetJobData(_overlayJob);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (playableGraph.IsValid() && playableGraph.IsPlaying())
		{
			playableGraph.Stop();
			playableGraph.Stop();
		}
		if (_atoms.IsCreated)
		{
			_atoms.Dispose();
		}
	}
}
