using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp;

public readonly ref struct TokenParser<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly CharacterIdMap _characterIdMap;
    private readonly int[][] _stateTransitions;
    private readonly TTokenType?[] _stateToTokenType;
    private readonly bool[] _isEndOfToken;
    private readonly bool _numbersAreText;

    public TokenParser() : this(TTokenType.Configuration)
    {
    }

    public TokenParser(TokenConfiguration<TTokenType> tokenConfiguration)
    {
        ITokenTreeNode<TTokenType> tokenTree = tokenConfiguration.TokenTree;
        _numbersAreText = tokenConfiguration.NumbersAreText;
        
        HashSet<char> allCharacters = tokenTree.AllCharacters;

        _characterIdMap = new CharacterIdMap(allCharacters);

        var stateTransitions = new List<int[]>();
        var stateToTokenType = new List<TTokenType?>();
        
        BuildTransitions(stateTransitions, stateToTokenType, tokenTree);

        _stateTransitions = [..stateTransitions];
        _stateToTokenType = [..stateToTokenType];

        _isEndOfToken = new bool[_stateToTokenType.Length];
        for(var i = 0; i < _stateToTokenType.Length; i++)
        {
            if (_stateToTokenType[i] is not null)
            {
                _isEndOfToken[i] = true;
            }
        }
    }

    private void BuildTransitions(List<int[]> stateTransitions, List<TTokenType?> stateToTokenType, ITokenTreeNode<TTokenType> node)
    {
        stateToTokenType.Add(node.TokenType);
        
        int maxCharacterId = GetMaxChildCharacterId(node);

        var transitions = new int[maxCharacterId + 1];
        Array.Fill(transitions, -1);
        stateTransitions.Add(transitions);

        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            int charId = _characterIdMap[childNode.Character];
            transitions[charId] = stateTransitions.Count;
            BuildTransitions(stateTransitions, stateToTokenType, childNode);
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
        tokenType = null;
        length = 0;

        int lastMatchLength = 0;
        TTokenType? lastMatchTokenType = null;

        if (_isEndOfToken[currentState] && _stateToTokenType[currentState] is not null)
        {
            lastMatchTokenType = _stateToTokenType[currentState];
            lastMatchLength = length;
        }

        for (int i = 0; i < buffer.Length; i++)
        {
            char c = buffer[i];
            
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

                        if (_isEndOfToken[currentState] && _stateToTokenType[currentState] is not null)
                        {
                            lastMatchTokenType = _stateToTokenType[currentState];
                            lastMatchLength = length;
                        }
                        continue;
                    }
                }
            }

            // Character not in map or no transition
            break;
        }

        if (lastMatchTokenType is not null)
        {
            // If moreDataAvailable is true, and we hit the end of the buffer, we might be in the middle of a larger token
            // that is a prefix of another valid token but we don't have enough data yet.
            // Wait, if moreDataAvailable, and length == buffer.Length, and it matched a token, should we return it or wait?
            // The original code handled this via 'PerformDefaultTransition' and checking if it's EndOfToken.
            // Actually, if we successfully consumed all buffer and more data is available, 
            // the state machine would wait by returning false here, allowing caller to buffer more.
            if (moreDataAvailable && length == buffer.Length)
            {
                // Is there a possible transition from the current state?
                // If there's any transition from here, we might need more data.
                if (HasPossibleTransitions(currentState))
                {
                    return false;
                }
            }
            
            tokenType = lastMatchTokenType;
            length = lastMatchLength;
            return true;
        }

        // Handle non-token matches (text, whitespace, numbers)
        if (buffer.IsEmpty)
        {
            return false;
        }

        char firstChar = buffer[0];
        if (char.IsWhiteSpace(firstChar))
        {
            tokenType = TokenType<TTokenType>.WhiteSpace;
            length = 1;
            while (length < buffer.Length && char.IsWhiteSpace(buffer[length]))
            {
                length++;
            }
            if (moreDataAvailable && length == buffer.Length)
                return false;
            return true;
        }

        if (!_numbersAreText && char.IsDigit(firstChar))
        {
            tokenType = TokenType<TTokenType>.Number;
            length = 1;
            while (length < buffer.Length && char.IsDigit(buffer[length]))
            {
                length++;
            }
            if (moreDataAvailable && length == buffer.Length)
                return false;
            return true;
        }
        
        tokenType = TokenType<TTokenType>.Text;
        length = 1;
        while (length < buffer.Length)
        {
            char c = buffer[length];
            if (char.IsWhiteSpace(c))
                break;
            if (!_numbersAreText && char.IsDigit(c))
                break;
            
            // Check if the current character could start a defined token
            if (_characterIdMap.TryGetCharacterId(c, out int characterId))
            {
                int[] transitions = _stateTransitions[0]; // Start state transitions
                if (characterId < transitions.Length && transitions[characterId] != -1)
                {
                    break;
                }
            }
            
            length++;
        }

        if (moreDataAvailable && length == buffer.Length)
            return false;
            
        return true;
    }

    private bool HasPossibleTransitions(int stateId)
    {
        int[] transitions = _stateTransitions[stateId];
        for (int i = 0; i < transitions.Length; i++)
        {
            if (transitions[i] != -1)
                return true;
        }
        return false;
    }
}