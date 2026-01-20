using System.Diagnostics.CodeAnalysis;
using PredicateMap;

namespace Tokensharp.StateMachine;

internal class MultipleStateLookup<TTokenType>(IPredicateMap<char, State<TTokenType>> predicateMap)
    : IStateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? state)
    {
        return predicateMap.TryGet(c, out state);
    }
}