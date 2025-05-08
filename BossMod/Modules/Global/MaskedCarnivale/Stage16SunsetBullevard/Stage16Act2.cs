namespace BossMod.Global.MaskedCarnivale.Stage16.Act2;

public enum OID : uint
{
    Boss = 0x26F4, // R=4.0
    Cyclops = 0x26F3, //R=3.2
    Helper = 0x233C, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // 26F4->player, no cast, single-target
    TenTonzeSlash = 14871, // 26F4->self, 4.0s cast, range 40+R 60-degree cone
    VoiceOfAuthority = 14874, // 26F4->self, 1.5s cast, single-target, spawns cyclops add
    OneOneOneTonzeSwing = 14872, // 26F4->self, 4.5s cast, range 8+R circle, knockback dist 20
    CryOfRage = 14875, // 26F4->self, 3.0s cast, range 50+R circle, gaze
    TheBullsVoice = 14779, // 26F4->self, 1.5s cast, single-target, damage buff
    PredatorialInstinct = 14685, // 26F4->self, no cast, range 50+R circle, raidwide attract with dist 50
    OneOneOneOneTonzeSwing = 14686, // 26F4->self, 9.0s cast, range 20+R circle, raidwide, needs diamondback to survive
    ZoomIn = 14873, // 26F4->player, 4.0s cast, width 8 rect unavoidable charge, knockback dist 20
    TenTonzeWave = 14876, // 26F4->self, 4.0s cast, range 40+R 60-degree cone
    TenTonzeWave2 = 15268, // 233C->self, 4.6s cast, range 10-20 donut
}

class OneOneOneOneTonzeSwing(BossModule module) : Components.RaidwideCast(module, AID.OneOneOneOneTonzeSwing, "Use Diamondback!");
class TenTonzeSlash(BossModule module) : Components.StandardAOEs(module, AID.TenTonzeSlash, new AOEShapeCone(44, 30.Degrees()));
class OneOneOneTonzeSwing(BossModule module) : Components.StandardAOEs(module, AID.OneOneOneTonzeSwing, new AOEShapeCircle(12));
class CryOfRage(BossModule module) : Components.CastGaze(module, AID.CryOfRage);
class TenTonzeWave(BossModule module) : Components.StandardAOEs(module, AID.TenTonzeWave, new AOEShapeCone(44, 30.Degrees()));
class TenTonzeWave2(BossModule module) : Components.StandardAOEs(module, AID.TenTonzeWave2, new AOEShapeDonut(10, 20));
class OneOneOneTonzeSwingKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.OneOneOneTonzeSwing, 20, shape: new AOEShapeCircle(12)); // actual knockback happens ~1.45s after snapshot
class ZoomIn(BossModule module) : Components.BaitAwayChargeCast(module, AID.ZoomIn, 4);

class ZoomInKB(BossModule module) : Components.Knockback(module) // actual knockback happens ~0.7s after snapshot
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Module.PrimaryActor.Position, 20, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ZoomIn)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ZoomIn)
            _activation = default;
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Tikbalang will spawn a cyclops a few seconds into the fight. Make sure\nto kill it before it reaches you. After that you can just slowly take down the\nboss. Use Diamondback to survive the 1111 Tonze Swing. Alternatively\nyou can try the Final Sting combo when he drops to about 75% health.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage16Act2States : StateMachineBuilder
{
    public Stage16Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OneOneOneOneTonzeSwing>()
            .ActivateOnEnter<OneOneOneTonzeSwing>()
            .ActivateOnEnter<OneOneOneTonzeSwingKB>()
            .ActivateOnEnter<TenTonzeSlash>()
            .ActivateOnEnter<CryOfRage>()
            .ActivateOnEnter<ZoomIn>()
            .ActivateOnEnter<ZoomInKB>()
            .ActivateOnEnter<TenTonzeWave>()
            .ActivateOnEnter<TenTonzeWave2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 626, NameID = 8113, SortOrder = 2)]
public class Stage16Act2 : BossModule
{
    public Stage16Act2(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Cyclops))
            Arena.Actor(s, ArenaColor.Object, false);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Cyclops => 1,
                OID.Boss => 0,
                _ => 0
            };
        }
    }
}
