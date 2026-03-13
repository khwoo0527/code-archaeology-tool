# 프로젝트 로드맵 - Code Archaeology

## 개요
- **목표**: C# 프로젝트 폴더를 열면 Roslyn 기반으로 클래스 구조와 의존성을 자동 분석하고, Microsoft.Msagl 인터랙티브 그래프로 시각화하는 WinForms 데스크톱 도구
- **전체 예상 기간**: 4 스프린트 (8주, 스프린트당 2주)
- **현재 진행 단계**: Sprint 2 — UI 개편 및 인터랙션 기능
- **팀 규모 가정**: 소규모 팀 (2-4명)

## 진행 상태 범례
- 완료
- 진행 중
- 예정
- 보류

---

## 프로젝트 현황 대시보드

| 항목 | 상태 |
|------|------|
| 전체 진행률 | ~35% (Sprint 1 완료) |
| 현재 Sprint | **Sprint 2** — UI 개편 + 인터랙션 |
| Sprint 1 결과 | Core 12개 + Extension 7개 전원 완료 ✅ |
| 다음 마일스톤 | Sprint 2 MVP — 3분할 레이아웃 + 노드 클릭 Class Info |
| Sprint Velocity | Sprint 1: 19 태스크 / 1일 (해커톤) |

---

## 기술 아키텍처 결정 사항

| 영역 | 선택 | 이유 |
|------|------|------|
| UI 프레임워크 | WinForms (.NET 8) | WinForms on .NET 8, Windows 10+ 지원 |
| 코드 분석 | Microsoft.CodeAnalysis.CSharp (Roslyn) | C# 공식 컴파일러 플랫폼, 정확한 구문 분석 |
| 그래프 렌더링 | Microsoft.Msagl + GraphViewerGdi | WinForms 네이티브 지원, 계층형 레이아웃 내장 |
| 빌드 도구 | MSBuild / Visual Studio 2022 | .NET 8 표준 빌드 환경 |

**레이어 분리 원칙:**
```
UI Layer (WinForms)  -->  Analysis Layer (Roslyn)  -->  Model Layer (Graph)
  MainForm                  FolderScanner                GraphModel
  GraphControl              RoslynAnalyzer               Node / Edge
  StatusBar                 MsaglRenderer
```

---

## Sprint 1 — 해커톤 타임박스 계획 (6시간)

> **원칙**: 각 태스크는 **30분 안에 눈으로 결과를 확인**할 수 있는 크기.
> Core(S-01~S-12) 완료 후 시간이 남으면 Extension 순서로 진행.
> 태스크 완료 시 체크박스를 `[x]`로 업데이트하고 `docs/sprints/sprint-1.md`에 기록.

### 타임라인 개요

| 시간 | 태스크 | 내용 |
|------|-------|------|
| 0:00 – 0:30 | S-01 | 솔루션 생성 + NuGet 설치 |
| 0:30 – 1:00 | S-02 | 폴더 구조 + MainForm 레이아웃 |
| 1:00 – 1:20 | S-03 | GraphModel 정의 |
| 1:20 – 1:35 | S-04 | FolderScanner |
| 1:35 – 2:05 | S-05 | RoslynAnalyzer — class 노드 추출 |
| 2:05 – 2:20 | S-06 | RoslynAnalyzer — interface 노드 추출 |
| 2:20 – 2:50 | S-07 | RoslynAnalyzer — 상속/인터페이스 엣지 추출 |
| 2:50 – 3:05 | S-08 | 에러 핸들링 |
| 3:05 – 3:35 | S-09 | MsaglRenderer + GViewer 연결 (노드 표시) |
| 3:35 – 4:05 | S-10 | MsaglRenderer — 엣지 추가 |
| 4:05 – 4:35 | S-11 | 폴더 열기 파이프라인 전체 연결 |
| 4:35 – 4:50 | S-12 | StatusBar 연동 |
| 4:50 – 6:00 | Buffer | 삽질 여유 + Extension |

