# Sprint 3 진행 기록

> **목표**: 고급 분석 기능 완성 — 변경 영향 분석, 코드 스멜 지표 시각화, UX 개선
> **시작일**: 2026-03-13

---

## 진행 상태

| 태스크 | 상태 | 완료 시각 | 메모 |
|--------|------|-----------|------|
| P5-02. 변경 영향 분석 | ✅ 완료 | 2026-03-13 | 역방향 BFS, 🔍 영향 분석 버튼, 빈 곳 클릭 자동 해제 |
| P4-07. 코드 스멜 지표 시각화 | ✅ 완료 | 2026-03-13 | Ca 비례 노드 크기, Instability 색상 보간, 📊 코드 스멜 버튼 |
| fix. 팬 드래그 후 위치 초기화 버그 | ✅ 완료 | 2026-03-13 | MouseDown/Move 드래그 감지로 click 이벤트 차단 |
| fix. 리빌드 시 줌 레벨 리셋 | ✅ 완료 | 2026-03-13 | ZoomF 저장/복원으로 포커스 해제 등에서 줌 유지 |

---

## 진행 로그

### fix. 리빌드 시 줌 레벨 보존 (2026-03-13)
- **결과**: 노드 선택/해제, 영향 분석 토글, 검색 등 그래프 리빌드가 발생하는 모든 상황에서 현재 줌 레벨이 유지됨.
- **구현**: `RebuildGraph()` 진입 시 `_gViewer?.ZoomF` 저장, 새 GViewer 생성 후 `ZoomF` 복원. 최초 로드(savedZoom=0)는 기본값 유지.
- **이슈/결정**: 빈 곳 클릭 시 `hadFocus || hadImpact`가 없으면 리빌드 자체를 생략하여 코드 스멜 뷰 유지. 실제 상태 변경이 있을 때만 리빌드.

### P4-07. 코드 스멜 지표 시각화 (2026-03-13)
- **결과**: 그래프 오버레이에 📊 코드 스멜 버튼 추가 (✋ 이동 / 🔍 영향 분석 / 📊 코드 스멜 3버튼 나란히). 활성화 시 Ca(나를 참조하는 수) 비례로 노드 폰트 크기 8~18 확대, Instability(Ce/(Ca+Ce)) 비례로 파랑→빨강 색상 보간. 빈 곳 클릭 시 코드 스멜 모드 유지.
- **구현**: `MsaglRenderer.BuildViewer()`에 `codeSmellMode` 파라미터 추가. 노드별 Ca/Ce 사전 계산 후 `maxCa` 정규화. Instability 0→1 구간을 RGB 보간 `(40,120,200)→(220,60,30)`으로 표현. `btnCodeSmell`은 오버레이 Button으로 Designer.cs `InitImpactButton()` 내에서 초기화.
- **이슈/결정**: 툴바에 넣었다가 다른 오버레이 버튼들과 일관성을 위해 그래프 좌상단 오버레이로 이동. 빈 곳 클릭 시 RebuildGraphFiltered() 호출이 코드 스멜 모드를 리셋하는 것처럼 보이는 문제 → 실제로는 리빌드 자체가 줌 리셋을 일으킨 것으로, 변경 없으면 리빌드 생략으로 해결.

### P5-02. 변경 영향 분석 (2026-03-13)
- **결과**: 노드 클릭으로 선택 후 🔍 영향 분석 버튼 클릭 시, 선택 노드를 직간접으로 참조하는 모든 클래스를 역방향 BFS로 탐색하여 주황색 하이라이트. 영향 루트(밝은 주황/굵은 테두리) + 영향 노드(어두운 주황) + 나머지 dim. 빈 곳 클릭 시 자동 해제. StatusBar에 "영향 범위: N개 클래스" 표시.
- **구현**: `ComputeImpactSet(rootId)` — Queue 기반 역방향 BFS, `edge.Target == current`인 edge.Source를 수집. `MsaglRenderer`에 `impactRootId`/`impactSet` 파라미터 추가 — hasImpact 모드에서 root/impact/dim 3단계 색상 분기. 엣지도 영향 경로 상에 있으면 주황 강조, 나머지 dim.
- **이슈/결정**: 초기엔 우클릭 컨텍스트 메뉴로 구현 → 호버 툴팁과 겹쳐 UX 불편 → 노드 선택 후 오버레이 버튼 방식으로 변경. 버튼은 노드 선택 시 활성화, 빈 곳 클릭 시 비활성화.

### fix. 팬 모드 드래그 후 그래프 위치 초기화 버그 (2026-03-13)
- **결과**: ✋ 이동 모드에서 드래그 후 마우스를 떼도 그래프 위치가 유지됨.
- **구현**: `gViewer_MouseDown`에서 `_mouseDownPoint` 저장. `gViewer_MouseMove`에서 5px 이상 이동 시 `_wasDragged = true`. `gViewer_MouseClick`에서 `_wasDragged`가 true이면 그래프 리빌드 생략.
- **이슈/결정**: 드래그 후 마우스 업 시 MouseClick 이벤트가 발생하여 `RebuildGraphFiltered()` 호출 → 뷰포트 리셋. WinForms에서 MouseClick은 드래그 여부와 무관하게 발생하므로 직접 감지 필요.

---

### 기술 구현력 보완 — 레이어 분리 + 품질 인프라 (2026-03-13)
- **결과**: 기술 구현력 항목 체계적 보완. `IAnalyzer` / `IFolderScanner` 인터페이스 신설로 UI↔Analysis 레이어 의존성 역전 적용. `Directory.Packages.props`로 NuGet 버전 중앙 집중 관리. `ARCHITECTURE.md`로 레이어 다이어그램·코드 증거 문서화.
- **구현**: `Analysis/IAnalyzer.cs`, `Analysis/IFolderScanner.cs` 인터페이스 생성. `MainForm.cs`에서 `IFolderScanner scanner = new FolderScanner()` / `IAnalyzer analyzer = new RoslynAnalyzer()`로 변경. 전 모델·분석 클래스에 XML 문서 주석(`///`) 추가. `Directory.Packages.props` 생성 + 양 csproj에서 버전 속성 제거.
- **이슈/결정**: S2-05(Search), S2-06(Tooltip)이 이전 스프린트에서 이미 구현 완료 상태임을 확인 → ROADMAP 체크박스만 업데이트.

---

## Sprint 3 완료 기준 달성 현황

| 기준 | 결과 |
|------|------|
| 변경 영향 분석 시 직간접 참조 클래스가 하이라이트된다 | ✅ |
| 코드 스멜 지표가 노드 크기/색상으로 시각화된다 | ✅ |
| 팬 드래그 후 뷰포트가 유지된다 | ✅ |
| 리빌드 시 줌 레벨이 유지된다 | ✅ |
| IAnalyzer / IFolderScanner 인터페이스로 레이어 분리가 강제된다 | ✅ |
| Directory.Packages.props로 NuGet 버전이 중앙 관리된다 | ✅ |
| 전 public API에 XML 문서 주석이 존재한다 | ✅ |
| ARCHITECTURE.md에 레이어 다이어그램과 코드 증거가 문서화된다 | ✅ |
| S2-05 Search / S2-06 Tooltip 완료 확인 | ✅ |
