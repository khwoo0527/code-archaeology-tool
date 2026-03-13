# PRD: Code Archaeology

> Product Requirements Document
> Version: 1.3
> Date: 2026-03-13
> Status: Draft

---

## 1. 개요

### 1.1 제품 정의

**Code Archaeology**는 C# 프로젝트 폴더를 열면 Roslyn을 통해 클래스 구조와 의존성을 자동 분석하고, 인터랙티브 그래프로 시각화하는 WinForms 데스크톱 도구이다.

### 1.2 해결하는 문제

팀 개발자가 처음 접하는 레거시 C# 코드베이스를 빠르게 파악하기 위해서는 클래스 간 관계를 수작업으로 추적해야 한다. 이 과정은 시간 소모가 크고 오류가 발생하기 쉽다. Code Archaeology는 이 탐색 과정을 자동화하고 시각화하여 코드 이해 시간을 단축한다.

### 1.3 목표 사용자

| 역할 | 주요 사용 시나리오 |
|------|-------------------|
| 팀 내 개발자 | 레거시 코드베이스 온보딩 시 클래스 구조 파악 |
| 팀 내 개발자 | 리팩토링 전 의존성 범위 확인 |

### 1.4 기존 솔루션과의 차별점

| 솔루션 | 한계 | Code Archaeology의 접근 |
|--------|------|------------------------|
| Visual Studio 클래스 다이어그램 | 수동으로 클래스를 하나씩 추가해야 함. 대규모 프로젝트에서 비실용적 | 폴더 선택 한 번으로 전체 의존성 자동 분석 |
| NDepend / Resharper | 유료 라이선스 필요, 팀 전체 도입 비용 발생 | 무료 오픈소스, 설치 없이 실행 파일 하나로 동작 |
| Graphviz + 수동 DOT 작성 | 개발자가 직접 의존성을 DOT 언어로 기술해야 함 | Roslyn 정적 분석으로 DOT 없이 자동 추출 |
| UML 도구 (StarUML 등) | 코드와 다이어그램이 별개로 관리되어 항상 최신 상태 불일치 | 코드 폴더를 직접 분석하므로 항상 현재 코드 기준 |

**핵심 차별점 요약:**
- **Zero-config**: 폴더 열기 한 번으로 즉시 시각화 (설정, 스캐닝 규칙 불필요)
- **C# 공식 컴파일러 API 활용**: Roslyn 기반으로 구문 오류 파일도 부분 분석 가능
- **로컬 실행**: 코드가 외부 서버로 전송되지 않아 보안 민감한 레거시 프로젝트에 적합

---

## 2. 기능 요구사항

### 2.1 핵심 기능 (Sprint 1 MVP)

#### FR-01. 폴더 열기
- 사용자는 C# 프로젝트 폴더(`.csproj` 포함 또는 `.cs` 파일 포함 폴더)를 선택할 수 있다.
- FolderBrowserDialog로 폴더 선택 UI를 제공한다.
- 폴더 선택 후 자동으로 분석을 시작한다.

#### FR-02. Roslyn 기반 의존성 분석

**분석 대상 타입 (Sprint 1):**

| 타입 | 설명 |
|------|------|
| `class` | 일반 클래스 |
| `interface` | 인터페이스 |

> `struct`, `record`, `enum`은 Sprint 2 이후 분석 대상으로 추가 예정

**분석 대상 의존성 유형:**

| 유형 | 예시 | 시각화 |
|------|------|--------|
| 클래스 상속 | `class Dog : Animal` | 실선 화살표 |
| 인터페이스 구현 | `class Svc : IService` | 점선 화살표 |
| 필드 타입 의존성 | `private OrderRepo _repo` | 가는 실선 화살표 |

- 분석 범위: 선택한 폴더 내 `.cs` 파일 (재귀 탐색)
- **외부 타입 제외**: NuGet/System 네임스페이스 타입은 그래프에서 제외
- **네임스페이스 표시**: 노드에 `Namespace.ClassName` 형식으로 표시

#### FR-03. 그래프 시각화
- **레이아웃**: 계층형(Hierarchical) — 상속 계층이 위→아래로 흐르는 트리 형태
- **렌더링 라이브러리**: Microsoft.Msagl (WinForms 네이티브)
- **뷰어**: `Microsoft.Msagl.GraphViewerGdi.GViewer`를 WinForms `Panel`에 배치하여 그래프를 렌더링
- **엣지 시각적 구분**:
  - 상속: 실선, 검정
  - 인터페이스 구현: 점선, 파랑
  - 필드 의존성: 실선, 회색

