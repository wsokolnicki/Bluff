using UnityEngine;

public class SpaceMovement : MonoBehaviour
{
    [HideInInspector] public bool MoveSpace = false;
    [HideInInspector] public Vector3 StartPosition = Vector3.zero;
    private Vector3 target = Vector3.zero;
    [SerializeField] private float speed = 1.5f;
    private float offset = 70f;

    private void Start()
    {
        StartPosition = transform.position;
        target = new Vector3(transform.position.x, transform.position.y - offset, transform.position.z);
    }

    void Update()
    {
        if (!MoveSpace)
        {
            return;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed);
            MoveSpace = (transform.position == target) ? false : true;
        }
    }
}
