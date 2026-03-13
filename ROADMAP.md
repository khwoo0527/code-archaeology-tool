# 프로젝트 로드맵 - Code Archaeology

## 개요
- **목표**: C# 프로젝트 폴더를 열면 Roslyn 기반으로 클래스 구조와 의존성을 자동 분석하고, Microsoft.Msagl 인터랙티브 그래프로 시각화하는 WinForms 데스크톱 도구
- **전체 예상 기간**: 4 스프린트 (8주, 스프린트당 2주)
- **현재 진행 단계**: Phase 1 준비 중
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
| 전체 진행률 | 0% |
| 현재 Phase | Phase 1 (프로젝트 셋업) |
| 다음 마일스톤 | Sprint 1 MVP - 기본 그래프 표시 |
| Sprint Velocity | 측정 전 |

---

## 기술 아키텍처 결정 사항

| 영역 | 선택 | 이유 |
|------|------|------|
| UI 프레임워크 | WinForms (.NET Framework 4.8) | Windows 10+ 기본 런타임, 별도 설치 불필요 |
| 코드 분석 | Microsoft.CodeAnalysis.CSharp (Roslyn) | C# 공식 컴파일러 플랫폼, 정확한 구문 분석 |
| 그래프 렌더링 | Microsoft.Msagl + GraphViewerGdi | WinForms 네이티브 지원, 계층형 레이아웃 내장 |
| 빌드 도구 | MSBuild / Visual Studio 2022 | .NET Framework 4.8 표준 빌드 환경 |

**레이어 분리 원칙:**
```
UI Layer (WinForms)  -->  Analysis Layer (Roslyn)  -->  Model Layer (Graph)
  MainForm                  FolderScanner                GraphModel
  GraphControl              RoslynAnalyzer               Node / Edge
  StatusBar                 MsaglRenderer
```

---

## Sprint 1 — 해커톤 타임박스 계획 (6시간)

> **전략**: Core(필수) → Extension(여유 시) 순서로 진행.
> 각 Phase의 Core를 완료한 뒤 시간이 남으면 Extension으로 이동.
> Extension은 빠진 것이 아니라 뒤에서 계속 이어나갈 수 있는 다음 목표.

### 타임라인 개요

| 시간 | Phase | 내용 |
|------|-------|------|
| 0:00 – 1:00 | Phase 1 | 프로젝트 셋업 + MainForm 레이아웃 |
| 1:00 – 3:00 | Phase 2 | 분석 엔진 (FolderScanner + RoslynAnalyzer + GraphModel) |
| 3:00 – 5:00 | Phase 3 | 그래프 렌더링 + 파이프라인 연결 + StatusBar |
| 5:00 – 6:00 | Buffer | 삽질 대비 여유 / Extension 진행 |

---

## Phase 1: 프로젝트 셋업 (0:00 – 1:00)

### 목표
빌드 가능한 WinForms 프로젝트를 만들고, NuGet 패키지를 설치하며, MainForm 레이아웃을 구성한다.

### Core (반드시 완료)

- [ ] **P1-01. 솔루션 및 프로젝트 생성**
  - Visual Studio 2022에서 WinForms App (.NET Framework 4.8) 프로젝트 생성
  - 솔루션명: `CodeArchaeology`, 프로젝트명: `CodeArchaeology`
  - `.gitignore` 확인 (bin/, obj/, .vs/ 제외)

- [ ] **P1-02. NuGet 패키지 설치**
  - `Microsoft.CodeAnalysis.CSharp` 설치
  - `Microsoft.Msagl` 설치
  - `Microsoft.Msagl.GraphViewerGdi` 설치

- [ ] **P1-03. 폴더 구조 생성**
  - `/Models` — TypeNode, DependencyEdge, AnalysisResult
  - `/Analysis` — FolderScanner, RoslynAnalyzer
  - `/Rendering` — MsaglRenderer

- [ ] **P1-04. MainForm 기본 레이아웃**
  - Toolbar: `ToolStrip` (폴더 열기 버튼, 새로고침 버튼)
  - Graph Canvas: `Panel` (Dock: Fill)
  - StatusBar: `StatusStrip` (분석 상태 레이블)

### Extension (시간 남으면)

