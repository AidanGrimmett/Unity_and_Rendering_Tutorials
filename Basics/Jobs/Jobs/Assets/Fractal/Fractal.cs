using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{

    struct FractalPart
    {
        public Vector3 direction;
        public Quaternion rotation;
        public Transform transform;
    }

    FractalPart[][] parts;

    [SerializeField, Range(1, 8)]
    int depth = 4;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    static Vector3[] directions =
    {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };

    static Quaternion[] rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(0, 0, -90), Quaternion.Euler(0, 0, 90),
        Quaternion.Euler(90, 0, 0), Quaternion.Euler(-90, 0, 0)
    };

    private void Awake()
    {
        parts = new FractalPart[depth][];
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
        }
        float scale = 1f;
        parts[0][0] = CreatePart(0, 0, scale);
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(li, ci, scale);
                }
            }
        }
    }

    private void Update()
    {
        Quaternion deltaRotation = Quaternion.Euler(0, 22.5f * Time.deltaTime, 0);

        FractalPart root = parts[0][0];
        root.rotation *= deltaRotation;
        root.transform.localRotation = root.rotation;
        parts[0][0] = root;
        for (int li = 1; li < parts.Length; li++)
        {
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                Transform parentTransform = parentParts[fpi / 5].transform;
                FractalPart part = levelParts[fpi];
                part.rotation *= deltaRotation;
                part.transform.localRotation = parentTransform.localRotation * part.rotation;
                part.transform.localPosition = parentTransform.localPosition + parentTransform.localRotation * (1.5f * part.transform.localScale.x * part.direction);
                levelParts[fpi] = part;
            }
        }
    }

    FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {
        var go = new GameObject("Fractal Part L" + levelIndex + " C" + childIndex);
        go.transform.localScale = scale * Vector3.one;
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = material;
        return new FractalPart { direction = directions[childIndex], rotation = rotations[childIndex], transform = go.transform };
    }
        
}