---

## [셋업] 프로젝트 기반 구성

### - [x] S-01. 솔루션 생성 + NuGet 설치 `~30분`

**구현:**
- Visual Studio 2022 → WinForms App (.NET 8) 프로젝트 생성
- 솔루션명: `CodeArchaeology`, 프로젝트명: `CodeArchaeology`
- NuGet 패키지 3종 설치:
  - `Microsoft.CodeAnalysis.CSharp`
  - `Microsoft.Msagl`
  - `Microsoft.Msagl.GraphViewerGdi`

**검증 (둘 다 확인):**
```
✅ Ctrl+Shift+B → 빌드 성공, 에러 0건
✅ F5 → 빈 WinForms 창이 뜬다
✅ NuGet 패키지 관리자 → 3종 설치 상태 확인
```

---

### - [x] S-02. 폴더 구조 + MainForm 레이아웃 `~30분`

**구현:**
- 프로젝트 내 폴더 생성: `Models/`, `Analysis/`, `Rendering/`
- MainForm 디자이너에서 레이아웃 구성:
  - `ToolStrip` (상단): 폴더 열기 버튼, 새로고침 버튼
  - `Panel` (중앙, Dock: Fill): 그래프 Canvas 영역
  - `StatusStrip` (하단): 분석 상태 레이블

**검증 (둘 다 확인):**
```
✅ F5 → Toolbar / 빈 Panel / StatusBar 3구역이 화면에 표시된다
✅ 창 크기를 조절해도 Panel이 채워진다 (Dock: Fill 확인)
```

---

## [분석 엔진] Roslyn 의존성 추출

### - [x] S-03. GraphModel 데이터 모델 정의 `~20분`

**구현** (`Models/` 폴더):
```csharp
// TypeNode.cs
class TypeNode { string Name, Namespace, FullName, FilePath; TypeKind Kind; int FieldCount, MethodCount; }

// DependencyEdge.cs
class DependencyEdge { string Source, Target; EdgeType Type; }
enum EdgeType { Inheritance, InterfaceImpl, FieldDependency }

// AnalysisResult.cs
class AnalysisResult { List<TypeNode> Nodes; List<DependencyEdge> Edges; List<string> Errors; }
```

**검증 (둘 다 확인):**
```
✅ Ctrl+Shift+B → 빌드 성공
✅ 세 클래스가 Models/ 폴더에 존재
```

---

### - [x] S-04. FolderScanner `~15분`

**구현** (`Analysis/FolderScanner.cs`):
```csharp
public static IEnumerable<string> GetCsFiles(string folderPath)
    => Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);
```

**검증 (둘 다 확인):**
```
✅ 디버거 브레이크포인트 → 샘플 폴더 경로 넣고 결과 목록 확인
✅ 하위 폴더의 .cs 파일도 수집되는지 확인
```

---

### - [x] S-05. RoslynAnalyzer — class 노드 추출 `~30분`

**구현** (`Analysis/RoslynAnalyzer.cs`):
- `CSharpSyntaxTree.ParseText()`로 각 .cs 파일 파싱
- `CSharpSyntaxWalker` 상속 → `VisitClassDeclaration()` 구현
- 클래스명, 네임스페이스 → `TypeNode(Kind=Class)` 생성

**검증 (둘 다 확인):**
```
✅ 디버거 → 샘플 .cs 파일 분석 후 Nodes 컬렉션에 class 이름 목록 출력 확인
✅ 네임스페이스가 올바르게 추출되는지 확인
```

---

### - [x] S-06. RoslynAnalyzer — interface 노드 추출 `~15분`

**구현:**
- `VisitInterfaceDeclaration()` 추가
- 인터페이스명, 네임스페이스 → `TypeNode(Kind=Interface)` 생성

**검증 (둘 다 확인):**
```
✅ 디버거 → IXxx 형태 인터페이스 이름이 Nodes에 포함되는지 확인
✅ class 노드와 interface 노드가 TypeKind로 구분되는지 확인
```

