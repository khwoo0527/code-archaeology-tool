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

> Microsoft C# 공식 스타일 가이드 및 .NET 팀 내부 코딩 표준을 기반으로,
> 이 프로젝트의 가독성·일관성·유지보수성을 위한 규칙을 정의한다.
> **모든 코드는 아래 규칙을 준수하며, 리뷰 시 체크포인트로 활용한다.**

---

### 8-1. 언어 및 플랫폼

- **언어**: C# 12 / .NET 8
- **Nullable**: 프로젝트 전체 `<Nullable>enable</Nullable>` — null 가능성은 반드시 `?`로 명시
- **ImplicitUsings**: `enable` — `System`, `System.Collections.Generic` 등 전역 using 자동 포함
- **최신 문법 적극 활용**:
  - `var` (타입 명확한 경우), `using` 선언, target-typed `new()`, 파일 범위 네임스페이스
  - Pattern matching (`is`, `switch` expression), null 병합 (`??`, `??=`), `is not null`
  - 단일 표현식 메서드 `=>` (람다 바디)

---

### 8-2. 네이밍 규칙

| 대상 | 규칙 | 예시 |
|------|------|------|
| 클래스 / 구조체 / 레코드 | PascalCase | `TypeNode`, `AnalysisResult` |
| 인터페이스 | `I` + PascalCase | `IFolderScanner`, `IAnalyzer` |
| Enum 타입 / 값 | PascalCase | `EdgeType`, `Inheritance` |
| 메서드 / 프로퍼티 / 이벤트 | PascalCase | `GetCsFiles()`, `FullName`, `OnAnalysisComplete` |
| private / protected 필드 | `_camelCase` | `_lastFolderPath`, `_nodes` |
| 로컬 변수 / 파라미터 | camelCase | `folderPath`, `nodeList`, `sourceFile` |
| 상수 (`const` / `static readonly`) | PascalCase | `MaxNodeCount`, `DefaultLayout` |
| 제네릭 타입 파라미터 | `T` 또는 `T` + 설명 | `T`, `TResult`, `TNode` |
| bool 변수 / 프로퍼티 | `is` / `has` / `can` 접두사 | `isVisible`, `hasErrors`, `canRefresh` |

> **이름은 축약하지 않는다.** `mgr` → `manager`, `btn` → `button` (단, 관용적 약어 `Id`, `Url`, `Html` 등은 허용)

---

### 8-3. 파일 및 프로젝트 구조

- **파일 1개 = 타입 1개** — 클래스, 인터페이스, enum 각각 별도 파일
- **파일명 = 타입명** (예: `TypeNode.cs` → `class TypeNode`)
- **파일 범위 네임스페이스** 사용 (중괄호 없이 한 줄로):
  ```csharp
  namespace CodeArchaeology.Models;   // ✅
  namespace CodeArchaeology.Models { } // ❌
  ```
- **네임스페이스**: `CodeArchaeology.{레이어}` 형식
  - `CodeArchaeology.Models` / `CodeArchaeology.Analysis` / `CodeArchaeology.Rendering` / `CodeArchaeology.UI`
- **using 정렬 순서** (자동 정렬 기준):
  1. `System.*`
  2. 서드파티 (`Microsoft.CodeAnalysis.*`, `Microsoft.Msagl.*`)
  3. 프로젝트 내부 (`CodeArchaeology.*`)

---

### 8-4. 코드 스타일

#### 중괄호
- **항상 Allman 스타일** — 중괄호는 반드시 새 줄에
  ```csharp
  // ✅
  if (condition)
  {
      DoSomething();
  }
  // ❌
  if (condition) { DoSomething(); }
  ```
- 단, 단일 표현식 프로퍼티·메서드는 람다 바디 허용:
  ```csharp
  public string FullName => $"{Namespace}.{Name}";   // ✅
  ```

#### 접근 제한자
- **항상 명시** — `private`, `public` 생략 금지
- 순서: `접근제한자 → static → readonly → 타입 → 이름`
  ```csharp
  private static readonly int MaxCount = 100;   // ✅
  ```

#### var 사용 기준
```csharp
var nodes = new List<TypeNode>();          // ✅ 우변에서 타입 명확
var graph = new Microsoft.Msagl...Graph(); // ✅ 명확
AnalysisResult result = GetResult();       // ✅ 우변 타입 불명확 — 명시
```

#### 컬렉션 초기화
```csharp
public List<TypeNode> Nodes { get; set; } = new();      // ✅ target-typed new
public List<string> Errors { get; set; } = new List<string>(); // ❌ 장황
```

#### 문자열
- 보간 문자열 우선: `$"{Namespace}.{Name}"` — 단순 `+` 연결 지양
- 긴 메시지: `string.Format()` 또는 여러 줄 보간 사용

#### 조건문
- 부정 조건은 early return으로 처리 (중첩 줄이기):
  ```csharp
  // ✅ early return
  if (string.IsNullOrEmpty(folderPath)) return;
  DoWork(folderPath);

  // ❌ 중첩
  if (!string.IsNullOrEmpty(folderPath))
  {
      DoWork(folderPath);
  }
  ```
- `switch` 대신 `switch expression` 활용 권장

#### LINQ
- 메서드 체이닝 스타일 우선 (쿼리 구문 지양):
  ```csharp
  var classNodes = nodes.Where(n => n.Kind == TypeKind.Class).ToList();  // ✅
  ```
- 한 줄이 길어지면 각 메서드를 줄 바꿈:
  ```csharp
  var result = nodes
      .Where(n => n.Kind == TypeKind.Class)
      .OrderBy(n => n.Name)
      .ToList();
  ```

