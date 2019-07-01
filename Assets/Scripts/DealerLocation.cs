using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealerLocation : MonoBehaviour
{
    [SerializeField] float xpos = 0.9f;
    [SerializeField] float ypos = 0.1f;

    void Start()
    {
        transform.position = Camera.main.ViewportToWorldPoint
            (new Vector3(xpos, ypos, 0));
    }
}
