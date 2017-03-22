using UnityEngine;

namespace MathUtils
{
    public static class RayUtils 
    {
        public static float DistanceToLine(this Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }

        public static bool IsPointForward(this Ray ray, Vector3 point)
        {
            return Vector3.Dot(ray.direction, point - ray.origin) > 0;
        }
    }
}
