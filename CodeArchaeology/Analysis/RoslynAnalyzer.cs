using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeArchaeology.Models;

namespace CodeArchaeology.Analysis;

public class RoslynAnalyzer
{
    public AnalysisResult Analyze(IReadOnlyList<string> filePaths)
    {
        var result = new AnalysisResult();
        var walkers = new List<TypeWalker>();

        // 1단계: 모든 파일에서 노드(타입) 수집
        foreach (var filePath in filePaths)
        {
            try
            {
                var code = File.ReadAllText(filePath);
                var tree = CSharpSyntaxTree.ParseText(code);

                // 문법 오류가 있어도 파싱 가능한 범위까지 분석 (Roslyn 특성)
                // 단, 에러 파일은 Errors 리스트에 기록하여 StatusBar에 표시
                var diagnostics = tree.GetDiagnostics()
                    .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                    .ToList();

                if (diagnostics.Count > 0)
                {
                    result.Errors.Add($"{Path.GetFileName(filePath)}: 문법 오류 {diagnostics.Count}건");
                }

                var root = tree.GetCompilationUnitRoot();
                var walker = new TypeWalker(filePath);
                walker.Visit(root);
                walkers.Add(walker);
                result.Nodes.AddRange(walker.Nodes);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
            }
        }

        // 2단계: 수집된 타입 목록 확정 후 엣지 추출 (내부 타입끼리만)
        var knownTypeNames = result.Nodes.Select(n => n.Name).ToHashSet();
        foreach (var walker in walkers)
        {
            result.Edges.AddRange(walker.GetEdges(knownTypeNames));
        }

        return result;
    }
}

internal class TypeWalker : CSharpSyntaxWalker
{
    private readonly string _filePath;
    private string _currentNamespace = string.Empty;

    public List<TypeNode> Nodes { get; } = new();

    // 엣지 추출을 위해 클래스 선언 노드를 보관
    private readonly List<ClassDeclarationSyntax> _classDeclarations = new();

    public TypeWalker(string filePath)
    {
        _filePath = filePath;
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _currentNamespace = node.Name.ToString();
        base.VisitNamespaceDeclaration(node);
        _currentNamespace = string.Empty;
    }

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        _currentNamespace = node.Name.ToString();
        base.VisitFileScopedNamespaceDeclaration(node);
        _currentNamespace = string.Empty;
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        Nodes.Add(new TypeNode
        {
            Name = node.Identifier.Text,
            Namespace = _currentNamespace,
            FilePath = _filePath,
            Kind = TypeKind.Class,
            FieldCount = node.Members.OfType<FieldDeclarationSyntax>().Count(),
            MethodCount = node.Members.OfType<MethodDeclarationSyntax>().Count()
        });

        _classDeclarations.Add(node);
        base.VisitClassDeclaration(node);
    }

    public IEnumerable<DependencyEdge> GetEdges(HashSet<string> knownTypeNames)
    {
        foreach (var classNode in _classDeclarations)
        {
            if (classNode.BaseList == null) continue;

            var sourceName = classNode.Identifier.Text;

            foreach (var baseType in classNode.BaseList.Types)
            {
                var targetName = baseType.Type switch
                {
                    SimpleNameSyntax simple => simple.Identifier.Text,
                    QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
                    _ => null
                };

                if (targetName == null || !knownTypeNames.Contains(targetName)) continue;

                // 대상이 interface면 InterfaceImpl, 아니면 Inheritance
                var edgeType = targetName.StartsWith("I") && char.IsUpper(targetName.ElementAtOrDefault(1))
                    ? EdgeType.InterfaceImpl
                    : EdgeType.Inheritance;

                yield return new DependencyEdge
                {
                    Source = sourceName,
                    Target = targetName,
                    Type = edgeType
                };
            }
        }
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        Nodes.Add(new TypeNode
        {
            Name = node.Identifier.Text,
            Namespace = _currentNamespace,
            FilePath = _filePath,
            Kind = TypeKind.Interface,
            MethodCount = node.Members.OfType<MethodDeclarationSyntax>().Count()
        });

        base.VisitInterfaceDeclaration(node);
    }
}
