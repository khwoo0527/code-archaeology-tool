# Sprint 1 진행 기록

> **목표**: C# 폴더를 열면 기본 그래프가 표시되는 상태 (해커톤 6시간 타임박스)
> **시작일**: 2026-03-13
> **완료 기준**: ROADMAP.md Sprint 1 MVP 완료 기준 참조

---

## 진행 상태

| 태스크 | 상태 | 완료 시각 | 메모 |
|--------|------|-----------|------|
| S-01. 솔루션 생성 + NuGet 설치 | ✅ 완료 | 2026-03-13 | |
| S-02. 폴더 구조 + MainForm 레이아웃 | ✅ 완료 | 2026-03-13 | |
| S-03. GraphModel 데이터 모델 정의 | ✅ 완료 | 2026-03-13 | |
| S-04. FolderScanner | ✅ 완료 | 2026-03-13 | |
| S-05. RoslynAnalyzer — class 노드 추출 | ✅ 완료 | 2026-03-13 | |
| S-06. RoslynAnalyzer — interface 노드 추출 | ✅ 완료 | 2026-03-13 | |
| S-07. RoslynAnalyzer — 상속/인터페이스 엣지 추출 | ✅ 완료 | 2026-03-13 | |
| S-08. 에러 핸들링 | ✅ 완료 | 2026-03-13 | |
| S-09. MsaglRenderer + GViewer 연결 (노드 표시) | ✅ 완료 | 2026-03-13 | |
| S-10. MsaglRenderer — 엣지 추가 | ✅ 완료 | 2026-03-13 | |
| S-11. 폴더 열기 파이프라인 전체 연결 | ✅ 완료 | 2026-03-13 | |
| S-12. StatusBar 연동 | ✅ 완료 | 2026-03-13 | |

---

## 진행 로그

### [S-01] 솔루션 생성 + NuGet 설치
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: 빌드 성공, 오류 0 / NuGet 3종 설치 확인 (CSharp 5.3.0, Msagl 1.1.6, GraphViewerGDI 1.1.7)

---

### [S-02] 폴더 구조 + MainForm 레이아웃
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: UI/Models/Analysis/Rendering 폴더 생성. ToolStrip(폴더열기/새로고침) + Panel(Dock:Fill) + StatusStrip 3구역 레이아웃 구성 완료. F5 실행 확인.

---

### [S-03] GraphModel 데이터 모델 정의
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: TypeNode(TypeKind: Class/Interface) / DependencyEdge(EdgeType: Inheritance/InterfaceImpl/FieldDependency) / AnalysisResult(Nodes+Edges+Errors) 3종 모델 정의. 빌드 오류 0건 확인.

---

### [S-04] FolderScanner
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: `.cs` 파일 재귀 수집 구현. StatusBar에 "발견된 .cs 파일: N개" 표시로 UI에서 즉시 확인. 하위 폴더 포함 수집 검증 완료.

---

### [S-05] RoslynAnalyzer — class 노드 추출
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: CSharpSyntaxWalker 기반 class 노드 추출 구현. 네임스페이스(일반/파일범위) 양쪽 지원. bin/obj 폴더 자동 제외 처리. StatusBar에서 "발견된 클래스: 9개 | .cs 파일: 8개 | 에러: 0개" UI 확인.
- **이슈/결정**: obj/ 내 빌드 생성 .cs 파일이 분석에 포함되는 문제 발견 → FolderScanner에 bin/obj 제외 필터 추가로 해결.

---

### [S-06] RoslynAnalyzer — interface 노드 추출
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: VisitInterfaceDeclaration() 추가. 샘플 파일(IAnimal, IMovable, Dog)로 인터페이스 2개/클래스 1개 정확히 추출 확인. TypeKind로 class/interface 구분 동작 검증.

---

### [S-07] RoslynAnalyzer — 상속/인터페이스 엣지 추출
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: 2단계 분석(노드 수집 → 엣지 추출) 구조로 구현. 내부 타입끼리만 엣지 생성(외부 타입 자동 필터). Dog→Animal(Inheritance) / Dog→IAnimal(InterfaceImpl) / Cat→Animal(Inheritance) 3개 관계 UI 확인.

---

### [S-08] 에러 핸들링
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: Roslyn Diagnostics 기반 문법 오류 감지 구현. 오류 파일도 파싱 가능한 범위까지 분석 후 Errors 리스트에 기록. 크래시 없이 정상 파일 계속 분석 확인. _TestSample 빌드 제외 처리.

---

### [S-09] MsaglRenderer + GViewer 연결
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: MsaglRenderer 구현. 노드 사각형(클래스)/타원(인터페이스) 구분. GViewer Panel 동적 삽입. 줌/팬 동작 확인.
- **이슈/결정**: 디자인(노드 색상, 엣지 스타일, 레이아웃 등)은 S-EX-01 및 Sprint 2에서 완성 예정 (평가기준 UX 항목 대응)