#### FR-04. 에러 표시
- 컴파일 에러가 있는 `.cs` 파일은 에러를 무시하고 파싱 가능한 범위에서 분석한다.
- 에러가 발생한 파일 목록과 에러 메시지를 화면 하단 상태 바에 표시한다.

### 2.2 UI 구성 (Sprint 1)

```
┌─────────────────────────────────────────────────────┐
│  [폴더 열기]  [새로고침]                              │  ← Toolbar
├─────────────────────────────────────────────────────┤
│                                                     │
│                                                     │
│                  Graph Canvas                       │
│              (Microsoft.Msagl)                      │
│                                                     │
│                                                     │
├─────────────────────────────────────────────────────┤
│  상태: 분석 완료 (24개 클래스) | 에러: 2개 파일      │  ← StatusBar
└─────────────────────────────────────────────────────┘
```

> Sprint 2에서 Toolbar에 검색 TextBox 추가 예정

### 2.3 향후 기능 (Sprint 2+)

| 우선순위 | 기능 | 설명 |
|---------|------|------|
| High | 노드 호버 툴팁 | 네임스페이스 / 필드 수 / 메서드 수 / 파일 경로 표시 |
| High | 노드 클릭 포커스 모드 | 1-hop 이웃 강조, 나머지 흐리게, 빈 영역 클릭 시 복원 |
| High | 검색/필터링 | 클래스 이름으로 노드 하이라이트 |
| High | 메서드 호출 의존성 | `A.Method()`를 B가 호출하는 런타임 의존 관계 분석 |
| High | 순환 의존성 감지 | 순환 상속/의존 관계 감지 및 경고 표시 |
| High | 코드 스멜 지표 | 클래스별 참조 횟수, 의존도 지수 시각화 |
| Medium | 네임스페이스 필터링 | 특정 네임스페이스만 선택적 표시 |
| Medium | 변경 영향 분석 | A 클래스 선택 시 영향 받는 클래스 하이라이트 |
| Low | PNG/SVG 내보내기 | 그래프를 이미지로 저장 |

---

## 3. 비기능 요구사항

### 3.1 성능
- 50개 클래스 분석 시 허용 시간: **제한 없음** (정확성 우선)
- 분석 중 UI 프리징 방지: 비동기(`async/await`) 처리 — Sprint 1 Extension, Sprint 2에서 필수화

### 3.2 플랫폼
- **OS**: Windows 10+
- **런타임**: .NET 8
- **화면 해상도**: 1920×1080 기준 최적화

### 3.3 코드 품질
- 레이어 분리: UI(WinForms) / 분석(Roslyn) / 모델(Graph) 분리
- 단일 책임 원칙 준수

---

## 4. 기술 스택

| 영역 | 기술 | 확정 버전 | 선택 이유 |
|------|------|----------|----------|
| UI 프레임워크 | WinForms (.NET 8 네이티브) | net8.0-windows | .NET 8에서 WinForms를 공식 지원 (Microsoft.WindowsDesktop.App 포함). 마이그레이션 없이 신규 .NET 8 프로젝트로 시작. |
| 코드 분석 | Microsoft.CodeAnalysis.CSharp (Roslyn) | 5.3.0 | C# 공식 컴파일러 플랫폼. SemanticModel 없이 SyntaxTree만으로 빠른 정적 분석 가능. |
| 그래프 레이아웃 | Microsoft.Msagl | 1.1.6 | WinForms 네이티브 GViewer 제공, Sugiyama 계층형 레이아웃 내장. |
| 그래프 뷰어 | Microsoft.Msagl.GraphViewerGDI | 1.1.7 | WinForms Panel에 직접 임베드 가능한 인터랙티브 뷰어. |
| 단위 테스트 | xUnit | 2.9.3 | .NET 공식 권장 테스트 프레임워크. |
| 테스트 실행기 | Microsoft.NET.Test.Sdk | 17.13.0 | `dotnet test` CLI 통합. |
| 빌드 도구 | MSBuild / .NET 8 SDK | 8.0.x | - |

### 4-1. .NET 8 WinForms 호환성 검증

본 프로젝트는 **.NET Framework → .NET 8 마이그레이션이 아닌 신규 .NET 8 프로젝트**로 시작하였다.
WinForms on .NET 8의 호환성은 다음 항목으로 검증되었다:

