using BossMod.QuestBattle;

namespace BossMod.Components;

public abstract class RotationModule<R>(BossModule module) : BossComponent(module) where R : UnmanagedRotation
{
    private readonly R _rotation = New<R>.Constructor<WorldState>()(module.WorldState);
    public sealed override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) => _rotation.Execute(actor, hints);
}
