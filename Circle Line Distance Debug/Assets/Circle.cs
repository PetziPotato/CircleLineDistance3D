using UnityEngine;

public class Circle
{
    public Vector3 center;
    public Vector3 normal;
    public float radius;

    public Circle()
    {
        this.center = Vector3.zero;
        this.normal = new Vector3(0, 0, 1);
        this.radius = 1;
    }

    public Circle(Vector3 center, Vector3 normal, float radius)
    {
        this.center = center;
        this.normal = normal;
        this.radius = radius;
    }
}