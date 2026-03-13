# Architecture

Code Archaeology는 4-레이어 단방향 의존성 구조로 설계되었습니다.

```
┌─────────────────────────────────────────────────────────┐
│  UI Layer  (CodeArchaeology.UI)                         │
│  MainForm — 사용자 입력 수신, 결과 표시                    │
└────────────────────┬────────────────────────────────────┘
                     │  IFolderScanner · IAnalyzer (인터페이스)
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Analysis Layer  (CodeArchaeology.Analysis)              │
│  FolderScanner — .cs 파일 수집                           │
│  RoslynAnalyzer — Syntax 기반 타입·의존성 추출            │
│  CycleDetector  — 순환 참조 탐지 (DFS)                   │
└────────────────────┬────────────────────────────────────┘
                     │  AnalysisResult (DTO)
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Model Layer  (CodeArchaeology.Models)                   │
│  TypeNode · DependencyEdge · AnalysisResult             │
└─────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│  Rendering Layer  (CodeArchaeology.Rendering)            │
│  MsaglRenderer — MSAGL GViewer 그래프 구성               │
└─────────────────────────────────────────────────────────┘
```

---

## 소스 파일 구조

```
CodeArchaeology/
├── Models/
│   ├── TypeNode.cs          — C# 타입 노드 (Name/Namespace/Kind/FieldCount/MethodCount)
│   ├── DependencyEdge.cs    — 의존성 엣지 (Source/Target/EdgeType)
│   └── AnalysisResult.cs   — 분석 결과 DTO (Nodes/Edges/Errors)
│
├── Analysis/
│   ├── IFolderScanner.cs   — 파일 수집 인터페이스
│   ├── IAnalyzer.cs        — 정적 분석 인터페이스
│   ├── FolderScanner.cs    — IFolderScanner 구현체 (bin/obj 제외, 재귀 탐색)
│   ├── RoslynAnalyzer.cs   — IAnalyzer 구현체 (SyntaxWalker 3단계 분석)
│   └── CycleDetector.cs    — DFS 기반 순환 참조 탐지
│
├── Rendering/
│   └── MsaglRenderer.cs    — MSAGL Graph 구성 + GViewer 반환
│
├── UI/
│   ├── MainForm.cs          — 이벤트 처리, 상태 관리, 레이아웃
│   ├── MainForm.Designer.cs — 컨트롤 초기화 (자동 생성)
│   └── DarkToolStripRenderer.cs — 다크 테마 ToolStrip 렌더러
│
└── _TestSample/             — 로컬 검증용 샘플 (빌드 제외)

CodeArchaeology.Tests/
├── RoslynAnalyzerTests.cs   — 11개 단위 테스트
├── CycleDetectorTests.cs    — 6개 단위 테스트
└── StructRecordEnumTests.cs — 4개 단위 테스트
```

---

## 레이어 분리 — 실제 코드 증거

### 1. UI는 인터페이스에만 의존 (구현체 직접 참조 없음)

`MainForm.cs` — `RunAnalysisAsync()`:
```csharp
// UI 레이어는 IFolderScanner / IAnalyzer 인터페이스만 참조
// → 구현체(FolderScanner, RoslynAnalyzer) 교체 가능
IFolderScanner scanner = new Analysis.FolderScanner();
IAnalyzer analyzer = new Analysis.RoslynAnalyzer();
var csFiles = scanner.GetCsFiles(folderPath);
return (analyzer.Analyze(csFiles), csFiles);
```

### 2. Analysis 레이어는 WinForms에 의존하지 않음

`RoslynAnalyzer.cs` — 파일 상단 using 목록:
```csharp
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CodeArchaeology.Models;
// System.Windows.Forms 없음 — UI 레이어와 완전히 분리
```

`FolderScanner.cs` — 전체 using:
```csharp
namespace CodeArchaeology.Analysis;
// 추가 using 없음 — System.IO는 ImplicitUsings으로 포함
```

### 3. Rendering 레이어는 AnalysisResult DTO만 입력으로 받음

`MsaglRenderer.cs` — `BuildViewer` 시그니처:
```csharp
// AnalysisResult(Model)만 입력 — MainForm(UI)을 직접 참조하지 않음
public GViewer BuildViewer(AnalysisResult result, string searchQuery = "",
    string focusNodeId = "", string impactRootId = "",
    HashSet<string>? impactSet = null, bool codeSmellMode = false)
```

### 4. Model 레이어는 외부 의존성 없음

`TypeNode.cs` — 전체 파일:
```csharp
namespace CodeArchaeology.Models;  // 단일 네임스페이스 선언만 존재
// using 없음 — 순수 POCO, 다른 레이어 참조 전혀 없음
public class TypeNode { ... }
```

---

## 인터페이스 목록

