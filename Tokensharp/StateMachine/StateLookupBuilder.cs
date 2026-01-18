using System.Collections.Frozen;

namespace Tokensharp.StateMachine;

internal class StateLookupBuilder<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly Dictionary<char, IState<TTokenType>> _states = new();
    
    public void Add(char character, IState<TTokenType> state)
    {
        _states.Add(character, state);
    }
    
    public IStateLookup<TTokenType> Build()
    {
        if (_states.Count == 1)
        {
            KeyValuePair<char, IState<TTokenType>> first = _states.First();
            return new SingleStateLookup<TTokenType>(first.Key, first.Value);
        }
        
        return new MultipleStateLookup<TTokenType>(_states.ToFrozenDictionary());
    }
}