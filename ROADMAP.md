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

## Phase 1: 프로젝트 셋업 및 기반 구축 (Sprint 1 - Week 1)

### 목표
프로젝트 솔루션 구조를 생성하고, NuGet 패키지를 설정하며, 빈 WinForms 윈도우가 실행되는 상태를 만든다.

### 작업 목록

- [ ] **P1-01. 솔루션 및 프로젝트 생성**
  - Visual Studio 2022에서 WinForms App (.NET Framework 4.8) 프로젝트 생성
  - 솔루션명: `CodeArchaeology`, 프로젝트명: `CodeArchaeology`
  - `.gitignore` 확인 (bin/, obj/, .vs/ 등 제외)

- [ ] **P1-02. NuGet 패키지 설치**
  - `Microsoft.CodeAnalysis.CSharp` (최신 안정 버전) 설치
  - `Microsoft.Msagl` 설치
  - `Microsoft.Msagl.GraphViewerGdi` 설치
  - packages.config 또는 PackageReference에 버전 고정

- [ ] **P1-03. 폴더 구조 생성**
  - `/Models` - GraphModel, Node, Edge 등 데이터 모델
  - `/Analysis` - FolderScanner, RoslynAnalyzer
  - `/Rendering` - MsaglRenderer
  - `/UI` - MainForm, 커스텀 컨트롤

- [ ] **P1-04. MainForm 기본 레이아웃 구성**
  - Toolbar 영역: `ToolStrip` (폴더 열기 버튼, 새로고침 버튼, 검색 TextBox)
  - Graph Canvas 영역: `Panel` (Dock: Fill) - Msagl GViewer를 배치할 영역
  - StatusBar 영역: `StatusStrip` (분석 상태, 에러 카운트 표시)
  - 1920x1080 기준 레이아웃 최적화

### 완료 기준 (Definition of Done)
- 솔루션이 빌드 에러 없이 컴파일된다
- NuGet 패키지 3종이 모두 설치되어 참조가 정상 동작한다
- MainForm 실행 시 Toolbar / Graph Canvas / StatusBar 3영역이 표시된다
- 폴더 구조가 레이어 분리 원칙에 맞게 생성되어 있다

### 검증 시나리오
```
1. Visual Studio에서 솔루션 빌드 (Build > Build Solution) - 에러 0건 확인
2. F5로 실행 - MainForm 창이 정상 표시되는지 확인
3. Toolbar에 폴더 열기/새로고침/검색 컨트롤이 보이는지 확인
4. StatusBar가 화면 하단에 표시되는지 확인
5. NuGet 패키지 관리자에서 3개 패키지 설치 상태 확인
```

### 기술 고려사항
- .NET Framework 4.8은 C# 7.3까지 지원 (async/await 사용 가능, 패턴 매칭 제한적)
- Microsoft.Msagl NuGet 버전과 GraphViewerGdi 버전 호환성 확인 필요
- GViewer 컨트롤은 런타임에 Panel에 동적으로 추가하는 방식 권장

---

## Phase 2: 핵심 분석 엔진 구현 (Sprint 1 - Week 1-2)

### 목표
폴더 내 .cs 파일을 스캔하고, Roslyn으로 class/interface의 상속, 인터페이스 구현, 필드 타입 의존성 3종을 추출하여 GraphModel로 변환한다.

### 작업 목록

- [ ] **P2-01. GraphModel 데이터 모델 정의**
  - `TypeNode` 클래스: Name, Namespace, FullName, TypeKind(Class/Interface), FilePath, FieldCount, MethodCount
  - `DependencyEdge` 클래스: Source, Target, EdgeType(Inheritance/InterfaceImpl/FieldDependency)
  - `AnalysisResult` 클래스: Nodes 컬렉션, Edges 컬렉션, Errors 리스트

- [ ] **P2-02. FolderScanner 구현**
  - 입력: 폴더 경로
  - 재귀적으로 `.cs` 파일 수집 (`Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories)`)
  - 출력: .cs 파일 경로 리스트

- [ ] **P2-03. RoslynAnalyzer 핵심 구현**
  - `CSharpSyntaxTree.ParseText()`로 각 .cs 파일 파싱
  - `CSharpSyntaxWalker` 상속 클래스 구현:
    - `VisitClassDeclaration()`: class 이름, 네임스페이스, 상속 정보, 인터페이스 구현 추출
    - `VisitInterfaceDeclaration()`: interface 이름, 네임스페이스, 상속 정보 추출
    - `VisitFieldDeclaration()`: 필드 타입 추출
  - partial class 병합 처리: 동일 FullName의 클래스는 하나로 합산
  - 외부 타입 필터링: 분석된 타입 목록에 없는 타입은 엣지에서 제외

- [ ] **P2-04. 에러 핸들링**
  - 파싱 에러가 있는 파일은 에러를 기록하고 건너뛴다 (전체 분석 중단 방지)
  - 에러 정보 수집: 파일 경로, 에러 메시지
  - AnalysisResult.Errors에 에러 목록 저장

