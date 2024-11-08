namespace Tokensharp;

public interface ITokenType<out T>where T : ITokenType<T>
{
    static abstract T Create(string lexeme);
    static abstract IEnumerable<T> TokenTypes { get; }
}