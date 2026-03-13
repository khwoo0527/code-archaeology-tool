// S-07 검증용 샘플 — 확인 후 삭제
namespace TestSample;

public interface IAnimal
{
    void Speak();
}

public class Animal { }

public class Dog : Animal, IAnimal
{
    public void Speak() { }
}

public class Cat : Animal
{
}
