namespace Tokensharp.StateMachine;

internal abstract class TextWhiteSpaceNumberLookupBase<TTokenType> : ITextWhiteSpaceNumberLookup<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public abstract TextWhiteSpaceNumberBase<TTokenType> GetState(in char c);
}