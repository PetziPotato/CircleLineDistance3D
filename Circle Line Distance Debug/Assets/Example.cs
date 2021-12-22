using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    public GameObject circleObj;
    public GameObject lineObj;
    public GameObject pointCircle1;
    public GameObject pointCircle2;
    public GameObject pointAverage;
    public GameObject pointLine1;
    public GameObject pointLine2;

    void Update()
    {
        Vector3 circleScale = circleObj.transform.localScale;
        circleObj.transform.localScale = new Vector3(circleScale.x, circleScale.y, circleScale.x);
        Circle circle = GetCircle();
        CircleLineDistance.Result result = CircleLineDistance.Distance(GetLine(), circle);
        pointCircle1.SetActive(false);
        pointCircle2.SetActive(false);
        pointAverage.SetActive(false);
        pointLine1.SetActive(false);
        pointLine2.SetActive(false);

        if (!result.equidistant)
        {
            if (result.lineClosest[0] != null)
                SetPoint(pointLine1, result.lineClosest[0]);

            if (result.lineClosest[1] != null)
                SetPoint(pointLine2, result.lineClosest[1]);

            if (result.circleClosest[0] != null)
                SetPoint(pointCircle1, result.circleClosest[0]);

            if (result.circleClosest[1] != null)
                SetPoint(pointCircle2, result.circleClosest[1]);

            if (result.circleClosest[0] != null && result.circleClosest[1] != null)
            {
                Vector3 middle = (result.circleClosest[0] + result.circleClosest[1]) / 2;
                Vector3 average = (circle.center - middle).normalized * circle.radius;
                SetPoint(pointAverage, average);
            }
        }
    }

    private void SetPoint(GameObject point, Vector3 position)
    {
        point.SetActive(true);
        point.transform.position = position;
    }

    private Circle GetCircle()
    {
        Vector3 center = circleObj.transform.position;
        Vector3 normal = circleObj.transform.rotation * Vector3.up;
        float radius = circleObj.transform.localScale.x/2;
        return new Circle(center, normal, radius);
    }

    private Line GetLine()
    {
        Vector3 point = lineObj.transform.position;
        Vector3 vector = lineObj.transform.rotation * Vector3.up;
        return new Line(point, vector);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Line line = GetLine();
        Gizmos.DrawLine(line.point - line.vector * 30, line.point + line.vector * 30);
        Circle circle = GetCircle();
        UnityEditor.Handles.DrawWireDisc(circle.center, circle.normal, circle.radius);
    }
}
