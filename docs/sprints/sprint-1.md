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
| S-08. 에러 핸들링 | 예정 | - | |
| S-09. MsaglRenderer + GViewer 연결 (노드 표시) | 예정 | - | |
| S-10. MsaglRenderer — 엣지 추가 | 예정 | - | |
| S-11. 폴더 열기 파이프라인 전체 연결 | 예정 | - | |
| S-12. StatusBar 연동 | 예정 | - | |

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
- **상태**: 예정
- **결과**: -

---

### [S-09] MsaglRenderer + GViewer 연결
- **상태**: 예정
- **결과**: -

---

### [S-10] MsaglRenderer — 엣지 추가
- **상태**: 예정
- **결과**: -

---

### [S-11] 폴더 열기 파이프라인 전체 연결
- **상태**: 예정
- **결과**: -

---

### [S-12] StatusBar 연동
- **상태**: 예정
- **결과**: -

---

## Extension 진행 (시간 여유 시)

| 태스크 | 상태 | 메모 |
|--------|------|------|
| S-EX-01. 엣지 색상/스타일 구분 | 예정 | |
| S-EX-02. 필드 타입 의존성 추출 | 예정 | |
| S-EX-03. 노드 라벨 네임스페이스 표시 | 예정 | |
| S-EX-04. partial class 병합 | 예정 | |
| S-EX-05. 비동기 처리 | 예정 | |

---

## 이슈 및 결정 사항

> 구현 중 발생한 이슈, 기술적 결정, 트레이드오프를 여기에 기록

---

## Sprint 1 완료 기준 체크

| 기준 | 결과 |
|------|------|
| C# 폴더를 열면 3클릭 이내에 그래프가 표시된다 | - |
| class/interface 노드가 그래프에 표시된다 | - |
| 상속/인터페이스 엣지가 그래프에 표시된다 | - |
| 에러 파일이 있어도 정상 파일은 분석된다 | - |
| StatusBar에 분석 결과 요약이 표시된다 | - |
