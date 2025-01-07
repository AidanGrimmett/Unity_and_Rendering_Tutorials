using UnityEngine;

public class FractalHierarchy : MonoBehaviour
{
    [SerializeField, Range(1, 8)]
    int depth = 4;

    bool counterRotate = false;

    private void Start()
    {
        name = "Fractal " + depth;
        if (depth <= 1)
            return;

        FractalHierarchy childDown = null;
        if (transform.parent == null)
        {
            childDown = CreateChild(Vector3.down, Quaternion.Euler(-180f, 0, 0));
            childDown.counterRotate = true;
        }

        FractalHierarchy childUp = CreateChild(Vector3.up, Quaternion.identity);
        FractalHierarchy childRight = CreateChild(Vector3.right, Quaternion.Euler(0, 0, -90f));
        FractalHierarchy childLeft = CreateChild(Vector3.left, Quaternion.Euler(0, 0, 90f));
        FractalHierarchy childForward = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0, 0));
        FractalHierarchy childBack = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0, 0));

        childUp.transform.SetParent(transform, false);
        childRight.transform.SetParent(transform, false);
        childLeft.transform.SetParent(transform, false);
        childForward.transform.SetParent(transform, false);
        childBack.transform.SetParent(transform, false);
        if (childDown != null)
            childDown.transform.SetParent(transform, false);
    }

    private FractalHierarchy CreateChild(Vector3 direction, Quaternion rotation)
    {
        FractalHierarchy child = Instantiate(this);
        child.depth = depth - 1;
        child.transform.localPosition = 0.75f * direction;
        child.transform.localRotation = rotation;
        child.transform.localScale = 0.5f * Vector3.one;
        return child;
    }

    private void Update()
    {
        if (!counterRotate)
            transform.Rotate(0, 22.5f * Time.deltaTime, 0f);
        else
            transform.Rotate(0, -22.5f * Time.deltaTime, 0f);
    }
}