| 항목 | 검증 방법 | 결과 |
|------|----------|------|
| WinForms 런타임 | `<UseWindowsForms>true</UseWindowsForms>` 빌드 확인 | ✅ 정상 빌드 |
| GDI+ 렌더링 | `GViewer`, `Graphics.CopyFromScreen`, `DrawToBitmap` 실행 확인 | ✅ 정상 동작 |
| Msagl 1.1.6 + .NET 8 | CI `windows-latest` + `net8.0-windows` 빌드 통과 | ✅ 확인 |
| 단일 실행 파일 | `--self-contained --runtime win-x64 -p:PublishSingleFile=true` | ✅ 배포 가능 |

### 4-2. NuGet 패키지 버전 중앙 관리

모든 NuGet 버전은 **`Directory.Packages.props`** 에서 중앙 집중 관리된다 (`ManagePackageVersionsCentrally=true`).
각 `.csproj`에서는 버전 없이 `<PackageReference Include="..." />` 만 선언한다.

```xml
<!-- Directory.Packages.props (저장소 루트) -->
<PackageVersion Include="Microsoft.CodeAnalysis.CSharp" Version="5.3.0" />
<PackageVersion Include="Microsoft.Msagl"               Version="1.1.6" />
<PackageVersion Include="Microsoft.Msagl.GraphViewerGDI" Version="1.1.7" />
<PackageVersion Include="xunit"                         Version="2.9.3" />
<PackageVersion Include="Microsoft.NET.Test.Sdk"        Version="17.13.0" />
```

---

## 5. 데이터 흐름

```
[사용자: 폴더 선택]
        │
        ▼
[FolderScanner] ─── .cs 파일 수집 (재귀)
        │
        ▼
[RoslynAnalyzer] ─── CSharpSyntaxTree.ParseText()
        │              SyntaxWalker로 클래스/상속/인터페이스/필드 추출
        │              외부 타입 필터링
        ▼
[GraphModel] ─── Node(클래스), Edge(의존성 유형)
        │
        ▼
[MsaglRenderer] ─── Microsoft.Msagl Graph 생성
        │              계층형 레이아웃 적용
        ▼
[GraphControl] ─── WinForms Panel에 렌더링 (GViewer)
        │
        ▼
[StatusBar] ─── 분석 결과 요약 / 에러 파일 목록
```

---

## 6. 제약 사항 및 가정

- `.csproj` 파일이 없어도 `.cs` 파일이 포함된 폴더면 분석 가능
- partial class는 하나의 클래스로 병합하여 처리
- generic 타입(`List<T>`)의 타입 인자는 의존성으로 추적하지 않음 (MVP)
- 분석은 정적 분석만 수행 (런타임 리플렉션 없음)
- 외부 NuGet 패키지 타입은 그래프에서 제외

---

## 7. Out of Scope (Sprint 1)

Sprint 1 MVP에서 **명시적으로 제외**하는 기능:

| 항목 | 설명 |
|------|------|
| 메서드 내부 Call Graph 분석 | 메서드 호출 관계 추적은 Sprint 2 이후 |
| Generic 타입 의존성 추적 | `List<T>`, `Dictionary<K,V>` 등 타입 인자 의존성 미추적 |
| Runtime Reflection 분석 | 정적 분석만 수행, 런타임 동작 분석 제외 |
| Multi-solution 분석 | 단일 폴더 단위 분석만 지원 |
| 외부 NuGet 패키지 타입 시각화 | 프로젝트 내부 타입만 노드로 표시 |
| struct / record / enum 분석 | Sprint 2 이후 분석 대상으로 추가 예정 |

---

## 8. 성공 기준 (Sprint 1)

| 기준 | 측정 방법 |
|------|----------|
| C# 폴더를 열면 3번의 클릭 이내에 그래프가 표시된다 | 직접 사용 테스트 |
| class/interface 노드가 그래프에 표시된다 | 샘플 프로젝트로 확인 |
| 상속 / 인터페이스 구현 엣지가 그래프에 표시된다 | 샘플 프로젝트로 확인 |
| 에러가 있는 파일이 포함된 폴더도 정상 파일은 분석된다 | 의도적으로 에러 파일 포함 후 테스트 |
| StatusBar에 분석 결과 요약이 표시된다 | 직접 확인 |

---

## 9. 검증 계획

