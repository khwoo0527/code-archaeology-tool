# Code Archaeology

C# 프로젝트 폴더를 열면 Roslyn으로 클래스 구조와 의존성을 자동 분석하고,
인터랙티브 그래프로 시각화하는 WinForms 데스크톱 도구

---

## 주요 기능

- **폴더 열기** — C# 프로젝트 폴더를 선택하면 자동 분석 시작
- **의존성 분석** — 클래스 상속 / 인터페이스 구현 / 필드 타입 의존성 추출
- **그래프 시각화** — 계층형 레이아웃, 의존성 종류별 엣지 색상 구분
- **노드 인터랙션** — 호버 툴팁, 클릭 시 1-hop 포커스 모드, 클래스명 검색
- **에러 처리** — 파싱 실패 파일은 건너뛰고 결과를 상태 바에 표시

## 스크린샷

> Sprint 1 완료 후 추가 예정

---

## 시작하기

### 요구 사항

- Windows 10 이상
- .NET Framework 4.8 (Windows 기본 내장)
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
4. 노드를 클릭하면 직접 연결된 클래스만 강조 표시
5. 검색창에 클래스명을 입력하면 해당 노드 하이라이트

---

## 기술 스택

| 영역 | 기술 |
|------|------|
| UI | WinForms (.NET Framework 4.8) |
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

## 로드맵

| Sprint | 목표 |
|--------|------|
| **Sprint 1** ← 현재 | 폴더 열기 → 기본 그래프 표시 (MVP) |
| Sprint 2 | 메서드 호출 의존성, 순환 의존성 감지, 코드 스멜 지표 |
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
