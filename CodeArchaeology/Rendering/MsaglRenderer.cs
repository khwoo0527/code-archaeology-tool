using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using CodeArchaeology.Models;

namespace CodeArchaeology.Rendering;

public class MsaglRenderer
{
    public GViewer BuildViewer(AnalysisResult result)
    {
        var graph = new Graph("dependency")
        {
            // 위→아래(TB) 계층형 레이아웃: 상속 계층이 자연스럽게 위에서 아래로 흐름
            LayoutAlgorithmSettings = new SugiyamaLayoutSettings
            {
                // 90도 회전 행렬로 LR→TB 전환: (cos90, -sin90, 0, sin90, cos90, 0) = (0, -1, 0, 1, 0, 0)
                Transformation = new Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation(0, -1, 0, 1, 0, 0),
                NodeSeparation = 20,
                LayerSeparation = 40
            }
        };

        // 노드 이름 → FullName 역방향 조회를 위한 맵
        var nameToFullName = result.Nodes
            .GroupBy(n => n.Name)
            .ToDictionary(g => g.Key, g => g.First().FullName);

        foreach (var node in result.Nodes)
        {
            var drawingNode = graph.AddNode(node.FullName);
            drawingNode.LabelText = node.FullName;

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
