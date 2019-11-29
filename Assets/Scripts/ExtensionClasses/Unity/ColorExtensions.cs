using UnityEngine;

namespace Assets.Scripts.ExtensionClasses
{
    public static class ColorExtensions
    {
        public static Color SetR(this Color self, float value)
        {
            return new Color(value, self.g, self.b, self.a);
        }

        public static Color SetG(this Color self, float value)
        {
            return new Color(self.r, value, self.b, self.a);
        }

        public static Color SetB(this Color self, float value)
        {
            return new Color(self.r, self.g, value, self.a);
        }

        public static Color SetA(this Color self, float value)
        {
            return new Color(self.r, self.g, self.b, value);
        }
    }
}
