using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal abstract class StateLookup<TTokenType> : IStateLookup<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public abstract bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? state);
}