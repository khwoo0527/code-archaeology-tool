namespace CodeArchaeology.Models;

/// <summary>
/// 코드 분석 결과를 담는 데이터 전송 객체(DTO).
/// Analysis 레이어 → UI / Rendering 레이어로 데이터를 전달하는 유일한 경계 객체.
/// </summary>
/// <remarks>
/// 레이어 경계 역할:
/// <list type="bullet">
///   <item>Analysis 레이어는 이 객체를 생성하여 반환한다.</item>
///   <item>Rendering 레이어(MsaglRenderer)는 이 객체를 읽어 그래프를 구성한다.</item>
///   <item>UI 레이어(MainForm)는 이 객체를 보관하고 필터링하여 재렌더링에 사용한다.</item>
/// </list>
/// </remarks>
public class AnalysisResult
{
    /// <summary>분석된 타입 노드 목록 (class / interface / struct / record / enum).</summary>
    public List<TypeNode> Nodes { get; set; } = new();

    /// <summary>타입 간 의존성 엣지 목록 (상속 / 인터페이스 구현 / 필드 의존성).</summary>
    public List<DependencyEdge> Edges { get; set; } = new();

    /// <summary>
    /// 파싱 실패 파일 목록. 분석 중 예외가 발생한 파일은 건너뛰고 여기에 기록된다.
    /// UI 레이어의 Error Log 패널에 표시된다.
    /// </summary>
    public List<string> Errors { get; set; } = new();
}