| 인터페이스 | 파일 | 구현체 | 역할 |
|-----------|------|--------|------|
| `IFolderScanner` | `Analysis/IFolderScanner.cs` | `FolderScanner` | .cs 파일 목록 수집 |
| `IAnalyzer` | `Analysis/IAnalyzer.cs` | `RoslynAnalyzer` | 정적 분석 수행 |

인터페이스 정의 (`IAnalyzer.cs`):
```csharp
/// <summary>
/// C# 소스 코드 정적 분석 인터페이스.
/// UI 레이어는 RoslynAnalyzer 구현체에 직접 의존하지 않고
/// 이 인터페이스를 통해 Analysis 레이어와 통신한다.
/// </summary>
public interface IAnalyzer
{
    AnalysisResult Analyze(IReadOnlyList<string> filePaths);
}
```

---

## 코드 품질 적용 사례

### 에러 처리 — try-catch + Errors 리스트 기록

`RoslynAnalyzer.cs`:
```csharp
foreach (var filePath in filePaths)
{
    try
    {
        var code = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        // ...분석 로직
    }
    catch (Exception ex)
    {
        // 예외를 무시하지 않고 Errors에 기록 → UI Error Log에 표시됨
        result.Errors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
    }
}
```

### Null 안전성 — Nullable enable + early return

`MainForm.cs`:
```csharp
// Nullable enable 전체 적용 — null 가능 타입은 ? 명시
private Models.AnalysisResult? _analysisResult;
private Microsoft.Msagl.GraphViewerGdi.GViewer? _gViewer;

// early return 패턴으로 중첩 방지
private void btnRefresh_Click(object sender, EventArgs e)
{
    if (string.IsNullOrEmpty(_lastFolderPath))
    {
        SetStatus("먼저 폴더를 선택해 주세요.");
        return;
    }
    _ = RunAnalysisAsync(_lastFolderPath);
}
```

### LINQ + target-typed new()

`RoslynAnalyzer.cs` — partial class 병합:
```csharp
var mergedNodes = result.Nodes
    .GroupBy(n => n.FullName)
    .Select(g => new TypeNode          // target-typed new 없이도 명확
    {
        Name        = g.First().Name,
        Namespace   = g.First().Namespace,
        FieldCount  = g.Sum(n => n.FieldCount),
        MethodCount = g.Sum(n => n.MethodCount),
        FieldNames  = g.SelectMany(n => n.FieldNames).ToList(),
        MethodNames = g.SelectMany(n => n.MethodNames).ToList()
    })
    .ToList();
```

### XML 문서 주석 — 모든 public API 적용

`AnalysisResult.cs`:
```csharp
/// <summary>
/// 코드 분석 결과를 담는 데이터 전송 객체(DTO).
/// Analysis 레이어 → UI / Rendering 레이어로 데이터를 전달하는 유일한 경계 객체.
/// </summary>
public class AnalysisResult
{
    /// <summary>분석된 타입 노드 목록 (class / interface / struct / record / enum).</summary>
    public List<TypeNode> Nodes { get; set; } = new();

    /// <summary>타입 간 의존성 엣지 목록 (상속 / 인터페이스 구현 / 필드 의존성).</summary>
    public List<DependencyEdge> Edges { get; set; } = new();

    /// <summary>파싱 실패 파일 목록 — UI Error Log 패널에 표시.</summary>
    public List<string> Errors { get; set; } = new();
}
```

### using 선언으로 리소스 자동 해제

`MainForm.cs`:
```csharp
private void btnOpenFolder_Click(object sender, EventArgs e)
{
    using var dialog = new FolderBrowserDialog();  // 자동 Dispose
    if (dialog.ShowDialog() == DialogResult.OK)
        _ = RunAnalysisAsync(dialog.SelectedPath);
}
```

---

## NuGet 패키지 버전 중앙 관리

`Directory.Packages.props` (저장소 루트):
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.3.0" />
    <PackageVersion Include="Microsoft.Msagl"               Version="1.1.6" />
    <PackageVersion Include="Microsoft.Msagl.GraphViewerGDI" Version="1.1.7" />
    <PackageVersion Include="xunit"                         Version="2.9.3" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk"        Version="17.13.0" />
  </ItemGroup>
</Project>
```

각 `.csproj`에서는 버전 없이 선언:
```xml
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" />  <!-- 버전은 Directory.Packages.props에서 관리 -->
```

---

## 테스트 구조

```
CodeArchaeology.Tests/
├── RoslynAnalyzerTests.cs   — 분석 정확성 (11 tests)
├── CycleDetectorTests.cs    — 순환 탐지 (6 tests)
└── StructRecordEnumTests.cs — 타입 종류별 파싱 (4 tests)
```

총 **21개 단위 테스트**, GitHub Actions CI (`windows-latest`) 자동 실행.
