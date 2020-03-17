namespace LoL
{
    public class DeserializableTypeAttribute : System.Attribute
    {
        // On compile, add class to generated static dictionary with { name, type }.
        public string Key { get; private set; }
        public DeserializableTypeAttribute() { }
        /// <summary>
        /// Add attribute with override key.
        /// Without the override key, key will be Type.Name.ToLower().
        /// </summary>
        /// <param name="key">Key.</param>
        public DeserializableTypeAttribute(string key) { Key = key; }
    }
}