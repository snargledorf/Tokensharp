namespace Tokensharp.StateMachine;

internal interface ITextWhiteSpaceNumberLookup<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    TextWhiteSpaceNumberBase<TTokenType> GetState(in char c);
}