---

### - [x] S-07. RoslynAnalyzer — 상속 / 인터페이스 엣지 추출 `~30분`

**구현:**
- `VisitClassDeclaration()`에서 base type, interface 목록 파싱
- 분석된 타입 목록(`Nodes`)에 있는 타입만 엣지로 생성 (이름 매칭)
- `DependencyEdge(Type=Inheritance)` / `DependencyEdge(Type=InterfaceImpl)` 생성

> ⚠️ S-05/S-06 완료 후 타입 목록이 확정된 다음 필터링 적용

**검증 (둘 다 확인):**
```
✅ 디버거 → Edges 컬렉션에 Source-Target 쌍 출력 확인
✅ System.Object 같은 외부 타입이 엣지에 포함되지 않는지 확인
```

---

### - [x] S-08. 에러 핸들링 `~15분`

**구현:**
- 파일별 파싱을 try-catch로 감싸기
- 실패 시 `AnalysisResult.Errors`에 파일 경로 + 메시지 추가, 계속 진행

**검증 (둘 다 확인):**
```
✅ 의도적으로 문법 오류 .cs 파일을 샘플에 포함
✅ 앱이 크래시 없이 나머지 파일을 정상 분석하는지 확인
✅ Errors 리스트에 해당 파일 경로가 기록되는지 확인
```

---

## [시각화] Msagl 렌더링 + 파이프라인

### - [x] S-09. MsaglRenderer + GViewer 연결 (노드 표시) `~30분`

**구현** (`Rendering/MsaglRenderer.cs`):
- `AnalysisResult.Nodes` → `Microsoft.Msagl.Drawing.Graph`의 `AddNode()`
- 라벨: `ClassName`
- 계층형(Sugiyama) 레이아웃 설정
- MainForm: `GViewer`를 Panel에 동적 추가 (`Controls.Add(gViewer)`)
- `gViewer.Graph = graph`

**검증 (둘 다 확인):**
```
✅ 하드코딩된 샘플 AnalysisResult로 GViewer 실행
✅ 화면에 노드 박스(사각형)들이 표시된다
✅ 줌/팬이 마우스로 동작한다 (GViewer 내장)
```

---

### - [x] S-10. MsaglRenderer — 엣지 추가 `~30분`

**구현:**
- `AnalysisResult.Edges` → `graph.AddEdge(source, target)`
- 우선 기본 스타일(단색, 화살표)로 표시

**검증 (둘 다 확인):**
```
✅ 노드 간 화살표가 그려진다
✅ 방향(화살표 머리)이 올바른지 확인 (부모 → 자식 방향)
```

---

### - [x] S-11. 폴더 열기 파이프라인 전체 연결 `~30분`

**구현:**
- 폴더 열기 버튼 클릭 → `FolderBrowserDialog`
- 폴더 선택 → `FolderScanner` → `RoslynAnalyzer` → `MsaglRenderer` → `GViewer` 순서로 연결
- 새로고침 버튼: 마지막 선택 폴더 경로 저장 후 재분석

**검증 (둘 다 확인):**
```
✅ 폴더 열기 버튼 클릭 → 폴더 선택 → 그래프 자동 표시 (3클릭 이내)
✅ 새로고침 버튼 → 동일 그래프 다시 렌더링
```

---

### - [x] S-12. StatusBar 연동 `~15분`

**구현:**
- 분석 시작: `"분석 중..."`
- 분석 완료: `"분석 완료 (N개 클래스) | 에러: M개 파일"`

**검증 (둘 다 확인):**
```
✅ 폴더 선택 후 StatusBar 텍스트가 업데이트된다
✅ 에러 파일이 있을 때 에러 카운트가 표시된다
```

---

## [Extension] 시간이 남으면

> Core(S-01~S-12) 완료 후 남은 시간에 순서대로 진행

