using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class StateLookupBuilder<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    private readonly AlwaysFalseLookup _noStatesLookup = new();

    public void Add(char character, State<TTokenType> state)
    {
        _states.Add(character, state);
    }

    public IStateLookup<TTokenType> Build()
    {
        if (_states.Count == 0)
            return _noStatesLookup;

        if (_states.Count == 1)
        {
            (char character, IState<TTokenType> state) = _states.First();
            return new SingleStateLookup<TTokenType>(character, state);
        }

        return new MultipleStateLookup<TTokenType>(_states.ToFrozenDictionary());
    }

    private class AlwaysFalseLookup : IStateLookup<TTokenType>
    {
        public bool TryGetState(in char c, [NotNullWhen(true)] out IState<TTokenType>? state)
        {
            state = null;
            return false;
        }
    }
}