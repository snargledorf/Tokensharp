using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal sealed class MultipleStateLookup<TTokenType>(FrozenDictionary<char, State<TTokenType>> dictionary)
    : StateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? state)
    {
        return dictionary.TryGetValue(c, out state);
    }
}