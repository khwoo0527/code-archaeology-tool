# Code Archaeology

[![CI](https://github.com/khwoo0527/code-archaeology-tool/actions/workflows/ci.yml/badge.svg)](https://github.com/khwoo0527/code-archaeology-tool/actions/workflows/ci.yml)

> C# 프로젝트 폴더를 열면 Roslyn으로 클래스 구조와 의존성을 자동 분석하고,
> Microsoft.Msagl 인터랙티브 그래프로 시각화하는 WinForms 데스크톱 도구

---

## 스크린샷

### 메인 화면 — 다크 테마 + 계층형 그래프

![main](docs/screenshots/main.png)

### 의존성 그래프 (클래스 / 인터페이스 / 필드 의존성)

![graph](docs/screenshots/graph.png)

> 스크린샷 폴더: `docs/screenshots/`

---

## 현재 상태

| 항목 | 내용 |
|------|------|
| Sprint 1 | ✅ 완료 — Core 12개 + Extension 7개 전원 완료 |
| Sprint 2 | 🔄 진행 중 — 3분할 레이아웃 + 인터랙션 |
| CI | ✅ GitHub Actions (build + test) |
| 단위 테스트 | ✅ 11개 전원 통과 |

---

## 주요 기능 (Sprint 1 완료)

| 기능 | 설명 |
|------|------|
| **폴더 열기** | C# 프로젝트 폴더 선택 → 자동 분석 (3클릭 이내) |
| **의존성 분석** | 클래스 상속 / 인터페이스 구현 / 필드 타입 의존성 추출 |
| **계층형 그래프** | Sugiyama TB 레이아웃 — 클래스 계층 구조를 위에서 아래로 표시 |
| **엣지 색상 구분** | 상속(검정 실선) / 인터페이스(파랑 점선) / 필드 의존성(회색 실선) |
| **노드 색상 구분** | 클래스(파랑 계열) / 인터페이스(보라 계열) 배경색 차별화 |
| **범례 패널** | 우상단 오버레이 — 엣지/노드 색상 의미 안내 |
| **partial class 병합** | 동일 FullName 노드 자동 통합, 필드/메서드 수 합산 |
| **비동기 분석** | Task.Run() 기반 — 대규모 프로젝트에서도 UI 프리징 없음 |
| **에러 처리** | 파싱 실패 파일은 건너뛰고 StatusBar에 에러 카운트 표시 |
| **다크 테마** | DarkToolStripRenderer + 파란 StatusBar — 모던 IDE 감성 |

---

## 시작하기

### 요구 사항

- Windows 10 이상
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (빌드용)

### 빌드 및 실행

```bash
git clone https://github.com/khwoo0527/code-archaeology-tool.git
cd code-archaeology-tool
dotnet build CodeArchaeology/CodeArchaeology.csproj
```

또는 Visual Studio에서 `CodeArchaeology.sln` 열고 `F5`

### 테스트 실행

```bash
dotnet test CodeArchaeology.Tests/CodeArchaeology.Tests.csproj --verbosity normal
```

---

## 사용 방법

1. 툴바의 **[폴더 열기]** 클릭
2. 분석할 C# 프로젝트 폴더 선택
3. 그래프가 자동으로 렌더링됨
4. StatusBar에서 분석된 클래스 수 / 에러 파일 수 확인
5. 줌/팬으로 그래프 탐색 (GViewer 내장)
6. **[새로고침]** 으로 코드 수정 후 즉시 재분석

---

## 기술 스택

| 영역 | 기술 | 버전 |
|------|------|------|
| UI | WinForms (.NET 8) | net8.0-windows |
| 코드 분석 | Microsoft.CodeAnalysis.CSharp (Roslyn) | 5.3.0 |
| 그래프 렌더링 | Microsoft.Msagl | 1.1.6 |
| 그래프 렌더링 | Microsoft.Msagl.GraphViewerGdi | 1.1.7 |
| 단위 테스트 | xUnit | 2.9.3 |
| CI/CD | GitHub Actions | windows-latest |

---

## 프로젝트 구조

```
CodeArchaeology/
├── Models/           ← TypeNode, DependencyEdge, AnalysisResult
├── Analysis/         ← FolderScanner, RoslynAnalyzer (Roslyn SyntaxWalker)
├── Rendering/        ← MsaglRenderer (Sugiyama 레이아웃)
├── UI/               ← MainForm, DarkToolStripRenderer
└── _TestSample/      ← 로컬 검증용 샘플 (빌드 제외)

CodeArchaeology.Tests/
└── RoslynAnalyzerTests.cs  ← 11개 단위 테스트
```

---

## 아키텍처

```
UI (WinForms)  →  Analysis (Roslyn)  →  Models (Graph)
  MainForm          FolderScanner         TypeNode
                    RoslynAnalyzer        DependencyEdge
                                          AnalysisResult
              →  Rendering (Msagl)
                    MsaglRenderer
```

레이어 간 단방향 의존성 원칙 준수 — UI는 Analysis를 직접 호출하지 않고 Model을 통해 데이터를 받는다.

---

## 왜 Code Archaeology인가?

| 기존 도구 | 한계 |
|----------|------|
| VS 클래스 다이어그램 | 클래스를 수동으로 하나씩 추가해야 함 |
| NDepend / ReSharper | 유료, 팀 전체 도입 비용 발생 |
| Graphviz | DOT 언어를 직접 작성해야 함 |
| UML 도구 | 코드와 다이어그램이 따로 관리되어 항상 불일치 |

**Code Archaeology**: 폴더 선택 한 번 → 로컬 실행 → 항상 현재 코드 기준 자동 분석

---

## 로드맵

| Sprint | 상태 | 목표 |
|--------|------|------|
| Sprint 1 | ✅ 완료 | 폴더 열기 → 의존성 분석 → 다크 테마 그래프 |
| Sprint 2 | 🔄 진행 중 | 3분할 레이아웃 + 노드 클릭 Class Info + 네임스페이스 필터 |
| Sprint 3 | 예정 | 메서드 호출 그래프 + 코드 스멜 지표 |
| Sprint 4 | 예정 | PNG 내보내기 + 전체 안정화 |

자세한 내용은 [ROADMAP.md](./ROADMAP.md) 참조

---

## 문서

- [PRD.md](./PRD.md) — 제품 요구사항 정의서
- [ROADMAP.md](./ROADMAP.md) — 전체 개발 로드맵
- [docs/sprints/sprint-1.md](./docs/sprints/sprint-1.md) — Sprint 1 진행 기록
- [docs/sprints/sprint-2.md](./docs/sprints/sprint-2.md) — Sprint 2 진행 기록

---

## 라이선스

MIT
