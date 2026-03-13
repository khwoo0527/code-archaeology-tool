namespace CodeArchaeology.Models;

public enum TypeKind { Class, Interface }

public class TypeNode
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
    public string FilePath { get; set; } = string.Empty;
    public TypeKind Kind { get; set; }
    public int FieldCount { get; set; }
    public int MethodCount { get; set; }
    public List<string> FieldNames { get; set; } = new();
    public List<string> MethodNames { get; set; } = new();
}
