using Tokensharp.TokenTree;

namespace Tokensharp;

public sealed class TokenConfiguration<TTokenType> : ITokenConfiguration<TTokenType>
    where TTokenType : TokenType<TTokenType>, ITokenType<TTokenType>
{
    internal TokenConfiguration(IEnumerable<LexemeToTokenType<TTokenType>> lexemeToTokenTypes, bool numbersAreText = false)
    {
        TokenTree = lexemeToTokenTypes.ToTokenTree();
        
        NumbersAreText = numbersAreText;
        
        HashSet<char> allCharacters = TokenTree.AllCharacters;

        CharacterIdMap = new CharacterIdMap(allCharacters);

        var stateTransitions = new List<int[]>();
        var stateToTokenType = new List<TTokenType?>();
        
        BuildTransitions(stateTransitions, stateToTokenType, TokenTree);

        StateTransitions = [..stateTransitions];
        StateToTokenType = [..stateToTokenType];

        IsEndOfToken = new bool[StateToTokenType.Length];
        for(var i = 0; i < StateToTokenType.Length; i++)
        {
            if (StateToTokenType[i] is not null)
            {
                IsEndOfToken[i] = true;
            }
        }
    }

    public bool NumbersAreText { get; }
    
    public static implicit operator TokenConfiguration<TTokenType>(LexemeToTokenType<TTokenType>[] lexemeToTokenTypes)
    {
        return new TokenConfiguration<TTokenType>(lexemeToTokenTypes);
    }
    
    internal ITokenTreeNode<TTokenType> TokenTree { get; }
    
    internal CharacterIdMap CharacterIdMap { get; }

    internal int[][] StateTransitions { get; }

    internal TTokenType?[] StateToTokenType { get; }

    internal bool[] IsEndOfToken { get; }

    private void BuildTransitions(List<int[]> stateTransitions, List<TTokenType?> stateToTokenType, ITokenTreeNode<TTokenType> node)
    {
        stateToTokenType.Add(node.TokenType);
        
        int maxCharacterId = GetMaxChildCharacterId(node);

        var transitions = new int[maxCharacterId + 1];
        Array.Fill(transitions, -1);
        stateTransitions.Add(transitions);

        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            int charId = CharacterIdMap[childNode.Character];
            transitions[charId] = stateTransitions.Count;
            BuildTransitions(stateTransitions, stateToTokenType, childNode);
        }
    }

    private int GetMaxChildCharacterId(ITokenTreeNode<TTokenType> node)
    {
        int maxCharacterId = -1;
        
        foreach (ITokenTreeNode<TTokenType> childNode in node)
        {
            int characterId = CharacterIdMap[childNode.Character];
            if (characterId > maxCharacterId)
            {
                maxCharacterId = characterId;
            }
        }

        return maxCharacterId;
    }
}