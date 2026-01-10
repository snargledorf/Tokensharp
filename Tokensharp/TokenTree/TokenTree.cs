namespace Tokensharp.TokenTree;

internal class TokenTree<TValue> : TokenTreeNodeCollection<TValue> where TValue : TokenType<TValue>, ITokenType<TValue>
{
}