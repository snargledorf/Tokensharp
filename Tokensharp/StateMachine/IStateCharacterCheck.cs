namespace Tokensharp.StateMachine;

internal interface IStateCharacterCheck
{
    bool CharacterIsValidForState(char c);
}