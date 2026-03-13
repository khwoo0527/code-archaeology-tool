using CodeArchaeology.Models;

namespace CodeArchaeology.Analysis;

/// <summary>
/// C# 소스 코드 정적 분석 인터페이스.
/// UI 레이어는 RoslynAnalyzer 구현체에 직접 의존하지 않고
/// 이 인터페이스를 통해 Analysis 레이어와 통신한다.
/// </summary>
public interface IAnalyzer
{
    /// <summary>
    /// 주어진 .cs 파일 목록을 분석하여 타입 노드와 의존성 엣지를 추출한다.
    /// </summary>
    /// <param name="filePaths">분석할 .cs 파일 경로 목록</param>
    /// <returns>
    /// 분석 결과 — Nodes(타입 목록), Edges(의존성 목록), Errors(파싱 실패 파일 목록) 포함.
    /// 일부 파일에서 파싱 오류가 발생해도 나머지 파일은 계속 분석된다.
    /// </returns>
    AnalysisResult Analyze(IReadOnlyList<string> filePaths);
}
