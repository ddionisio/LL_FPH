using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using LoL.Utils;
using UnityEditor;

namespace LoL
{
    public static partial class ScriptGenerator
    {
        const string GeneratedPath = "/Scripts/Generated/";

        [UnityEditor.Callbacks.DidReloadScripts(0)]
        public static void CreateDirectories()
        {
            System.IO.Directory.CreateDirectory($"{Application.dataPath}{GeneratedPath}");
        }

        [UnityEditor.Callbacks.DidReloadScripts(100)]
        public static void AddLoLGeneratedScriptDefinition()
        {
            AddScriptDefinition("LOL_GENERATED", BuildTargetGroup.WebGL, BuildTargetGroup.iOS, BuildTargetGroup.Android, BuildTargetGroup.Standalone);
            AssetDatabase.Refresh();
        }

        static void AddScriptDefinition(string definition, params BuildTargetGroup[] buildTargetGroups)
        {
            // Add if missing.
            foreach (var targetGroup in buildTargetGroups)
            {
                var currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
                if (!currentSymbols.Contains(definition))
                {
                    currentSymbols += $";{definition}";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, currentSymbols);
                }
            }
        }

        /// <summary>
        /// Generates the LoL Types reference file.
        /// This file is used to deserialize a class from json using a type name reference.
        /// Add the attribute, [DeserializableType] to class you would like added to the type dictionary.
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void CreateLoLTypesReferenceFile()
        {
            string typesFileName = "LoLTypes.cs";
            string typeReplaceKey = "<<<types>>>";
            string fileNamespace = "LoL";
            // Doesn't actually need the spacing but I put it in for ease of reading...
            List<string> typesTemplate = new List<string>
            {
                "using System;",
                "using System.Collections.Generic;",
                string.Empty,
                $"namespace {fileNamespace}",
                "{",
                "    // Generated from Attributes. Do not manual edit.",
                "    public static partial class Types",
                "    {",
                "        static Dictionary<string, Type> _Types = new Dictionary<string, Type>",
                "        {",
                typeReplaceKey,
                "        };",
                "    }",
                "}"
            };

            // Get all the classes that have EventData attribute.
            var types = GetTypesWithAttribute<DeserializableTypeAttribute>(typeof(DeserializableTypeAttribute).Assembly);
            types = types.Where(t => !t.type.IsGenericType).OrderBy(t => t.attribute.Key).ToArray();
            var typesString = new List<string>(types.Length);
            for (int i = 0; i < types.Length; ++i)
            {
                var type = types[i].type;
                // Remove the LoL. from the name, the Types class is already under the LoL namespace.
                // Replace any + ( internal classes ), with a .
                var typeName = type.FullName.Replace($"{fileNamespace}.", "").Replace('+', '.');
                var typeKey = (types[i].attribute.Key ?? type.Name).ToLower();
                typesString.Add($@"            {{ ""{typeKey}"", typeof({typeName}) }}{(i != types.Length - 1 ? "," : "")}");
            }

            // Replace with new entries of type.
            Replace(typesTemplate, typeReplaceKey, typesString);

            // Write all the lines to the file.
            System.IO.File.WriteAllLines(Application.dataPath + GeneratedPath + typesFileName, typesTemplate);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void CreateIStateSaveableFiles()
        {
            string projectKeyPath = $"{Application.dataPath}{GeneratedPath}ProjectKeys/";
            string statePath = $"{Application.dataPath}{GeneratedPath}State/";

            // Get all the classes that have State attribute.
            var types = GetTypesWithAttribute<StateAttribute>(typeof(DeserializableTypeAttribute).Assembly);
            types = types.Where(t => !t.type.IsGenericType).OrderBy(t => t.attribute.CreateOrder).ToArray();

            for (int i = 0; i < types.Length; ++i)
            {
                var type = types[i].type;

                string project_key = null;
                string fileNamespace = type.Namespace;
                string typeName = type.Name.Replace('+', '.');

                string stateFileName = $"{type.FullName.Replace(".", "-").ToLower()}.cs";
                string deserializeReplaceKey = "<<<deserialize>>>";
                string resetReplaceKey = "<<<reset>>>";
                string serializeReplaceKey = "<<<serialize>>>";
                string stateKey = typeName.ToLower();

                string projectKeyFilename = $"{fileNamespace.Replace(".", "-").ToLower()}.json";
                // Check if the global project key file exists for the namespace, if not, create one.
                if (System.IO.File.Exists(projectKeyPath + projectKeyFilename) && string.IsNullOrEmpty(project_key))
                {
                    project_key = SimpleJSON.JSON.Parse(System.IO.File.ReadAllText(projectKeyPath + projectKeyFilename))[nameof(project_key)];
                }
                else
                {
                    project_key = $"lol-{DateTimeExtensions.GetCurrentUnixTimestampMillis()}";
                    System.IO.Directory.CreateDirectory(projectKeyPath);
                    System.IO.File.WriteAllText(projectKeyPath + projectKeyFilename, new SimpleJSON.JSONObject { [nameof(project_key)] = project_key }.ToString(4));
                }

                // Doesn't actually need the spacing but I put it in for ease of reading...
                List<string> stateTemplate = new List<string>
                {
                    "using LoL;",
                    "using LoL.Data;",
                    "using LoL.SimpleJSON;",
                    string.Empty,
                    $"namespace {fileNamespace}",
                    "{",
                    "    // Generated from State Attribute. Do not manual edit.",
                    $"    public partial class {typeName} : IStateSaveable",
                    "    {",
                    "        void IDeserializable.Deserialize(JSONNode jsonNode)",
                    "        {",
                    deserializeReplaceKey,
                    "        }",
                    string.Empty,
                    "        void IResetable.Reset()",
                    "        {",
                    resetReplaceKey,
                    "        }",
                    string.Empty,
                    "        JSONNode ISerializable.Serialize()",
                    "        {",
                    "            return CreateJSONObject();",
                    "        }",
                    string.Empty,
                    "        JSONNode CreateJSONObject()",
                    "        {",
                    "            return new SimpleJSON.JSONObject()",
                    serializeReplaceKey,
                    "            ;",
                    "        }",
                    string.Empty,
                    "        public override string ToString()",
                    "        {",
                    $"            return \"{typeName}: \" + CreateJSONObject().ToString();",
                    "        }",
                    string.Empty,
                    $"        public static void Load(int version, System.Action<{typeName}> onLoad)",
                    "        {",
                    $"            State.Load(\"{project_key}-{stateKey}\", version, onLoad);",
                    "        }",
                    string.Empty,
                    $"        public static void Save(int version, {typeName} data)",
                    "        {",
                    $"            State.Save(\"{project_key}-{stateKey}\", version, data);",
                    "        }",
                    string.Empty,
                    "#if UNITY_EDITOR",
                    "        public static void Reset(int version)",
                    "        {",
                    $"            State.Reset(\"{project_key}-{stateKey}\", version);",
                    "        }",
                    "#endif",
                    "    }",
                    "}"
                };

                var fields = type.GetFields().Where(f => f.IsPublic || f.IsDefined(typeof(SerializeField)));
                var deserializeString = new List<string>(fields.Count());
                var resetString = new List<string>(fields.Count());
                var serializeString = new List<string>(fields.Count());
                var obj = Activator.CreateInstance(type);

                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(obj) ?? "null";
                    // Value type and string auto cast.
                    var deserializedValue = $"jsonNode[nameof({field.Name})]";
                    var serializedValue = $".Write(nameof({field.Name}), {field.Name})";
                    // a State field.
                    if (field.FieldType.IsDefined(typeof(StateAttribute), true) || field.FieldType is Data.IDeserializable)
                    {
                        fieldValue = "null";
                        var fieldTypeName = field.FieldType.FullName.Replace('+', '.');
                        deserializedValue = $"jsonNode[nameof({field.Name})].Deserialize<{fieldTypeName}>()";
                    }
                    // Enum field.
                    else if (field.FieldType.IsEnum)
                    {
                        fieldValue = "default";
                        var fieldTypeName = field.FieldType.FullName.Replace('+', '.');
                        deserializedValue = $"jsonNode[nameof({field.Name})].ToEnum<{fieldTypeName}>()";
                        serializedValue = $".Write(nameof({field.Name}), {field.Name}, true)";
                    }
                    // Bool value needs to be lower case.
                    else if (field.FieldType == typeof(bool))
                    {
                        fieldValue = fieldValue.ToString().ToLower();
                    }
                    // String needs to be wrapped in ""
                    else if (field.FieldType == typeof(string) || field.FieldType == typeof(char))
                    {
                        fieldValue = $@"""{fieldValue.ToString()}""";
                    }
                    // Arrays
                    else if (field.FieldType.IsArray)
                    {
                        fieldValue = "null";
                        var fieldTypeName = field.FieldType.FullName.Replace('+', '.');
                        var fullName = field.FieldType.FullName.Substring(0, field.FieldType.FullName.Length - 2);
                        var elementType = Type.GetType(string.Format("{0},{1}", fullName, field.FieldType.Assembly.GetName().Name));

                        if (elementType.IsDefined(typeof(StateAttribute), true) || elementType is Data.IDeserializable)
                        {
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToArray<{elementType}>()";
                        }
                        else if (elementType == typeof(string))
                        {
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToArrayString()";
                        }
                        else if (elementType.IsEnum)
                        {
                            var enumName = elementType.FullName.Replace('+', '.');
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToArrayEnum<{enumName}>()";
                        }
                        else if (elementType == typeof(int))
                        {
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToArrayInt32()";
                        }
                        else
                        {
                            Debug.LogError($"State Array field type in {obj.GetType()} is not supported: {field.Name} - {field.FieldType}");
                        }
                    }
                    // Lists
                    else if (field.FieldType.IsGenericType && (field.FieldType.GetGenericTypeDefinition() == typeof(List<>)))
                    {
                        var listType = field.FieldType.GetGenericArguments()[0];
                        var fieldTypeName = field.FieldType.FullName.Replace('+', '.');
                        var hasNullDefault = fieldValue.Equals("null");

                        if (listType.IsDefined(typeof(StateAttribute), true) || listType is Data.IDeserializable)
                        {
                            fieldValue = hasNullDefault ? "null" : $"new System.Collections.Generic.List<{listType}>()";
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToList<{listType}>({hasNullDefault.ToString().ToLower()})";
                        }
                        else
                        {
                            Debug.LogError($"State List field type in {obj.GetType()} is not supported: {field.Name} - {field.FieldType}");
                        }
                    }
                    // Dictionaries
                    else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        var keyType = field.FieldType.GetGenericArguments()[0];
                        var valueType = field.FieldType.GetGenericArguments()[1];
                        var hasNullDefault = fieldValue.Equals("null");
                        var fieldTypeName = field.FieldType.FullName.Replace('+', '.');
                        if (valueType.IsDefined(typeof(StateAttribute), true) || valueType is Data.IDeserializable)
                        {
                            if (keyType == typeof(string))
                            {
                                fieldValue = hasNullDefault ? "null" : $"new System.Collections.Generic.Dictionary<string, {valueType}>()";
                                deserializedValue = $"jsonNode[nameof({field.Name})].ToDictionaryString<{valueType}>({hasNullDefault.ToString().ToLower()})";
                            }
                            else if (keyType == typeof(int))
                            {
                                fieldValue = hasNullDefault ? "null" : $"new System.Collections.Generic.Dictionary<int, {valueType}>()";
                                deserializedValue = $"jsonNode[nameof({field.Name})].ToDictionaryInt<{valueType}>({hasNullDefault.ToString().ToLower()})";
                            }
                        }
                        // Refactor all this to be more generic.
                        else if (valueType == typeof(int) && keyType == typeof(string))
                        {
                            fieldValue = hasNullDefault ? "null" : "new System.Collections.Generic.Dictionary<string, int>()";
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToDictionaryStringInt({hasNullDefault.ToString().ToLower()})";
                        }
                        else if (valueType == typeof(string) && keyType == typeof(int))
                        {
                            fieldValue = hasNullDefault ? "null" : "new System.Collections.Generic.Dictionary<int, string>()";
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToDictionaryIntString({hasNullDefault.ToString().ToLower()})";
                        }
                        else if (valueType == typeof(string) && keyType == typeof(string))
                        {
                            fieldValue = hasNullDefault ? "null" : "new System.Collections.Generic.Dictionary<string, string>()";
                            deserializedValue = $"jsonNode[nameof({field.Name})].ToDictionaryStringString({hasNullDefault.ToString().ToLower()})";
                        }
                        else
                        {
                            Debug.LogError($"State Dictionary field type in {obj.GetType()} is not supported: {field.Name} - {field.FieldType}");
                        }
                    }

                    deserializeString.Add($@"             {field.Name} = {deserializedValue};");
                    resetString.Add($@"             {field.Name} = {fieldValue};");
                    serializeString.Add($@"                {serializedValue}");
                }

                //// Replace with new entries of type.
                Replace(stateTemplate, deserializeReplaceKey, deserializeString);
                Replace(stateTemplate, resetReplaceKey, resetString);
                Replace(stateTemplate, serializeReplaceKey, serializeString);

                // Write all the lines to the file.
                System.IO.Directory.CreateDirectory(statePath);
                System.IO.File.WriteAllLines(statePath + stateFileName, stateTemplate);
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void AddLoLGameframeScriptDefinition()
        {
            // Check if the lol ubergame jslib exists, if so, add the environment var.
            if (System.IO.File.Exists(Application.dataPath + "/Plugins/LoLUbergame/LoLUbergame.jslib"))
            {
                AddScriptDefinition("LOL_GAMEFRAME", BuildTargetGroup.WebGL, BuildTargetGroup.iOS, BuildTargetGroup.Android, BuildTargetGroup.Standalone);
            }
        }

        static void Replace(List<string> template, string replaceMatch, List<string> replaceWith)
        {
            int replaceIndex = template.IndexOf(replaceMatch);
            template.RemoveAt(replaceIndex);
            template.InsertRange(replaceIndex, replaceWith);
        }

        static void Replace(List<string> template, string replaceMatch, string replaceWith)
        {
            int replaceIndex = template.IndexOf(replaceMatch);
            template.RemoveAt(replaceIndex);
            template.Insert(replaceIndex, replaceWith);
        }

        static (Type type, T attribute)[] GetTypesWithAttribute<T>(Assembly assembly) where T : Attribute
        {
            return
                // Partition on the type list initially.
                (from t in assembly.GetTypes().AsParallel()
                 let attributes = t.GetCustomAttributes(typeof(T), true)
                 where attributes != null && attributes.Length > 0
                 select (t, (T)attributes[0])).ToArray();
        }
    }
}