- [ ] **P1-EX-01. Toolbar에 검색 TextBox 추가** (Sprint 2 검색 기능 준비)
- [ ] **P1-EX-02. 1920x1080 기준 레이아웃 세부 조정**

### 완료 기준
- 빌드 에러 없이 컴파일되고 F5로 MainForm 창이 뜬다
- NuGet 3종 설치 완료

### 기술 고려사항
- .NET Framework 4.8 → C# 7.3 지원 (async/await 가능)
- Msagl NuGet 버전 호환성: `Microsoft.Msagl`과 `Microsoft.Msagl.GraphViewerGdi` 버전 일치 확인
- GViewer는 런타임에 Panel에 동적으로 추가하는 방식 권장

---

## Phase 2: 분석 엔진 (1:00 – 3:00)

### 목표
.cs 파일을 수집하고 Roslyn으로 class/interface 노드와 의존성 엣지를 추출해 GraphModel에 담는다.

### Core (반드시 완료)

- [ ] **P2-01. GraphModel 데이터 모델**
  - `TypeNode`: Name, Namespace, FullName, TypeKind(Class/Interface), FilePath, FieldCount, MethodCount
  - `DependencyEdge`: Source, Target, EdgeType(Inheritance / InterfaceImpl / FieldDependency)
  - `AnalysisResult`: Nodes, Edges, Errors 컬렉션

- [ ] **P2-02. FolderScanner**
  - `Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)`로 재귀 수집

- [ ] **P2-03a. RoslynAnalyzer — class 노드 추출** _(~30분, 검증: 콘솔/디버거로 클래스 이름 목록 확인)_
  - `CSharpSyntaxTree.ParseText()`로 파싱
  - `VisitClassDeclaration()`: 클래스명, 네임스페이스 추출 → TypeNode 생성

- [ ] **P2-03b. RoslynAnalyzer — interface 노드 추출** _(~15분, 검증: 인터페이스 이름 목록 확인)_
  - `VisitInterfaceDeclaration()`: 인터페이스명, 네임스페이스 추출 → TypeNode 생성

- [ ] **P2-03c. RoslynAnalyzer — 상속 / 인터페이스 구현 엣지 추출** _(~30분, 검증: 엣지 Source-Target 쌍 콘솔 출력)_
  - base type / interface 목록 → DependencyEdge (Inheritance / InterfaceImpl) 생성
  - 분석된 타입 목록에 없는 타입은 엣지에서 제외 (단순 이름 매칭)

- [ ] **P2-04. 기본 에러 핸들링**
  - try-catch로 파싱 실패 파일을 건너뛰고 Errors 리스트에 기록

### Extension (시간 남으면)

- [ ] **P2-EX-01. 필드 타입 의존성 추출**
  - `VisitFieldDeclaration()`으로 필드 타입 추출 → FieldDependency 엣지 생성
- [ ] **P2-EX-02. partial class 병합**
  - 동일 FullName 노드를 하나로 합산 (FieldCount, MethodCount 누적)
- [ ] **P2-EX-03. 외부 타입 정밀 필터링**
  - `using` 문 파싱으로 네임스페이스 추론 보완
- [ ] **P2-EX-04. 비동기 분석 (`Task.Run`)**
  - UI 프리징 방지 (소규모 프로젝트에선 동기도 무방)

### 완료 기준
- 샘플 .cs 파일에서 class/interface 노드와 상속/인터페이스 엣지가 추출된다
- 에러 파일이 있어도 나머지 파일은 정상 분석된다

### 기술 고려사항
- Roslyn ParseText는 SemanticModel 없이 SyntaxTree만 사용 (네임스페이스 매칭은 이름 기반 휴리스틱)
- 필드 타입 매칭 정확도는 MVP에서 타협 (SemanticModel 도입은 Sprint 2 검토)
- Generic 타입 인자(`List<T>`)는 MVP에서 의존성 추적하지 않음

---

## Phase 3: 그래프 시각화 (3:00 – 5:00)

### 목표
GraphModel을 Msagl 그래프로 변환하고 GViewer에 렌더링한다. 폴더 열기 파이프라인을 연결하고 StatusBar를 완성해 MVP를 완료한다.

### Core (반드시 완료)

- [ ] **P3-01a. MsaglRenderer — 노드만 렌더링** _(~30분, 검증: GViewer에 노드 박스가 표시되면 성공)_
  - AnalysisResult.Nodes → `graph.AddNode()`, 라벨에 `ClassName` 표시
  - 계층형(Sugiyama) 레이아웃 설정 후 GViewer에 표시

