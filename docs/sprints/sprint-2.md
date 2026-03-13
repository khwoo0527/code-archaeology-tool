# Sprint 2 진행 기록

> **목표**: 3분할 레이아웃 기반의 인터랙티브 UI 완성 — 노드 클릭 정보 패널, 네임스페이스 필터, 클래스 검색
> **시작일**: 2026-03-13

## Sprint 2 계획 배경 — Phase 4에서 재편된 이유

Sprint 1 완료 시점까지 원래 로드맵은 **Phase 4: 노드 인터랙션 및 고급 분석**을 Sprint 2로 진행할 예정이었다.
Phase 4의 내용은 툴팁, 포커스 모드, 검색, 메서드 호출 그래프, 순환 감지, 코드 스멜 지표 등 분석 기능 중심이었다.

그러나 Sprint 1 결과물을 레퍼런스 디자인(`UI느낌.png`)과 비교한 결과 다음 문제가 드러났다:

- 인터랙션 기능(노드 클릭 상세 정보, 에러 목록, 네임스페이스 필터)이 들어갈 **전용 공간 자체가 없다**
- 단일 그래프 패널 위에 기능을 계속 올리는 방식은 정보 표시 밀도와 UX 측면에서 구조적 한계
- 결국 **레이아웃 구조 없이 기능만 추가하면 결과물이 기능적으로도 미완성처럼 보인다**는 판단

이에 따라 **Phase 4의 고급 분석 항목(메서드 호출, 코드 스멜 지표)은 Sprint 3으로 순연**하고,
3분할 레이아웃 확립 + 그 위에서 동작하는 핵심 인터랙션 기능을 **Sprint 2의 전부**로 재편했다.

Phase 4에서 이월된 항목 매핑:

| Phase 4 원래 항목 | Sprint 2 대응 태스크 |
|-------------------|---------------------|
| P4-01. 노드 호버 툴팁 | S2-06 |
| P4-02. 노드 클릭 포커스 모드 | S2-EX-01 |
| P4-03. 검색/필터링 | S2-05 |
| P4-04. 에러 상세 표시 | S2-03 |
| P4-06. 순환 의존성 감지 | S2-EX-02 |
| P4-08. struct/record/enum 지원 | S2-EX-03 |
| P4-05. 메서드 호출 의존성 | → Sprint 3 이월 |
| P4-07. 코드 스멜 지표 시각화 | → Sprint 3 이월 |

---

---

## 진행 상태

| 태스크 | 상태 | 완료 시각 | 메모 |
|--------|------|-----------|------|
| S2-01. UI 3분할 레이아웃 | ✅ 완료 | 2026-03-13 | 4-way SplitContainer(outer/inner/left/right) 전면 리사이즈 가능, 다크 테마 일관 적용 |
| S2-02. 좌측 패널 — Namespace Filter | ✅ 완료 | 2026-03-13 | 체크 해제 시 즉시 그래프 재구성 확인 |
| S2-03. 좌측 패널 — Error Log | ✅ 완료 | 2026-03-13 | 오너드로우 + 빨간 ● 인디케이터 + All Namespaces 마스터 토글 |
| S2-04. 우측 패널 — Class Info (노드 클릭 연동) | ✅ 완료 | 2026-03-13 | 카드 UI + TreeView 탈피 + Dependency Metrics 분리 섹션 |
| S2-05. 툴바 Search | ✅ 완료 | 2026-03-13 | 클래스명·FullName 대소문자 무관 검색, 비매칭 노드 dimming |
| S2-06. 노드 호버 툴팁 | ✅ 완료 | 2026-03-13 | OwnerDraw 다크 툴팁 + 접기/펼치기 범례 + Fields/Methods 확장 패널 |

---

## 진행 로그

### S2-EX-04. Export PNG (2026-03-13)
- **결과**: 툴바 "💾 PNG 내보내기" 버튼 클릭 → SaveFileDialog → 현재 그래프 화면을 PNG로 저장. 범례(우측 상단 오버레이 패널) 포함 캡처.
- **구현**: `pnlGraph.RectangleToScreen()`으로 화면 좌표 계산 후 `Graphics.CopyFromScreen()`으로 실제 픽셀 복사. 저장 완료 시 StatusBar에 전체 경로 표시.
- **이슈/결정**: `DrawToBitmap()` 방식은 WM_PRINT 기반이라 GViewer 위에 오버레이된 `pnlLegend`(BringToFront)를 캡처하지 못함 → `CopyFromScreen`으로 전환하여 화면에 보이는 모든 컨트롤 포함.

