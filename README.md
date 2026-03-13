# Code Archaeology

C# 프로젝트 폴더를 열면 Roslyn으로 클래스 구조와 의존성을 자동 분석하고,
인터랙티브 그래프로 시각화하는 WinForms 데스크톱 도구

---

## 주요 기능

### Sprint 1 (현재)
- **폴더 열기** — C# 프로젝트 폴더를 선택하면 자동 분석 시작
- **의존성 분석** — 클래스 상속 / 인터페이스 구현 추출 (class, interface 타입)
- **그래프 시각화** — 계층형 레이아웃으로 의존 관계 표시
- **에러 처리** — 파싱 실패 파일은 건너뛰고 상태 바에 결과 표시

### Sprint 2+ (예정)
- **노드 인터랙션** — 호버 툴팁, 클릭 시 1-hop 포커스 모드, 클래스명 검색
- **필드 타입 의존성** — 필드로 참조하는 타입 간 의존성 추출
- **엣지 색상 구분** — 상속 / 인터페이스 / 필드 의존성 시각적 구분
- **순환 의존성 감지** — 순환 관계 경고 표시
- **코드 스멜 지표** — 클래스별 참조 횟수, 의존도 지수 시각화

## 스크린샷

> Sprint 1 완료 후 추가 예정

---

## 시작하기

### 요구 사항

- Windows 10 이상
- .NET 8 Runtime
- Visual Studio 2022

### 빌드

```bash
git clone https://github.com/khwoo0527/code-archaeology-tool.git
cd code-archaeology-tool
```

Visual Studio에서 `CodeArchaeology.sln`을 열고 빌드(`Ctrl+Shift+B`)

### 실행

빌드 후 `bin/Debug/CodeArchaeology.exe` 실행
또는 Visual Studio에서 `F5`

---

## 사용 방법

1. 툴바의 **[폴더 열기]** 클릭
2. 분석할 C# 프로젝트 폴더 선택
3. 그래프가 자동으로 렌더링됨
4. 상태 바에서 분석된 클래스 수와 에러 파일 수 확인
5. 줌/팬으로 그래프 탐색 (GViewer 내장 기능)

---

## 기술 스택

| 영역 | 기술 |
|------|------|
| UI | WinForms (.NET 8) |
| 코드 분석 | Microsoft.CodeAnalysis.CSharp (Roslyn) |
| 그래프 렌더링 | Microsoft.Msagl + GraphViewerGdi |
| 빌드 | MSBuild / Visual Studio 2022 |

---

## 프로젝트 구조

```
CodeArchaeology/
├── UI/               # WinForms (MainForm, GraphControl)
├── Analysis/         # Roslyn 분석 (FolderScanner, RoslynAnalyzer)
├── Model/            # 그래프 모델 (Node, Edge, GraphModel)
└── Rendering/        # MSAGL 렌더링 (MsaglRenderer)
```

---

## 왜 Code Archaeology인가?

| 기존 도구 | 한계 |
|----------|------|
| VS 클래스 다이어그램 | 클래스를 수동으로 하나씩 추가해야 함 |
| NDepend / Resharper | 유료, 팀 전체 도입 비용 |
| Graphviz | DOT 언어를 직접 작성해야 함 |
| UML 도구 | 코드와 다이어그램이 따로 관리되어 항상 불일치 |

**Code Archaeology**: 폴더 선택 한 번 → 로컬에서 즉시 분석 → 항상 현재 코드 기준

---

## 로드맵

| Sprint | 목표 |
|--------|------|
| **Sprint 1** ← 현재 | 폴더 열기 → class/interface 분석 → 기본 그래프 표시 |
| Sprint 2 | 노드 인터랙션, 필드 의존성, 엣지 색상 구분, 순환 감지, 코드 스멜 |
| Sprint 3 | 네임스페이스 필터링, 변경 영향 분석 |
| Sprint 4 | PNG/SVG 내보내기, 프로그레스 바, 전체 안정화 |

자세한 내용은 [ROADMAP.md](./ROADMAP.md) 참조

---

## 문서

- [PRD.md](./PRD.md) — 제품 요구사항 정의서
- [ROADMAP.md](./ROADMAP.md) — 전체 개발 로드맵

---

## 라이선스

MIT
