namespace Tokenizer;

public interface ITokenType<T>where T : ITokenType<T>
{
    static abstract T Create(string token, int id);
    static abstract T LastUserDefinedTokenType { get; }
}