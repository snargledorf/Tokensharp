using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Tokensharp.StateMachine;

internal class StateLookupBuilder<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, State<TTokenType>> _states = new();
    
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
            (char character, State<TTokenType> state) = _states.First();
            return new SingleStateLookup<TTokenType>(character, state);
        }

        var swiftStateBuilder = new Dictionary<char, IState<TTokenType>>();

        foreach ((char character, State<TTokenType> state) in _states)
            swiftStateBuilder.Add(character, state);

        return new MultipleStateLookup<TTokenType>(swiftStateBuilder.ToFrozenDictionary());
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