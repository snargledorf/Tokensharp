using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class NodeStateBase<TTokenType>(ITokenTreeNode<TTokenType> node)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override bool IsEndOfToken => false;
    
    protected ITokenTreeNode<TTokenType> Node { get; } = node;

    public override string ToString()
    {
        return $"{GetType().Name} Node: {Node}";
    }
}