### 9-1. 사용성 테스트 — 실행 결과

**테스트 목표**: 팀 내 개발자가 도움 없이 도구를 열고 그래프를 확인할 수 있는가

**테스트 환경**: Windows 10 Enterprise (21H2), 1920×1080 해상도, .NET 8.0.x

**시나리오별 실행 결과 (Sprint 3 완료 기준):**

| # | 시나리오 | 성공 기준 | 실행 결과 |
|---|---------|---------|---------|
| T-01 | 처음 앱을 실행한 사용자가 설명 없이 C# 폴더를 분석한다 | 3분 내 그래프 확인 | ✅ 클릭 2회(폴더 열기 → 폴더 선택)로 그래프 즉시 표시 |
| T-02 | 10개 클래스를 포함한 샘플 프로젝트를 분석한다 | 모든 클래스가 노드로 표시됨 | ✅ `_TestSample/` 폴더 분석 시 Animal/Cat/Dog/IAnimal 등 전원 표시 |
| T-03 | 구문 오류 파일이 포함된 폴더를 분석한다 | 앱이 크래시 없이 정상 파일만 분석 | ✅ Error Log 패널에 오류 파일명 표시, 나머지 정상 분석 |
| T-04 | 분석 결과를 보고 특정 클래스의 의존 관계를 파악한다 | 상속/인터페이스 엣지를 통해 관계 확인 | ✅ 노드 클릭 → 우측 Class Info + Dependency Metrics(Ca/Ce/Instability) 표시 |
| T-05 | 대규모 폴더(본 프로젝트 자체, 15+ 파일)를 분석한다 | UI 프리징 없이 분석 완료 | ✅ `Task.Run()` 비동기 처리로 UI 응답성 유지 |

**테스트 샘플 프로젝트:**
- 소규모 (~10 클래스): `CodeArchaeology/_TestSample/` — 저장소에 포함된 검증용 샘플
- 중규모 (15+ 파일): 본 프로젝트(`CodeArchaeology/`) 폴더 자체 분석으로 검증

**실행 가능 바이너리:**
- GitHub Releases: https://github.com/khwoo0527/code-archaeology-tool/releases/tag/v1.0.0
- CI 아티팩트: GitHub Actions `windows-latest` 빌드 — 매 push마다 자동 생성 (30일 보관)
- 빌드 방법: `dotnet run --project CodeArchaeology/CodeArchaeology.csproj`
- 스크린샷: `docs/screenshots/` 폴더 (main.png, graph.png)

### 9-2. 자동화 단위 테스트

**테스트 프로젝트**: `CodeArchaeology.Tests/` (xUnit 2.9.3 기반)
**저장소 경로**: https://github.com/khwoo0527/code-archaeology-tool/tree/master/CodeArchaeology.Tests

**테스트 케이스 목록 (총 21개, 전원 통과):**

| 파일 | 케이스 수 | 검증 대상 |
|------|----------|----------|
| `RoslynAnalyzerTests.cs` | 11개 | 클래스 노드 추출, 인터페이스 노드 추출, 상속 엣지, InterfaceImpl 엣지, 필드 의존성 엣지, partial class 병합, 에러 처리, 네임스페이스 추출 등 |
| `CycleDetectorTests.cs` | 6개 | 비순환(NoCycle), 직접 순환(A→B→A), 3노드 순환, 선형 체인, 혼합(순환+비순환), 빈 그래프 |
| `StructRecordEnumTests.cs` | 4개 | struct 파싱, record 파싱, enum 파싱, 전체 타입 혼합 |

**실제 테스트 코드 샘플** (`CodeArchaeology.Tests/RoslynAnalyzerTests.cs`):
```csharp
[Fact]
public void Analyze_SingleClass_ReturnsOneClassNode()
{
    var path = WriteTempFile("""
        namespace MyApp;
        public class Foo { }
        """);

    var result = _analyzer.Analyze([path]);

    Assert.Single(result.Nodes);
    Assert.Equal("Foo", result.Nodes[0].Name);
    Assert.Equal("MyApp", result.Nodes[0].Namespace);
    Assert.Equal(TypeKind.Class, result.Nodes[0].Kind);
}

[Fact]
public void Analyze_InheritanceEdge_ReturnsInheritanceEdge()
{
    var path = WriteTempFile("""
        public class Animal { }
        public class Dog : Animal { }
        """);

    var result = _analyzer.Analyze([path]);

    var edge = result.Edges.Single(e => e.Type == EdgeType.Inheritance);
    Assert.Equal("Dog", edge.Source);
    Assert.Equal("Animal", edge.Target);
}
```