---

### [S-10] MsaglRenderer — 엣지 추가
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: 상속(검정실선) / 인터페이스(파랑점선) / 필드(회색실선) 엣지 색상·스타일 구분 구현. Dog→Animal, Dog→IAnimal, Cat→Animal 3개 엣지 UI 확인.
- **이슈/결정**: Msagl.Drawing.Color와 System.Drawing.Color 네임스페이스 충돌 → 전체 경로 명시로 해결.

---

### [S-11] 폴더 열기 파이프라인 전체 연결
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: FolderScanner → RoslynAnalyzer → MsaglRenderer → GViewer 파이프라인 완전 연결. 3클릭 이내 그래프 표시 달성. 새로고침 재분석, 대기 커서, 에러 카운트 표시 구현.

---

### [S-12] StatusBar 연동
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: 분석 시작 시 "분석 중..." 표시, 완료 시 "클래스: N개 | 인터페이스: M개 | 엣지: K개 | 에러: L개 파일" 형식으로 StatusBar 업데이트. 분석 중 대기 커서(Cursor.WaitCursor) 처리. 새로고침 포함 모든 분석 시나리오에서 정상 동작 확인.

---

## Extension 진행 (시간 여유 시)

| 태스크 | 상태 | 메모 |
|--------|------|------|
| S-EX-01. 엣지 색상/스타일 구분 | ✅ 완료 | S-10에서 구현됨 (MsaglRenderer.cs:35-47) |
| S-EX-02. 필드 타입 의존성 추출 | ✅ 완료 | VisitFieldDeclaration() 추가 |
| S-EX-07. 그래프 레이아웃 튜닝 + 범례 패널 | ✅ 완료 | 사용자 피드백 반영 — TB 레이아웃, 우상단 범례 오버레이 |
| S-EX-03. 노드 라벨 네임스페이스 표시 | ✅ 완료 | |
| S-EX-04. partial class 병합 | ✅ 완료 | |
| S-EX-05. 비동기 처리 | ✅ 완료 | |
| S-EX-06. 노드 색상 디자인 | ✅ 완료 | 클래스 연파랑·인터페이스 연보라, 범례 패널 동기화 |

---

### [S-EX-01] 엣지 색상/스타일 구분
- **상태**: ✅ 완료 (S-10에서 선구현, 2026-03-13)
- **결과**: S-10 구현 시 엣지 색상/스타일 구분이 함께 적용됨. 상속(검정실선) / 인터페이스(파랑점선 `DashStyle.Dash`) / 필드(회색실선) 3종 구분 렌더링. Msagl.Drawing.Color와 System.Drawing.Color 네임스페이스 충돌 → 전체 경로 명시로 해결.
- **이슈/결정**: S-09 계획 시 "S-EX-01에서 완성 예정"으로 기록했으나, S-10 구현 중 함께 처리. 별도 태스크 없이 완료.

---

### [S-EX-02] 필드 타입 의존성 추출
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: `VisitFieldDeclaration()` + `ExtractTypeName()` 추가로 필드 선언에서 타입 의존성 추출 구현. `_currentClassName` 컨텍스트 추적. 5종 타입 패턴 처리: Simple/Generic/Qualified/Nullable/Array. `PredefinedTypeSyntax`(int, string 등) 스킵. `GetEdges()`에 내부 타입 필터·자기 참조 제외·중복 제거 적용. `Sample.cs`에 `Dog._friend: Cat` 케이스 추가 → Dog→Cat 회색실선 엣지 UI 확인.
- **이슈/결정**: `GenericNameSyntax`가 `SimpleNameSyntax` 서브타입이라 switch expression 순서 오류 발생 → `GenericNameSyntax` 먼저 처리하도록 수정.

---

### [S-EX-07] 그래프 레이아웃 튜닝 + 범례 패널
- **상태**: ✅ 완료 (2026-03-13, 사용자 피드백 반영)
- **결과**:
  - **레이아웃 방향 수정**: 기본 LR(좌→우) 배치 → TB(위→아래) 계층형으로 변경. `SugiyamaLayoutSettings`에 90도 회전 변환 행렬 `(0,-1,0,1,0,0)` 적용. 노드 간격 `NodeSeparation=20`, 레이어 간격 `LayerSeparation=40` 설정. 상속 계층이 위→아래로 자연스럽게 흐름.
  - **범례 패널**: 그래프 우상단 오버레이 Panel 추가. 엣지 3종(상속/인터페이스 구현/필드 의존성)을 실제 선 색상·스타일로 렌더링하여 표시. `Controls.Clear()` 후 재추가 + `BringToFront()`로 GViewer 위에 항상 표시. `Anchor = Top|Right`로 창 크기 변경 시에도 우상단 고정.
- **이슈/결정**: `PlaneTransformation.Rotation90DegreesClockwise` 정적 속성이 MSAGL 1.1.6에 없음 → 직접 변환 행렬로 대체.

