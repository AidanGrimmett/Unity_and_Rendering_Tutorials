using UnityEngine;

public class Fractal : MonoBehaviour
{
    static readonly int matricesId = Shader.PropertyToID("_Matrices");

    static MaterialPropertyBlock propertyBlock; //allows you to change material properties between different objects using the same material

    struct FractalPart
    {
        public Vector3 direction, worldPosition;
        public Quaternion rotation, worldRotation;
        public float spinAngle; //used to maintain fresh quaternions for transformation matrices,
                         //floating point errors plague us otherwise.
    }

    [SerializeField, Range(1, 9)]
    int depth = 4;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    FractalPart[][] parts;

    Matrix4x4[][] matrices; //usually, the transform component contains the necessary transformation matrices
                            //as we are now using procedural drawing instead of GOs, we need to store these manually.

    static Vector3[] directions = { Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

    static Quaternion[] rotations = 
    { 
        Quaternion.identity, 
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler (0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f) 
    };

    ComputeBuffer[] matricesBuffers;

    FractalPart CreatePart(int childIndex) => new FractalPart
    {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };

    private void OnEnable()
    {
        //set up the parts array
        //parts is an array of FractalPart[] arrays, with `depth` elements (ie, (int)depth FractalPart[] arrays)
        parts = new FractalPart[depth][];
        matrices = new Matrix4x4[depth][];
        matricesBuffers = new ComputeBuffer[depth];
        int stride = 16 * 4; //4x4 matrix has 16 floats, so 16 * 4 stride length

        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
            matrices[i] = new Matrix4x4[length];
            matricesBuffers[i] = new ComputeBuffer(length, stride);
        }//each level of the parts array has a FractalPart[] which has 5 times the points as the last level

        parts[0][0] = CreatePart(0);

        //li = level index
        //fpi = fractal part index
        //ci = child index
        for (int li = 1; li < parts.Length; li++)
        {
            FractalPart[] levelParts = parts[li]; //collection of parts on this level
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
        }
        parts = null;
        matrices = null;
        matricesBuffers = null;
    }

    private void OnValidate()
    {
        if (parts !=null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        float spinAngleDelta = 22.5f * Time.deltaTime;

        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta; //changing local struct variable value will not change the array element
        rootPart.worldRotation = rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f);
        parts[0][0] = rootPart; //copy back to array to update
        matrices[0][0] = Matrix4x4.TRS(rootPart.worldPosition, rootPart.worldRotation, Vector3.one);
        float scale = 1f;
        for (int li = 1; li < parts.Length; li++) //traverse every level of the parts array (skipping 0, as the root obj never moves)
        {
            scale *= 0.5f;
            //keep references to the current part and it's parent
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            Matrix4x4[] levelMatrices = matrices[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)// traverse every fractal part on the level
            {
                FractalPart parent = parentParts[fpi / 5];
                FractalPart part = levelParts[fpi];
                part.spinAngle += spinAngleDelta;
                part.worldRotation = parent.worldRotation * (part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f)); //Quaternion multiplication performs the second rotation, followed by the first rotation so order matters
                part.worldPosition = parent.worldPosition + 
                    parent.worldRotation * 
                    (1.5f * scale * part.direction); //set position using scale and direction, applying parents rotation too.
                levelParts[fpi] = part;

                levelMatrices[fpi] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, scale * Vector3.one);
            }
        }

        var bounds = new Bounds(Vector3.zero, 3f * Vector3.one); //This is used by unity's culling system to decide whether the fractal needs to be drawn
                                                                 //fractal will approach a limit less than 3 with infinite size, so this is a safe bounds
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i]; //this level buffer reference (holding instance transformation matrices)
            buffer.SetData(matrices[i]); //updates compute buffer with new data for rendering and sends data to the GPU
            propertyBlock.SetBuffer(matricesId, buffer); //populates the _Matrices property in the shader
            Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, propertyBlock); //draw mesh instances procedurally, with correct property block
        }
    }
}
