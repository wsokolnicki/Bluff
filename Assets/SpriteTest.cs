using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTest : MonoBehaviour
{
    public GameObject arrow;
    public GameObject checkMark;
    List<GameObject> points;

    private void Start()
    {
        points = new List<GameObject>();    
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && points.Count <2 )
        {
            Vector2 pointPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var point = Instantiate(checkMark, pointPosition, Quaternion.identity);
            points.Add(point);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (points.Count != 2)
                return;

            Vector2 vectorBetweenTwoPoints = (points[1].transform.position - points[0].transform.position).normalized;
            var magnitude = (points[1].transform.position - points[0].transform.position).magnitude;
            var angle = Vector2.Angle(transform.up, vectorBetweenTwoPoints);
            int minusOrPlus = (points[1].transform.position.x > points[0].transform.position.x) ? -1 : 1;

            var arrowGO = Instantiate(arrow, points[0].transform.position, Quaternion.identity);

            arrowGO.transform.eulerAngles = new Vector3(0, 0, minusOrPlus * angle);
            arrowGO.transform.localScale = new Vector3(1, magnitude/7, 1);
        }
    }
}
