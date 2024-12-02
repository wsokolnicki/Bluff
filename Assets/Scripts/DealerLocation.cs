using UnityEngine;

public class DealerLocation : MonoBehaviour
{
    [SerializeField] private float xpos = 0.9f;
    [SerializeField] private float ypos = 0.1f;

    void Start()
    {
        transform.position = Camera.main.ViewportToWorldPoint(new Vector3(xpos, ypos, 0));
    }
}
