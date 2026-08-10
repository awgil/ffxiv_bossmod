namespace BossMod.Global.MaskedCarnivale.Stage32.Act1;

public enum OID : uint
{
    Boss = 0x3FA5, //R=2.5
    GlitteringSlime = 0x3FAB, // R=2.0
    BallOfFire = 0x3FA6, // R=1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 34444, // Boss->player, no cast, single-target

    GoldorFireIIIVisual = 34447, // Boss->self, 4.2s cast, single-target
    GoldorFireIII1 = 34448, // Helper->location, 5.0s cast, range 8 circle
    GoldorFireIII2 = 34449, // Helper->location, 2.5s cast, range 8 circle
    GoldorBlast = 34450, // Boss->self, 3.5s cast, range 60 width 10 rect
    SlimySummon = 34462, // Boss->self, 4.0s cast, single-target, summons glittering slime
    Rupture = 34463, // GlitteringSlime->self, 8.0s cast, range 100 circle, enrage, slime must be killed fast with The Ram's Voice + Ultravibration
    GoldorQuakeVisual = 34456, // Boss->self, 3.0s cast, single-target
    GoldorQuake1 = 34457, // Helper->self, 4.0s cast, range 10 circle
    GoldorQuake2 = 34458, // Helper->self, 5.5s cast, range 10-20 donut
    GoldorQuake3 = 34459, // Helper->self, 7.0s cast, range 20-30 donut
    GoldorFire = 34445, // Boss->self, 4.0s cast, single-target
    GoldorAeroIII = 34460, // Boss->self, 4.0s cast, range 50 circle, raidwide, knockback 10 away from source
    Burn = 34446, // BallOfFire->self, 12.0s cast, range 10 circle
    GoldorGravity = 34451, // Boss->self, 5.0s cast, single-target
    GoldorGravity2 = 34452, // Helper->player, no cast, single-target, heavy damage + heavy
    GoldorThunderIIIVisual = 34453, // Boss->self, 4.0s cast, single-target
    GoldorThunderIII1 = 34454, // Helper->player, no cast, range 5 circle, applies cleansable electrocution
    GoldorThunderIII2 = 34455, // Helper->location, 2.5s cast, range 6 circle
    GoldorBlizzardIIIVisual = 34589, // Boss->self, 6.0s cast, single-target, interruptible, freezes player
    GoldorBlizzardIII = 34590 // Helper->player, no cast, range 6 circle
}

public enum SID : uint
{
    Heavy = 240, // Helper->player, extra=0x63
    Electrocution = 3779, // Helper->player, extra=0x0
}

sealed class SlimySummon(BossModule module) : Components.CastHint(module, (uint)AID.SlimySummon, "Prepare to kill add ASAP");
sealed class GoldorFireIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GoldorFireIII1, (uint)AID.GoldorFireIII2], 8f);
sealed class GoldorBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorBlast, new AOEShapeRect(60f, 5f));
sealed class Rupture(BossModule module) : Components.CastHint(module, (uint)AID.Rupture, "Kill slime ASAP! (The Ram's Voice + Ultravibration)", true);

sealed class GoldorQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GoldorQuake1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.GoldorQuake1 => 0,
                (uint)AID.GoldorQuake2 => 1,
                (uint)AID.GoldorQuake3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.5d));
        }
    }
}

sealed class GoldorAeroIII(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GoldorAeroIII, 10f)
{
    private readonly Burn _aoe = module.FindComponent<Burn>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class GoldorAeroIIIRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.GoldorAeroIII);
sealed class Burn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burn, 10f);
sealed class GoldorGravity(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.GoldorGravity, (uint)AID.GoldorGravity2, 0.8f, "Dmg + Heavy debuff");
sealed class GoldorThunderIII(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.GoldorThunderIIIVisual, (uint)AID.GoldorThunderIII1, 0.8f, "Prepare to cleanse Electrocution");
sealed class GoldorThunderIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorThunderIII2, 6f);
sealed class GoldorBlizzardIII(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GoldorBlizzardIIIVisual);

sealed class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Electrocution) != null)
        {
            hints.Add($"Cleanse Electrocution!");
        }
        if (actor.FindStatus((uint)SID.Heavy) != null)
        {
            hints.Add("Use Loom to dodge AOEs!");
        }

        var fires = Module.Enemies((uint)OID.BallOfFire);
        var count = fires.Count;
        if (count == 0)
        {
            return;
        }
        for (var i = 0; i < count; ++i)
        {
            var fire = fires[i];
            if (!fire.IsDead)
            {
                hints.Add("Destroy at least one fireball to create a safe spot!");
                return;
            }
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"For this fight The Ram's Voice, Ultravibration, Diamondback,\nExuviation, Flying Sardine, Loom, a physical dmg ability and a healing\nability (preferably Pom Cure with healer mimicry) are mandatory.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirements for achievement: Don't destroy the crystal in act 2,\nuse no sprint, use all 6 magic elements, take no optional damage.", false);
    }
}

sealed class Stage32Act1States : StateMachineBuilder
{
    public Stage32Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SlimySummon>()
            .ActivateOnEnter<GoldorFireIII>()
            .ActivateOnEnter<GoldorBlast>()
            .ActivateOnEnter<Rupture>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<GoldorQuake>()
            .ActivateOnEnter<GoldorAeroIII>()
            .ActivateOnEnter<GoldorAeroIIIRaidwide>()
            .ActivateOnEnter<GoldorGravity>()
            .ActivateOnEnter<GoldorThunderIII>()
            .ActivateOnEnter<GoldorThunderIII2>()
            .ActivateOnEnter<GoldorBlizzardIII>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 948u, NameID = 12471u, SortOrder = 1)]
public sealed class Stage32Act1 : BossModule
{
    public Stage32Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BallOfFire), Colors.Object);
        Arena.Actors(Enemies((uint)OID.GlitteringSlime), Colors.Object);
    }
}
