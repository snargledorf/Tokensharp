using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal interface IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    bool TryGetState(in char c, [NotNullWhen(true)] out IState<TTokenType>? state);
}