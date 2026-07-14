using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Haiyu.Analyzers;

[Generator]
public class SettingsGenerator : IIncrementalGenerator
{
    private const string AttributeSource = """
        using System;

        namespace TavernAgent.Analyzers
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
            public class SettingsAttribute : Attribute
            {
                public string? Name { get; set; }
                public Type? Type { get; set; }
                public bool Nullable { get; set; }
                public string? DefaultValue { get; set; }
            }
        }
        """;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource("SettingsAttribute.g.cs", AttributeSource));

        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: TransformCandidate)
            .Where(x => x is not null)
            .Collect();

        context.RegisterSourceOutput(provider, GenerateSource);
    }

    private static Candidate? TransformCandidate(GeneratorSyntaxContext ctx, CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax) ctx.Node;
        var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl, ct) as INamedTypeSymbol;
        if (symbol is null) return null;

        var settingsAttrs = symbol.GetAttributes()
            .Where(a => a.AttributeClass?.Name is "SettingsAttribute" or "Settings")
            .ToList();

        if (settingsAttrs.Count == 0) return null;

        if (!InheritsFrom(symbol, "SettingBase")) return null;

        var entries = new List<SettingEntry>();
        foreach (var attr in settingsAttrs)
        {
            var name = GetNamedArg(attr, "Name") as string;
            var type = GetNamedArg(attr, "Type") as INamedTypeSymbol;
            if (name is null || type is null) continue;

            var typeName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var nullable = GetNamedArg(attr, "Nullable") as bool? ?? false;
            var defaultValue = GetNamedArg(attr, "DefaultValue") as string;

            var isString = type.SpecialType == SpecialType.System_String;
            var isInt32 = type.SpecialType == SpecialType.System_Int32;
            var isInt64 = type.SpecialType == SpecialType.System_Int64;
            var isSingle = type.SpecialType == SpecialType.System_Single;
            var isDouble = type.SpecialType == SpecialType.System_Double;
            var isBoolean = type.SpecialType == SpecialType.System_Boolean;
            var isObject = type.SpecialType == SpecialType.System_Object;
            var isDateTime = type.Name == "DateTime" && type.ContainingNamespace.ToDisplayString() == "System";
            var isGuid = type.Name == "Guid" && type.ContainingNamespace.ToDisplayString() == "System";

            entries.Add(new SettingEntry(
                name, typeName,
                nullable, defaultValue,
                isString, isInt32, isInt64, isSingle, isDouble, isBoolean, isDateTime, isGuid,isObject
            ));
        }

        if (entries.Count == 0) return null;

        return new Candidate(symbol.ContainingNamespace.ToDisplayString(), symbol.Name, entries);
    }

    private static object? GetNamedArg(AttributeData attr, string name)
    {
        var arg = attr.NamedArguments.FirstOrDefault(x => x.Key == name).Value;
        return arg.IsNull ? null : arg.Value;
    }

    private static bool InheritsFrom(INamedTypeSymbol type, string baseName)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.Name == baseName) return true;
            current = current.BaseType;
        }
        return false;
    }

    private static void GenerateSource(SourceProductionContext spc, ImmutableArray<Candidate?> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (candidate is null) continue;
            var source = GenerateClassSource(candidate);
            spc.AddSource($"{candidate.ClassName}.Settings.g.cs", source);
        }
    }

    private static string GenerateClassSource(Candidate candidate)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Text.Json;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine($"namespace {candidate.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"partial class {candidate.ClassName}");
        sb.AppendLine("{");

        foreach (var entry in candidate.Settings)
        {
            var returnType = entry.Nullable ? entry.TypeName + "?" : entry.TypeName;
            var paramType = entry.Nullable ? entry.TypeName + "?" : entry.TypeName;

            sb.AppendLine();
            sb.AppendLine($"    public async Task<{returnType}> Get{entry.Name}Async()");
            sb.AppendLine("    {");
            sb.Append(GetReaderSource(entry));
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public async Task Set{entry.Name}Async({paramType} value)");
            sb.AppendLine("    {");
            sb.Append(GetWriterSource(entry));
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GetReaderSource(SettingEntry entry)
    {
        var name = entry.Name;
        var fallback = GetFallbackExpression(entry);

        if (entry.IsString)
        {
            if (entry.HasDefaultValue)
                return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                     + $"        return val ?? \"{entry.DefaultValue}\";\n";

            if (entry.Nullable)
                return $"        return await Read(\"{name}\").ConfigureAwait(false);\n";

            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return val ?? string.Empty;\n";
        }

        if (entry.IsInt32)
            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return int.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsInt64)
            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return long.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsSingle)
            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return float.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsDouble)
            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return double.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsBoolean)
            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return bool.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsDateTime)
            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return DateTime.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsGuid)
            return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
                 + $"        return Guid.TryParse(val, out var r) ? r : {fallback};\n";

        return $"        var val = await Read(\"{name}\").ConfigureAwait(false);\n"
             + $"        if (string.IsNullOrEmpty(val)) return {fallback};\n"
             + $"        return JsonSerializer.Deserialize<{entry.TypeName}>(val);\n";
    }

    private static string GetFallbackExpression(SettingEntry entry)
    {
        if (entry.HasDefaultValue)
        {
            if (entry.IsInt32 || entry.IsInt64 || entry.IsDouble)
                return entry.DefaultValue!;
            if (entry.IsSingle)
                return entry.DefaultValue! + "f";

            if (entry.IsBoolean)
                return entry.DefaultValue!;

            if (entry.IsDateTime)
                return $"DateTime.Parse(\"{entry.DefaultValue}\")";

            if (entry.IsGuid)
                return $"Guid.Parse(\"{entry.DefaultValue}\")";

            return $"JsonSerializer.Deserialize<{entry.TypeName}>(\"{entry.DefaultValue}\")";
        }

        if (entry.Nullable)
            return entry.IsString ? "null" : "default";

        if (entry.IsSingle) return "0f";
        if (entry.IsDouble) return "0.0";
        if (entry.IsInt64) return "0L";
        if (entry.IsInt32) return "0";
        if (entry.IsBoolean) return "false";
        if (entry.IsDateTime) return "DateTime.MinValue";
        if (entry.IsGuid) return "Guid.Empty";

        return "default";
    }

    private static string GetWriterSource(SettingEntry entry)
    {
        var name = entry.Name;

        if (entry.IsString)
            return $"        await Write(value, \"{name}\").ConfigureAwait(false);\n";

        if (entry.Nullable)
        {
            if (entry.IsBoolean || entry.IsInt32 || entry.IsInt64 || entry.IsSingle || entry.IsDouble
                || entry.IsDateTime || entry.IsGuid)
                return $"        await Write(value?.ToString(), \"{name}\").ConfigureAwait(false);\n";

            return $"        var json = value is not null ? JsonSerializer.Serialize<{entry.TypeName}>(value) : null;\n"
                 + $"        await Write(json, \"{name}\").ConfigureAwait(false);\n";
        }

        if (entry.IsBoolean || entry.IsInt32 || entry.IsInt64 || entry.IsSingle || entry.IsDouble
            || entry.IsDateTime || entry.IsGuid)
            return $"        await Write(value.ToString(), \"{name}\").ConfigureAwait(false);\n";

        return $"        var json = JsonSerializer.Serialize<{entry.TypeName}>(value);\n"
             + $"        await Write(json, \"{name}\").ConfigureAwait(false);\n";
    }

    private sealed class SettingEntry
    {
        public string Name { get; }
        public string TypeName { get; }
        public bool Nullable { get; }
        public string? DefaultValue { get; }
        public bool HasDefaultValue => DefaultValue is not null;
        public bool IsString { get; }
        public bool IsInt32 { get; }
        public bool IsInt64 { get; }
        public bool IsSingle { get; }
        public bool IsDouble { get; }
        public bool IsBoolean { get; }
        public bool IsDateTime { get; }
        public bool IsGuid { get; }
        public bool IsObject { get; }
        public SettingEntry(string name, string typeName,
            bool nullable, string? defaultValue,
            bool isString, bool isInt32, bool isInt64, bool isSingle,
            bool isDouble, bool isBoolean, bool isDateTime, bool isGuid, bool isObject)
        {
            Name = name;
            TypeName = typeName;
            Nullable = nullable;
            DefaultValue = defaultValue;
            IsString = isString;
            IsInt32 = isInt32;
            IsInt64 = isInt64;
            IsSingle = isSingle;
            IsDouble = isDouble;
            IsBoolean = isBoolean;
            IsDateTime = isDateTime;
            IsGuid = isGuid;
            IsObject = isObject;
        }
    }

    private sealed class Candidate
    {
        public string Namespace { get; }
        public string ClassName { get; }
        public List<SettingEntry> Settings { get; }

        public Candidate(string ns, string className, List<SettingEntry> settings)
        {
            Namespace = ns;
            ClassName = className;
            Settings = settings;
        }
    }
}
