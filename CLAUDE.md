# CLAUDE.md — Code Archaeology

이 파일은 Claude Code가 새 세션을 시작할 때 자동으로 읽는 AI 컨텍스트 파일입니다.
새 대화를 시작하면 반드시 이 파일을 먼저 읽고 현재 상태를 파악하세요.

---

## 1. 프로젝트 개요

**Code Archaeology**는 C# 프로젝트 폴더를 열면 Roslyn으로 클래스 구조와 의존성을 자동 분석하고, Microsoft.Msagl 계층형 그래프로 시각화하는 WinForms 데스크톱 도구입니다.

- **문서**: [`PRD.md`](./PRD.md) — 요구사항 전체 명세
- **로드맵**: [`ROADMAP.md`](./ROADMAP.md) — Sprint 계획 및 타임라인
- **저장소**: https://github.com/khwoo0527/code-archaeology-tool

---

## 2. 현재 상태

| 항목 | 내용 |
|------|------|
| 현재 Sprint | **Sprint 1** |
| Sprint 목표 | C# 폴더를 열면 기본 그래프가 표시되는 상태 |
| 진행 상황 | 문서화 완료, 구현 시작 전 |
| 브랜치 | `master` |

> Sprint 진행 상황은 [`docs/sprints/sprint-1.md`](./docs/sprints/sprint-1.md)에서 실시간 업데이트됩니다.

---

## 3. 기술 스택

| 영역 | 기술 | 비고 |
|------|------|------|
| UI | WinForms (.NET 8) | C# 12 지원 |
| 코드 분석 | Microsoft.CodeAnalysis.CSharp (Roslyn) | SyntaxTree 기반 정적 분석 |
| 그래프 렌더링 | Microsoft.Msagl + GraphViewerGdi | 계층형(Sugiyama) 레이아웃 |
| 빌드 | MSBuild / Visual Studio 2022 | |

---

## 4. 프로젝트 구조

```
CodeArchaeology/           ← Visual Studio 솔루션 루트 (구현 시작 후 생성)
├── Models/                ← TypeNode, DependencyEdge, AnalysisResult
├── Analysis/              ← FolderScanner, RoslynAnalyzer
├── Rendering/             ← MsaglRenderer
└── UI/                    ← MainForm (WinForms)

docs/
└── sprints/
    └── sprint-1.md        ← Sprint 1 진행 기록

PRD.md                     ← 제품 요구사항 정의서 (v1.1)
ROADMAP.md                 ← 전체 개발 로드맵
CLAUDE.md                  ← 이 파일 (AI 컨텍스트)
```

---

## 5. 아키텍처 원칙

**레이어 분리 (변경 금지):**
```
UI (WinForms) → Analysis (Roslyn) → Model (Graph)
```

- UI는 Analysis를 직접 호출하지 않고, Model을 통해 데이터를 받는다
- RoslynAnalyzer는 WinForms에 의존하지 않는다
- MsaglRenderer는 AnalysisResult만 입력으로 받는다

**Karpathy 원칙 적용:**
- 단순하게 시작: 동기 처리 → 작동 확인 후 비동기로 전환
- 직접 눈으로 확인: 각 태스크는 30분 내 결과를 눈으로 확인 가능한 크기
- 과도한 추상화 경계: 지금 필요 없는 인터페이스/추상 클래스는 만들지 않는다

---

## 6. Sprint 1 핵심 구현 순서

> 자세한 내용은 ROADMAP.md Phase 1~3 참조

### 반드시 완료 (Core)
1. WinForms 프로젝트 셋업 + NuGet 3종 설치
2. MainForm 레이아웃 (Toolbar + Panel + StatusBar)
3. FolderScanner (`.cs` 파일 재귀 수집)
4. RoslynAnalyzer — class 노드 추출 → interface 노드 추출 → 상속/인터페이스 엣지 추출
5. MsaglRenderer — 노드 렌더링 → 엣지 추가
6. 폴더 열기 파이프라인 연결
7. StatusBar 연동

### 시간 남으면 (Extension)
- 필드 타입 의존성 추출
- 엣지 색상/스타일 구분 (상속:검정/인터페이스:파랑/필드:회색)
- partial class 병합
- 비동기(`Task.Run`) 처리

---

## 7. Sprint 1 완료 기준

| 기준 | 확인 방법 |
|------|----------|
| C# 폴더 열기 → 3클릭 이내 그래프 표시 | 직접 실행 테스트 |
| class/interface 노드가 그래프에 표시됨 | 샘플 프로젝트로 확인 |
| 상속/인터페이스 구현 엣지가 표시됨 | 샘플 프로젝트로 확인 |
| 에러 파일 포함 폴더도 정상 파일은 분석됨 | 의도적 에러 파일 포함 테스트 |
| StatusBar에 분석 결과 요약 표시 | 직접 확인 |

---

## 8. 코드 컨벤션

- **언어**: C# 12 (.NET 8)
- **네이밍**: PascalCase (클래스/메서드), camelCase (로컬 변수), `_camelCase` (private 필드)
- **파일 1개 = 클래스 1개** 원칙
- 주석은 비즈니스 의도가 불분명한 곳에만 작성 (자명한 코드에는 생략)

---

## 9. 작업 시 주의사항

- `참고.txt`, `평가기준.txt`는 gitignore 처리됨 — 커밋하지 말 것
- `bin/`, `obj/`, `.vs/`는 gitignore 처리됨
- 커밋 메시지는 `type: 설명` 형식 사용 (`feat`, `fix`, `docs`, `chore`, `refactor`)
- 구현 중 ROADMAP.md의 체크박스(`[ ]`)를 완료 시 `[x]`로 업데이트할 것
- Sprint 진행 상황은 `docs/sprints/sprint-1.md`에 실시간 기록할 것

---

## 9-1. 태스크 진행 원칙 (반드시 준수)

**태스크 1개 완료 사이클:**
```
1. 구현
2. 빌드/실행으로 검증 체크리스트 확인
3. 사용자에게 결과 보고 → 사용자 직접 확인 (F5 등)
4. git commit
5. 다음 태스크로 이동
```

- **여러 태스크를 연속으로 진행하지 않는다** — 반드시 사용자 확인 후 커밋 완료 시 다음 태스크 시작
- 태스크 완료 후 ROADMAP.md 체크박스 `[x]` 업데이트 + `docs/sprints/sprint-1.md` 기록은 커밋에 포함

---

## 10. 다음 세션에서 할 일

새 세션 시작 시:
1. 이 파일 읽기 (자동)
2. `docs/sprints/sprint-1.md` 읽어서 마지막 진행 상태 파악
3. ROADMAP.md에서 완료된 체크박스 확인
4. 중단된 지점부터 이어서 진행
