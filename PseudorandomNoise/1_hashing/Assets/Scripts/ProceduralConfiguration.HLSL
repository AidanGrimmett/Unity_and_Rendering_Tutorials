#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    StructuredBuffer<uint> _Hashes;
#endif

float4 _Config;

void ConfigureProcedural()
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        float v = floor(_Config.y * unity_InstanceID);
        float u = unity_InstanceID - _Config.x * v;
    #endif
}