### S2-EX-03. struct / record / enum 지원 (2026-03-13)
- **결과**: C# `struct` / `record` / `enum` 타입이 그래프 노드로 인식됨. 타입별 노드 모양과 색상 구분 — Struct(다이아몬드/녹청), Record(라운드 박스/황토), Enum(헥사곤/자주), Interface(원/보라), Class(박스/파랑). 순환 노드는 모든 타입에서 빨간색 강조.
- **구현**: `TypeKind` enum에 `Struct`/`Record`/`Enum` 추가. `TypeWalker`에 `VisitStructDeclaration()` / `VisitRecordDeclaration()` / `VisitEnumDeclaration()` 추가. `MsaglRenderer`에 각 타입별 색상 상수 + `switch (node.Kind)` 분기 확장. `_TestSample/Sample.cs`에 `Point`(struct) / `PersonRecord`(record) / `Direction`(enum) 검증 샘플 추가.
- **이슈/결정**: Dim 모드(검색/포커스)에서 Struct/Record/Enum은 shape 구분 없이 단일 Box로 처리 → 간결성 유지. MSAGL `Shape.Hexagon` 지원 확인 후 채택.

### S2-06. 노드 호버 툴팁 + UI 개선 (2026-03-13)
- **결과**: 노드 호버 시 다크 테마 커스텀 툴팁(클래스명/Kind/Namespace/Fields/Methods 수) 표시. 범례 기본 펼침 상태로 시작, 헤더 클릭으로 접기/펼치기 토글. CLASS INFO에서 Fields/Methods 행 클릭 시 실제 이름 목록이 아래로 확장. 검색창 우측 정렬 + 너비 확대. GViewer 내장 툴바 제거 후 그래프 좌상단에 이동 모드 토글 버튼 + 스페이스바 임시 pan 모드 추가.
- **구현**: `ToolTip.OwnerDraw = true` + `Draw`/`Popup` 이벤트로 다크 배경·파란 액센트 바·커스텀 폰트 렌더링. `pnlLegend` 내부를 `lblLegendHeader`(클릭) + `pnlLegendContent`(Paint)로 분리. `pnlFieldsDetail`/`pnlMethodsDetail` 패널을 DockStyle.Top으로 rowFields/rowMethods 아래 삽입 — 클릭 시 PopulateDetailPanel()로 이름 레이블 동적 생성 + 높이 조정. `KeyPreview = true` + `ProcessCmdKey`/`OnKeyUp`으로 스페이스바 임시 pan 모드 구현.
- **이슈/결정**: `ToolTip.SetToolTip()`은 GViewer 내부 마우스 처리로 인해 동작 안함 → `ToolTip.Show()`로 MouseMove에서 직접 표시. GViewer 내장 툴바 아이콘 배경색이 흰색으로 고정(비트맵 자체) → 툴바 숨기고 독립 이동 버튼으로 대체. `OnKeyUp` 단독으로는 GViewer 포커스 시 미동작 → `KeyPreview = true`로 해결.

### S2-05. 툴바 Search (2026-03-13)
- **결과**: 툴바 검색창 입력 시 매칭 노드(클래스명 또는 FullName 포함)는 정상 색상 유지, 비매칭 노드는 어두운 Dim 색상으로 처리. 검색창 비우면 즉시 전체 복원.
- **구현**: `ToolStripTextBox txtSearch` 추가. `TextChanged` → `_currentSearch` 갱신 → `RebuildGraphFiltered()` 호출. `MsaglRenderer.BuildViewer(result, searchQuery)` 파라미터 확장 — `node.FullName.Contains(query, OrdinalIgnoreCase)`로 매칭 판단. 비매칭 노드에 DimFill/DimBorder/DimText 적용.
- **이슈/결정**: 초기 구현은 `node.Name`만 검색 → 네임스페이스 포함 검색("testsample.") 시 미매칭. `node.Namespace`만 추가해도 네임스페이스 문자열 끝에 점이 없어 부분 실패 → `node.FullName`(`Namespace.Name` 조합)으로 통합하여 해결.

### S2-04. 우측 패널 — Class Info + Dependency Metrics (2026-03-13)
- **결과**: 노드 클릭 시 CLASS INFO / DEPENDENCY METRICS 두 섹션 카드로 분리 표시. 클래스명(굵게) + Kind/Namespace/File/Fields/Methods/Dependencies 행별 표시. Ca(Afferent) / Ce(Efferent) / Instability(F2) 수치 우측 정렬로 표시.
- **구현**: TreeView 방식 → 키:값 행 기반 카드 UI로 전면 재설계. `MakeInfoRow()` / `MakeMetricRow()` 헬퍼로 행 생성. `TypeNode`에 `FieldNames` / `MethodNames` 리스트 추가 — Roslyn `VisitClassDeclaration`에서 실제 이름 수집. Dependency Metrics는 기존 엣지 데이터에서 직접 계산 (별도 분석 불필요).
- **이슈/결정**: 레퍼런스 UI(`UI느낌.png`) 검토 후 TreeView 방식이 시각적으로 맞지 않아 카드 스타일로 재작성. Ca/Ce 계산은 엣지 집계로 충분 — SemanticModel 불필요.

