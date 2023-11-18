namespace CustomScriptMakerUtility
{
    public sealed class ScriptTemplate
    {
        public const string NAMESPACE_PH = "#namespace-name#";
        public const string TYPE_PH = "main-type";
        public const string TYPE_NAME_PH = "#type-name#";
        public const string TYPE_MODIFIER_PH = "#type-mod#";
        public static readonly string SCRIPT_TEMPLATE = 
            $"namespace {NAMESPACE_PH}\r\n{{\r\n    " +
            $"public{TYPE_MODIFIER_PH} {TYPE_PH} {TYPE_NAME_PH}\r\n    " +
            $"{{\r\n         \r\n    }}\r\n}}";

        public readonly string namespaceName;
        public readonly string type;
        public readonly string typeName;
        public readonly string typeModifier;

        public ScriptTemplate(string namespace_, string type, string typeName, string modifier) 
        {
            this.namespaceName = namespace_;
            this.type = type;
            this.typeName = typeName;
            this.typeModifier = modifier;
        }

        public string GenerateScriptContent()
        {
            string defNamespaces = "";
            foreach (string s in ScriptMaker.config.nsDefault)
                defNamespaces += NamespaceCodeFormat(s);
            defNamespaces += "\r\n";
            return defNamespaces + SCRIPT_TEMPLATE
                    .Replace(NAMESPACE_PH, namespaceName)
                    .Replace(TYPE_PH, type)
                    .Replace(TYPE_NAME_PH, typeName)
                    .Replace(TYPE_MODIFIER_PH, typeModifier);
        }

        public static string NamespaceCodeFormat(string namespaceName) 
            => $"using {namespaceName};\n";
    }
}