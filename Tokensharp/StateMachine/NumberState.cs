using System.Diagnostics.CodeAnalysis;
using Tokensharp.TokenTree;

namespace Tokensharp.StateMachine;

internal class NumberState<TTokenType>(ITokenTreeNode<TTokenType> rootNode) 
    : TextWhiteSpaceNumberStateBase<TTokenType>(rootNode)
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    public override EndOfTokenState<TTokenType> EndOfTokenStateInstance { get; } =
        EndOfTokenState<TTokenType>.For(TokenType<TTokenType>.Number);

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

    public override bool CharacterIsValidForState(char c) => char.IsDigit(c);
}