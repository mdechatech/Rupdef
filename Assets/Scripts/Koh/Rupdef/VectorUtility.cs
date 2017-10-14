using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Koh.Rupdef
{
    public static class VectorUtility
    {
        [Pure]
        public static bool Approximately(this Vector2 v, Vector2 other)
        {
            return Mathf.Approximately(v.x, other.x) &&
                   Mathf.Approximately(v.y, other.y);
        }

        [Pure]
        public static bool Approximately(this Vector3 v, Vector3 other)
        {
            return Mathf.Approximately(v.x, other.x) &&
                   Mathf.Approximately(v.y, other.y) &&
                   Mathf.Approximately(v.z, other.z);
        }

        [Pure]
        public static Vector2 WithX(this Vector2 v, float x)
        {
            return new Vector2(x, v.y);
        }

        [Pure]
        public static Vector2 WithY(this Vector2 v, float y)
        {
            return new Vector2(v.x, y);
        }

        [Pure]
        public static Vector2 MultiplyByVector(this Vector2 v, Vector2 vector)
        {
            return new Vector2(
                v.x * vector.x,
                v.y * vector.y);
        }

        [Pure]
        public static Vector2 DivideByVector(this Vector2 v, Vector2 vector)
        {
            return new Vector2(
                v.x / vector.x,
                v.y / vector.y);
        }

        [Pure]
        public static Vector3 WithX(this Vector3 v, float x)
        {
            return new Vector3(x, v.y, v.z);
        }

        [Pure]
        public static Vector3 WithY(this Vector3 v, float y)
        {
            return new Vector3(v.x, y, v.z);
        }

        [Pure]
        public static Vector3 WithXy(this Vector3 v, float x, float y)
        {
            return new Vector3(x, y, v.z);
        }

        [Pure]
        public static Vector3 WithXy(this Vector3 v, Vector2 xy)
        {
            return new Vector3(xy.x, xy.y, v.z);
        }

        [Pure]
        public static Vector3 WithZ(this Vector3 v, float z)
        {
            return new Vector3(v.x, v.y, z);
        }

        [Pure]
        public static Vector3 MultiplyByVector(this Vector3 v, Vector3 vector)
        {
            return new Vector3(
                v.x * vector.x,
                v.y * vector.y,
                v.z * vector.z);
        }

        [Pure]
        public static Vector3 DivideByVector(this Vector3 v, Vector3 vector)
        {
            return new Vector3(
                v.x / vector.x,
                v.y / vector.y,
                v.z / vector.z);
        }
    }
}
