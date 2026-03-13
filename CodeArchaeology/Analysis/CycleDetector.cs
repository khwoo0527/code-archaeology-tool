using CodeArchaeology.Models;

namespace CodeArchaeology.Analysis;

/// <summary>
/// DFS 기반 순환 의존성 감지 — 방향 그래프에서 사이클에 속한 엣지 집합 반환
/// </summary>
public static class CycleDetector
{
    public static HashSet<(string Source, string Target)> FindCycleEdges(AnalysisResult result)
    {
        // 인접 리스트 구성 (방향 그래프)
        var adj = result.Nodes.ToDictionary(n => n.Name, _ => new List<string>());
        foreach (var edge in result.Edges)
        {
            if (adj.ContainsKey(edge.Source) && adj.ContainsKey(edge.Target))
                adj[edge.Source].Add(edge.Target);
        }

        var color  = result.Nodes.ToDictionary(n => n.Name, _ => 0); // 0=white, 1=gray, 2=black
        var cycleEdges = new HashSet<(string, string)>();

        foreach (var node in result.Nodes)
        {
            if (color[node.Name] == 0)
                Dfs(node.Name, adj, color, cycleEdges);
        }

        return cycleEdges;
    }

    private static void Dfs(
        string node,
        Dictionary<string, List<string>> adj,
        Dictionary<string, int> color,
        HashSet<(string, string)> cycleEdges)
    {
        color[node] = 1; // 방문 중

        foreach (var neighbor in adj[node])
        {
            if (color[neighbor] == 1)
            {
                // 역방향 엣지 = 사이클
                cycleEdges.Add((node, neighbor));
            }
            else if (color[neighbor] == 0)
            {
                Dfs(neighbor, adj, color, cycleEdges);
            }
        }

        color[node] = 2; // 완료
    }
}
