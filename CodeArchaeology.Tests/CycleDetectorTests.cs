using CodeArchaeology.Analysis;
using CodeArchaeology.Models;
using Xunit;

namespace CodeArchaeology.Tests;

/// <summary>
/// CycleDetector 순환 의존성 탐지 단위 테스트.
/// DFS 기반 back-edge 탐지 방식 — 사이클을 형성하는 역방향 엣지를 반환한다.
/// </summary>
public class CycleDetectorTests
{
    private static AnalysisResult MakeResult(params (string Source, string Target)[] edges)
    {
        var result = new AnalysisResult();
        foreach (var (src, tgt) in edges)
        {
            if (!result.Nodes.Any(n => n.Name == src))
                result.Nodes.Add(new TypeNode { Name = src, Kind = TypeKind.Class });
            if (!result.Nodes.Any(n => n.Name == tgt))
                result.Nodes.Add(new TypeNode { Name = tgt, Kind = TypeKind.Class });
            result.Edges.Add(new DependencyEdge { Source = src, Target = tgt, Type = EdgeType.FieldDependency });
        }
        return result;
    }

    [Fact]
    public void FindCycleEdges_NoCycle_ReturnsEmpty()
    {
        // A → B → C (비순환)
        var result = MakeResult(("A", "B"), ("B", "C"));

        var cycles = CycleDetector.FindCycleEdges(result);

        Assert.Empty(cycles);
    }

    [Fact]
    public void FindCycleEdges_DirectCycle_ReturnsBackEdge()
    {
        // A → B → A: DFS가 B에서 A(gray)를 발견 → back edge ("B","A") 반환
        var result = MakeResult(("A", "B"), ("B", "A"));

        var cycles = CycleDetector.FindCycleEdges(result);

        Assert.NotEmpty(cycles);
        // 사이클에 참여하는 노드(A, B)가 cycle edge에 포함되어야 함
        var involvedNodes = cycles.SelectMany(e => new[] { e.Source, e.Target }).ToHashSet();
        Assert.Contains("A", involvedNodes);
        Assert.Contains("B", involvedNodes);
    }

    [Fact]
    public void FindCycleEdges_ThreeNodeCycle_ReturnsBackEdge()
    {
        // A → B → C → A: back edge는 C→A
        var result = MakeResult(("A", "B"), ("B", "C"), ("C", "A"));

        var cycles = CycleDetector.FindCycleEdges(result);

        Assert.NotEmpty(cycles);
        // 사이클의 시작점 A가 cycle edge에 포함되어야 함
        var involvedNodes = cycles.SelectMany(e => new[] { e.Source, e.Target }).ToHashSet();
        Assert.Contains("A", involvedNodes);
    }

    [Fact]
    public void FindCycleEdges_LinearChainNotCyclic_ReturnsEmpty()
    {
        // A → B → C → D (선형, 사이클 없음)
        var result = MakeResult(("A", "B"), ("B", "C"), ("C", "D"));

        var cycles = CycleDetector.FindCycleEdges(result);

        Assert.Empty(cycles);
    }

    [Fact]
    public void FindCycleEdges_MixedGraph_OnlyCycleNodesInvolved()
    {
        // D → E (비순환) + A → B → A (순환)
        var result = MakeResult(("D", "E"), ("A", "B"), ("B", "A"));

        var cycles = CycleDetector.FindCycleEdges(result);

        // D, E는 사이클에 무관 — cycle edges에 포함되지 않아야 함
        Assert.DoesNotContain(("D", "E"), cycles);
        var involvedNodes = cycles.SelectMany(e => new[] { e.Source, e.Target }).ToHashSet();
        Assert.DoesNotContain("D", involvedNodes);
        Assert.DoesNotContain("E", involvedNodes);
    }

    [Fact]
    public void FindCycleEdges_EmptyGraph_ReturnsEmpty()
    {
        var result = new AnalysisResult();

        var cycles = CycleDetector.FindCycleEdges(result);

        Assert.Empty(cycles);
    }
}