---

### [S-EX-03] 노드 라벨 네임스페이스 표시
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: 노드 라벨을 기존 `ClassName` 단독 표기에서 `Namespace.ClassName` 전체 경로 형식으로 전환. `TypeNode.FullName` 프로퍼티(네임스페이스 없을 시 `Name` 단독 반환, 있을 시 `Namespace.Name` 반환)가 이미 구현되어 있어 `MsaglRenderer.cs`의 `LabelText` 할당 한 줄 변경으로 완료. 동일 클래스명이 여러 네임스페이스에 걸쳐 존재하는 대규모 프로젝트 분석 시 노드 식별 모호성이 제거됨.

---

### [S-EX-04] partial class 병합
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: `partial class`가 여러 파일에 걸쳐 선언된 경우 파일별로 독립 노드가 생성되어 그래프에 동일 클래스가 중복 표시되던 문제를 해결. `Analyze()` 1단계(타입 수집) 완료 직후 `FullName` 기준 `GroupBy`로 동일 타입을 병합하고 `FieldCount`·`MethodCount`는 파일별 선언 합산으로 처리. 엣지 추출(기존 2단계)은 병합 후의 통합 노드 목록을 기준으로 수행하도록 단계 순서 조정. 검증용으로 `SamplePartial.cs`에 `partial class Dog { Fetch() }` 추가 — Sample.cs의 Dog(Speak)와 병합되어 단일 노드로 표시됨을 UI에서 확인.
- **이슈/결정**: 병합 후 `FilePath`는 첫 번째 발견 파일 경로를 대표값으로 채택. 향후 툴팁 기능(P4-01) 구현 시 모든 partial 파일 경로 목록을 별도 보관하는 방향으로 확장 검토.

---

### [S-EX-05] 비동기 처리
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: `RunAnalysis()` → `RunAnalysisAsync()` 전환. `FolderScanner` + `RoslynAnalyzer` 등 CPU 바운드 분석 작업 전체를 `Task.Run()`으로 백그라운드 스레드에 위임하여 대규모 프로젝트 분석 중 UI 스레드 블로킹 및 창 프리징 현상을 원천 차단. GViewer 생성 및 Controls 조작 등 UI 반영 로직은 `await` 복귀 후 UI 스레드에서 수행되어 스레드 안전성 보장. 이벤트 핸들러(`btnOpenFolder_Click`, `btnRefresh_Click`)는 `async void` 패턴 대신 `_ = RunAnalysisAsync()` 형태로 호출하여 fire-and-forget 의도를 명시적으로 표현. 예외 처리도 `catch`로 포괄하여 백그라운드 예외가 조용히 묻히지 않도록 처리.
- **이슈/결정**: `async void` 이벤트 핸들러는 예외가 전파되지 않는 위험이 있어 `Task` 반환 메서드를 별도로 분리하고 호출부에서 명시적 무시(`_`) 처리.

---

### [S-EX-06] 노드 색상 디자인
- **상태**: ✅ 완료 (2026-03-13)
- **결과**: 클래스(사각형)와 인터페이스(타원) 노드에 배경색·테두리색을 도입하여 타입 종류를 색상으로 즉시 식별 가능하도록 개선. 클래스는 연파랑 배경(`RGB 210,230,255`) + 네이비 테두리(`RGB 30,80,160`)로 안정적이고 신뢰감 있는 인상을 표현. 인터페이스는 연보라 배경(`RGB 230,210,255`) + 진보라 테두리(`RGB 120,60,180`)로 추상적 계약(contract) 성격을 직관적으로 전달. 범례 패널도 실제 노드 색상과 완전히 동기화하여 — 텍스트 설명 대신 색칠된 도형(사각형/타원)으로 표현 — 시각적 일관성을 확보. 범례 패널 높이 100→140px 확장.

---

## 이슈 및 결정 사항

> 구현 중 발생한 이슈, 기술적 결정, 트레이드오프를 여기에 기록

---

## Sprint 1 완료 기준 체크

| 기준 | 결과 |
|------|------|
| C# 폴더를 열면 3클릭 이내에 그래프가 표시된다 | ✅ S-11에서 달성. 폴더 선택 즉시 그래프 표시 확인 |
| class/interface 노드가 그래프에 표시된다 | ✅ S-09/10에서 달성. 사각형(클래스)/타원(인터페이스) 구분 표시 |
| 상속/인터페이스 엣지가 그래프에 표시된다 | ✅ S-10에서 달성. 상속(검정실선)/인터페이스(파랑점선) 구분 |
| 에러 파일이 있어도 정상 파일은 분석된다 | ✅ S-08에서 달성. 에러 파일은 Errors 리스트 기록, 분석 계속 |
| StatusBar에 분석 결과 요약이 표시된다 | ✅ S-12에서 달성. 클래스/인터페이스/엣지/에러 카운트 표시 |
