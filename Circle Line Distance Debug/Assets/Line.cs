using UnityEngine;
using System.Collections;

public class Line {

    public Vector3 point;
    public Vector3 vector;

	public Line(Vector3 point, Vector3 vector)
    {
        this.point = point;
        vector.Normalize();
        this.vector = vector;
    }

    public Vector3 Intersection(Line line)
    {
        // https://math.stackexchange.com/questions/270767/find-intersection-of-two-3d-lines/271366

        Vector3 pointsVector = line.point - this.point;
        Vector3 cross1 = Vector3.Cross(line.vector, pointsVector);
        Vector3 cross2 = Vector3.Cross(line.vector, this.vector);

        //if (cross1.magnitude == 0 || cross2.magnitude == 0)
            //Debug.Log("No intersection!");

        int orientation;

        if (Vector3.Normalize(cross1) == Vector3.Normalize(cross2))
            orientation = 1;
        else
            orientation = -1;

        return this.point + orientation * ((cross1.magnitude / cross2.magnitude) * this.vector);
    }
}
