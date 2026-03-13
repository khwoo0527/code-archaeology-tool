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
    // struct / record / enum 색상
    private static readonly Microsoft.Msagl.Drawing.Color StructFill   = new(40, 90, 70);   // 진한 녹청
    private static readonly Microsoft.Msagl.Drawing.Color StructBorder = new(80, 200, 150);  // 밝은 녹청
    private static readonly Microsoft.Msagl.Drawing.Color RecordFill   = new(90, 70, 30);   // 진한 황토
    private static readonly Microsoft.Msagl.Drawing.Color RecordBorder = new(220, 180, 80);  // 밝은 황토
    private static readonly Microsoft.Msagl.Drawing.Color EnumFill     = new(60, 50, 90);   // 진한 자주
    private static readonly Microsoft.Msagl.Drawing.Color EnumBorder   = new(160, 130, 240); // 밝은 자주
    // 순환 의존성 색상
    private static readonly Microsoft.Msagl.Drawing.Color CycleEdge    = new(220, 60, 60);
    private static readonly Microsoft.Msagl.Drawing.Color CycleFill    = new(100, 30, 30);
    // 영향 분석 색상
    private static readonly Microsoft.Msagl.Drawing.Color ImpactRootFill   = new(200, 100, 20);  // 진한 주황
    private static readonly Microsoft.Msagl.Drawing.Color ImpactRootBorder = new(255, 160, 60);  // 밝은 주황
    private static readonly Microsoft.Msagl.Drawing.Color ImpactFill       = new(120, 70, 10);   // 어두운 주황
    private static readonly Microsoft.Msagl.Drawing.Color ImpactBorder     = new(220, 140, 50);  // 중간 주황
    private static readonly Microsoft.Msagl.Drawing.Color ImpactEdge       = new(220, 140, 50);

    public GViewer BuildViewer(AnalysisResult result, string searchQuery = "", string focusNodeId = "",
        string impactRootId = "", HashSet<string>? impactSet = null)
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

        var hasSearch   = !string.IsNullOrWhiteSpace(searchQuery);
        var hasFocus    = !string.IsNullOrWhiteSpace(focusNodeId);
        var hasImpact   = !string.IsNullOrWhiteSpace(impactRootId);
        impactSet     ??= new HashSet<string>();
        var cycleEdges  = Analysis.CycleDetector.FindCycleEdges(result);
        var cycleNodes  = new HashSet<string>(
            cycleEdges.SelectMany(e => new[] { e.Source, e.Target }));

        // 포커스 모드: 클릭 노드 + 1-hop 이웃 집합
        HashSet<string> focusSet = new();
        if (hasFocus)
        {
            focusSet.Add(focusNodeId);
            foreach (var edge in result.Edges)
            {
                if (edge.Source == focusNodeId) focusSet.Add(edge.Target);
                if (edge.Target == focusNodeId) focusSet.Add(edge.Source);
            }
        }

        foreach (var node in result.Nodes)
        {
            var dn = graph.AddNode(node.FullName);
            dn.LabelText = node.FullName;

            // 영향 분석 모드가 활성이면 영향 노드 우선 처리
            if (hasImpact)
            {
                var isRoot   = node.Name == impactRootId;
                var isImpact = impactSet.Contains(node.Name);

                dn.Label.FontSize = 10;
                dn.Attr.Shape     = node.Kind == TypeKind.Interface ? Shape.Ellipse : Shape.Box;
                if (isRoot)
                {
                    dn.Label.FontColor = NodeText;
                    dn.Attr.FillColor  = ImpactRootFill;
                    dn.Attr.Color      = ImpactRootBorder;
                    dn.Attr.LineWidth  = 3.0;
                }
                else if (isImpact)
                {
                    dn.Label.FontColor = NodeText;
                    dn.Attr.FillColor  = ImpactFill;
                    dn.Attr.Color      = ImpactBorder;
                    dn.Attr.LineWidth  = 2.0;
                }
                else
                {
                    dn.Label.FontColor = DimText;
                    dn.Attr.FillColor  = DimFill;
                    dn.Attr.Color      = DimBorder;
                    dn.Attr.LineWidth  = 1.0;
                }
                continue;
            }

            var isMatch = (!hasSearch ||
                node.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                node.FullName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                && (!hasFocus || focusSet.Contains(node.Name));

            if (isMatch)
            {
                dn.Label.FontColor = NodeText;
                dn.Label.FontSize = 10;

                var inCycle = cycleNodes.Contains(node.Name);

                dn.Attr.LineWidth = inCycle ? 2.5 : 1.5;

                switch (node.Kind)
                {
                    case TypeKind.Interface:
                        dn.Attr.Shape     = Shape.Ellipse;
                        dn.Attr.FillColor = inCycle ? CycleFill : IfaceFill;
                        dn.Attr.Color     = inCycle ? CycleEdge : IfaceBorder;
                        break;
                    case TypeKind.Struct:
                        dn.Attr.Shape     = Shape.Diamond;
                        dn.Attr.FillColor = inCycle ? CycleFill : StructFill;
                        dn.Attr.Color     = inCycle ? CycleEdge : StructBorder;
                        break;
                    case TypeKind.Record:
                        dn.Attr.Shape     = Shape.Box;
                        dn.Attr.FillColor = inCycle ? CycleFill : RecordFill;
                        dn.Attr.Color     = inCycle ? CycleEdge : RecordBorder;
                        dn.Attr.XRadius   = 10;
                        dn.Attr.YRadius   = 10;
                        break;
                    case TypeKind.Enum:
                        dn.Attr.Shape     = Shape.Hexagon;
                        dn.Attr.FillColor = inCycle ? CycleFill : EnumFill;
                        dn.Attr.Color     = inCycle ? CycleEdge : EnumBorder;
                        break;
                    default: // Class
                        dn.Attr.Shape     = Shape.Box;
                        dn.Attr.FillColor = inCycle ? CycleFill : ClassFill;
                        dn.Attr.Color     = inCycle ? CycleEdge : ClassBorder;
                        dn.Attr.XRadius   = 3;
                        dn.Attr.YRadius   = 3;
                        break;
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

            // 영향 분석 모드: 영향 경로 엣지 강조, 나머지 dim
            if (hasImpact)
            {
                var onPath = (impactSet.Contains(edge.Source) || edge.Source == impactRootId)
                          && (impactSet.Contains(edge.Target) || edge.Target == impactRootId);
                if (onPath)
                {
                    de.Attr.Color     = ImpactEdge;
                    de.Attr.LineWidth = 2.0;
                }
                else
                {
                    de.Attr.Color     = DimBorder;
                    de.Attr.LineWidth = 0.5;
                }
                continue;
            }

            var edgeDimmed = hasFocus &&
                (!focusSet.Contains(edge.Source) || !focusSet.Contains(edge.Target));
            var isCycleEdge = cycleEdges.Contains((edge.Source, edge.Target));

            if (edgeDimmed)
            {
                de.Attr.Color     = DimBorder;
                de.Attr.LineWidth = 0.5;
            }
            else if (isCycleEdge)
            {
                de.Attr.Color     = CycleEdge;
                de.Attr.LineWidth = 2.5;
            }
            else
            {
                switch (edge.Type)
                {
                    case EdgeType.Inheritance:
                        de.Attr.Color     = EdgeInherit;
                        de.Attr.LineWidth = 1.5;
                        break;
                    case EdgeType.InterfaceImpl:
                        de.Attr.Color = EdgeIface;
                        de.Attr.AddStyle(Style.Dashed);
                        de.Attr.LineWidth = 1.5;
                        break;
                    case EdgeType.FieldDependency:
                        de.Attr.Color     = EdgeField;
                        de.Attr.LineWidth = 1;
                        break;
                }
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
