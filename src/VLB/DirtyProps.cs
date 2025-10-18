using System;

namespace VLB;

[Flags]
public enum DirtyProps
{
	None = 0,
	Intensity = 2,
	HDRPExposureWeight = 4,
	ColorMode = 8,
	Color = 0x10,
	BlendingMode = 0x20,
	Cone = 0x40,
	SideSoftness = 0x80,
	Attenuation = 0x100,
	Dimensions = 0x200,
	RaymarchingQuality = 0x400,
	Jittering = 0x800,
	NoiseMode = 0x1000,
	NoiseIntensity = 0x2000,
	NoiseVelocityAndScale = 0x4000,
	CookieProps = 0x8000,
	ShadowProps = 0x10000,
	AllWithoutMaterialChange = 0x1E8D6,
	OnlyMaterialChangeOnly = 0x1728,
	All = 0x1FFFE
}