---

### 8-5. 클래스 멤버 선언 순서

같은 클래스 내 멤버는 아래 순서로 선언한다:

1. `const` / `static readonly` 필드
2. `private` 필드
3. 생성자
4. `public` 프로퍼티
5. `public` 메서드
6. `private` / `protected` 메서드

---

### 8-6. 주석

- **자명한 코드에는 주석 생략** — 코드 자체가 의도를 드러내야 한다
- **주석이 필요한 경우**: 비즈니스 로직, 비직관적 결정, 외부 API 제약, 알려진 한계
  ```csharp
  // Roslyn SyntaxTree만으로는 외부 타입 판별 불가 — 이름 기반 필터링으로 대체
  var internalOnly = edges.Where(e => knownTypes.Contains(e.Target));
  ```
- `TODO:` / `FIXME:` / `HACK:` 태그 적극 활용 (이슈 추적 용도)
- Public 클래스·메서드는 `///` XML 문서 주석 권장

---

### 8-7. 예외 및 방어 코드

- **외부 입력 경계에서만 검증** — 내부 메서드 간 과도한 null 체크 지양
- `try-catch`는 예외를 **절대 무시하지 않는다** — 반드시 `Errors` 리스트 기록 또는 상위 전파
  ```csharp
  // ✅
  try { /* 파일 파싱 */ }
  catch (Exception ex) { result.Errors.Add($"{filePath}: {ex.Message}"); }

  // ❌ 예외 무시
  catch (Exception) { }
  ```
- 예외 타입은 구체적으로 — `catch (Exception)` 최상위 남용 금지
- `using` 선언으로 리소스 해제 보장:
  ```csharp
  using var dialog = new FolderBrowserDialog();   // ✅ 자동 Dispose
  ```

---

### 8-8. 비동기 (Async/Await)

- 비동기 메서드명은 `Async` 접미사 필수: `AnalyzeAsync()`, `LoadFilesAsync()`
- `async void` 는 이벤트 핸들러에서만 허용 (그 외 `async Task` 사용)
- `Task.Run()`은 CPU 바운드 작업에만 사용 — I/O는 네이티브 `async` API 사용
- `ConfigureAwait(false)` — UI 컨텍스트가 불필요한 라이브러리 코드에 적용

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
3. 사용자에게 번호 매긴 검증 절차 제시 → 사용자 직접 수행
4. git commit (ROADMAP 체크박스 + sprint 기록 포함)
5. 다음 태스크로 이동
```

- **여러 태스크를 연속으로 진행하지 않는다** — 반드시 사용자 확인 후 커밋 완료 시 다음 태스크 시작
- 태스크 완료 후 ROADMAP.md 체크박스 `[x]` 업데이트 + `docs/sprints/sprint-1.md` 기록은 커밋에 포함

**검증 절차 제시 형식** — 태스크 완료 시 아래 형식으로 빠짐없이 안내:
```
1. [실행 방법] — 어떻게 실행하는지
2. [확인 항목] — 무엇이 보여야 하는지 (기대값 명시)
3. [엣지 케이스] — 놓치기 쉬운 추가 확인 항목
```
> 사용자가 절차를 그대로 따라가기만 해도 검증이 완료되는 수준으로 작성한다.

---

## 9-3. UI-First 검증 원칙

> "눈으로 보이는 것이 가장 확실한 증거다."

모든 태스크의 검증은 **실행 중인 앱의 UI에서 직접 확인**하는 것을 최우선으로 한다.

**원칙:**
- 내부 로직(분석 엔진, 데이터 모델 등) 구현 후에도 **결과를 UI에 노출**시켜 즉시 눈으로 확인
- 파이프라인이 완전히 연결되지 않은 단계에서는 **임시 검증 코드**(StatusBar 출력 등)를 활용
- 디버거·직접 실행 창 등 개발자 도구 의존 검증은 UI 확인이 불가능한 경우에만 사용

**임시 검증 코드 관리:**
- 임시 코드는 반드시 `// TODO S-XX: 파이프라인 연결 후 제거` 주석으로 명시
- 해당 태스크가 실제 파이프라인으로 대체되면 즉시 제거

**기대 효과:**
- 구현 오류를 가장 빠른 시점에 발견
- 사용자와 개발자가 동일한 화면을 보며 소통 → 피드백 루프 단축
- 해커톤 환경에서 "돌아가는 데모" 상태를 항상 유지

---

## 9-2. AI-Native 문서화 원칙

> 모든 기술적 결정과 협업 과정은 문서로 남긴다. 코드는 `what`을 말하고, 문서는 `why`를 말한다.

- **의사결정 즉시 문서화**: 구현 방향, 트레이드오프, 범위 변경 등 모든 결정은 ROADMAP.md 또는 sprint 문서에 기록
- **컨텍스트 보존**: 단순 요청도 아키텍처 원칙·개발 컨벤션·팀 규칙 형태로 다듬어 문서화
- **스프린트 실시간 갱신**: 태스크 완료 시마다 `docs/sprints/sprint-1.md`에 진행 상황, 이슈, 결정 사항 기록
- **지속적 문서 개선**: 개발 중 발견되는 인사이트·기술 부채·리스크는 발견 즉시 ROADMAP.md에 반영

---

## 10. 다음 세션에서 할 일

새 세션 시작 시:
1. 이 파일 읽기 (자동)
2. `docs/sprints/sprint-1.md` 읽어서 마지막 진행 상태 파악
3. ROADMAP.md에서 완료된 체크박스 확인
4. 중단된 지점부터 이어서 진행
