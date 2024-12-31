using UnityEngine;

public class Fractal : MonoBehaviour
{

    struct FractalPart
    {
        public Vector3 direction;
        public Quaternion rotation;
        public Transform transform;
    }

    [SerializeField, Range(1, 8)]
    int depth = 4;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    FractalPart[][] parts;

    static Vector3[] directions = { Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };

    static Quaternion[] rotations = 
    { 
        Quaternion.identity, 
        Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler (0f, 0f, 90f),
        Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f) 
    };

    FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {
        GameObject part = new GameObject("Fractal Part L" + levelIndex + " C" + childIndex);
        part.transform.localScale = scale * Vector3.one;
        part.transform.SetParent(transform);
        part.AddComponent<MeshFilter>().mesh = mesh;
        part.AddComponent<MeshRenderer>().material = material;

        return new FractalPart
        {
            direction = directions[childIndex],
            rotation = rotations[childIndex],
            transform = part.transform
        };
    }

    private void Awake()
    {
        //set up the parts array
        //parts is an array of FractalPart[] arrays, with `depth` elements (ie, (int)depth FractalPart[] arrays)
        parts = new FractalPart[depth][];

        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
        }//each level of the parts array has a FractalPart[] which has 5 times the points as the last level

        float scale = 1f;
        parts[0][0] = CreatePart(0, 0, scale);
        //li = level index
        //fpi = fractal part index
        //ci = child index
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f; //reduce scale by half each level
            FractalPart[] levelParts = parts[li]; //collection of parts on this level
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5) //iterate over all parts on level, increases by 5 as we have another loop which wil create 5 parts
            {
                for (int ci = 0; ci < 5; ci++) //create 5 parts, child index will help distinguish between directions
                {
                    levelParts[fpi + ci] = CreatePart(li, ci, scale);
                }
            }
        }
    }

    private void Update()
    {
        Quaternion deltaRotation = Quaternion.Euler(0f, 22.5f * Time.deltaTime, 0f);

        FractalPart rootPart = parts[0][0];
        rootPart.rotation *= deltaRotation; //changing local struct variable value will not change the array element
        rootPart.transform.localRotation = rootPart.rotation;
        parts[0][0] = rootPart; //copy back to array

        for (int li = 1; li < parts.Length; li++) //traverse every level of the parts array (skipping 0, as the root obj never moves)
        {
            //keep references to the current part and it's parent
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)// traverse every fractal part on the level
            {
                Transform parentTransform = parentParts[fpi / 5].transform; // get the parent part's transform
                FractalPart part = levelParts[fpi];
                part.rotation *= deltaRotation;
                part.transform.localRotation = parentTransform.localRotation * part.rotation; //Quaternion multiplication performs the second rotation, followed by the first rotation so order matters
                part.transform.localPosition = parentTransform.position + 
                    parentTransform.localRotation * 
                    (1.5f * part.transform.localScale.x * part.direction); //set position using scale and direction, applying parents rotation too.
                levelParts[fpi] = part;
            }
        }
    }
}
