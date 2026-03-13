using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeArchaeology.Models;

namespace CodeArchaeology.Analysis;

public class RoslynAnalyzer
{
    public AnalysisResult Analyze(IReadOnlyList<string> filePaths)
    {
        var result = new AnalysisResult();

        foreach (var filePath in filePaths)
        {
            try
            {
                var code = File.ReadAllText(filePath);
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetCompilationUnitRoot();
                var walker = new TypeWalker(filePath);
                walker.Visit(root);

                result.Nodes.AddRange(walker.Nodes);
            }
            catch (Exception ex)
            {
                // S-08에서 에러 핸들링 강화 예정
                result.Errors.Add($"{filePath}: {ex.Message}");
            }
        }

        return result;
    }
}

internal class TypeWalker : CSharpSyntaxWalker
{
    private readonly string _filePath;
    private string _currentNamespace = string.Empty;

    public List<TypeNode> Nodes { get; } = new();

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

        base.VisitClassDeclaration(node);
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
