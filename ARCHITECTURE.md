# Architecture

Code Archaeology는 3-레이어 단방향 의존성 구조로 설계되었습니다.

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

## 레이어 분리 원칙

| 규칙 | 코드 증거 |
|------|-----------|
| UI는 구현체가 아닌 인터페이스에만 의존 | `MainForm.cs`: `IFolderScanner scanner = new FolderScanner();` `IAnalyzer analyzer = new RoslynAnalyzer();` |
| Analysis 레이어는 WinForms를 참조하지 않음 | `RoslynAnalyzer.cs`, `FolderScanner.cs` — `using System.Windows.Forms` 없음 |
| Rendering 레이어는 AnalysisResult DTO만 입력으로 받음 | `MsaglRenderer.BuildViewer(AnalysisResult result, ...)` |
| Model 레이어는 다른 레이어를 참조하지 않음 | `TypeNode`, `DependencyEdge`, `AnalysisResult` — 외부 의존성 없음 |

## 인터페이스 목록

| 인터페이스 | 위치 | 구현체 |
|-----------|------|--------|
| `IFolderScanner` | `Analysis/IFolderScanner.cs` | `FolderScanner` |
| `IAnalyzer` | `Analysis/IAnalyzer.cs` | `RoslynAnalyzer` |

## NuGet 패키지 관리

중앙 집중식 버전 관리 — `Directory.Packages.props` 참조.

| 패키지 | 버전 | 용도 |
|--------|------|------|
| `Microsoft.CodeAnalysis.CSharp` | 5.3.0 | Roslyn 구문 분석 |
| `Microsoft.Msagl` | 1.1.6 | 그래프 레이아웃 엔진 |
| `Microsoft.Msagl.GraphViewerGDI` | 1.1.7 | WinForms 그래프 뷰어 |
| `xunit` | 2.9.3 | 단위 테스트 |
| `Microsoft.NET.Test.Sdk` | 17.13.0 | 테스트 실행 |

## 테스트 구조

```
CodeArchaeology.Tests/
├── RoslynAnalyzerTests.cs   — 분석 정확성 (11 tests)
├── CycleDetectorTests.cs    — 순환 탐지 (6 tests)
└── StructRecordEnumTests.cs — 타입 종류별 파싱 (4 tests)
```

총 **21개 단위 테스트**, GitHub Actions CI (`windows-latest`) 자동 실행.
