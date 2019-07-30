using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMovement : MonoBehaviour
{
    [HideInInspector] public bool moveSpace = false;
    [HideInInspector] public Vector3 startPosition;
    Vector3 target;
    [SerializeField] float speed = 0.75f;
    float offset = 70f;

    private void Start()
    {
        startPosition = transform.position;
        target = new Vector3(transform.position.x, transform.position.y - offset, transform.position.z);
    }

    void Update()
    {
        if (!moveSpace)
            return;
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed);
            moveSpace = (transform.position == target) ? false : true;
        }
    }
}
