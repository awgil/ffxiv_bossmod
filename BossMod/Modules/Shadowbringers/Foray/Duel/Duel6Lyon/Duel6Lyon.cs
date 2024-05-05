namespace BossMod.Shadowbringers.Foray.Duel.Duel6Lyon;

class Duel6LyonStates : StateMachineBuilder
{
    public Duel6LyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OnFire>()
            .ActivateOnEnter<WildfiresFury>()
            .ActivateOnEnter<HeavenAndEarth>()
            .ActivateOnEnter<HeartOfNatureConcentric>()
            .ActivateOnEnter<TasteOfBloodAndDuelOrDie>()
            .ActivateOnEnter<FlamesMeet>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<WindsPeakKB>()
            .ActivateOnEnter<SplittingRage>()
            .ActivateOnEnter<NaturesBlood>()
            .ActivateOnEnter<MoveMountains>()
            .ActivateOnEnter<WildfireCrucible>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "SourP", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 31)]
public class Duel6Lyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(50f, -410f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        var tasteOfBlood = FindComponent<TasteOfBloodAndDuelOrDie>();
        if (tasteOfBlood?.Casters.Count > 0)
        {
            foreach (var caster in tasteOfBlood.Casters)
            {
                bool isDueler = tasteOfBlood.Duelers.Contains(caster);
                Arena.Actor(caster, isDueler ? ArenaColor.Danger : ArenaColor.Enemy, true);
            }
        }
        else
        {
            base.DrawEnemies(pcSlot, pc);
        }
    }
}