**테스트 실행 방법:**
```bash
dotnet test CodeArchaeology.Tests --verbosity normal
# 출력 예시:
# 통과! - 실패: 0, 통과: 21, 건너뜀: 0, 전체: 21, 기간: 130ms
```

**현재 결과:** ✅ 21 / 21 통과

---

### 9-3. CI/CD 자동화 파이프라인

**파이프라인 파일**: `.github/workflows/ci.yml`

**트리거 조건**: `master` 브랜치 push 또는 Pull Request

**파이프라인 단계:**

| 단계 | 명령 | 설명 |
|------|------|------|
| Checkout | `actions/checkout@v4` | 소스 체크아웃 |
| Setup .NET 8 | `actions/setup-dotnet@v4` | .NET 8 SDK 설치 |
| Restore | `dotnet restore` | NuGet 패키지 복원 |
| Build | `dotnet build --configuration Release` | Release 빌드 |
| Test | `dotnet test CodeArchaeology.Tests` | **단위 테스트 21개 자동 실행** |
| Publish | `dotnet publish --runtime win-x64 --self-contained` | 단일 실행 파일 생성 |
| Upload Artifact | `actions/upload-artifact@v4` | 빌드 아티팩트 30일 보관 |

**CI 실행 결과 확인**: https://github.com/khwoo0527/code-archaeology-tool/actions/workflows/ci.yml

**현재 상태**: [![CI](https://github.com/khwoo0527/code-archaeology-tool/actions/workflows/ci.yml/badge.svg)](https://github.com/khwoo0527/code-archaeology-tool/actions/workflows/ci.yml) — 최근 커밋 전원 빌드·테스트 통과 (success)

---

### 9-4. 디자인 검토 프로세스

**UI 설계 원칙:**
- **3-클릭 룰**: 핵심 기능(폴더 열기 → 분석 → 그래프 확인)은 3번 클릭 이내
- **상태 가시성**: 분석 진행 상황은 항상 StatusBar에 표시
- **오류 명시**: 분석 실패 파일은 숨기지 않고 StatusBar에 카운트 표시

**디자인 검토 체크리스트:**

| 항목 | 기준 |
|------|------|
| 초기 진입 | 앱 실행 직후 "폴더 열기" 버튼이 즉시 눈에 띄는가 |
| 피드백 | 분석 중임을 사용자가 알 수 있는가 (StatusBar 상태 메시지) |
| 에러 처리 | 에러 발생 시 앱이 멈추지 않고 사용자에게 안내하는가 |
| 그래프 가독성 | 노드 라벨이 겹치지 않고 읽을 수 있는가 |
| 탐색성 | 줌/팬으로 그래프를 자유롭게 탐색할 수 있는가 |

**검토 시점:**
- Sprint 1 완료 직후: 팀원 1인 이상 T-01 시나리오 수행 후 피드백 수집
- Sprint 2 시작 전: 피드백 반영 여부 확인 및 다음 스프린트 UI 방향 결정

---

### 9-5. 반응형 레이아웃 및 해상도 호환성

**레이아웃 구조**: `SplitContainer` 중첩 3분할 — 창 크기 변경 시 자동 비율 조정

| 영역 | 구현 방식 | 동작 |
|------|----------|------|
| 좌측 사이드바 | `SplitContainer.Panel1` (고정 190px 최소) | 창 축소 시 MinSize 보장 |
| 중앙 그래프 | `GViewer` Dock=Fill | 남은 공간 전체 차지 |
| 우측 사이드바 | `SplitContainer.Panel2` (고정 230px 최소) | 창 축소 시 MinSize 보장 |

**해상도 테스트 결과:**

| 해상도 | 결과 | 비고 |
|--------|------|------|
| 1920×1080 (FHD) | ✅ 정상 | 기본 테스트 환경 |
| 1280×720 (HD) | ✅ 정상 | SplitContainer 자동 축소 |
| 1366×768 (노트북) | ✅ 정상 | 최소 창 크기(800×600) 이상 |

**Windows 호환성**: Windows 10 이상 (Windows 10 Enterprise 21H2 검증 완료).
.NET 8 Desktop Runtime 설치 필요 — 또는 self-contained 단일 실행 파일로 배포 가능.