- [ ] **P2-05. 비동기 분석 처리**
  - `Task.Run()`으로 분석 로직을 백그라운드 스레드에서 실행
  - UI 스레드 프리징 방지
  - 분석 시작/완료 이벤트를 UI에 전달

### 완료 기준 (Definition of Done)
- 샘플 C# 폴더(10개 이상 .cs 파일)에 대해 class/interface 노드가 정확히 추출된다
- 상속, 인터페이스 구현, 필드 타입 의존성 3종의 엣지가 정확히 추출된다
- partial class가 하나의 노드로 병합된다
- 외부 타입(System.*, 등)이 노드/엣지에서 제외된다
- 에러가 있는 .cs 파일이 포함되어도 나머지 파일은 정상 분석된다
- 분석 중 UI가 프리징되지 않는다

### 검증 시나리오
```
1. 테스트용 C# 프로젝트 폴더를 준비 (상속, 인터페이스, 필드 의존성 포함)
2. FolderScanner로 .cs 파일 목록 수집 확인
3. RoslynAnalyzer로 분석 실행 후 AnalysisResult 검증:
   - 노드 수가 예상 class/interface 수와 일치
   - 엣지 유형별 개수 확인
   - 외부 타입이 결과에 미포함 확인
4. 의도적 구문 에러 파일 포함 후 분석 - 에러 파일 외 정상 분석 확인
5. 50개 클래스 규모 폴더에서 비동기 분석 시 UI 응답성 확인
```

### 기술 고려사항
- Roslyn ParseText는 단일 파일 파싱이므로 SemanticModel 없이 SyntaxTree만 사용
- 필드 타입의 정확한 네임스페이스 매칭은 SyntaxTree만으로 한계가 있음 -> 타입 이름 문자열 매칭으로 처리 (MVP 타협점)
- `using` 문 분석으로 네임스페이스 추론 보완 가능
- generic 타입 인자(`List<T>`)는 MVP에서 의존성 추적하지 않음

---

## Phase 3: 그래프 시각화 및 상호작용 (Sprint 1 - Week 2)

### 목표
AnalysisResult를 Microsoft.Msagl 그래프로 변환하고, 계층형 레이아웃으로 렌더링하며, 노드 호버/클릭/검색 상호작용을 구현한다.

### 작업 목록

- [ ] **P3-01. MsaglRenderer 구현**
  - AnalysisResult -> Microsoft.Msagl.Drawing.Graph 변환
  - 노드 생성: `graph.AddNode()`, 라벨에 `Namespace.ClassName` 표시
  - 엣지 생성 및 스타일 적용:
    - 상속: 실선, 검정색 (`Color.Black`, `Style.Solid`)
    - 인터페이스 구현: 점선, 파란색 (`Color.Blue`, `Style.Dashed`)
    - 필드 의존성: 실선, 회색 (`Color.Gray`, `Style.Solid`, 가는 선)
  - 계층형(Sugiyama) 레이아웃 설정

- [ ] **P3-02. GViewer 통합**
  - MainForm의 Graph Canvas Panel에 `GViewer` 컨트롤 배치
  - `GViewer.Graph` 속성에 생성된 Graph 할당
  - 기본 줌/팬 기능은 GViewer 내장 기능 활용

- [ ] **P3-03. 폴더 열기 연동**
  - Toolbar의 "폴더 열기" 버튼 클릭 -> `FolderBrowserDialog` 표시
  - 폴더 선택 -> FolderScanner -> RoslynAnalyzer -> MsaglRenderer -> GViewer 파이프라인 연결
  - "새로고침" 버튼: 마지막으로 열었던 폴더를 다시 분석

- [ ] **P3-04. 노드 호버 툴팁**
  - GViewer의 `ObjectUnderMouseCursor` 이벤트 활용
  - 노드 위에 마우스 올리면 ToolTip 표시:
    - 네임스페이스
    - 필드 수 / 메서드 수
    - 파일 경로

- [ ] **P3-05. 노드 클릭 포커스 모드**
  - 노드 클릭 시: 클릭한 노드 + 1-hop 이웃 노드를 강조 표시
  - 나머지 노드는 opacity 감소 (흐리게 처리)
  - 빈 영역 클릭 시: 모든 노드 강조 해제, 원래 상태 복원

- [ ] **P3-06. 검색/필터링 기능**
  - Toolbar 검색 TextBox에 클래스 이름 입력 시 해당 노드 하이라이트
  - 실시간 검색 (TextChanged 이벤트)
  - 매칭 노드 강조 + 그래프 중심을 해당 노드로 이동

- [ ] **P3-07. StatusBar 연동**
  - 분석 상태 표시: "분석 중...", "분석 완료 (N개 클래스)"
  - 에러 파일 수 표시: "에러: N개 파일"
  - 에러 파일 목록 클릭 시 상세 정보 표시 (MessageBox 또는 별도 창)

