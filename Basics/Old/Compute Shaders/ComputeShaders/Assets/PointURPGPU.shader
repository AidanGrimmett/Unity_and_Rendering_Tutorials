#include "PointGPU.hlsl"

struct Input
{
    float3 worldPos;
};

float _Smoothness;

void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
{
		surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
		surface.Smoothness = _Smoothness;
}