using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;
using CodeArchaeology.Models;

namespace CodeArchaeology.Rendering;

public class MsaglRenderer
{
    public GViewer BuildViewer(AnalysisResult result)
    {
        var graph = new Graph("dependency");

        foreach (var node in result.Nodes)
        {
            var drawingNode = graph.AddNode(node.FullName);
            drawingNode.LabelText = node.Name;

            // 인터페이스는 타원형, 클래스는 기본 사각형으로 구분
            drawingNode.Attr.Shape = node.Kind == TypeKind.Interface
                ? Shape.Ellipse
                : Shape.Box;
        }

        var gViewer = new GViewer
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Graph = graph
        };

        return gViewer;
    }
}
