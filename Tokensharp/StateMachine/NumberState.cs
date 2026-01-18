using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class NumberState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static NumberState<TTokenType>? _instance;

    protected override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance =>
        field ??= WhiteSpaceState<TTokenType>.For(Node);

    protected override NumberState<TTokenType> NumberStateInstance => this;

    protected override TextState<TTokenType> TextStateInstance => field ??= TextState<TTokenType>.For(Node);
   
    public override EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; } = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Number);

    internal static NumberState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        return _instance = new NumberState<TTokenType>(treeNode.RootNode);
    }

    protected override bool TryGetStateNextState(char c, [NotNullWhen(true)] out IState<TTokenType>? nextState)
    {
        if (!char.IsDigit(c))
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
        return char.IsDigit(c);
    }
}