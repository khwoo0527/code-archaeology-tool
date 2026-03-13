namespace CodeArchaeology.Models;

public enum EdgeType { Inheritance, InterfaceImpl, FieldDependency }

public class DependencyEdge
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public EdgeType Type { get; set; }
}
