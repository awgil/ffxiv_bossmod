namespace BossMod.Heavensward.Alliance.A23Headstone;

class TremblingEpigraph(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TremblingEpigraph));
class FlaringEpigraph(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlaringEpigraph));
class BigBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BigBurst));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4868)]
class A23Headstone(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-168, 225), 20), new Circle(new(-152.53f, 252.76f), 20), new Circle(new(-184.63f, 197.09f), 20)]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Parthenope), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.VoidFire), ArenaColor.Enemy);
    }
}