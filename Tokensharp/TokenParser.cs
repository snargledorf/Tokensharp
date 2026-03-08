using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Tokensharp.StateMachine;
using Tokensharp.TokenTree;

namespace Tokensharp;

public readonly ref struct TokenParser<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly CharacterIdMap _characterIdMap;
    private readonly int[][] _stateTransitions;
    private readonly TTokenType[] _stateToTokenType;
    
    private readonly StartState<TTokenType> _startState;

    public TokenParser() : this(TTokenType.Configuration)
    {
    }

    public TokenParser(TokenConfiguration<TTokenType> tokenConfiguration)
    {
        _startState = tokenConfiguration.StartState;

        ITokenTreeNode<TTokenType> tokenTree = tokenConfiguration.TokenTree;
        
        HashSet<char> allCharacters = tokenTree.AllCharacters;

        _characterIdMap = new CharacterIdMap(allCharacters);

        var stateTransitions = new List<int[]>();
        
        BuildTransitions(stateTransitions, tokenTree);

        _stateTransitions = new int[stateTransitions.Count][];
        for (int i = 0; i < stateTransitions.Count; i++)
            _stateTransitions[i] = stateTransitions[i].ToArray();
    }

    private void BuildTransitions(List<int[]> stateTransitions, ITokenTreeNode<TTokenType> node)
    {
        // TODO Set state to token type. Need to have the state ID (which is currently the current count of state transitions)
        
        int maxCharacterId = GetMaxChildCharacterId(node);

        var transitions = new int[maxCharacterId + 1];
        stateTransitions.Add(transitions);

        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            int charId = _characterIdMap[childNode.Character];
            transitions[charId] = stateTransitions.Count;
            BuildTransitions(stateTransitions, childNode);
        }
    }

    private int GetMaxChildCharacterId(ITokenTreeNode<TTokenType> node)
    {
        int maxCharacterId = 0;
        
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            int characterId = _characterIdMap[childNode.Character];
            if (characterId > maxCharacterId)
                maxCharacterId = characterId;
        }

        return maxCharacterId;
    }

    public bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable,
        [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out ReadOnlySpan<char> lexeme)
    {
        if (TryParseToken(buffer, moreDataAvailable, out tokenType, out int length))
        {
            lexeme = buffer[..length];
            return true;
        }

        tokenType = null;
        lexeme = default;
        return false;
    }
    
    public bool TryParseToken(ReadOnlySpan<char> buffer, bool moreDataAvailable, [NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out int length)
    {
        int currentState = 0;
        var context = new StateMachineContext();

        tokenType = null;
        length = 0;

        foreach (char c in buffer)
        {
            if (_characterIdMap.TryGetCharacterId(c, out int characterId))
            {
                int[] transitions = _stateTransitions[currentState];

                if (characterId < transitions.Length)
                {
                    int nextState = transitions[characterId];
                    if (nextState != -1)
                    {
                        currentState = nextState;
                        length++;
                    }
                }
            }
            else
            {
                length++;
            }
        }

        if (!moreDataAvailable)
        {
            while (!currentState.IsEndOfToken)
                currentState = currentState.PerformDefaultTransition(ref context);
        }
            
        Debug.Assert(currentState is not null, "Default state transition resulted in null state");
            
        return currentState.FinalizeToken(ref context, ref tokenType, ref length);
    }
}