- [ ] **P3-01b. MsaglRenderer — 엣지 추가** _(~30분, 검증: 노드 간 화살표가 그려지면 성공)_
  - AnalysisResult.Edges → `graph.AddEdge()` (기본 스타일 단색으로 우선 표시)

- [ ] **P3-02. GViewer 통합**
  - Panel에 `GViewer` 배치 (Dock: Fill)
  - `GViewer.Graph`에 생성된 Graph 할당

- [ ] **P3-03. 폴더 열기 파이프라인 연결**
  - 폴더 열기 버튼 → `FolderBrowserDialog` → FolderScanner → RoslynAnalyzer → MsaglRenderer → GViewer
  - 새로고침 버튼: 마지막 폴더 재분석

- [ ] **P3-04. StatusBar 연동**
  - "분석 완료 (N개 클래스) | 에러: M개 파일" 형태 표시

### Extension (시간 남으면)

- [ ] **P3-EX-01. 엣지 색상/스타일 구분**
  - 상속: 실선, 검정 / 인터페이스 구현: 점선, 파랑 / 필드 의존성: 실선, 회색
- [ ] **P3-EX-02. 노드 라벨에 네임스페이스 표시**
  - `Namespace.ClassName` 형식으로 변경
- [ ] **P3-EX-03. 새로고침 버튼 활성화 로직**
  - 폴더 열기 전에는 비활성화

### 완료 기준
- 폴더를 열면 3번의 클릭 이내에 그래프가 표시된다
- StatusBar에 분석 결과 요약이 표시된다

### 검증 시나리오
```
1. 앱 실행 → 폴더 열기 → 샘플 C# 폴더 선택 → 그래프 표시 확인
2. StatusBar에 "분석 완료 (N개 클래스)" 형태 표시 확인
3. 새로고침 버튼 클릭 → 동일 폴더 재분석 확인
```

### 기술 고려사항
- GViewer 자체 줌/팬/드래그 기능 활용 (별도 구현 불필요)
- 엣지 스타일(점선 등) Msagl API 삽질 가능성 → Extension으로 분리한 이유

---

## Sprint 1 MVP 완료 기준

| 기준 | 측정 방법 |
|------|----------|
| C# 폴더를 열면 3번의 클릭 이내에 그래프가 표시된다 | 직접 테스트 |
| class/interface 노드가 그래프에 표시된다 | 샘플 프로젝트 확인 |
| 상속/인터페이스 구현 엣지가 표시된다 | 샘플 프로젝트 확인 |
| 에러 파일이 있어도 정상 파일은 분석된다 | 의도적 에러 파일 포함 테스트 |
| StatusBar에 분석 결과 요약이 표시된다 | 직접 확인 |

---

## Phase 4: 노드 인터랙션 및 고급 분석 (Sprint 2)

### 목표
Sprint 1에서 미뤄진 노드 인터랙션 기능을 구현하고, 메서드 호출 의존성·순환 감지·코드 스멜 지표를 추가한다.

### 작업 목록

- [ ] **P4-01. 노드 호버 툴팁**
  - GViewer의 `ObjectUnderMouseCursor` 이벤트 활용
  - 노드 위에 마우스 올리면 ToolTip 표시: 네임스페이스 / 필드 수 / 메서드 수 / 파일 경로

- [ ] **P4-02. 노드 클릭 포커스 모드**
  - 노드 클릭 시: 클릭한 노드 + 1-hop 이웃 노드를 강조 표시
  - 나머지 노드는 opacity 감소 (흐리게 처리)
  - 빈 영역 클릭 시: 모든 노드 강조 해제, 원래 상태 복원

- [ ] **P4-03. 검색/필터링 기능**
  - Toolbar에 검색 TextBox 추가
  - 클래스 이름 입력 시 해당 노드 하이라이트 (TextChanged 이벤트)
  - 매칭 노드 강조 + 그래프 중심을 해당 노드로 이동

- [ ] **P4-04. 에러 상세 표시**
  - StatusBar 에러 영역 클릭 시 에러 파일 목록 + 메시지 상세 표시 (MessageBox 또는 별도 창)

