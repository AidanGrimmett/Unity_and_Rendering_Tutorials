using UnityEngine;

public class Fractal : MonoBehaviour
{

    struct FractalPart
    {
        public Vector3 direction;
        public Vector3 rotation;
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

    void CreatePart(int levelIndex, int childIndex, float scale)
    {
        GameObject part = new GameObject("Fractal Part L" + levelIndex + " C" + childIndex);
        part.transform.SetParent(transform);
        part.AddComponent<MeshFilter>().mesh = mesh;
        part.AddComponent<MeshRenderer>().material = material;
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
        CreatePart(0, 0, scale);
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
                    CreatePart(li, ci, scale);
                }
            }
        }
    }
}
