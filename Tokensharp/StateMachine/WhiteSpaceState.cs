using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class WhiteSpaceState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static WhiteSpaceState<TTokenType>? _instance;

    internal override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance => this;
    internal override NumberState<TTokenType> NumberStateInstance => field ??= NumberState<TTokenType>.For(RootNode);
    internal override TextState<TTokenType> TextStateInstance => field ??= TextState<TTokenType>.For(RootNode);

    protected override TTokenType TokenType => TokenType<TTokenType>.WhiteSpace;
    public override EndOfTokenState<TTokenType> EndOfTokenState { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.WhiteSpace);

    internal static WhiteSpaceState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        return _instance = new WhiteSpaceState<TTokenType>(treeNode.RootNode);
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!char.IsWhiteSpace(c))
        {
            nextState = EndOfTokenState;
            return true;
        }
        
        if (base.TryGetStateNextState(c, out nextState))
            return true;
        
        nextState = this;
        return true;
    }

    public override bool CharacterIsValidForToken(char c)
    {
        return char.IsWhiteSpace(c);
    }
}