---
name: Code Archaeology 프로젝트 컨텍스트
description: C# 의존성 시각화 WinForms 도구 - 기술스택, 스프린트 구조, 주요 결정사항
type: project
---

## 프로젝트 개요
- C# 프로젝트 폴더를 Roslyn으로 분석하여 클래스 의존성 그래프를 시각화하는 WinForms 데스크톱 도구
- 기술스택: WinForms + .NET Framework 4.8 + Roslyn + Microsoft.Msagl

## 로드맵 구조 (2026-03-13 생성)
- Phase 1-3: Sprint 1 MVP (폴더열기 -> 기본 그래프 표시, class/interface만, 의존성 3종)
- Phase 4: Sprint 2 (메서드 호출, 순환 감지, 코드 스멜, struct/record/enum)
- Phase 5: Sprint 3 (네임스페이스 필터, 영향 분석)
- Phase 6: Sprint 4 (PNG 내보내기, 안정화)

## 주요 기술 결정
- SyntaxTree만 사용 (SemanticModel 미사용) - MVP 타협점, Phase 4에서 재검토
- 필드 타입 매칭은 이름 문자열 + using문 휴리스틱
- .NET Framework 4.8 = C# 7.3 제약
- GViewer 이벤트 모델 조기 검증 필요 (리스크 항목)
