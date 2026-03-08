using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal sealed class MultipleStateLookup<TTokenType> : StateLookup<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly State<TTokenType>?[] _states;
    private readonly int _maxChar;
    private readonly int _offset;

    public MultipleStateLookup(FrozenDictionary<char, State<TTokenType>> dictionary)
    {
        _offset = dictionary.Min(x => (int)x.Key);
        _maxChar = dictionary.Max(x => (int)x.Key);
        
        _states = new State<TTokenType>[_maxChar - _offset + 1];
        foreach (KeyValuePair<char, State<TTokenType>> keyValuePair in dictionary)
            _states[keyValuePair.Key - _offset] = keyValuePair.Value;
    }

    public override bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? state)
    {
        if (c < _offset || c > _maxChar)
        {
            state = null;
            return false;
        }

        state = _states[c - _offset];
        return state != null;
    }
}