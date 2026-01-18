using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class WhiteSpaceState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static WhiteSpaceState<TTokenType>? _instance;

    protected override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance => this;
    protected override NumberState<TTokenType> NumberStateInstance => field ??= NumberState<TTokenType>.For(Node);
    protected override TextState<TTokenType> TextStateInstance => field ??= TextState<TTokenType>.For(Node);
    
    public override EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.WhiteSpace);

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
            nextState = EndOfTokenStateInstance;
            return true;
        }
        
        if (base.TryGetStateNextState(c, out nextState))
            return true;
        
        nextState = this;
        return true;
    }

    public override bool CharacterIsValidForState(char c)
    {
        return char.IsWhiteSpace(c);
    }
}