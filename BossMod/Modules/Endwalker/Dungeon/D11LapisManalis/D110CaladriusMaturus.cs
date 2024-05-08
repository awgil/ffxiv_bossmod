namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D110CaladriusMaturus;

public enum OID : uint
{
    Boss = 0x3D56, //R=3.96
    Caladrius = 0x3CE2, //R=1.8
}

public enum AID : uint
{
    AutoAttack = 872, // Caladrius/Boss->player, no cast, single-target
    TransonicBlast = 32535, // Caladrius->self, 4.0s cast, range 9 90-degree cone
}

class TransonicBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TransonicBlast), new AOEShapeCone(9, 45.Degrees()));

class D110CaladriusMaturusStates : StateMachineBuilder
{
    public D110CaladriusMaturusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TransonicBlast>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Caladrius).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 12078)]
public class D110CaladriusMaturus(WorldState ws, Actor primary) : BossModule(ws, primary, new(47, -570.5f), new ArenaBoundsRect(8.5f, 11.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Caladrius), ArenaColor.Enemy);
    }
}
