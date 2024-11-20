using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalProcedural : MonoBehaviour
{

    struct FractalPart
    {
        public Vector3 direction, worldPos;
        public Quaternion rotation, worldRot;
        public float spinAngle;
    }

    FractalPart[][] parts;

    Matrix4x4[][] matrices;

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
        matrices = new Matrix4x4[depth][];
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new FractalPart[length];
            matrices[i] = new Matrix4x4[length];
        }
        parts[0][0] = CreatePart(0);
        for (int li = 1; li < parts.Length; li++)
        {
            FractalPart[] levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }
    }

    private void Update()
    {
        float spinAngleDelta = 22.5f * Time.deltaTime;

        FractalPart root = parts[0][0];
        root.spinAngle += spinAngleDelta;
        root.worldRot = root.rotation * Quaternion.Euler(0f, root.spinAngle, 0f);
        parts[0][0] = root;
        matrices[0][0] = Matrix4x4.TRS(root.worldPos, root.worldRot, Vector3.one);
        float scale = 1f;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            FractalPart[] parentParts = parts[li - 1];
            FractalPart[] levelParts = parts[li];
            Matrix4x4[] levelMatrices = matrices[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi++)
            {
                FractalPart parent = parentParts[fpi / 5];
                FractalPart part = levelParts[fpi];
                part.spinAngle += spinAngleDelta;
                part.worldRot = parent.worldRot * (part.rotation * Quaternion.Euler(0, spinAngleDelta, 0));
                part.worldPos = parent.worldPos + parent.worldRot * (1.5f * scale * part.direction);
                levelParts[fpi] = part;
                levelMatrices[fpi] = Matrix4x4.TRS(part.worldPos, part.worldRot, scale * Vector3.one);
            }
        }
    }

    FractalPart CreatePart(int childIndex) => new FractalPart
    {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };
        
}
