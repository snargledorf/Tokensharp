using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class MultipleStateLookup<TTokenType>(FrozenDictionary<char, State<TTokenType>> predicateMap)
    : IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? state)
    {
        return predicateMap.TryGetValue(c, out state);
    }
}