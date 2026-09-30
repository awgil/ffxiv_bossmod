namespace BossMod.RealmReborn.Dungeon.D04Halatali.D042ThunderclapGuivre;

public enum OID : uint
{
    Boss = 0x4644,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    Electrify = 40595, // Boss->location, 4.0s cast, range 6 circle

    HydroelectricShockVisual = 40593, // Boss->self, 9.0+1.0s cast, single-target visual (water electrifies)
    HydroelectricShock = 41113, // Helper->self, 10.0s cast, custom arena (get to dry ground)

    Levinfang = 40594 // Boss->player, 5.0s cast, single-target
}

class Electrify(BossModule module) : Components.StandardAOEs(module, AID.Electrify, 6);
class HydroelectricShock(BossModule module) : BossComponent(module)
{
    private static readonly (WPos center, float radius)[] DryPads = [(new(-182.8f, -106.6f), 4), (new(-174.6f, -162.9f), 2.5f)];
    private DateTime _activation;

    private static bool OnPad(WPos pos) => DryPads.Any(p => pos.InCircle(p.center, p.radius));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation != default)
            hints.Add("Get to dry ground!", !OnPad(actor.Position));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default)
            return;
        var pad = DryPads.MinBy(p => (p.center - actor.Position).LengthSq());
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pad.center, pad.radius), _activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_activation != default)
            foreach (var p in DryPads)
                Arena.ZoneCircle(p.center, p.radius, ArenaColor.SafeFromAOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydroelectricShock)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HydroelectricShock)
            _activation = default;
    }
}

class Levinfang(BossModule module) : Components.SingleTargetCast(module, AID.Levinfang);

class D042ThunderclapGuivreStates : StateMachineBuilder
{
    public D042ThunderclapGuivreStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrify>()
            .ActivateOnEnter<HydroelectricShock>()
            .ActivateOnEnter<Levinfang>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7, NameID = 1196)]
public class D042ThunderclapGuivre(ModuleInit init) : BossModule(init, new(-179, -133), new ArenaBoundsRect(26, 34));
