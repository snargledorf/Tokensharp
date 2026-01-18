using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class MultipleStateLookup<TTokenType>(FrozenDictionary<char, IState<TTokenType>> states) 
    : IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryGetState(char c, [NotNullWhen(true)] out IState<TTokenType>? state)
    {
        return states.TryGetValue(c, out state);
    }
}