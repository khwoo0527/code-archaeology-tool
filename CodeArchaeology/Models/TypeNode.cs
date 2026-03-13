namespace CodeArchaeology.Models;

/// <summary>C# 타입의 종류.</summary>
public enum TypeKind { Class, Interface, Struct, Record, Enum }

/// <summary>
/// 분석된 C# 타입 하나를 나타내는 그래프 노드.
/// Rendering 레이어의 MsaglRenderer가 이 데이터를 기반으로 시각 노드를 생성한다.
/// </summary>
public class TypeNode
{
    /// <summary>타입의 단순 이름 (예: <c>OrderService</c>).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>소속 네임스페이스 (예: <c>MyApp.Services</c>).</summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// 네임스페이스를 포함한 완전한 이름 (예: <c>MyApp.Services.OrderService</c>).
    /// 그래프 노드 ID로 사용되어 동일 이름의 타입을 다른 네임스페이스에서 구분한다.
    /// </summary>
    public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

    /// <summary>타입이 선언된 소스 파일의 절대 경로.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>타입 종류 (class / interface / struct / record / enum).</summary>
    public TypeKind Kind { get; set; }

    /// <summary>선언된 필드 수 (partial class는 전체 파일 합산).</summary>
    public int FieldCount { get; set; }

    /// <summary>선언된 메서드 수 (partial class는 전체 파일 합산).</summary>
    public int MethodCount { get; set; }

    /// <summary>필드 이름 목록 — Class Info 패널 펼치기에 사용.</summary>
    public List<string> FieldNames { get; set; } = new();

    /// <summary>메서드 이름 목록 — Class Info 패널 펼치기에 사용.</summary>
    public List<string> MethodNames { get; set; } = new();
}
