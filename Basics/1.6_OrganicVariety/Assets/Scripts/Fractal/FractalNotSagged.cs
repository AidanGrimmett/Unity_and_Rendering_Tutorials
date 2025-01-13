using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

public class FractalNotSagged : MonoBehaviour
{
    //Defining a job requires a struct type which implements a job interface
    //the IJobFor interface requires an Execute(int) function with no return
    //this will replace the innermost loop of the update method
    //meaning, all variables we need in that execution need to be added as fields to the struct
    [BurstCompile(CompileSynchronously = true)] //tell unity to compile with burst
    struct UpdateFractalLevelJob : IJobFor
    {
        public float spinAngleDelta;
        public float scale;

        [ReadOnly] // helpful as it indicates that this will be constant during the job execution, alleviating race condition problems. Can safely be read in parallel.
        public NativeArray<FractalPart> parents;
        public NativeArray<FractalPart> parts;

        [WriteOnly]
        public NativeArray<float3x4> matrices;

        public void Execute(int i)
        {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];
            part.spinAngle += spinAngleDelta;


            part.worldRotation = mul(parent.worldRotation, mul(part.rotation, quaternion.RotateY(part.spinAngle))); //quaternion multiplication performs the second rotation, followed by the first rotation so order matters
            part.worldPosition = parent.worldPosition +
                mul(parent.worldRotation, 1.5f * scale * part.direction); //set position using scale and direction, applying parents rotation too.
            parts[i] = part;

            float3x3 r = float3x3(part.worldRotation) * scale; //to save data transfer to GPU we manually assemble a 3x4 matrix
            matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition); //starting with the 3x3 rotation then adding the world position
        }
    }

    static readonly int
        matricesId = Shader.PropertyToID("_Matrices"),
        colourAId = Shader.PropertyToID("_ColourA"),
        colourBId = Shader.PropertyToID("_ColourB"),
        sequenceNumbersId = Shader.PropertyToID("_SequenceNumbers");

    static MaterialPropertyBlock propertyBlock; //allows you to change material properties between different objects using the same material

    struct FractalPart
    {
        public float3 direction, worldPosition;
        public quaternion rotation, worldRotation;
        public float spinAngle; //used to maintain fresh quaternions for transformation matrices,
                                //floating point errors plague us otherwise.
    }

    [SerializeField, Range(3, 10)]
    int depth = 5;

    [SerializeField]
    Mesh mesh, leafMesh;

    [SerializeField]
    Material material;

    [SerializeField]
    Gradient gradientA, gradientB;

    [SerializeField]
    Color leafColourA, leafColourB;

    NativeArray<FractalPart>[] parts;

    NativeArray<float3x4>[] matrices; //usually, the transform component contains the necessary transformation matrices
                                      //as we are now using procedural drawing instead of GOs, we need to store these manually.

    static float3[] directions = { up(), right(), left(), forward(), back() };

    static quaternion[] rotations =
    {
        quaternion.identity,
        quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
        quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
    };

    ComputeBuffer[] matricesBuffers;
    Vector4[] sequenceNumbers; //used for storing random colour offset for each level

    FractalPart CreatePart(int childIndex) => new FractalPart
    {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };

    private void OnEnable()
    {
        //set up the parts array
        //parts is an array of FractalPart[] arrays, with `depth` elements (ie, (int)depth FractalPart[] arrays)
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<float3x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];
        sequenceNumbers = new Vector4[depth];

        int stride = 12 * 4; //3x4 matrix has 12 floats, so 16 * 4 stride length

        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }//each level of the parts array has a FractalPart[] which has 5 times the points as the last level

        parts[0][0] = CreatePart(0);

        //li = level index
        //fpi = fractal part index
        //ci = child index
        for (int li = 1; li < parts.Length; li++)
        {
            NativeArray<FractalPart> levelParts = parts[li]; //collection of parts on this level
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5) //iterate over all parts on level, increases by 5 as we have another loop which wil create 5 parts
            {
                for (int ci = 0; ci < 5; ci++) //create 5 parts, child index will help distinguish between directions
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }

        propertyBlock ??= new MaterialPropertyBlock(); // shorthand for: if propertyBlock is null, propertyBlock = new...
    }

    private void OnDisable()
    {
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
        sequenceNumbers = null;
    }

    private void OnValidate()
    {
        if (parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;

        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta; //changing local struct variable value will not change the array element
        rootPart.worldRotation = mul(transform.rotation, mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;
        float objectScale = transform.lossyScale.x;
        parts[0][0] = rootPart; //copy back to array to update
        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);
        float scale = objectScale;
        JobHandle jobHandle = default; //represents a handle to the job https://docs.unity3d.com/ScriptReference/Unity.Jobs.JobHandle.html
        for (int li = 1; li < parts.Length; li++) //traverse every level of the parts array (skipping 0, as the root obj never moves)
        {
            scale *= 0.5f;
            jobHandle = new UpdateFractalLevelJob // create the job and set all fields
            {
                spinAngleDelta = spinAngleDelta,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            }.ScheduleParallel(parts[li].Length, 5, jobHandle); //schedule and add to jobHandle dependencies
        }
        jobHandle.Complete(); //ensure all jobs are executed fully before proceeding.

        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one); //This is used by unity's culling system to decide whether the fractal needs to be drawn
                                                                                         //fractal will approach a limit less than 3 with infinite size, so this is a safe bounds
        int leafIndex = matricesBuffers.Length - 1;
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i]; //this level buffer reference (holding instance transformation matrices)
            buffer.SetData(matrices[i]); //updates compute buffer with new data for rendering and sends data to the GPU
            Mesh instanceMesh;
            Color colorA, colorB;
            if (i == leafIndex)
            {
                colorA = leafColourA;
                colorB = leafColourB;
                instanceMesh = leafMesh;
            }
            else
            {
                float gradientInterpolator = i / (matricesBuffers.Length - 1f); //normalised progress per level (ie, 8 depth fractal, 4 is 0.5f)
                colorA = gradientA.Evaluate(gradientInterpolator);
                colorB = gradientB.Evaluate(gradientInterpolator);
                instanceMesh = mesh;
            }
            propertyBlock.SetColor(colourAId, colorA);
            propertyBlock.SetColor(colourBId, colorB);

            propertyBlock.SetBuffer(matricesId, buffer); //populates the _Matrices property in the shader
            propertyBlock.SetVector(sequenceNumbersId, sequenceNumbers[i]);

            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, buffer.count, propertyBlock); //draw mesh instances procedurally, with correct property block
        }
    }
}
