using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class WhiteSpaceState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : RootState<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    private static readonly EndOfTokenState<TTokenType> EndOfTokenState = EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.WhiteSpace);

    private TextState<TTokenType>? _textState;
    private NumberState<TTokenType>? _numberState;

    private static WhiteSpaceState<TTokenType>? _instance;

    internal override WhiteSpaceState<TTokenType> WhiteSpaceStateInstance => this;
    internal override NumberState<TTokenType> NumberStateInstance => _numberState ?? throw new InvalidOperationException($"{nameof(NumberStateInstance)} not initialized");
    internal override TextState<TTokenType> TextStateInstance => _textState ?? throw new InvalidOperationException($"{nameof(TextStateInstance)} not initialized");

    internal static WhiteSpaceState<TTokenType> For(ITokenTreeNode<TTokenType> treeNode)
    {
        if (_instance is { } instance)
            return instance;
        
        _instance = new WhiteSpaceState<TTokenType>(treeNode.RootNode);

        _instance.Initialize();

        return _instance;
    }

    private void Initialize()
    {
        _textState = TextState<TTokenType>.For(RootNode);
        _numberState = NumberState<TTokenType>.For(RootNode);
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

    protected override bool TryGetDefaultState([NotNullWhen(true)] out IState<TTokenType>? defaultState)
    {
        defaultState = EndOfTokenState;
        return true;
    }

    protected override IState<TTokenType> GetFallbackEndOfTokenState(ITokenTreeNode<TTokenType> node)
    {
        return EndOfTokenState;
    }

    public override void OnEnter(StateMachineContext<TTokenType> context)
    {
        context.TokenType = TokenType<TTokenType>.WhiteSpace;
        base.OnEnter(context);
    }
}