- [x] **S-EX-01. 엣지 색상/스타일 구분** — 상속:검정실선 / 인터페이스:파랑점선 / 필드:회색실선 ⭐ UX 평가 대응 (S-10에서 선구현)
- [x] **S-EX-02. 필드 타입 의존성 추출** — `VisitFieldDeclaration()` 추가
- [x] **S-EX-03. 노드 라벨 네임스페이스 표시** — `Namespace.ClassName` 형식
- [x] **S-EX-04. partial class 병합** — 동일 FullName 노드 합산
- [x] **S-EX-05. 비동기 처리** — `Task.Run()`으로 UI 프리징 방지
- [x] **S-EX-06. 노드 색상 디자인** — 클래스/인터페이스 배경색 구분, 폰트 가독성 개선 ⭐ UX 평가 대응
- [x] **S-EX-07. 그래프 레이아웃 튜닝** — 계층형(Sugiyama) 레이아웃 명시 적용, 노드 간격 조정 ⭐ UX 평가 대응 (TB 방향 + 범례 패널 추가)

---

## Sprint 1 MVP 완료 기준

| 기준 | 검증 방법 |
|------|----------|
| C# 폴더를 열면 3클릭 이내에 그래프가 표시된다 | S-11 직접 실행 |
| class/interface 노드가 그래프에 표시된다 | S-09/10 샘플 확인 |
| 상속/인터페이스 엣지가 그래프에 표시된다 | S-10 샘플 확인 |
| 에러 파일이 있어도 정상 파일은 분석된다 | S-08 검증 시나리오 |
| StatusBar에 분석 결과 요약이 표시된다 | S-12 직접 확인 |

---

## Phase 4: [원래 계획] 노드 인터랙션 및 고급 분석

> ⚠️ **Sprint 1 회고 후 계획 변경됨.** 아래는 Sprint 2로 진입하기 전 원래 로드맵 원문 보존.
> 변경 사유는 바로 아래 Sprint 2 섹션 도입부에 기술.
> 원래 계획의 일부 항목(호버 툴팁, 검색, 포커스 모드, 순환 감지 등)은 Sprint 2 / Sprint 3으로 분산 이월됨.

### 원래 목표
Sprint 1에서 미뤄진 노드 인터랙션 기능을 구현하고, 메서드 호출 의존성·순환 감지·코드 스멜 지표를 추가한다.

### 원래 작업 목록

- [ ] **P4-01. 노드 호버 툴팁** — GViewer `ObjectUnderMouseCursor` 이벤트 활용, 네임스페이스/필드수/메서드수/파일 경로 표시 → **Sprint 2 S2-06으로 이월**
- [ ] **P4-02. 노드 클릭 포커스 모드** — 클릭 노드 + 1-hop 이웃 강조, 나머지 dimming, 빈 영역 클릭 시 해제 → **Sprint 2 S2-EX-01으로 이월**
- [ ] **P4-03. 검색/필터링 기능** — Toolbar 검색 TextBox, 클래스 이름 입력 시 매칭 노드 하이라이트 → **Sprint 2 S2-05로 이월**
- [ ] **P4-04. 에러 상세 표시** — StatusBar 에러 영역 클릭 시 상세 창 표시 → **Sprint 2 S2-03으로 이월**
- [ ] **P4-05. 메서드 호출 의존성 분석** — `VisitInvocationExpression()`, MethodCall EdgeType, 주황색 점선 → **Sprint 3 이월**
- [ ] **P4-06. 순환 의존성 감지** — DFS/Tarjan 사이클 탐지, 빨간색 엣지, 경고 메시지 → **Sprint 2 S2-EX-02로 이월**
- [ ] **P4-07. 코드 스멜 지표 시각화** — Afferent/Efferent Coupling, Instability 지표, 노드 크기/색상 인코딩 → **Sprint 3 이월**
- [ ] **P4-08. struct / record / enum 지원** — TypeKind 확장, 노드 모양 구분 → **Sprint 2 S2-EX-03으로 이월**

