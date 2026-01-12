using Tokensharp.StateMachine;

namespace Tokensharp;

public interface ITokenType<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    static abstract TTokenType Create(string identifier);
    static abstract TokenConfiguration<TTokenType> Configuration { get; }
}