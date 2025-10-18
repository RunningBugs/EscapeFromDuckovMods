using System;

namespace VLB;

[Flags]
public enum BeamProps
{
	Transform = 1,
	Color = 2,
	BlendingMode = 4,
	Intensity = 8,
	SideSoftness = 0x10,
	SpotShape = 0x20,
	FallOffAttenuation = 0x40,
	Noise3D = 0x80,
	SDConeGeometry = 0x100,
	SDSoftIntersectBlendingDist = 0x200,
	Props2D = 0x400
}
