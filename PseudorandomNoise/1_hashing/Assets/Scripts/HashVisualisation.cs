using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
public class HashVisualisation : MonoBehaviour
{
    static int
        hashesId = Shader.PropertyToID("_Hashes"), //shader identifier
        configId = Shader.PropertyToID("_Config");

    [SerializeField]
    Mesh instanceMesh;

    [SerializeField]
    Material material;

    [SerializeField, Range(1, 512)]
    int resolution = 16; //hash visualisation resolution

    NativeArray<uint> hashes; //uint essentially a packet of 32 bits to use for hash (unsigned int)
                              //native array bridges managed code (this) with native code/memory.
                              //is essentially a nice wrapper that allows you to access malloc with little overhead,
                              //but still provides some memory leak checks and other nice features

    ComputeBuffer hashesBuffer;

    MaterialPropertyBlock propertyBlock;

    RenderParams rParams;

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)] //compile with burst
    struct HashJob : IJobFor
    {
        [WriteOnly]
        public NativeArray<uint> hashes;

        public void Execute(int i)
        {
            hashes[i] = (uint)i;
        }
    }

    private void OnEnable()
    {
        int length = resolution * resolution; //calculate 2d resolution
        hashes = new NativeArray<uint>(length, Allocator.Persistent); //persistent allocation is slower, but can last indefinitely if needed. 
        hashesBuffer = new ComputeBuffer(length, 4); //compute buffer with (int)length spaces, each 4 bytes long

        new HashJob //create job
        {
            hashes = hashes
        }.ScheduleParallel(hashes.Length, resolution, default).Complete();

        hashesBuffer.SetData(hashes);
        propertyBlock ??= new MaterialPropertyBlock(); //if the property block is null, create a new propertyblock
                                                       //Property blocks allow you to set per-instance properties without modifying the material itself
        propertyBlock.SetBuffer(hashesId, hashesBuffer); //links the GPU-side hashesBuffer to the shader
        propertyBlock.SetVector(configId, new Vector4(resolution, 1f / resolution)); //assign new vector4 to the configId property

        rParams = new RenderParams();
        rParams.material = material;
        rParams.matProps = propertyBlock;
    }

    private void OnDisable()
    {
        hashes.Dispose();
        hashesBuffer.Release();
        hashesBuffer = null;
    }

    private void OnValidate()
    {
        if (hashesBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        Graphics.RenderMeshPrimitives(rParams, instanceMesh, 0, hashes.Length); //uncharted territories.
    }
}
