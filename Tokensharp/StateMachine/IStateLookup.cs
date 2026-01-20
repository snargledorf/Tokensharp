using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal interface IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? state);
}