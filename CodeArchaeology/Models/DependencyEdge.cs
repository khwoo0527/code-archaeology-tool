namespace CodeArchaeology.Models;

/// <summary>두 타입 간 의존성의 종류.</summary>
public enum EdgeType
{
    /// <summary>클래스 상속 (<c>class Dog : Animal</c>).</summary>
    Inheritance,

    /// <summary>인터페이스 구현 (<c>class Dog : IAnimal</c>).</summary>
    InterfaceImpl,

    /// <summary>필드 타입 참조 (<c>private OrderService _service;</c>).</summary>
    FieldDependency
}

/// <summary>
/// 두 타입 간의 단방향 의존성 엣지.
/// MsaglRenderer가 이 데이터를 기반으로 그래프 엣지를 색상/스타일로 구분하여 렌더링한다.
/// </summary>
public class DependencyEdge
{
    /// <summary>의존하는 쪽 타입 이름 (예: <c>Dog</c> → Animal 상속 시 <c>Dog</c>).</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>의존 대상 타입 이름 (예: <c>Animal</c>).</summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>의존성 종류 — 렌더링 색상/스타일 결정에 사용.</summary>
    public EdgeType Type { get; set; }
}
