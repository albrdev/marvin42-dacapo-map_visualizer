using UnityEngine;

namespace Assets.Scripts.ExtensionClasses
{
    public static class Vector3Extensions
    {
        public static Vector3 SetX(this Vector3 self, float value)
        {
            return new Vector3(value, self.y, self.z);
        }

        public static Vector3 SetY(this Vector3 self, float value)
        {
            return new Vector3(self.x, value, self.z);
        }

        public static Vector3 SetZ(this Vector3 self, float value)
        {
            return new Vector3(self.x, self.y, value);
        }

        public static Vector3 SwapXY(this Vector3 self)
        {
            return new Vector3(self.y, self.x, self.z);
        }

        public static Vector3 SwapYZ(this Vector3 self)
        {
            return new Vector3(self.x, self.z, self.y);
        }

        public static Vector3 SwapXZ(this Vector3 self)
        {
            return new Vector3(self.z, self.y, self.x);
        }
    }
}