---

## Sprint 2 — [재편] UI 개편 및 인터랙션 기능

> **Phase 4 → Sprint 2 전환 결정 (2026-03-13):**
> Sprint 1 완료 후 레퍼런스 디자인(`UI느낌.png`)을 검토한 결과, 단일 그래프 패널 위에 기능을 올리는 방식(Phase 4 원래 계획)은 구조적으로 한계가 명확했다.
> 노드 클릭 정보 패널, 네임스페이스 필터, 에러 로그 등 인터랙션 기능은 전용 사이드바 없이는 UI가 감당하기 어렵다는 판단 하에, **3분할 레이아웃 확립을 Sprint 2의 선결 과제**로 재편.
> Phase 4의 고급 분석 항목(메서드 호출, 코드 스멜 지표)은 Sprint 3으로 순연.
> 자세한 진행 기록은 [`docs/sprints/sprint-2.md`](./docs/sprints/sprint-2.md) 참조.

### Core 태스크 (S2-01 ~ S2-06)

- [x] **S2-01. UI 3분할 레이아웃** — 좌측 사이드바(190px) + 중앙 그래프 + 우측 사이드바(230px)
- [ ] **S2-02. 좌측 패널 — Namespace Filter** — 발견된 네임스페이스 체크박스 목록, 체크 변경 시 그래프 즉시 재구성
- [ ] **S2-03. 좌측 패널 — Error Log** — 분석 오류 파일 목록 상세 표시 (기존 StatusBar 단순 카운트 → 전용 패널)
- [ ] **S2-04. 우측 패널 — Class Info** — 노드 클릭 시 Name/Namespace/File/Fields/Methods 상세 정보 표시
- [ ] **S2-05. 툴바 Search** — 클래스 이름 입력 시 매칭 노드 하이라이트 + 비매칭 노드 dimming
- [ ] **S2-06. 노드 호버 툴팁** — GViewer `ObjectUnderMouseCursor` 활용, 풍부한 메타데이터 표시

### Extension 태스크 (S2-EX)

- [ ] **S2-EX-01. 노드 클릭 포커스 모드** — 클릭 노드 + 1-hop 이웃 강조, 나머지 dimming, 빈 영역 클릭 시 해제
- [ ] **S2-EX-02. 순환 의존성 감지** — DFS 기반 사이클 탐지, 순환 엣지 빨간색 강조, StatusBar 경고
- [ ] **S2-EX-03. struct / record / enum 지원** — TypeKind 확장, 다이아몬드/오각형 등 노드 모양 구분
- [ ] **S2-EX-04. Export PNG** — GViewer 렌더링 결과 Bitmap 캡처 후 SaveFileDialog로 저장

### 완료 기준 (Definition of Done)
- 3분할 레이아웃이 창 크기 변경에도 안정적으로 동작한다
- 네임스페이스 체크박스 변경 시 그래프가 즉시 갱신된다
- 노드 클릭 시 우측 패널에 해당 클래스의 상세 정보가 표시된다
- 클래스 검색 시 매칭/비매칭 노드가 시각적으로 구분된다
- 노드 호버 시 툴팁이 표시된다

