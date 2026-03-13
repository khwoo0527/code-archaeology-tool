using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using Microsoft.Msagl.Layout.Layered;
using CodeArchaeology.Models;

namespace CodeArchaeology.Rendering;

public class MsaglRenderer
{
    // 다크 테마 기준 색상
    private static readonly Microsoft.Msagl.Drawing.Color DarkBg      = new(30, 30, 30);
    private static readonly Microsoft.Msagl.Drawing.Color ClassFill    = new(45, 80, 130);   // 진한 파랑
    private static readonly Microsoft.Msagl.Drawing.Color ClassBorder  = new(100, 160, 240);  // 밝은 파랑 테두리
    private static readonly Microsoft.Msagl.Drawing.Color IfaceFill    = new(80, 45, 120);   // 진한 보라
    private static readonly Microsoft.Msagl.Drawing.Color IfaceBorder  = new(180, 120, 255); // 밝은 보라 테두리
    private static readonly Microsoft.Msagl.Drawing.Color NodeText     = new(220, 220, 220); // 밝은 글자
    private static readonly Microsoft.Msagl.Drawing.Color EdgeInherit  = new(200, 200, 200); // 밝은 회색 (다크bg 위 가시성)
    private static readonly Microsoft.Msagl.Drawing.Color EdgeIface    = new(100, 160, 255); // 밝은 파랑 점선
    private static readonly Microsoft.Msagl.Drawing.Color EdgeField    = new(120, 120, 140); // 중간 회색
    // 검색 dimming 색상
    private static readonly Microsoft.Msagl.Drawing.Color DimFill      = new(38, 38, 42);
    private static readonly Microsoft.Msagl.Drawing.Color DimBorder    = new(58, 58, 62);
    private static readonly Microsoft.Msagl.Drawing.Color DimText      = new(75, 75, 80);

    public GViewer BuildViewer(AnalysisResult result, string searchQuery = "")
    {
        var graph = new Graph("dependency")
        {
            LayoutAlgorithmSettings = new SugiyamaLayoutSettings
            {
                // 90도 회전 행렬로 LR→TB 전환
                Transformation = new Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation(0, -1, 0, 1, 0, 0),
                NodeSeparation = 20,
                LayerSeparation = 40
            }
        };

        // 그래프 배경 다크 처리
        graph.Attr.BackgroundColor = DarkBg;

        var nameToFullName = result.Nodes
            .GroupBy(n => n.Name)
            .ToDictionary(g => g.Key, g => g.First().FullName);

        var hasSearch = !string.IsNullOrWhiteSpace(searchQuery);

        foreach (var node in result.Nodes)
        {
            var dn = graph.AddNode(node.FullName);
            dn.LabelText = node.FullName;

            var isMatch = !hasSearch ||
                node.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                node.FullName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                dn.Label.FontColor = NodeText;
                dn.Label.FontSize = 10;

                if (node.Kind == TypeKind.Interface)
                {
                    dn.Attr.Shape = Shape.Ellipse;
                    dn.Attr.FillColor = IfaceFill;
                    dn.Attr.Color = IfaceBorder;
                    dn.Attr.LineWidth = 1.5;
                }
                else
                {
                    dn.Attr.Shape = Shape.Box;
                    dn.Attr.FillColor = ClassFill;
                    dn.Attr.Color = ClassBorder;
                    dn.Attr.LineWidth = 1.5;
                    dn.Attr.XRadius = 3;
                    dn.Attr.YRadius = 3;
                }
            }
            else
            {
                dn.Label.FontColor = DimText;
                dn.Label.FontSize = 10;
                dn.Attr.Shape = node.Kind == TypeKind.Interface ? Shape.Ellipse : Shape.Box;
                dn.Attr.FillColor = DimFill;
                dn.Attr.Color = DimBorder;
                dn.Attr.LineWidth = 1;
                if (node.Kind != TypeKind.Interface)
                {
                    dn.Attr.XRadius = 3;
                    dn.Attr.YRadius = 3;
                }
            }
        }

        foreach (var edge in result.Edges)
        {
            if (!nameToFullName.TryGetValue(edge.Source, out var sourceId)) continue;
            if (!nameToFullName.TryGetValue(edge.Target, out var targetId)) continue;

            var de = graph.AddEdge(sourceId, targetId);

            switch (edge.Type)
            {
                case EdgeType.Inheritance:
                    de.Attr.Color = EdgeInherit;
                    de.Attr.LineWidth = 1.5;
                    break;
                case EdgeType.InterfaceImpl:
                    de.Attr.Color = EdgeIface;
                    de.Attr.AddStyle(Style.Dashed);
                    de.Attr.LineWidth = 1.5;
                    break;
                case EdgeType.FieldDependency:
                    de.Attr.Color = EdgeField;
                    de.Attr.LineWidth = 1;
                    break;
            }
        }

        var gViewer = new GViewer
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30),
            OutsideAreaBrush = System.Drawing.Brushes.Transparent,
            Graph = graph
        };

        return gViewer;
    }
}
