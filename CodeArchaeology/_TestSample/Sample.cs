// S-07 검증용 샘플 — 확인 후 삭제
namespace TestSample;

public interface IAnimal
{
    void Speak();
}

public class Animal { }

public class Dog : Animal, IAnimal
{
    private Cat _friend;         // FieldDependency: Dog → Cat
    public void Speak() { }
}

public class Cat : Animal
{
    private Dog _dog;            // FieldDependency: Cat → Dog (Dog↔Cat 순환!)
}

// 순환 의존성 검증용: A → B → C → A
public class NodeA { private NodeB _b; }
public class NodeB { private NodeC _c; }
public class NodeC { private NodeA _a; }
