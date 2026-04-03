using System.Diagnostics.CodeAnalysis;

namespace Tokensharp;

public readonly ref struct TokenParser<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly TokenConfiguration<TTokenType> _configuration;

    public TokenParser() : this(TTokenType.Configuration)
    {
    }

    public TokenParser(TokenConfiguration<TTokenType> tokenConfiguration)
    {
        _configuration = tokenConfiguration;
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
        var session = new ParseSession(_configuration, buffer, moreDataAvailable);
        return session.Parse(out tokenType, out length);
    }

    private abstract class DefaultCharacterCheck
    {
        public abstract bool IsValid(char c);

        public static DefaultCharacterCheck GetCharacterCheck(TokenType<TTokenType> tokenType, bool numbersAreText)
        {
            if (tokenType == TokenType<TTokenType>.WhiteSpace)
                return new WhiteSpaceDefaultCharacterCheck();
            
            if (tokenType == TokenType<TTokenType>.Number)
                return new NumberDefaultCharacterCheck();
            
            if (tokenType == TokenType<TTokenType>.Text)
                return new TextDefaultCharacterCheck(numbersAreText);
            
            throw new InvalidOperationException("Invalid default token type");
        }

        private sealed class TextDefaultCharacterCheck(bool numbersAreText) : DefaultCharacterCheck
        {
            public override bool IsValid(char c)
            {
                return !char.IsWhiteSpace(c) && (numbersAreText || !char.IsDigit(c));
            }
        }

        private sealed class NumberDefaultCharacterCheck : DefaultCharacterCheck
        {
            public override bool IsValid(char c)
            {
                return char.IsDigit(c);
            }
        }

        private sealed class WhiteSpaceDefaultCharacterCheck : DefaultCharacterCheck
        {
            public override bool IsValid(char c)
            {
                return char.IsWhiteSpace(c);
            }
        }
    }

    private ref struct ParseSession
    {
        private readonly TokenConfiguration<TTokenType> _configuration;
        private readonly ReadOnlySpan<char> _buffer;
        private readonly bool _moreDataAvailable;

        private int _currentCustomState;
        private int _customTokenStartIndex;
        private TTokenType? _lastMatchTokenType;
        private int _lastMatchLength;

        private readonly TokenType<TTokenType> _defaultTokenType;
        private bool _defaultIsValid;
        private int _defaultTokenLength;
        private readonly DefaultCharacterCheck _isValidDefaultCharacter;

        public ParseSession(TokenConfiguration<TTokenType> configuration, ReadOnlySpan<char> buffer, bool moreDataAvailable)
        {
            _configuration = configuration;
            _buffer = buffer;
            _moreDataAvailable = moreDataAvailable;

            _currentCustomState = 0;
            _customTokenStartIndex = -1;
            _lastMatchTokenType = null;
            _lastMatchLength = 0;

            _defaultIsValid = true;
            _defaultTokenLength = 0;
            _defaultTokenType = GetDefaultTokenType(buffer);
            _isValidDefaultCharacter = DefaultCharacterCheck.GetCharacterCheck(_defaultTokenType, configuration.NumbersAreText);
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
                    _defaultIsValid = _isValidDefaultCharacter.IsValid(c);
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
            
            if (!_configuration.NumbersAreText && char.IsDigit(firstChar))
            {
                return TokenType<TTokenType>.Number;
            }
            
            return TokenType<TTokenType>.Text;
        }

        private bool ProcessCustomTransition(char c, int currentIndex)
        {
            if (!_configuration.CharacterIdMap.TryGetCharacterId(c, out int charId))
                return false;
            
            int[] transitions = _configuration.StateTransitions[_currentCustomState];
            if (charId >= transitions.Length || transitions[charId] == -1) 
                return false;
            
            if (_currentCustomState == 0)
            {
                _customTokenStartIndex = currentIndex;
            }
                    
            _currentCustomState = transitions[charId];
                    
            if (_configuration.IsEndOfToken[_currentCustomState])
            {
                _lastMatchTokenType = _configuration.StateToTokenType[_currentCustomState];
                _lastMatchLength = currentIndex + 1;
            }

            return true;
        }

        private void ResetCustomMatchState()
        {
            _currentCustomState = 0;
            _customTokenStartIndex = -1;
            _lastMatchTokenType = null;
            _lastMatchLength = 0;
        }

        private bool ShouldWaitMoreData()
        {
            if (_currentCustomState != 0 && _configuration.HasPossibleTransitions[_currentCustomState])
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
    }
}