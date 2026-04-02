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
        int maxCharacterId = -1;
        
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

        if (_isEndOfToken[currentState])
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

                        if (_isEndOfToken[currentState])
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

        if (moreDataAvailable && length == buffer.Length && HasPossibleTransitions(currentState))
        {
            return false;
        }

        if (lastMatchTokenType is not null)
        {
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
                if (StartsCustomToken(buffer[length..], moreDataAvailable))
                    break;
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
                if (StartsCustomToken(buffer[length..], moreDataAvailable))
                    break;
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
            
            if (StartsCustomToken(buffer[length..], moreDataAvailable))
                break;
            
            length++;
        }

        if (moreDataAvailable && length == buffer.Length)
            return false;
            
        return true;
    }

    private bool StartsCustomToken(ReadOnlySpan<char> buffer, bool moreDataAvailable)
    {
        if (buffer.IsEmpty)
            return false;

        char c = buffer[0];
        if (!_characterIdMap.TryGetCharacterId(c, out int characterId))
            return false;

        int[] startTransitions = _stateTransitions[0];
        if (characterId >= startTransitions.Length || startTransitions[characterId] == -1)
            return false;

        int currentState = 0;
        int matchLength = 0;
        bool isPotentialPrefix = false;

        for (int i = 0; i < buffer.Length; i++)
        {
            c = buffer[i];
            
            if (_characterIdMap.TryGetCharacterId(c, out characterId))
            {
                int[] transitions = _stateTransitions[currentState];

                if (characterId < transitions.Length)
                {
                    int nextState = transitions[characterId];
                    if (nextState != -1)
                    {
                        currentState = nextState;
                        
                        if (_isEndOfToken[currentState])
                        {
                            matchLength = i + 1;
                        }
                        
                        if (i == buffer.Length - 1)
                        {
                            isPotentialPrefix = true;
                        }
                        
                        continue;
                    }
                }
            }

            break;
        }

        if (matchLength > 0)
            return true;
        
        if (moreDataAvailable && isPotentialPrefix)
        {
            if (HasPossibleTransitions(currentState))
                return true;
        }

        return false;
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