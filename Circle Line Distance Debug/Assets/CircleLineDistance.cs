using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleLineDistance : MonoBehaviour
{
    public struct Result
    {
        public float distance, sqrDistance;
        public int numClosestPairs;
        public Vector3[] lineClosest, circleClosest;
        public bool equidistant;
    };


    public static Result Distance(Line line, Circle circle)
    {
        Vector3 D = line.point - circle.center;
        Vector3 MxN = Vector3.Cross(line.vector, circle.normal);
        Vector3 DxN = Vector3.Cross(D, circle.normal);
        Vector3 diff;
        Result result = new Result();
        result.lineClosest = new Vector3[2];
        result.circleClosest = new Vector3[2];

        float m0sqr = Vector3.Dot(MxN, MxN);

        if (m0sqr > 0)
        {
            // Compute the critical points s for F'(s) = 0.
            Vector3 P0, P1;
            float s, t;
            int numRoots = 0;
            float[] roots = new float[3];

            // The line direction M and the plane normal N are not parallel.  Move the line origin B = (b0,b1,b2) to B' = B + lambda*line.vector = (0,b1',b2').

            float m0 = (float)Math.Sqrt(m0sqr);
            float rm0 = circle.radius * m0;
            float lambda = -Vector3.Dot(MxN, DxN) / m0sqr;
            Vector3 oldD = D;
            D += lambda * line.vector;
            DxN += lambda * MxN;
            float m2b2 = Vector3.Dot(line.vector, D);
            float b1sqr = Vector3.Dot(DxN, DxN);

            if (b1sqr > 0)
            {
                // B' = (0,b1',b2') where b1' != 0.  See Sections 1.1.2 and 1.2.2
                // of the PDF documentation.

                float b1 = (float)Math.Sqrt(b1sqr);
                float rm0sqr = circle.radius * m0sqr;

                if (rm0sqr > b1)
                {
                    float twoThirds = 2 / 3;
                    float sHat = (float)(Math.Sqrt(Math.Pow(rm0sqr * b1sqr, twoThirds) - b1sqr) / m0);
                    float gHat = (float)(rm0sqr * sHat / Math.Sqrt(m0sqr * sHat * sHat + b1sqr));
                    float cutoff = gHat - sHat;
                    if (m2b2 <= -cutoff)
                    {
                        s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, -m2b2, -m2b2 + rm0);
                        roots[numRoots++] = s;
                        if (m2b2 == -cutoff)
                        {
                            roots[numRoots++] = -sHat;
                        }
                    }
                    else if (m2b2 >= cutoff)
                    {
                        s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, -m2b2 - rm0, -m2b2);
                        roots[numRoots++] = s;
                        if (m2b2 == cutoff)
                        {
                            roots[numRoots++] = sHat;
                        }
                    }
                    else
                    {
                        if (m2b2 <= 0)
                        {
                            s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, -m2b2, -m2b2 + rm0);
                            roots[numRoots++] = s;
                            s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, -m2b2 - rm0, -sHat);
                            roots[numRoots++] = s;
                        }
                        else
                        {
                            s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, -m2b2 - rm0, -m2b2);
                            roots[numRoots++] = s;
                            s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, sHat, -m2b2 + rm0);
                            roots[numRoots++] = s;
                        }
                    }
                }
                else
                {
                    if (m2b2 < 0)
                    {
                        s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, -m2b2,
                            -m2b2 + rm0);
                    }
                    else if (m2b2 > 0)
                    {
                        s = Bisect(m2b2, rm0sqr, m0sqr, b1sqr, -m2b2 - rm0,
                            -m2b2);
                    }
                    else
                    {
                        s = 0;
                    }
                    roots[numRoots++] = s;
                }
            }
            else
            {
                // The new line origin is B' = (0,0,b2').
                if (m2b2 < 0)
                {
                    s = -m2b2 + rm0;
                    roots[numRoots++] = s;
                }
                else if (m2b2 > 0)
                {
                    s = -m2b2 - rm0;
                    roots[numRoots++] = s;
                }
                else
                {
                    s = -m2b2 + rm0;
                    roots[numRoots++] = s;
                    s = -m2b2 - rm0;
                    roots[numRoots++] = s;
                }
            }

            List<ClosestInfo> candidates = new List<ClosestInfo>();

            for (int i = 0; i < numRoots; ++i)
            {
                t = roots[i] + lambda;
                ClosestInfo info = new ClosestInfo();
                Vector3 NxDelta = Vector3.Cross(circle.normal, oldD + t * line.vector);

                if (NxDelta != Vector3.zero)
                {
                    GetPair(line, circle, oldD, t, out info.lineClosest, out info.circleClosest);
                    info.equidistant = false;
                }
                else
                {
                    Vector3 U = GetOrthogonal(circle.normal, true);
                    info.lineClosest = circle.center;
                    info.circleClosest = circle.center + circle.radius * U;
                    info.equidistant = true;
                }
                diff = info.lineClosest - info.circleClosest;
                info.sqrDistance = Vector3.Dot(diff, diff);
                candidates.Add(info);
            }

            candidates.Sort();

            result.numClosestPairs = 1;
            result.lineClosest[0] = candidates[0].lineClosest;
            result.circleClosest[0] = candidates[0].circleClosest;
            if (numRoots > 1 && candidates[1].sqrDistance == candidates[0].sqrDistance)
            {
                result.numClosestPairs = 2;
                result.lineClosest[1] = candidates[1].lineClosest;
                result.circleClosest[1] = candidates[1].circleClosest;
            }

            result.equidistant = false;
        }
        else
        {
            // The line direction and the plane normal are parallel.
            if (DxN != Vector3.zero)
            {
                // The line is A+t*N but with A != C.
                result.numClosestPairs = 1;
                GetPair(line, circle, D, -Vector3.Dot(line.vector, D), out result.lineClosest[0], out result.circleClosest[0]);
                result.equidistant = false;
            }
            else
            {
                // The line is C+t*N, so C is the closest point for the line and
                // all circle points are equidistant from it.
                Vector3 U = GetOrthogonal(circle.normal, true);
                result.numClosestPairs = 1;
                result.lineClosest[0] = circle.center;
                result.circleClosest[0] = circle.center + circle.radius * U;
                result.equidistant = true;
            }
        }

        diff = result.lineClosest[0] - result.circleClosest[0];
        result.sqrDistance = Vector3.Dot(diff, diff);
        result.distance = (float)Math.Sqrt(result.sqrDistance);
        return result;
    }

    public static Vector3 GetOrthogonal(Vector3 v, bool unitLength)
    {
        float cmax = Math.Abs(v[0]);
        int imax = 0;

        for (int i = 1; i < 3; ++i)
        {
            float c = Math.Abs(v[i]);
            if (c > cmax)
            {
                cmax = c;
                imax = i;
            }
        }

        Vector3 result = Vector3.zero;
        int inext = imax + 1;

        if (inext == 3)
        {
            inext = 0;
        }

        result[imax] = v[inext];
        result[inext] = -v[imax];

        if (unitLength)
        {
            float sqrDistance = result[imax] * result[imax] + result[inext] * result[inext];
            float invLength = (float)(1 / Math.Sqrt(sqrDistance));
            result[imax] *= invLength;
            result[inext] *= invLength;
        }

        return result;
    }

    public static void GetPair(Line line, Circle circle, Vector3 D, float t, out Vector3 lineClosest, out Vector3 circleClosest)
    {
        Vector3 delta = D + t * line.vector;
        lineClosest = circle.center + delta;
        delta -= Vector3.Dot(circle.normal, delta) * circle.normal;
        delta.Normalize();
        circleClosest = circle.center + circle.radius * delta;
    }

    public static float Bisect(float m2b2, float rm0sqr, float m0sqr, float b1sqr, float smin, float smax)
    {
        Func<float, float> G = s => (float)(s + m2b2 - rm0sqr * s / Math.Sqrt(m0sqr * s * s + b1sqr));

        /*
         The function is known to be increasing, so we can specify - 1 and + 1 as the function values at the bounding interval endpoints.
         The use of 'double' is intentional in case float is a BSNumber or BSRational type.
         We want the bisections to terminate in a reasonable amount of time.
        */

        uint maxIterations = 500;
        float root;
        FindRoot(G, smin, smax, -1, 1, maxIterations, out root);
        return root;
    }


    public static int FindRoot(Func<float, float> F, float t0, float t1, float f0, float f1, uint maxIterations, out float root)
    {
        root = -999;

        if (t0 < t1)
        {
            // Test the endpoints to see whether F(t) is zero.
            if (f0 == 0)
            {
                root = t0;
                return 1;
            }

            if (f1 == 0)
            {
                root = t1;
                return 1;
            }

            if (f0 * f1 > 0)
            {
                // It is not known whether the interval bounds a root.
                return 0;
            }

            int i;
            root = t0;

            for (i = 2; i <= maxIterations; ++i)
            {
                root = 0.5f * (t0 + t1);

                if (root == t0 || root == t1)
                {
                    // The numbers t0 and t1 are consecutive floating-point numbers.
                    break;
                }

                float fm = F(root);
                float product = fm * f0;

                if (product < 0)
                {
                    t1 = root;
                    f1 = fm;
                }
                else if (product > 0)
                {
                    t0 = root;
                    t0 = fm;
                }
                else
                {
                    break;
                }
            }
            return i;
        }
        else
        {
            // The interval endpoints are invalid.
            return 0;
        }
    }

}
