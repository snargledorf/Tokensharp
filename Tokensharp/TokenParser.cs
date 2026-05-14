using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Tokensharp.Trie;

namespace Tokensharp;

public ref struct TokenParser<TTokenType>(ReadOnlySpan<char> buffer,
    bool moreDataAvailable,
    TokenParserState<TTokenType> state)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private readonly ReadOnlySpan<char> _buffer = buffer;

    private readonly bool _numbersAreText = state.NumbersAreText;
    private readonly TrieNode<TTokenType> _trieRootNode = state.TrieRootNode ?? TTokenType.Configuration.TrieRootNode;
    
    private int _consumedChars;
    private int _startOfLexemeIndex;

    public int CharsConsumed => _consumedChars;

    public int StartOfLexemeIndex => _startOfLexemeIndex;

    public TokenParserState<TTokenType> CurrentState => new(_trieRootNode, _numbersAreText);
    
    public TokenType<TTokenType>? TokenType { get; private set; }
    
    public ReadOnlySpan<char> Lexeme { get; private set; }

    public TokenParser(ReadOnlySpan<char> buffer, bool moreDataAvailable = false) 
        : this(buffer, moreDataAvailable, default(TokenParserState<TTokenType>))
    {
    }

    public TokenParser(ReadOnlySpan<char> buffer, TokenConfiguration<TTokenType> tokenConfiguration)
        : this(buffer, false, tokenConfiguration)
    {
    }

    public TokenParser(ReadOnlySpan<char> buffer, bool moreDataAvailable,
        TokenConfiguration<TTokenType> tokenConfiguration)
        : this(buffer, moreDataAvailable, new TokenParserState<TTokenType>(tokenConfiguration))
    {
    }

    public TokenParser(ReadOnlySpan<char> buffer, TokenParserState<TTokenType> state)
        : this(buffer, false, state)
    {
    }

    [MemberNotNullWhen(true, nameof(TokenType))]
    public bool Read()
    {
        TokenType = null;
        Lexeme = ReadOnlySpan<char>.Empty;
        
        if (_buffer.IsEmpty || _consumedChars >= _buffer.Length)
            return false;
        
        int lastAcceptIndex = -1;
        TrieNode<TTokenType>? lastAcceptTrieNode = null;

        _startOfLexemeIndex = _consumedChars;
        
        char c = _buffer[_startOfLexemeIndex];
        int currentIndex = _startOfLexemeIndex + 1;

        DetermineBaseTokenType(c, out bool baseIsDigit, out bool baseIsWhiteSpace, out TTokenType baseTokenType);
        
        if (_trieRootNode.TryGetChildNode(c, out TrieNode<TTokenType>? currentNode) && currentNode.HasValue)
        {
            if (currentNode.HasChildren)
            {
                lastAcceptIndex = currentIndex;
                lastAcceptTrieNode = currentNode;
            }
            else
                return SetToken(currentNode.Value!, currentIndex);
        }

        int typeSwitchIndex = -1;
        int startOfMidParseUserDefinedToken = -1;
        TrieNode<TTokenType>? possibleMidParseUserDefinedTokenNode = null;
        
        for (; currentIndex < _buffer.Length; currentIndex++)
        {
            c = _buffer[currentIndex];

            if (typeSwitchIndex == -1)
            {
                if (baseIsDigit)
                {
                    if (!char.IsDigit(c))
                        typeSwitchIndex = currentIndex;
                }
                else if (baseIsWhiteSpace)
                {
                    if (!char.IsWhiteSpace(c))
                        typeSwitchIndex = currentIndex;
                }
                else
                {
                    if (char.IsWhiteSpace(c) || (!_numbersAreText && char.IsDigit(c)))
                        typeSwitchIndex = currentIndex;
                }
            }

            if (currentNode is not null)
            {
                if (currentNode.TryGetChildNode(c, out currentNode))
                {
                    if (currentNode.HasValue)
                    {
                        lastAcceptIndex = currentIndex+1;
                        lastAcceptTrieNode = currentNode;
                    }
                }
                else if (lastAcceptIndex != -1)
                {
                    break;
                }
                else
                {
                    currentNode = null;
                    
                    if (_trieRootNode.TryGetChildNode(c, out possibleMidParseUserDefinedTokenNode))
                    {
                        if (possibleMidParseUserDefinedTokenNode.HasValue)
                            return SetToken(baseTokenType, startOfMidParseUserDefinedToken);
                
                        startOfMidParseUserDefinedToken = currentIndex;
                    }
                }
            }
            else if (typeSwitchIndex != -1)
            {
                break;
            }
            else if (possibleMidParseUserDefinedTokenNode is not null)
            {
                if (possibleMidParseUserDefinedTokenNode.TryGetChildNode(c, out possibleMidParseUserDefinedTokenNode))
                {
                    if (possibleMidParseUserDefinedTokenNode.HasValue)
                        return SetToken(baseTokenType, startOfMidParseUserDefinedToken);
                }
                else
                {
                    startOfMidParseUserDefinedToken = -1;
                }
            }
            else if (_trieRootNode.TryGetChildNode(c, out possibleMidParseUserDefinedTokenNode))
            {
                startOfMidParseUserDefinedToken = currentIndex;
                
                if (possibleMidParseUserDefinedTokenNode.HasValue)
                    return SetToken(baseTokenType, startOfMidParseUserDefinedToken);
            }
        }

        if (lastAcceptTrieNode is { HasValue: true } acceptTrieNode)
        {
            if (acceptTrieNode.HasChildren && moreDataAvailable)
                return false;

            return SetToken(acceptTrieNode.Value!, lastAcceptIndex);
        }

        if (currentNode is not null && moreDataAvailable)
            return false;

        if (typeSwitchIndex != -1)
            return SetToken(baseTokenType, typeSwitchIndex);

        return !moreDataAvailable && SetToken(baseTokenType, currentIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SetToken(TTokenType tokenType, int endOfTokenIndex)
    {
        TokenType = tokenType;
        Lexeme = _buffer[_startOfLexemeIndex..endOfTokenIndex];
        _consumedChars = endOfTokenIndex;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DetermineBaseTokenType(char c, out bool isDigit, out bool isWhiteSpace, out TTokenType baseTokenType)
    {
        if (char.IsWhiteSpace(c))
        {
            isDigit = false;
            isWhiteSpace = true;
            baseTokenType = TokenType<TTokenType>.WhiteSpace;
        }
        else if (char.IsDigit(c) && !_numbersAreText)
        {
            isDigit = true;
            isWhiteSpace = false;
            baseTokenType = TokenType<TTokenType>.Number;
        }
        else
        {
            isDigit = false;
            isWhiteSpace = false;
            baseTokenType = TokenType<TTokenType>.Text;
        }
    }
}