using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal sealed class StateLookupBuilder<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, State<TTokenType>> _states = new();
    
    private readonly AlwaysFalseLookup _noStatesLookup = new();

    public void Add(char character, State<TTokenType> state)
    {
        _states.Add(character, state);
    }

    public StateLookup<TTokenType> Build()
    {
        if (_states.Count == 0)
            return _noStatesLookup;

        if (_states.Count == 1)
        {
            (char character, State<TTokenType> state) = _states.First();
            return new SingleStateLookup<TTokenType>(character, state);
        }

        return new MultipleStateLookup<TTokenType>(_states.ToFrozenDictionary());
    }

    private class AlwaysFalseLookup : StateLookup<TTokenType>
    {
        public override bool TryGetState(char c, [NotNullWhen(true)] out State<TTokenType>? state)
        {
            state = null;
            return false;
        }
    }
}