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

        // 2단계: partial class 병합 — 동일 FullName 노드를 하나로 합산
        // (FieldCount/MethodCount는 파일별 선언 합계, FilePath는 첫 번째 파일 기준)
        var mergedNodes = result.Nodes
            .GroupBy(n => n.FullName)
            .Select(g => new TypeNode
            {
                Name = g.First().Name,
                Namespace = g.First().Namespace,
                FilePath = g.First().FilePath,
                Kind = g.First().Kind,
                FieldCount = g.Sum(n => n.FieldCount),
                MethodCount = g.Sum(n => n.MethodCount),
                FieldNames = g.SelectMany(n => n.FieldNames).ToList(),
                MethodNames = g.SelectMany(n => n.MethodNames).ToList()
            })
            .ToList();
        result.Nodes.Clear();
        result.Nodes.AddRange(mergedNodes);

        // 3단계: 수집된 타입 목록 확정 후 엣지 추출 (내부 타입끼리만)
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
    private string? _currentClassName;

    public List<TypeNode> Nodes { get; } = new();

    // 엣지 추출을 위해 클래스 선언 노드를 보관
    private readonly List<ClassDeclarationSyntax> _classDeclarations = new();

    // 필드 의존성: (클래스명, 필드타입명) 쌍
    private readonly List<(string ClassName, string FieldTypeName)> _fieldDependencies = new();

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
            MethodCount = node.Members.OfType<MethodDeclarationSyntax>().Count(),
            FieldNames = node.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(f => f.Declaration.Variables.Select(v => v.Identifier.Text))
                .ToList(),
            MethodNames = node.Members.OfType<MethodDeclarationSyntax>()
                .Select(m => m.Identifier.Text)
                .ToList()
        });

        _classDeclarations.Add(node);
        _currentClassName = node.Identifier.Text;
        base.VisitClassDeclaration(node);
        _currentClassName = null;
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        if (_currentClassName == null)
        {
            base.VisitFieldDeclaration(node);
            return;
        }

        var typeName = ExtractTypeName(node.Declaration.Type);
        if (typeName != null)
        {
            _fieldDependencies.Add((_currentClassName, typeName));
        }

        base.VisitFieldDeclaration(node);
    }

    private static string? ExtractTypeName(TypeSyntax typeSyntax)
    {
        return typeSyntax switch
        {
            // int, string 등 빌트인 타입은 스킵
            PredefinedTypeSyntax => null,
            // List<OrderService> → "OrderService" (GenericNameSyntax는 SimpleNameSyntax 서브타입이라 먼저 처리)
            GenericNameSyntax generic => generic.TypeArgumentList.Arguments
                .Select(ExtractTypeName)
                .FirstOrDefault(n => n != null),
            // OrderService → "OrderService"
            SimpleNameSyntax simple => simple.Identifier.Text,
            // Models.OrderService → "OrderService"
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            // OrderService? → "OrderService"
            NullableTypeSyntax nullable => ExtractTypeName(nullable.ElementType),
            // OrderService[] → "OrderService"
            ArrayTypeSyntax array => ExtractTypeName(array.ElementType),
            _ => null
        };
    }

    public IEnumerable<DependencyEdge> GetEdges(HashSet<string> knownTypeNames)
    {
        // 상속/인터페이스 엣지
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

        // 필드 의존성 엣지 (내부 타입끼리만, 자기 자신 제외, 중복 제거)
        var seen = new HashSet<(string, string)>();
        foreach (var (className, fieldTypeName) in _fieldDependencies)
        {
            if (!knownTypeNames.Contains(fieldTypeName)) continue;
            if (className == fieldTypeName) continue;

            var pair = (className, fieldTypeName);
            if (!seen.Add(pair)) continue;

            yield return new DependencyEdge
            {
                Source = className,
                Target = fieldTypeName,
                Type = EdgeType.FieldDependency
            };
        }
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        Nodes.Add(new TypeNode
        {
            Name        = node.Identifier.Text,
            Namespace   = _currentNamespace,
            FilePath    = _filePath,
            Kind        = TypeKind.Interface,
            MethodCount = node.Members.OfType<MethodDeclarationSyntax>().Count(),
            MethodNames = node.Members.OfType<MethodDeclarationSyntax>()
                .Select(m => m.Identifier.Text).ToList()
        });
        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        Nodes.Add(new TypeNode
        {
            Name        = node.Identifier.Text,
            Namespace   = _currentNamespace,
            FilePath    = _filePath,
            Kind        = TypeKind.Struct,
            FieldCount  = node.Members.OfType<FieldDeclarationSyntax>().Count(),
            MethodCount = node.Members.OfType<MethodDeclarationSyntax>().Count(),
            FieldNames  = node.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(f => f.Declaration.Variables.Select(v => v.Identifier.Text)).ToList(),
            MethodNames = node.Members.OfType<MethodDeclarationSyntax>()
                .Select(m => m.Identifier.Text).ToList()
        });

        _currentClassName = node.Identifier.Text;
        base.VisitStructDeclaration(node);
        _currentClassName = null;
    }

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        Nodes.Add(new TypeNode
        {
            Name        = node.Identifier.Text,
            Namespace   = _currentNamespace,
            FilePath    = _filePath,
            Kind        = TypeKind.Record,
            FieldCount  = node.Members.OfType<FieldDeclarationSyntax>().Count(),
            MethodCount = node.Members.OfType<MethodDeclarationSyntax>().Count(),
            FieldNames  = node.Members.OfType<FieldDeclarationSyntax>()
                .SelectMany(f => f.Declaration.Variables.Select(v => v.Identifier.Text)).ToList(),
            MethodNames = node.Members.OfType<MethodDeclarationSyntax>()
                .Select(m => m.Identifier.Text).ToList()
        });

        _currentClassName = node.Identifier.Text;
        base.VisitRecordDeclaration(node);
        _currentClassName = null;
    }

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        Nodes.Add(new TypeNode
        {
            Name      = node.Identifier.Text,
            Namespace = _currentNamespace,
            FilePath  = _filePath,
            Kind      = TypeKind.Enum
        });
        base.VisitEnumDeclaration(node);
    }
}
