using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testOBB : MonoBehaviour
{
 public Transform A;
 public Transform B;

 public int samples = 8;
 public Vector3 prevPos;
 public Quaternion prevRot;

 void Start()
 {
     prevPos = A.position;
     prevRot = A.rotation;
 }

 void FixedUpdate()
 {
     Vector3 curPos = A.position;
     float distanceMoved = Vector3.Distance(curPos, prevPos);
     Vector3 directionMoved = (curPos - prevPos).normalized;

     Quaternion curRot = A.rotation;
     float rotationChange = Quaternion.Dot(prevRot, curRot);

      if (Intersects(ToObb(A), ToObb(B)) == true)
      {
          A.position = prevPos;
      }
      else if (distanceMoved > 1 || prevRot.eulerAngles != curRot.eulerAngles)
      {
          bool gotHit = false;
          for (int i = 0; i < samples; i++)
          {
                A.position = prevPos + (directionMoved * (distanceMoved / samples) * i);
                A.rotation = Quaternion.Lerp(prevRot, curRot, (1.0f /samples) * i);

                if (Intersects(ToObb(A), ToObb(B)) == true)
                {
                    gotHit = true;
                    break;
                }
          }

          if (gotHit == false) { A.position = curPos; A.rotation = curRot; }
      }

      prevPos = A.position;
      prevRot = A.rotation;
 }

 static Obb ToObb(Transform t)
 {
      return new Obb(t.position, t.localScale, t.rotation);
 }

 class Obb
 {
      public readonly Vector3[] Vertices;
      public readonly Vector3 Right;
      public readonly Vector3 Up;
      public readonly Vector3 Forward;
      public readonly Vector3 Center;

      public Obb(Vector3 center, Vector3 size, Quaternion rotation)
      {
           var max = size / 2;
           var min = -max;

           Vertices = new[]
           {
                center + rotation * min,
                center + rotation * new Vector3(max.x, min.y, min.z),
                center + rotation * new Vector3(min.x, max.y, min.z),
                center + rotation * new Vector3(max.x, max.y, min.z),
                center + rotation * new Vector3(min.x, min.y, max.z),
                center + rotation * new Vector3(max.x, min.y, max.z),
                center + rotation * new Vector3(min.x, max.y, max.z),
                center + rotation * max,
           };

           Right = rotation * Vector3.right;
           Up = rotation * Vector3.up;
           Forward = rotation * Vector3.forward;
           Center = center;
      }
 }

 static bool Intersects(Obb a, Obb b)
 {
      if (Separated(a.Vertices, b.Vertices, a.Right, a))
           return false;
      if (Separated(a.Vertices, b.Vertices, a.Up, a))
           return false;
      if (Separated(a.Vertices, b.Vertices, a.Forward, a))
           return false;

      if (Separated(a.Vertices, b.Vertices, b.Right, a))
           return false;
      if (Separated(a.Vertices, b.Vertices, b.Up, a))
           return false;
      if (Separated(a.Vertices, b.Vertices, b.Forward, a))
           return false;

      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Right, b.Right), a))
           return false;
      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Right, b.Up), a))
           return false;
      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Right, b.Forward), a))
           return false;

      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Up, b.Right), a))
           return false;
      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Up, b.Up), a))
           return false;
      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Up, b.Forward), a))
           return false;

      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Forward, b.Right), a))
           return false;
      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Forward, b.Up), a))
           return false;
      if (Separated(a.Vertices, b.Vertices, Vector3.Cross(a.Forward, b.Forward), a))
           return false;

      return true;
 }

 static bool Separated(Vector3[] vertsA, Vector3[] vertsB, Vector3 axis, Obb obb)
 {
      // Handles the cross product = {0,0,0} case
      if (axis == Vector3.zero)
           return false;

      var aMin = float.MaxValue;
      var aMax = float.MinValue;
      var bMin = float.MaxValue;
      var bMax = float.MinValue;

      // Define two intervals, a and b. Calculate their min and max values
      for (var i = 0; i < 8; i++)
      {
           var aDist = Vector3.Dot(vertsA[i], axis);
           aMin = aDist < aMin ? aDist : aMin;
           aMax = aDist > aMax ? aDist : aMax;
           var bDist = Vector3.Dot(vertsB[i], axis);
           bMin = bDist < bMin ? bDist : bMin;
           bMax = bDist > bMax ? bDist : bMax;
      }

      // One-dimensional intersection test between a and b
      var longSpan = Mathf.Max(aMax, bMax) - Mathf.Min(aMin, bMin);
      var sumSpan = aMax - aMin + bMax - bMin;

      return longSpan >= sumSpan; // > to treat touching as intersection
 }
}
