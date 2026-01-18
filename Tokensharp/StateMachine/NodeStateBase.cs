using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal abstract class NodeStateBase<TTokenType>(ITokenTreeNode<TTokenType> node)
    : State<TTokenType> where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    protected ITokenTreeNode<TTokenType> Node { get; } = node;

    public IStateLookup<TTokenType> StateLookup
    {
        get => field ?? throw new InvalidOperationException("State lookup not initialized");
        set;
    }

    protected bool TryGetStateForChildNode(char c, [NotNullWhen(true)] out IState<TTokenType>? state)
    {
        if (StateLookup.TryGetState(c, out state))
            return true;

        state = null;
        return false;
    }

    public override string ToString()
    {
        return $"{GetType().Name} Node: {Node}";
    }
}