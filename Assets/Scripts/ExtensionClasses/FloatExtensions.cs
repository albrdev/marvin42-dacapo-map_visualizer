namespace Assets.Scripts.ExtensionClasses
{
    public static class FloatExtensions
    {
        public static float ToPercentage(this float self)
        {
            return self * 100f;
        }

        public static float FromPercentage(this float self)
        {
            return self / 100f;
        }
    }
}
