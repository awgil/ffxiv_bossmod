namespace BossMod.Stormblood.Quest.StainedInScarlet;

public enum OID : uint
{
    Boss = 0x1AF5, // R0.500
    Arya = 0x1AF3, // R0.500
    EmptyAssassin = 0x1AF4, // R0.500
    Voidfire = 0x1AFA, // R1.000
}

public enum AID : uint
{
    DarkFireIIIBoss = 1305, // Boss->location, 2.2s cast, range 5 circle
    DarkFireIII = 8017, // Helper->location, 2.2s cast, range 5 circle
    BlackBlood = 8009, // Boss->player, 4.7s cast, range 5 circle stack with Arya
    EnchantedMoulinet = 8015, // Boss->self, 2.7s cast, range 6 120-degree cone
    Brand = 7870, // Boss->player, 2.7s cast, width 4 rect charge, always at the player so not dodgeable
    Soulsteal = 8018, // Boss->player, 7.7s cast, single-target
    FireBurst = 8019, // Voidfire->self, 24.7s cast, range 10 circle, gives Suppuration; covers the arena, a killed Voidfire leaves the only safe gap
}

class DarkFireIIIBoss(BossModule module) : Components.StandardAOEs(module, AID.DarkFireIIIBoss, 5);
class DarkFireIII(BossModule module) : Components.StandardAOEs(module, AID.DarkFireIII, 5);
class BlackBlood(BossModule module) : Components.StackWithCastTargets(module, AID.BlackBlood, 5);
class EnchantedMoulinet(BossModule module) : Components.StandardAOEs(module, AID.EnchantedMoulinet, new AOEShapeCone(6, 60.Degrees()));

class FireBurst(BossModule module) : Components.StandardAOEs(module, AID.FireBurst, 10)
{
    // latched: a killed Voidfire is destroyed ~9s later, before the rest burst; until one dies there is no safe spot
    public bool GapOpen;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => GapOpen ? base.ActiveAOEs(slot, actor) : [];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action == WatchedAction)
            GapOpen = Casters.Count > 0 && (GapOpen || caster.IsDead);
    }
}

class LambardStates : StateMachineBuilder
{
    public LambardStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkFireIIIBoss>()
            .ActivateOnEnter<DarkFireIII>()
            .ActivateOnEnter<BlackBlood>()
            .ActivateOnEnter<EnchantedMoulinet>()
            .ActivateOnEnter<FireBurst>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68118, NameID = 6670)]
public class Lambard(ModuleInit init) : BossModule(init, new(392, -353), new ArenaBoundsCircle(25))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var arya = Enemies(OID.Arya).FirstOrDefault(a => !a.IsDead);
        var alive = Enemies(OID.Voidfire).Where(v => !v.IsDead && v.IsTargetable);
        var voidfire = FindComponent<FireBurst>()?.GapOpen == true ? null : alive.FirstOrDefault(v => v.InstanceID == actor.TargetID) ?? alive.Closest(actor.Position);
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor == voidfire ? 2
                : (OID)h.Actor.OID == OID.Voidfire ? AIHints.Enemy.PriorityPointless
                : (OID)h.Actor.OID == OID.EmptyAssassin ? 1 : 0;

        if (actor.Class != Class.RDM || hints.ForbiddenZones.Any(z => z.shape.Check(actor.Position)))
            return;
        // rotation GCDs sit above High, so a heal at High never goes out
        if (actor.HPMP.CurHP < actor.HPMP.MaxHP * 3 / 10)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.RDM.AID.Vercure), actor, ActionQueue.Priority.VeryHigh);
        else if (arya != null && arya.HPMP.CurHP < arya.HPMP.MaxHP / 2)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.RDM.AID.Vercure), arya, ActionQueue.Priority.VeryHigh);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly && x.IsTargetable), ArenaColor.Enemy);
}
