using System;
using UnityEngine;

public class ClosestInfo : IComparable
{
    public float sqrDistance;
    public Vector3 lineClosest, circleClosest;
    public bool equidistant;

    public int CompareTo(object obj)
    {
        return sqrDistance.CompareTo(((ClosestInfo) obj).sqrDistance);
    }
}
