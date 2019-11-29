namespace UnityEngine
{
    public class ReadOnlyPropertyAttribute : PropertyAttribute
    {
        public string DisplayName { get; protected set; } = null;

        public ReadOnlyPropertyAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public ReadOnlyPropertyAttribute() { }
    }
}
