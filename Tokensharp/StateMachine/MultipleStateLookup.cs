using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class MultipleStateLookup<TTokenType>(FrozenDictionary<char, IState<TTokenType>> dictionary)
    : IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryGetState(in char c, [NotNullWhen(true)] out IState<TTokenType>? state)
    {
        return dictionary.TryGetValue(c, out state);
    }
}