### S2-03. 좌측 패널 — Error Log + Namespace 개선 (2026-03-13)
- **결과**: Error Log에 오너드로우(OwnerDrawFixed) 적용 — 빨간 ● 인디케이터 + 줄무늬 배경. "All Namespaces" 마스터 체크박스 추가 — 전체 토글. 그래프 영역 다크 테마 전환 — 노드/엣지 색상 다크 배경 기준으로 재조정.
- **구현**: `lstErrors_DrawItem` 오너드로우 핸들러. `chkAllNamespaces_CheckedChanged` — ItemCheck 이벤트 비활성화 후 일괄 SetItemChecked 처리. `MsaglRenderer` 다크 팔레트 — 클래스(진파랑/밝은파랑테두리), 인터페이스(진보라/밝은보라테두리), 엣지(밝은회색/밝은파랑점선/중간회색).
- **이슈/결정**: 그래프 다크화 시 기존 노드 색상(연파랑/연보라) 가시성 저하 → 다크 배경에 어울리는 진한 Fill + 밝은 Border 조합으로 전면 교체.

### S2-02. 좌측 패널 — Namespace Filter (2026-03-13)
- **결과**: 분석 완료 후 발견된 네임스페이스가 체크박스 목록으로 표시. 체크 해제 시 해당 네임스페이스 노드가 그래프에서 즉시 제거됨.
- **구현**: `PopulateNamespaceFilter()` — 분석 후 distinct 네임스페이스 수집. `clbNamespaces_ItemCheck` → `BeginInvoke(RebuildGraphFiltered)` — ItemCheck가 상태 변경 전에 발화하는 WinForms 특성 대응.
- **이슈/결정**: ItemCheck 이벤트 타이밍 문제 — BeginInvoke 없이 바로 호출 시 이전 상태 기준으로 필터링됨. BeginInvoke로 다음 메시지 루프까지 지연 처리.

### S2-01. UI 3분할 레이아웃 (2026-03-13, 확장 완료)
- **결과**: 좌/중/우 3분할 + 좌측 내부(Namespace Filter ↕ Error Log) + 우측 내부(Class Info ↕ Dependency Metrics) 총 4개 SplitContainer로 모든 경계 드래그 리사이즈 가능. 창 크기 변경 안정 확인.
- **구현**: `splitOuter`(좌|우), `splitInner`(그래프|우측), `splitLeft`(네임스페이스|에러), `splitRight`(클래스정보|메트릭) 4-way 중첩 구조. `SplitterDistance`/`Panel*MinSize`는 Shown 이벤트에서 지연 설정 — Designer 초기화 시점에는 컨트롤 크기가 미확정이므로 생성자에서 설정 시 InvalidOperationException 발생.
- **이슈/결정**: WinForms SplitContainer 기본 너비(~150px)가 Panel1MinSize+Panel2MinSize보다 작으면 초기화 시 예외 발생 → MinSize와 SplitterDistance 모두 Shown 이벤트로 이동. clbNamespaces를 DockStyle.Top(Height 고정)에서 DockStyle.Fill로 변경 — splitLeft.Panel1 내부에서 남은 공간을 채우도록 수정.

---

## Extension 진행 (시간 여유 시)

| 태스크 | 상태 | 메모 |
|--------|------|------|
| S2-EX-01. 노드 클릭 포커스 모드 | ✅ 완료 | 클릭 노드 + 1-hop 강조, 나머지 dimming. 재클릭/빈 곳 클릭 시 포커스 해제 |
| S2-EX-02. 순환 의존성 감지 | ✅ 완료 | DFS 사이클 탐지 + 빨간 엣지/노드, 범례 항목 추가 |
| S2-EX-03. struct / record / enum 지원 | ✅ 완료 | TypeKind 확장 — Struct(다이아몬드/녹청), Record(라운드박스/황토), Enum(헥사곤/자주) |
| S2-EX-04. Export PNG | ✅ 완료 | 화면 직접 캡처(CopyFromScreen)로 범례 포함 PNG 저장 |

---

## Sprint 2 완료 기준

| 기준 | 검증 방법 |
|------|----------|
| 3분할 레이아웃이 창 크기 변경에도 안정적으로 동작한다 | 직접 창 리사이즈 |
| 네임스페이스 체크박스 변경 시 그래프가 즉시 재구성된다 | _TestSample 분석 후 체크 해제 |
| 노드 클릭 시 우측 패널에 상세 정보가 표시된다 | GViewer 노드 클릭 확인 |
| 클래스 이름 검색 시 매칭/비매칭 노드가 시각적으로 구분된다 | Search 박스 입력 테스트 |
| 노드 호버 시 툴팁이 표시된다 | 마우스 올려두기 확인 |
