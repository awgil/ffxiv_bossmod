namespace BossMod.Shadowbringers.Hunt.RankS.Aglaope;

public enum OID : uint
{
    Boss = 0x281E, // R=2.4
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    FourfoldSuffering = 16819, // Boss->self, 5.0s cast, range 5-50 donut
    SeductiveSonata = 16824, // Boss->self, 3.0s cast, range 40 circle, applies Seduced for 6s (forced march towards boss at 1.7y/s)
    DeathlyVerse = 17074, // Boss->self, 5.0s cast, range 6 circle (right after Seductive Sonata, instant kill), 6*1.7 = 10.2 + 6 = 16.2y minimum distance to survive
    Tornado = 18040, // Boss->location, 3.0s cast, range 6 circle
    AncientAero = 16823, // Boss->self, 3.0s cast, range 40+R width 6 rect
    SongOfTorment = 16825, // Boss->self, 5.0s cast, range 50 circle, interruptible raidwide with bleed
    AncientAeroIII = 18056, // Boss->self, 3.0s cast, range 30 circle, knockback 10, away from source
}

public enum SID : uint
{
    Seduced = 991, // Boss->player, extra=0x11
    Bleeding = 642, // Boss->player, extra=0x0
}

class SongOfTorment(BossModule module) : Components.CastInterruptHint(module, AID.SongOfTorment, hintExtra: "Raidwide + Bleed");

//TODO: ideally this AOE should just wait for Effect Results, since they can be delayed by over 2.1s, which would cause unknowning players and AI to run back into the death zone, 
//not sure how to do this though considering there can be anywhere from 0-32 targets with different time for effect results each
class SeductiveSonata(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private DateTime _time;
    private bool casting;
    private static readonly AOEShapeCircle circle = new(16.2f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (casting || (_time != default && _time > WorldState.CurrentTime))
            yield return new(circle, Module.PrimaryActor.Position, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SeductiveSonata)
        {
            casting = true;
            _activation = Module.CastFinishAt(spell);
            _time = Module.CastFinishAt(spell, 2.2f);
        }
    }

    public override void Update()
    {
        if (_time != default && _time < WorldState.CurrentTime)
        {
            _time = default;
            casting = false;
        }
    }
}

class DeathlyVerse(BossModule module) : Components.StandardAOEs(module, AID.DeathlyVerse, new AOEShapeCircle(6));
class Tornado(BossModule module) : Components.StandardAOEs(module, AID.Tornado, 6);
class FourfoldSuffering(BossModule module) : Components.StandardAOEs(module, AID.FourfoldSuffering, new AOEShapeDonut(5, 50));
class AncientAero(BossModule module) : Components.StandardAOEs(module, AID.AncientAero, new AOEShapeRect(42.4f, 3));
class AncientAeroIII(BossModule module) : Components.RaidwideCast(module, AID.AncientAeroIII);
class AncientAeroIIIKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AncientAeroIII, 10, shape: new AOEShapeCircle(30));

class AglaopeStates : StateMachineBuilder
{
    public AglaopeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SongOfTorment>()
            .ActivateOnEnter<SeductiveSonata>()
            .ActivateOnEnter<DeathlyVerse>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<FourfoldSuffering>()
            .ActivateOnEnter<AncientAero>()
            .ActivateOnEnter<AncientAeroIII>()
            .ActivateOnEnter<AncientAeroIIIKB>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8653)]
public class Aglaope(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
