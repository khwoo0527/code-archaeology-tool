namespace CodeArchaeology.Analysis;

/// <summary>
/// C# 소스 파일 수집 인터페이스.
/// Analysis 레이어의 진입점 — UI 레이어는 이 인터페이스에만 의존한다.
/// </summary>
public interface IFolderScanner
{
    /// <summary>
    /// 지정 폴더에서 분석 대상 .cs 파일 경로 목록을 반환한다.
    /// bin / obj 등 빌드 산출물 폴더는 자동 제외된다.
    /// </summary>
    /// <param name="folderPath">탐색할 루트 폴더 경로</param>
    /// <returns>분석 대상 .cs 파일의 절대 경로 목록 (빌드 폴더 제외)</returns>
    IReadOnlyList<string> GetCsFiles(string folderPath);
}
