using CodeArchaeology.Analysis;
using CodeArchaeology.Models;
using Xunit;

namespace CodeArchaeology.Tests;

/// <summary>
/// struct / record / enum 타입 분석 단위 테스트.
/// </summary>
public class StructRecordEnumTests : IDisposable
{
    private readonly List<string> _tempFiles = new();
    private readonly RoslynAnalyzer _analyzer = new();

    private string WriteTempFile(string code)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.cs");
        File.WriteAllText(path, code);
        _tempFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        foreach (var path in _tempFiles)
            if (File.Exists(path)) File.Delete(path);
    }

    [Fact]
    public void Analyze_Struct_ReturnsStructKind()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public struct Point { public int X; public int Y; }
            """);

        var result = _analyzer.Analyze([path]);

        var node = Assert.Single(result.Nodes);
        Assert.Equal("Point", node.Name);
        Assert.Equal(TypeKind.Struct, node.Kind);
        Assert.Equal(2, node.FieldCount);
    }

    [Fact]
    public void Analyze_Record_ReturnsRecordKind()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public record Person(string Name, int Age);
            """);

        var result = _analyzer.Analyze([path]);

        var node = Assert.Single(result.Nodes);
        Assert.Equal("Person", node.Name);
        Assert.Equal(TypeKind.Record, node.Kind);
    }

    [Fact]
    public void Analyze_Enum_ReturnsEnumKind()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public enum Direction { North, South, East, West }
            """);

        var result = _analyzer.Analyze([path]);

        var node = Assert.Single(result.Nodes);
        Assert.Equal("Direction", node.Name);
        Assert.Equal(TypeKind.Enum, node.Kind);
    }

    [Fact]
    public void Analyze_AllTypes_ReturnsCorrectKinds()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public class MyClass { }
            public interface IMyInterface { }
            public struct MyStruct { }
            public record MyRecord(string Value);
            public enum MyEnum { A, B }
            """);

        var result = _analyzer.Analyze([path]);

        Assert.Equal(5, result.Nodes.Count);
        Assert.Single(result.Nodes, n => n.Kind == TypeKind.Class);
        Assert.Single(result.Nodes, n => n.Kind == TypeKind.Interface);
        Assert.Single(result.Nodes, n => n.Kind == TypeKind.Struct);
        Assert.Single(result.Nodes, n => n.Kind == TypeKind.Record);
        Assert.Single(result.Nodes, n => n.Kind == TypeKind.Enum);
    }
}