- [ ] **P4-05. 메서드 호출 의존성 분석**
  - `VisitInvocationExpression()`으로 메서드 호출 추출
  - 호출자 클래스 -> 피호출자 클래스 의존성 엣지 생성
  - EdgeType에 `MethodCall` 유형 추가
  - 시각화: 별도 색상/스타일 (예: 주황색 점선)

- [ ] **P4-06. 순환 의존성 감지**
  - 그래프에서 Cycle Detection 알고리즘 구현 (DFS 기반 Tarjan 또는 간단 DFS)
  - 순환이 감지되면 해당 엣지를 빨간색으로 강조 표시
  - StatusBar 또는 별도 경고 패널에 순환 의존성 경고 메시지 표시
  - 순환 그룹 클릭 시 관련 노드 하이라이트

- [ ] **P4-07. 코드 스멜 지표 시각화**
  - 클래스별 지표 계산:
    - 참조 횟수 (Afferent Coupling, Ca): 다른 클래스가 이 클래스를 참조하는 수
    - 의존도 지수 (Efferent Coupling, Ce): 이 클래스가 의존하는 다른 클래스 수
    - 불안정성 지표 (Instability): Ce / (Ca + Ce)
  - 노드 크기 또는 색상 농도로 지표 시각화
  - 툴팁에 지표 수치 추가 표시

- [ ] **P4-08. struct / record / enum 지원**
  - RoslynAnalyzer에 `VisitStructDeclaration()`, `VisitRecordDeclaration()`, `VisitEnumDeclaration()` 추가
  - TypeKind enum에 Struct, Record, Enum 추가
  - 노드 모양 또는 색상으로 타입 종류 구분 (예: class=사각형, interface=원, struct=다이아몬드)

### 완료 기준 (Definition of Done)
- 노드 호버 시 네임스페이스, 필드수/메서드수, 파일 경로가 툴팁으로 표시된다
- 노드 클릭 시 포커스 모드가 동작하고, 빈 영역 클릭 시 해제된다
- 검색 시 매칭 노드가 하이라이트된다
- 메서드 호출 관계가 별도 색상의 엣지로 그래프에 표시된다
- 순환 의존성이 존재할 때 빨간색 엣지와 경고 메시지가 표시된다
- 각 노드의 참조 횟수/의존도 지표가 툴팁에 표시된다
- struct, record, enum이 그래프에 노드로 표시되고 class/interface와 구분된다

### 검증 시나리오
```
1. 메서드 호출이 포함된 샘플 프로젝트 분석 -> 호출 의존성 엣지 표시 확인
2. 의도적 순환 의존성 포함 샘플 분석 -> 빨간색 경고 엣지 및 메시지 확인
3. 높은 의존도 클래스의 노드가 시각적으로 두드러지는지 확인
4. struct/record/enum 포함 프로젝트 분석 -> 타입별 노드 구분 확인
5. 기존 Sprint 1 기능이 정상 동작하는지 회귀 테스트
```

### 기술 고려사항
- 메서드 호출 분석은 SyntaxTree만으로 호출 대상 타입을 정확히 식별하기 어려움 -> SemanticModel 도입 검토 또는 이름 기반 휴리스틱 적용
- 순환 감지는 방향 그래프 기준 Strongly Connected Components (SCC) 알고리즘 사용
- record는 .NET Framework 4.8에서 공식 지원하지 않으나, Roslyn 파서는 구문 인식 가능

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
| .NET Framework 4.8의 C# 버전 제약 (7.3) | Low | Low | 최신 C# 기능 없이도 구현 가능. 필요 시 .NET 6+ 마이그레이션 백로그 추가 |

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
| 검토 필요 | .NET 6+ 마이그레이션 | WinForms on .NET 6+로 전환 시 최신 C# 기능 활용 가능 |

---

## 기술 부채 관리

| 항목 | 발생 예상 Phase | 해결 예정 Phase |
|------|----------------|----------------|
| 필드 타입 매칭 휴리스틱 (SemanticModel 미사용) | Phase 2 | Phase 4 검토 |
| 단위 테스트 부재 | Phase 1-3 | Phase 5 |
| 하드코딩된 색상/스타일 값 | Phase 3 | Phase 5 |
| 대규모 프로젝트 성능 최적화 | Phase 4 | Phase 6 |
