namespace CodeArchaeology.Models;

public class AnalysisResult
{
    public List<TypeNode> Nodes { get; set; } = new();
    public List<DependencyEdge> Edges { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