### 기술 고려사항
- GViewer는 DockStyle.Fill로 pnlGraph 내부에 삽입되므로 외부 패널 레이아웃과 분리 용이
- 네임스페이스 필터 변경 시 `AnalysisResult`를 필터링한 새 객체를 MsaglRenderer에 전달하여 그래프 재빌드
- `ObjectUnderMouseCursor` 반환 타입이 `object`이므로 `IViewerNode` 또는 `DrawingNode` 캐스팅 필요 — 빌드 시 확인
- 순환 감지는 SCC(Tarjan's Algorithm) 또는 DFS 색상 마킹 방식 중 선택

---

## Phase 5: 사용성 개선 기능 (Sprint 3)

### 목표
네임스페이스 필터링, 변경 영향 분석 등 중간 우선순위 기능을 추가하여 실사용 편의성을 높인다.

### 작업 목록

- [ ] **P5-01. 네임스페이스 필터링**
  - 사이드 패널 또는 드롭다운에 네임스페이스 목록 표시 (체크박스)
  - 선택한 네임스페이스에 해당하는 노드만 그래프에 표시
  - 필터 변경 시 실시간 그래프 업데이트

- [ ] **P5-02. 변경 영향 분석**
  - 특정 클래스 노드 우클릭 -> "영향 분석" 컨텍스트 메뉴
  - 선택한 클래스를 직간접적으로 참조하는 모든 클래스를 하이라이트 (N-hop 전파)
  - 영향 범위를 숫자로 StatusBar에 표시

- [ ] **P5-03. 코드 품질 리팩토링**
  - Phase 1-4에서 발생한 기술 부채 정리
  - 단위 테스트 추가 (RoslynAnalyzer 핵심 로직)
  - 코드 리뷰 반영 사항 처리

### 완료 기준 (Definition of Done)
- 네임스페이스 필터로 특정 네임스페이스의 노드만 표시/숨기기가 동작한다
- 변경 영향 분석 시 직간접 참조 클래스가 하이라이트된다
- 핵심 분석 로직에 대한 단위 테스트가 존재하고 통과한다

### 검증 시나리오
```
1. 다수 네임스페이스가 포함된 프로젝트 분석 -> 네임스페이스 필터 체크박스 확인
2. 특정 네임스페이스만 선택 -> 해당 노드만 그래프에 표시 확인
3. 클래스 우클릭 -> 영향 분석 -> 관련 클래스 하이라이트 확인
4. 단위 테스트 전체 실행 -> 통과 확인
```

### 기술 고려사항
- 네임스페이스 필터는 Graph를 다시 생성하지 않고 노드 Visible 속성 조절로 구현 가능한지 Msagl API 확인 필요
- 영향 분석의 N-hop 전파는 BFS 알고리즘으로 구현
- 리팩토링은 스프린트 capacity의 20% 이내로 배분

---

## Phase 6: 내보내기 및 마무리 (Sprint 4)

### 목표
PNG/SVG 내보내기 기능을 추가하고, 전체 기능의 안정성을 확보하며 릴리스를 준비한다.

### 작업 목록

- [ ] **P6-01. PNG/SVG 내보내기**
  - Toolbar에 "내보내기" 버튼 추가
  - 현재 그래프를 PNG 이미지로 저장 (GViewer의 렌더링 결과를 Bitmap으로 캡처)
  - SVG 내보내기 (Msagl의 SvgGraphWriter 활용 가능 여부 확인)
  - SaveFileDialog로 저장 경로 선택

- [ ] **P6-02. 전체 회귀 테스트**
  - Sprint 1-3 전체 기능 회귀 테스트
  - 다양한 규모의 C# 프로젝트로 스트레스 테스트
  - 에러 핸들링 경계값 테스트

- [ ] **P6-03. 사용자 경험 개선**
  - 분석 중 프로그레스 바 표시
  - 최근 열었던 폴더 기록 (Recent Folders)
  - 기본 키보드 단축키 (Ctrl+O: 폴더 열기, F5: 새로고침, Ctrl+F: 검색 포커스)

### 완료 기준 (Definition of Done)
- PNG 내보내기가 정상 동작하고 이미지 품질이 양호하다
- 전체 기능이 회귀 없이 동작한다
- 프로그레스 바와 최근 폴더 기능이 동작한다

### 검증 시나리오
```
1. 그래프 표시 상태에서 내보내기 -> PNG 파일 저장 및 이미지 확인
2. 앱 재시작 -> 최근 폴더 목록에서 이전 폴더 선택 -> 분석 실행 확인
3. Ctrl+O -> 폴더 열기 대화상자 표시 확인
4. 50개+ 클래스 프로젝트 분석 시 프로그레스 바 표시 확인
5. 전체 기능 시나리오 통합 테스트 수행
```

### 기술 고려사항
- PNG 내보내기는 GViewer 컨트롤의 `DrawToBitmap()` 또는 Msagl 자체 렌더링 API 활용
- 최근 폴더 기록은 `Properties.Settings` 또는 로컬 JSON 파일에 저장
- SVG 내보내기가 Msagl에서 직접 지원되지 않으면 Won't Have로 전환 가능

---

## 의존성 맵

```
Phase 1 (프로젝트 셋업)
   |
   v
Phase 2 (분석 엔진)  <-- Phase 1의 프로젝트 구조 및 NuGet 필요
   |
   v
Phase 3 (그래프 시각화)  <-- Phase 2의 AnalysisResult 필요
   |
   |--- Sprint 1 MVP 완료 ---
   |
   v
Phase 4 (메서드/순환/코드스멜)  <-- Phase 2의 RoslynAnalyzer 확장
   |
   v
Phase 5 (네임스페이스 필터/영향 분석)  <-- Phase 3의 그래프 + Phase 4의 지표
   |
   v
Phase 6 (내보내기/마무리)  <-- 전체 기능 안정화 후
```

---

## 리스크 및 완화 전략

| 리스크 | 영향도 | 발생 가능성 | 완화 전략 |
|--------|--------|------------|----------|
| Msagl GViewer 이벤트 모델이 호버/클릭 포커스 모드에 부적합 | High | Medium | Phase 1에서 GViewer 프로토타이핑으로 조기 검증. 대안: 직접 GDI+ 렌더링 |
| Roslyn SyntaxTree만으로 필드 타입 매칭 정확도 부족 | Medium | High | 타입 이름 + using 문 조합 휴리스틱 적용. 정확도 이슈 발생 시 SemanticModel 부분 도입 |
| 대규모 프로젝트(100+ 클래스)에서 그래프 가독성 저하 | Medium | High | 네임스페이스 필터링(Phase 5)으로 해결. 노드 축소/그룹화 기능 백로그 추가 |
| Microsoft.Msagl NuGet 패키지 버전 호환성 문제 | Medium | Low | Phase 1에서 즉시 확인. 호환 버전 조합 문서화 |
| Msagl NuGet 패키지 .NET 8 호환성 | Low | Low | S-01에서 즉시 빌드 확인. 호환 버전 조합 문서화 |

---

## 마일스톤

| 마일스톤 | 목표 시점 | 핵심 산출물 |
|---------|----------|------------|
| M1: Sprint 1 MVP | Week 2 완료 | 폴더 열기 -> 그래프 표시 + StatusBar 분석 결과 표시 실행 파일 |
| M2: 고급 분석 | Week 4 완료 | 메서드 호출, 순환 감지, 코드 스멜, struct/record/enum 지원 |
| M3: 사용성 개선 | Week 6 완료 | 네임스페이스 필터, 변경 영향 분석, 단위 테스트 |
| M4: v1.0 릴리스 | Week 8 완료 | PNG 내보내기, 전체 안정화, 키보드 단축키, 최근 폴더 |

---

## 향후 계획 (Backlog)

PRD에 명시되었거나 개발 과정에서 도출될 수 있는 향후 기능:

| 우선순위 | 기능 | 비고 |
|---------|------|------|
| Could Have | Generic 타입 의존성 추적 | `List<T>`, `Dictionary<K,V>` 등 타입 인자 의존성 |
| Could Have | Multi-solution 분석 | 여러 솔루션 폴더를 동시에 분석 |
| Could Have | 노드 그룹화 (네임스페이스별) | 대규모 프로젝트 가독성 향상 |
| Won't Have (v1) | Runtime Reflection 분석 | 정적 분석 범위를 벗어남 |
| Won't Have (v1) | 외부 NuGet 패키지 타입 시각화 | 그래프 복잡도 과도 증가 |
| Could Have | .NET AOT 게시 | 단일 실행 파일로 배포 시 런타임 설치 불필요 |

---

## 품질 인프라 — 중간 평가 대응 (2026-03-13 추가)

> **배경**: 중간 평가 결과 검증 계획 항목이 4/15점으로 가장 낮은 점수를 기록.
> 평가 피드백: "자동화된 단위/통합 테스트 코드 없음", "CI/CD 자동화 전무".
> Sprint 기능 구현과 병행하여 신뢰성 인프라를 즉시 구축하기로 결정.

- [x] **QI-01. GitHub Actions CI 파이프라인** — `master` push/PR 시 자동 빌드 검증 (`.github/workflows/ci.yml`)
- [x] **QI-02. xUnit 단위 테스트 프로젝트** — `CodeArchaeology.Tests` 신설, RoslynAnalyzer 핵심 로직 11개 테스트 케이스 전원 통과
- [x] **QI-03. CI 테스트 연동** — `dotnet test` CI 파이프라인 통합 완료, `continue-on-error` 제거

---

## 기술 부채 관리

| 항목 | 발생 예상 Phase | 해결 예정 Phase |
|------|----------------|----------------|
| 필드 타입 매칭 휴리스틱 (SemanticModel 미사용) | Phase 2 | Phase 4 검토 |
| 단위 테스트 부재 | Phase 1-3 | ✅ QI-02 완료 (2026-03-13) |
| 하드코딩된 색상/스타일 값 | Phase 3 | Phase 5 |
| 대규모 프로젝트 성능 최적화 | Phase 4 | Phase 6 |

---

## 변경 이력 (Change Log)

> 계획 대비 실제 진행 과정에서 발생한 범위 변경, 설계 결정 전환, 우선순위 조정을 시계열로 기록한다.
> 코드 레벨의 기술 결정은 `docs/sprints/sprint-N.md`의 이슈/결정 항목에 상세 서술한다.

| 날짜 | 분류 | 항목 | 변경 내용 | 사유 |
|------|------|------|-----------|------|
| 2026-03-13 | 범위 확장 | S-EX-01 선구현 | S-EX-01(엣지 색상 구분)을 별도 태스크 없이 S-10 구현 중 함께 처리 | S-10에서 엣지 스타일을 함께 다루는 것이 자연스러워 즉시 구현. 별도 태스크 비용 절감 |
| 2026-03-13 | 범위 확장 | S-EX-07 추가 | 계획에 없던 Sugiyama TB 레이아웃 전환 + 범례 패널을 Extension에 추가 | 사용자 피드백: 기본 LR 레이아웃으로 클래스 다수 시 가로 팽창·위아래 공백 현상 발생 |
| 2026-03-13 | 다크 테마 | 다크 UI 적용 | DarkToolStripRenderer + 파란 StatusBar + 노드 배경색 적용 | 사용자 요청: "2026년도에 이 UI는 너무 오래된 느낌" — 모던 개발자 도구 감성으로 전환 |
| 2026-03-13 | 스프린트 재편 | Phase 4 → Sprint 2 재구성 | Phase 4의 노드 인터랙션 항목들을 UI 개편 중심으로 재우선순위 지정 | 사용자 피드백: 레퍼런스 UI(`UI느낌.png`) 기준으로 3분할 레이아웃이 선행되어야 나머지 인터랙션 기능이 의미 있음. UI 구조 없이 기능만 추가하는 방식은 사용성 측면에서 부적절하다고 판단 |
| 2026-03-13 | 품질 인프라 | CI/CD + 테스트 완료 | GitHub Actions CI 구축(QI-01) + xUnit 11개 테스트 전원 통과(QI-02) + CI 연동(QI-03) 당일 완료 | 중간 평가 피드백: 검증 계획 4/15점 — "CI/CD 전무, 자동화 테스트 없음" 직접 지적. 재평가 대응을 위해 Sprint 2와 병행하여 즉시 추진 결정 |
