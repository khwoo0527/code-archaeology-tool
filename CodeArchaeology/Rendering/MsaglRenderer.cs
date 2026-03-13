using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using CodeArchaeology.Models;

namespace CodeArchaeology.Rendering;

public class MsaglRenderer
{
    public GViewer BuildViewer(AnalysisResult result)
    {
        var graph = new Graph("dependency");

        // 노드 이름 → FullName 역방향 조회를 위한 맵
        var nameToFullName = result.Nodes
            .GroupBy(n => n.Name)
            .ToDictionary(g => g.Key, g => g.First().FullName);

        foreach (var node in result.Nodes)
        {
            var drawingNode = graph.AddNode(node.FullName);
            drawingNode.LabelText = node.Name;

            drawingNode.Attr.Shape = node.Kind == TypeKind.Interface
                ? Shape.Ellipse
                : Shape.Box;
        }

        foreach (var edge in result.Edges)
        {
            if (!nameToFullName.TryGetValue(edge.Source, out var sourceId)) continue;
            if (!nameToFullName.TryGetValue(edge.Target, out var targetId)) continue;

            var drawingEdge = graph.AddEdge(sourceId, targetId);

            switch (edge.Type)
            {
                case EdgeType.Inheritance:
                    drawingEdge.Attr.Color = Microsoft.Msagl.Drawing.Color.Black;
                    break;
                case EdgeType.InterfaceImpl:
                    drawingEdge.Attr.Color = Microsoft.Msagl.Drawing.Color.Blue;
                    drawingEdge.Attr.AddStyle(Style.Dashed);
                    break;
                case EdgeType.FieldDependency:
                    drawingEdge.Attr.Color = Microsoft.Msagl.Drawing.Color.Gray;
                    break;
            }
        }

        var gViewer = new GViewer
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Graph = graph
        };

        return gViewer;
    }
}
