namespace Tokensharp;

public static class TokenTypeExtensions
{
    extension<TTokenType>(TTokenType tokenType) where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
    {
        public bool IsUserDefined => tokenType != TokenType<TTokenType>.Text &&
                                     tokenType != TokenType<TTokenType>.Number &&
                                     tokenType != TokenType<TTokenType>.WhiteSpace;
    }
}