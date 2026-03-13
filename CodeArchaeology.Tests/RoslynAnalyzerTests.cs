using CodeArchaeology.Analysis;
using CodeArchaeology.Models;
using Xunit;

namespace CodeArchaeology.Tests;

/// <summary>
/// RoslynAnalyzer 핵심 로직 단위 테스트.
/// 실제 .cs 파일을 임시 경로에 기록 후 분석 — UI 의존 없이 분석 엔진만 격리 검증.
/// </summary>
public class RoslynAnalyzerTests : IDisposable
{
    private readonly List<string> _tempFiles = new();
    private readonly RoslynAnalyzer _analyzer = new();

    // ── 헬퍼 ────────────────────────────────────────────────────────────────

    /// <summary>임시 .cs 파일을 생성하고 경로를 반환한다.</summary>
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
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    // ── 노드 추출 테스트 ─────────────────────────────────────────────────────

    [Fact]
    public void Analyze_SingleClass_ReturnsOneClassNode()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public class Foo { }
            """);

        var result = _analyzer.Analyze([path]);

        Assert.Single(result.Nodes);
        var node = result.Nodes[0];
        Assert.Equal("Foo", node.Name);
        Assert.Equal("MyApp", node.Namespace);
        Assert.Equal(TypeKind.Class, node.Kind);
    }

    [Fact]
    public void Analyze_SingleInterface_ReturnsOneInterfaceNode()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public interface IFoo { void DoWork(); }
            """);

        var result = _analyzer.Analyze([path]);

        Assert.Single(result.Nodes);
        var node = result.Nodes[0];
        Assert.Equal("IFoo", node.Name);
        Assert.Equal(TypeKind.Interface, node.Kind);
    }

    [Fact]
    public void Analyze_ClassAndInterface_ReturnsBothNodes()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public interface IAnimal { }
            public class Dog { }
            """);

        var result = _analyzer.Analyze([path]);

        Assert.Equal(2, result.Nodes.Count);
        Assert.Contains(result.Nodes, n => n.Kind == TypeKind.Class && n.Name == "Dog");
        Assert.Contains(result.Nodes, n => n.Kind == TypeKind.Interface && n.Name == "IAnimal");
    }

    [Fact]
    public void Analyze_Class_CountsFieldsAndMethods()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public class Order {
                private int _id;
                private string _name;
                public void Save() { }
                public void Delete() { }
                public void Load() { }
            }
            """);

        var result = _analyzer.Analyze([path]);

        var node = Assert.Single(result.Nodes);
        Assert.Equal(2, node.FieldCount);
        Assert.Equal(3, node.MethodCount);
    }

    // ── 엣지 추출 테스트 ─────────────────────────────────────────────────────

    [Fact]
    public void Analyze_InheritanceRelation_ReturnsInheritanceEdge()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public class Animal { }
            public class Dog : Animal { }
            """);

        var result = _analyzer.Analyze([path]);

        Assert.Single(result.Edges);
        var edge = result.Edges[0];
        Assert.Equal("Dog", edge.Source);
        Assert.Equal("Animal", edge.Target);
        Assert.Equal(EdgeType.Inheritance, edge.Type);
    }

    [Fact]
    public void Analyze_InterfaceImplementation_ReturnsInterfaceImplEdge()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public interface IAnimal { }
            public class Cat : IAnimal { }
            """);

        var result = _analyzer.Analyze([path]);

        Assert.Single(result.Edges);
        var edge = result.Edges[0];
        Assert.Equal("Cat", edge.Source);
        Assert.Equal("IAnimal", edge.Target);
        Assert.Equal(EdgeType.InterfaceImpl, edge.Type);
    }

    [Fact]
    public void Analyze_FieldDependency_ReturnsFieldDependencyEdge()
    {
        var path = WriteTempFile("""
            namespace MyApp;
            public class Engine { }
            public class Car {
                private Engine _engine;
            }
            """);

        var result = _analyzer.Analyze([path]);

        var fieldEdge = result.Edges.FirstOrDefault(e => e.Type == EdgeType.FieldDependency);
        Assert.NotNull(fieldEdge);
        Assert.Equal("Car", fieldEdge.Source);
        Assert.Equal("Engine", fieldEdge.Target);
    }

    [Fact]
    public void Analyze_ExternalTypeField_IsNotIncludedAsEdge()
    {
        // string, int 같은 빌트인 타입이나 프로젝트 외부 타입은 엣지로 만들지 않는다
        var path = WriteTempFile("""
            namespace MyApp;
            public class Foo {
                private string _name;
                private int _count;
                private List<string> _items;
            }
            """);

        var result = _analyzer.Analyze([path]);

        Assert.Empty(result.Edges);
    }

    // ── partial class 병합 테스트 ────────────────────────────────────────────

    [Fact]
    public void Analyze_PartialClass_MergesIntoSingleNode()
    {
        var path1 = WriteTempFile("""
            namespace MyApp;
            public partial class Dog {
                private string _name;
                public void Bark() { }
            }
            """);
        var path2 = WriteTempFile("""
            namespace MyApp;
            public partial class Dog {
                public void Fetch() { }
            }
            """);

        var result = _analyzer.Analyze([path1, path2]);

        var dogNodes = result.Nodes.Where(n => n.Name == "Dog").ToList();
        Assert.Single(dogNodes);  // 병합 후 1개
        Assert.Equal(1, dogNodes[0].FieldCount);   // path1의 필드
        Assert.Equal(2, dogNodes[0].MethodCount);  // Bark + Fetch 합산
    }

    // ── 에러 핸들링 테스트 ───────────────────────────────────────────────────

    [Fact]
    public void Analyze_SyntaxErrorFile_RecordsErrorAndContinues()
    {
        var badPath = WriteTempFile("this is not valid C# {{{{");
        var goodPath = WriteTempFile("""
            namespace MyApp;
            public class ValidClass { }
            """);

        var result = _analyzer.Analyze([badPath, goodPath]);

        // 에러 파일은 Errors에 기록되고, 정상 파일은 분석 완료
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Nodes, n => n.Name == "ValidClass");
    }

    [Fact]
    public void Analyze_EmptyFileList_ReturnsEmptyResult()
    {
        var result = _analyzer.Analyze([]);

        Assert.Empty(result.Nodes);
        Assert.Empty(result.Edges);
        Assert.Empty(result.Errors);
    }
}
