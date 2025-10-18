namespace VLB;

public enum RenderQueue
{
	Custom = 0,
	Background = 1000,
	Geometry = 2000,
	AlphaTest = 2450,
	GeometryLast = 2500,
	Transparent = 3000,
	Overlay = 4000
}