### 완료 기준 (Definition of Done)
- 폴더를 열면 3번의 클릭(폴더 열기 버튼 -> 폴더 선택 -> 확인) 이내에 그래프가 표시된다
- 상속/인터페이스/필드 엣지가 색상과 선 스타일로 시각적으로 구분된다
- 노드 호버 시 네임스페이스, 필드수/메서드수, 파일 경로가 툴팁으로 표시된다
- 노드 클릭 시 포커스 모드가 동작하고, 빈 영역 클릭 시 해제된다
- 검색 시 매칭 노드가 하이라이트된다
- StatusBar에 분석 결과 요약과 에러 파일 수가 표시된다

### 검증 시나리오
```
1. 앱 실행 -> 폴더 열기 버튼 클릭 -> 샘플 C# 폴더 선택 -> 그래프 표시 확인
2. 그래프에서 엣지 색상/스타일 구분 확인 (상속:검정실선, 인터페이스:파랑점선, 필드:회색실선)
3. 노드에 마우스 호버 -> 툴팁에 네임스페이스, 필드수, 메서드수, 파일경로 표시 확인
4. 노드 클릭 -> 1-hop 이웃만 강조, 나머지 흐리게 처리 확인
5. 빈 영역 클릭 -> 전체 노드 정상 표시로 복원 확인
6. 검색 TextBox에 클래스 이름 입력 -> 매칭 노드 하이라이트 확인
7. StatusBar에 "분석 완료 (N개 클래스) | 에러: M개 파일" 형태 표시 확인
8. 새로고침 버튼 클릭 -> 동일 폴더 재분석 및 그래프 갱신 확인
```

### 기술 고려사항
- GViewer는 자체 줌/팬/드래그 기능을 제공하므로 별도 구현 불필요
- 노드 opacity 조절은 Msagl의 `Node.Attr.FillColor`에 Alpha 값 조절로 구현
- 검색 시 `GViewer.CenterToGroup()` 또는 직접 뷰포트 이동으로 해당 노드로 스크롤
- GViewer의 이벤트 모델을 먼저 프로토타이핑하여 호버/클릭 동작 검증 권장

---

## Sprint 1 MVP 마일스톤

**목표 달성 기준 (PRD Section 8 기반):**

| 기준 | 측정 방법 | Phase |
|------|----------|-------|
| C# 폴더를 열면 3번의 클릭 이내에 그래프가 표시된다 | 직접 사용 테스트 | Phase 3 |
| 클래스 상속/인터페이스/필드 엣지가 시각적으로 구분된다 | 샘플 프로젝트로 확인 | Phase 3 |
| 에러가 있는 파일이 포함된 폴더도 정상 파일은 분석된다 | 의도적 에러 파일 포함 후 테스트 | Phase 2 |
| 호버 시 클래스 정보가 툴팁으로 표시된다 | 직접 확인 | Phase 3 |

---

## Phase 4: 메서드 호출 의존성 및 순환 감지 (Sprint 2)

### 목표
메서드 호출 의존성 분석, 순환 의존성 감지 및 경고, 코드 스멜 지표를 추가한다.

### 작업 목록

- [ ] **P4-01. 메서드 호출 의존성 분석**
  - `VisitInvocationExpression()`으로 메서드 호출 추출
  - 호출자 클래스 -> 피호출자 클래스 의존성 엣지 생성
  - EdgeType에 `MethodCall` 유형 추가
  - 시각화: 별도 색상/스타일 (예: 주황색 점선)

- [ ] **P4-02. 순환 의존성 감지**
  - 그래프에서 Cycle Detection 알고리즘 구현 (DFS 기반 Tarjan 또는 간단 DFS)
  - 순환이 감지되면 해당 엣지를 빨간색으로 강조 표시
  - StatusBar 또는 별도 경고 패널에 순환 의존성 경고 메시지 표시
  - 순환 그룹 클릭 시 관련 노드 하이라이트

- [ ] **P4-03. 코드 스멜 지표 시각화**
  - 클래스별 지표 계산:
    - 참조 횟수 (Afferent Coupling, Ca): 다른 클래스가 이 클래스를 참조하는 수
    - 의존도 지수 (Efferent Coupling, Ce): 이 클래스가 의존하는 다른 클래스 수
    - 불안정성 지표 (Instability): Ce / (Ca + Ce)
  - 노드 크기 또는 색상 농도로 지표 시각화
  - 툴팁에 지표 수치 추가 표시

- [ ] **P4-04. struct / record / enum 지원**
  - RoslynAnalyzer에 `VisitStructDeclaration()`, `VisitRecordDeclaration()`, `VisitEnumDeclaration()` 추가
  - TypeKind enum에 Struct, Record, Enum 추가
  - 노드 모양 또는 색상으로 타입 종류 구분 (예: class=사각형, interface=원, struct=다이아몬드)

### 완료 기준 (Definition of Done)
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
| M1: Sprint 1 MVP | Week 2 완료 | 폴더 열기 -> 그래프 표시 + 호버/클릭/검색 동작하는 실행 파일 |
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
