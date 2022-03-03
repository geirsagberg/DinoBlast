namespace BunnyLand.Debugger;

public class Wrapper<T>
{
    public T Value { get; set; }

    public Wrapper(T val)
    {
        Value = val;
    }

    public static implicit operator T(Wrapper<T> w)
    {
        return w.Value;
    }

    public static implicit operator Wrapper<T>(T s)
    {
        return new Wrapper<T>(s);
    }
}