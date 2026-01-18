using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class StartState<TTokenType>(ITokenTreeNode<TTokenType> rootNode)
    : RootState<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance { get; } = WhiteSpaceState<TTokenType>.For(rootNode);
    protected override NumberState<TTokenType> NumberStateInstance { get; } = NumberState<TTokenType>.For(rootNode);
    protected override TextState<TTokenType> TextStateInstance { get; } = TextState<TTokenType>.For(rootNode);

    public override bool CharacterIsValidForState(char c)
    {
        return true;
    }
}