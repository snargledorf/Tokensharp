using Tokensharp.StateMachine;

namespace Tokensharp;

public interface ITokenType<out TTokenType> where TTokenType : ITokenType<TTokenType>
{
    static abstract TTokenType Create(string lexeme);
    static abstract IEnumerable<TTokenType> TokenTypes { get; }

    public static TokenConfiguration<TTokenType> DefaultConfiguration =>
        field ??= new TokenConfiguration<TTokenType>(TTokenType.TokenTypes);
}