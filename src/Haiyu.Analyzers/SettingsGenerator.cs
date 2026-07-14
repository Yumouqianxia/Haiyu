using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Haiyu.Analyzers;

[Generator]
public class SettingsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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
        var classDecl = (ClassDeclarationSyntax)ctx.Node;
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
            if (string.IsNullOrWhiteSpace(name)) continue;

            var typeSymbol = ResolveTypeSymbol(attr);
            if (typeSymbol is null) continue;

            var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var nullable = GetNamedArg(attr, "Nullable") as bool? ?? false;
            var defaultValue = GetNamedArg(attr, "DefaultValue") as string;
            var jsonTypeInfoContextType = GetNamedArg(attr, "JsonTypeInfoContextType") as INamedTypeSymbol;
            var jsonTypeInfoPropertyName = GetNamedArg(attr, "JsonTypeInfoPropertyName") as string;
            var hasJsonTypeInfo = jsonTypeInfoContextType is not null
                && !string.IsNullOrWhiteSpace(jsonTypeInfoPropertyName);

            var isString = typeSymbol.SpecialType == SpecialType.System_String;
            var isInt32 = typeSymbol.SpecialType == SpecialType.System_Int32;
            var isInt64 = typeSymbol.SpecialType == SpecialType.System_Int64;
            var isSingle = typeSymbol.SpecialType == SpecialType.System_Single;
            var isDouble = typeSymbol.SpecialType == SpecialType.System_Double;
            var isBoolean = typeSymbol.SpecialType == SpecialType.System_Boolean;
            var isDateTime = typeSymbol.Name == "DateTime" && typeSymbol.ContainingNamespace.ToDisplayString() == "System";
            var isGuid = typeSymbol.Name == "Guid" && typeSymbol.ContainingNamespace.ToDisplayString() == "System";

            var isComplex = !isString && !isInt32 && !isInt64 && !isSingle
                && !isDouble && !isBoolean && !isDateTime && !isGuid;

            entries.Add(new SettingEntry(
                name, typeName, nullable, defaultValue,
                isString, isInt32, isInt64, isSingle, isDouble,
                isBoolean, isDateTime, isGuid, isComplex, hasJsonTypeInfo,
                jsonTypeInfoContextType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                jsonTypeInfoPropertyName));
        }

        if (entries.Count == 0) return null;

        return new Candidate(symbol.ContainingNamespace.ToDisplayString(), symbol.Name, entries);
    }

    private static INamedTypeSymbol? ResolveTypeSymbol(AttributeData attr)
    {
        var typeArg = GetNamedArg(attr, "Type") as INamedTypeSymbol;
        if (typeArg is not null)
            return typeArg;

        var attrClass = attr.AttributeClass;
        if (attrClass is { TypeArguments.Length: > 0 })
        {
            var firstTypeArg = attrClass.TypeArguments[0] as INamedTypeSymbol;
            if (firstTypeArg is not null && firstTypeArg.SpecialType != SpecialType.System_Object)
                return firstTypeArg;
        }

        return typeArg;
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
        sb.AppendLine("using System.Text.Json.Serialization.Metadata;");
        sb.AppendLine("using System.Threading;");
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
            sb.AppendLine($"    public async Task<{returnType}> Get{entry.Name}Async(CancellationToken ct = default)");
            sb.AppendLine("    {");
            sb.Append(GetReaderSource(entry));
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public async Task Set{entry.Name}Async({paramType} value, CancellationToken ct = default)");
            sb.AppendLine("    {");
            sb.Append(GetWriterSource(entry));
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string EscapeStringLiteral(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
    }

    private static string GetReaderSource(SettingEntry entry)
    {
        var name = entry.Name;
        var fallback = GetFallbackExpression(entry);
        var escapedName = EscapeStringLiteral(name);

        if (entry.IsString)
        {
            if (entry.HasDefaultValue)
                return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                     + $"        return val ?? \"{EscapeStringLiteral(entry.DefaultValue!)}\";\n";

            if (entry.Nullable)
                return $"        return await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n";

            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return val ?? string.Empty;\n";
        }

        if (entry.IsInt32)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return int.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsInt64)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return long.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsSingle)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return float.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsDouble)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return double.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsBoolean)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return bool.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsDateTime)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return DateTime.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.IsGuid)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        return Guid.TryParse(val, out var r) ? r : {fallback};\n";

        if (entry.HasJsonTypeInfo)
            return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
                 + $"        if (string.IsNullOrEmpty(val)) return {fallback};\n"
                 + $"        var jti = {GetJsonTypeInfoAccess(entry)};\n"
                 + $"        return JsonSerializer.Deserialize(val, jti!);\n";

        return $"        var val = await ReadAsync(\"{escapedName}\", ct).ConfigureAwait(false);\n"
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
                return entry.DefaultValue!.ToLowerInvariant();
            if (entry.IsDateTime)
                return $"DateTime.Parse(\"{EscapeStringLiteral(entry.DefaultValue!)}\")";
            if (entry.IsGuid)
                return $"Guid.Parse(\"{EscapeStringLiteral(entry.DefaultValue!)}\")";
            if (entry.IsComplex)
            {
                if (entry.HasJsonTypeInfo)
                    return $"JsonSerializer.Deserialize(\"{EscapeStringLiteral(entry.DefaultValue!)}\", {GetJsonTypeInfoAccess(entry)})";

                return $"JsonSerializer.Deserialize<{entry.TypeName}>(\"{EscapeStringLiteral(entry.DefaultValue!)}\")";
            }
            return entry.DefaultValue!;
        }

        if (entry.Nullable)
            return "default";

        if (entry.IsSingle) return "0f";
        if (entry.IsDouble) return "0.0";
        if (entry.IsInt64) return "0L";
        if (entry.IsInt32) return "0";
        if (entry.IsBoolean) return "false";
        if (entry.IsDateTime) return "DateTime.MinValue";
        if (entry.IsGuid) return "Guid.Empty";
        if (entry.IsComplex) return "default";

        return "default";
    }

    private static string GetWriterSource(SettingEntry entry)
    {
        var name = entry.Name;
        var escapedName = EscapeStringLiteral(name);

        if (entry.IsString)
            return $"        await WriteAsync(value, \"{escapedName}\", ct).ConfigureAwait(false);\n";

        if (entry.HasJsonTypeInfo)
        {
            if (entry.Nullable)
                return $"        string? json = null;\n"
                     + $"        if (value is not null)\n"
                     + $"        {{\n"
                     + $"            var jti = {GetJsonTypeInfoAccess(entry)};\n"
                     + $"            json = JsonSerializer.Serialize(value, jti!);\n"
                     + $"        }}\n"
                     + $"        await WriteAsync(json, \"{escapedName}\", ct).ConfigureAwait(false);\n";

            return $"        var jti = {GetJsonTypeInfoAccess(entry)};\n"
                 + $"        var json = JsonSerializer.Serialize(value, jti!);\n"
                 + $"        await WriteAsync(json, \"{escapedName}\", ct).ConfigureAwait(false);\n";
        }

        if (entry.IsComplex)
        {
            if (entry.Nullable)
                return $"        string? json = null;\n"
                     + $"        if (value is not null)\n"
                     + $"            json = JsonSerializer.Serialize<{entry.TypeName}>(value);\n"
                     + $"        await WriteAsync(json, \"{escapedName}\", ct).ConfigureAwait(false);\n";

            return $"        var json = JsonSerializer.Serialize<{entry.TypeName}>(value);\n"
                 + $"        await WriteAsync(json, \"{escapedName}\", ct).ConfigureAwait(false);\n";
        }

        if (entry.Nullable)
            return $"        await WriteAsync(value?.ToString(), \"{escapedName}\", ct).ConfigureAwait(false);\n";

        return $"        await WriteAsync(value.ToString(), \"{escapedName}\", ct).ConfigureAwait(false);\n";
    }

    private static string GetJsonTypeInfoAccess(SettingEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.JsonTypeInfoContextType)
            && !string.IsNullOrWhiteSpace(entry.JsonTypeInfoPropertyName))
        {
            return $"(JsonTypeInfo<{entry.TypeName}>){entry.JsonTypeInfoContextType}.Default.{entry.JsonTypeInfoPropertyName}";
        }

        return $"(JsonTypeInfo<{entry.TypeName}>)GetJsonTypeInfo(\"{entry.Name}\")";
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
        public bool IsComplex { get; }
        public bool HasJsonTypeInfo { get; }
        public string? JsonTypeInfoContextType { get; }
        public string? JsonTypeInfoPropertyName { get; }

        public SettingEntry(string name, string typeName, bool nullable, string? defaultValue,
            bool isString, bool isInt32, bool isInt64, bool isSingle, bool isDouble,
            bool isBoolean, bool isDateTime, bool isGuid, bool isComplex, bool hasJsonTypeInfo,
            string? jsonTypeInfoContextType, string? jsonTypeInfoPropertyName)
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
            IsComplex = isComplex;
            HasJsonTypeInfo = hasJsonTypeInfo;
            JsonTypeInfoContextType = jsonTypeInfoContextType;
            JsonTypeInfoPropertyName = jsonTypeInfoPropertyName;
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
