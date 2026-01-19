using System.Diagnostics.CodeAnalysis;
using SwiftState;

namespace Tokensharp.StateMachine;

internal class StateLookupBuilder<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    private readonly AlwaysFalseLookup _noStatesLookup = new();

    public void Add(char character, IState<TTokenType> state)
    {
        _states.Add(character, state);
    }

    public IStateLookup<TTokenType> Build()
    {
        if (_states.Count == 0)
            return _noStatesLookup;

        if (_states.Count == 1)
        {
            (char key, IState<TTokenType> value) = _states.First();
            return new SingleStateLookup<TTokenType>(key, value);
        }

        var swiftStateBuilder = new StateBuilder<char, SwiftStateId<TTokenType>>(new SwiftStateId<TTokenType>());

        foreach ((char character, IState<TTokenType> state) in _states)
            swiftStateBuilder.When(character, new SwiftStateId<TTokenType>(state));

        return new MultipleStateLookup<TTokenType>(swiftStateBuilder.Build());
    }

    private class AlwaysFalseLookup : IStateLookup<TTokenType>
    {
        public bool TryGetState(char c, [NotNullWhen(true)] out IState<TTokenType>? state)
        {
            state = null;
            return false;
        }
    }
}