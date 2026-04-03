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
            {
                maxCharacterId = characterId;
            }
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
        var session = new ParseSession(this, buffer, moreDataAvailable);
        return session.Parse(out tokenType, out length);
    }

    private ref struct ParseSession
    {
        private readonly TokenParser<TTokenType> _parser;
        private readonly ReadOnlySpan<char> _buffer;
        private readonly bool _moreDataAvailable;

        private int _currentCustomState;
        private int _customTokenStartIndex;
        private TTokenType? _lastMatchTokenType;
        private int _lastMatchLength;

        private readonly TokenType<TTokenType> _defaultTokenType;
        private bool _defaultIsValid;
        private int _defaultTokenLength;

        public ParseSession(TokenParser<TTokenType> parser, ReadOnlySpan<char> buffer, bool moreDataAvailable)
        {
            _parser = parser;
            _buffer = buffer;
            _moreDataAvailable = moreDataAvailable;

            _currentCustomState = 0;
            _customTokenStartIndex = -1;
            _lastMatchTokenType = null;
            _lastMatchLength = 0;

            _defaultIsValid = true;
            _defaultTokenLength = 0;
            _defaultTokenType = GetDefaultTokenType(buffer);
        }

        public bool Parse([NotNullWhen(true)] out TokenType<TTokenType>? tokenType, out int length)
        {
            if (_buffer.IsEmpty)
            {
                tokenType = null;
                length = 0;
                return false;
            }

            int i = 0;
            for (; i < _buffer.Length; i++)
            {
                char c = _buffer[i];

                bool customTransitionFound = ProcessCustomTransition(c, i);

                if (!customTransitionFound && _currentCustomState != 0)
                {
                    if (_lastMatchTokenType != null)
                    {
                        ResolveMatch(out tokenType, out length);
                        return true;
                    }
                    
                    ResetCustomMatchState();
                    
                    if (!_defaultIsValid)
                    {
                        break;
                    }
                    
                    ProcessCustomTransition(c, i);
                }

                if (_defaultIsValid)
                {
                    _defaultIsValid = IsValidDefaultCharacter(c);
                    if (_defaultIsValid)
                    {
                        _defaultTokenLength = i + 1;
                    }
                }

                if (!_defaultIsValid && _currentCustomState == 0)
                {
                    break;
                }
            }

            if (_moreDataAvailable && i == _buffer.Length)
            {
                if (ShouldWaitMoreData())
                {
                    length = 0;
                    tokenType = null;
                    return false;
                }
            }

            if (_lastMatchTokenType != null)
            {
                ResolveMatch(out tokenType, out length);
                return true;
            }

            tokenType = _defaultTokenType;
            length = _defaultTokenLength;
            return true;
        }

        private TokenType<TTokenType> GetDefaultTokenType(ReadOnlySpan<char> buffer)
        {
            if (buffer.IsEmpty)
            {
                return TokenType<TTokenType>.Text;
            }
         
            char firstChar = buffer[0];
            if (char.IsWhiteSpace(firstChar))
            {
                return TokenType<TTokenType>.WhiteSpace;
            }
            
            if (!_parser._numbersAreText && char.IsDigit(firstChar))
            {
                return TokenType<TTokenType>.Number;
            }
            
            return TokenType<TTokenType>.Text;
        }

        private bool ProcessCustomTransition(char c, int currentIndex)
        {
            if (_parser._characterIdMap.TryGetCharacterId(c, out int charId))
            {
                int[] transitions = _parser._stateTransitions[_currentCustomState];
                if (charId < transitions.Length && transitions[charId] != -1)
                {
                    if (_currentCustomState == 0)
                    {
                        _customTokenStartIndex = currentIndex;
                    }
                    
                    _currentCustomState = transitions[charId];
                    
                    if (_parser._isEndOfToken[_currentCustomState])
                    {
                        _lastMatchTokenType = _parser._stateToTokenType[_currentCustomState];
                        _lastMatchLength = currentIndex + 1;
                    }

                    return true;
                }
            }

            return false;
        }

        private void ResetCustomMatchState()
        {
            _currentCustomState = 0;
            _customTokenStartIndex = -1;
            _lastMatchTokenType = null;
            _lastMatchLength = 0;
        }

        private bool IsValidDefaultCharacter(char c)
        {
            if (_defaultTokenType == TokenType<TTokenType>.WhiteSpace)
                return char.IsWhiteSpace(c);
            
            if (_defaultTokenType == TokenType<TTokenType>.Number)
                return char.IsDigit(c);
            
            if (_defaultTokenType == TokenType<TTokenType>.Text)
                return !char.IsWhiteSpace(c) && (_parser._numbersAreText || !char.IsDigit(c));

            return false;
        }

        private bool ShouldWaitMoreData()
        {
            if (_currentCustomState != 0 && HasPossibleTransitions(_currentCustomState))
            {
                return _customTokenStartIndex == 0;
            }
            
            return _currentCustomState == 0 && _defaultIsValid;
        }

        private void ResolveMatch(out TokenType<TTokenType> tokenType, out int length)
        {
            if (_customTokenStartIndex == 0)
            {
                tokenType = _lastMatchTokenType!;
                length = _lastMatchLength;
            }
            else
            {
                tokenType = _defaultTokenType;
                length = _customTokenStartIndex;
            }
        }

        private bool HasPossibleTransitions(int stateId)
        {
            return _parser._stateTransitions[stateId].AsSpan().ContainsAnyExcept(-1);
        }
    }
}