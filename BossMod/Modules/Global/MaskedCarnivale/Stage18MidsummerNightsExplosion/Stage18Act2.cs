namespace BossMod.Global.MaskedCarnivale.Stage18.Act2;

public enum OID : uint
{
    Boss = 0x2725, //R=3.0
    Keg = 0x2726 //R=0.65
}

public enum AID : uint
{
    WildCharge = 15055, // Boss->players, 3.5s cast, width 8 rect charge
    Explosion = 15054, // Keg->self, 2.0s cast, range 10 circle
    Fireball = 15051, // Boss->location, 4.0s cast, range 6 circle
    RipperClaw = 15050, // Boss->self, 4.0s cast, range 5+R 90-degree cone
    TailSmash = 15052, // Boss->self, 4.0s cast, range 12+R 90-degree cone
    BoneShaker = 15053 // Boss->self, no cast, range 50 circle, harmless raidwide
}

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 10f);
sealed class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireball, 6f);
sealed class RipperClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RipperClaw, new AOEShapeCone(8f, 45f.Degrees()));
sealed class TailSmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSmash, new AOEShapeCone(15f, 45f.Degrees()));

sealed class WildCharge(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.WildCharge, 4f)
{
    private readonly List<Actor> kegs = [with(12)];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Keg)
        {
            kegs.Add(actor);
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.Keg)
        {
            kegs.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count != 0 && kegs.Count != 0)
        {
            hints.Add("Aim charge at a keg!");
        }
    }
}

sealed class KegExplosion(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> kegs = [with(12)];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Keg)
        {
            kegs.Add(actor);
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.Keg)
        {
            kegs.Remove(actor);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = kegs.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(kegs[i].Position, 10f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = kegs.Count;
        for (var i = 0; i < count; ++i)
        {
            if (actor.Position.InCircle(kegs[i].Position, 10f))
            {
                hints.Add("In keg explosion radius!");
                return;
            }
        }
    }
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Same as last stage. Make the manticores run to the kegs and their attacks\nwill make them blow up. Their attacks will also do friendly fire damage\nto each other.\nThe Ram's Voice and Ultravibration combo can be used to kill manticores.");
    }
}

sealed class Stage18Act2States : StateMachineBuilder
{
    public Stage18Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<WildCharge>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage18Act2.Kegs);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 628, NameID = 8116, SortOrder = 2)]
public sealed class Stage18Act2 : BossModule
{
    public Stage18Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
        ActivateComponent<KegExplosion>();
    }
    public static readonly uint[] Kegs = [(uint)OID.Boss, (uint)OID.Keg];

    protected override bool CheckPull() => IsAnyActorInCombat(Kegs);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
        Arena.Actors(Enemies((uint)OID.Keg), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }
}
