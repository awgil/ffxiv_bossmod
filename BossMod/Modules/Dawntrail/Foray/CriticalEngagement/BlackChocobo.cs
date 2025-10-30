namespace BossMod.Dawntrail.Foray.CriticalEngagement.BlackChocobo;

public enum OID : uint
{
    BlackStar = 0x46A5,
    Chocobo1 = 0x46A6,
    Boss = 0x46A7,
    Chocobo2 = 0x46A8,
    Chocobo4 = 0x46A9,
    DeathWallHelper = 0x4861,
    Helper = 0x233C,
}

public enum AID : uint
{
    DeathWall = 41394,
    ChocoAero = 41165, // Boss->player, no cast, single-target
    ChocoAttack = 43032, // Chocobo1->player, no cast, single-target
    ChocoBeak = 41163, // Chocobo2->location, 5.0s cast, width 4 rect charge
    ChocoMaelfeather = 41164, // Chocobo4->self, 5.0s cast, range 8 circle
    AutoAttack = 43260, // BlackStar->player, no cast, single-target
    ChocoWindstorm = 41147, // BlackStar->self, 7.0s cast, range 16 circle
    ChocoCyclone = 41148, // BlackStar->self, 7.0s cast, range 8-30 donut
    ChocoSlaughterCast = 41149, // BlackStar->self, 5.0s cast, single-target
    ChocoSlaughterFirst = 41151, // Helper->self, 5.0s cast, range 5 circle
    ChocoSlaughterRest = 41152, // Helper->location, no cast, range 5 circle
    ChocoBlades1 = 41155, // Helper->self, 5.0s cast, range 40 45-degree cone
    ChocoBlades2 = 41156, // Helper->self, 8.0s cast, range 40 45-degree cone
    ChocoDoublades = 41153, // BlackStar->self, 5.0s cast, single-target
    Unk = 41154, // BlackStar->self, no cast, single-target
    ChocoAeroII = 41162, // Helper->location, 3.0s cast, range 4 circle
}

class ChocoBeak(BossModule module) : Components.ChargeAOEs(module, AID.ChocoBeak, 2);
class ChocoMaelfeather(BossModule module) : Components.StandardAOEs(module, AID.ChocoMaelfeather, 8);
class Priority(BossModule module) : Components.AddsMulti(module, [OID.Chocobo1, OID.BlackStar]);
class ChocoWindstorm(BossModule module) : Components.StandardAOEs(module, AID.ChocoWindstorm, 16);
class ChocoCyclone(BossModule module) : Components.StandardAOEs(module, AID.ChocoCyclone, new AOEShapeDonut(8, 30));
class ChocoBlades(BossModule module) : Components.GroupedAOEs(module, [AID.ChocoBlades1, AID.ChocoBlades2], new AOEShapeCone(40, 22.5f.Degrees()), maxCasts: 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChocoBlades1:
                Casters.Insert(0, caster);
                break;
            case AID.ChocoBlades2:
                Casters.Add(caster);
                break;
        }
    }
}
class ChocoAeroII(BossModule module) : Components.StandardAOEs(module, AID.ChocoAeroII, 4);

class ChocoSlaughter(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(5))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChocoSlaughterFirst)
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = caster.Rotation.ToDirection() * 5,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1.1f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 5
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ChocoSlaughterFirst or AID.ChocoSlaughterRest)
        {
            var l = Lines.FindIndex(l => l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (l >= 0)
            {
                AdvanceLine(Lines[l], caster.Position);
                if (Lines[l].ExplosionsLeft <= 0)
                    Lines.RemoveAt(l);
            }
        }
    }
}

class BlackChocoboStates : StateMachineBuilder
{
    private readonly BlackChocobo _module;

    public BlackChocoboStates(BlackChocobo module) : base(module)
    {
        _module = module;

        SimplePhase(0, id => Timeout(id, 9999, "P1 enrage"), "P1")
            .ActivateOnEnter<ChocoBeak>()
            .ActivateOnEnter<ChocoMaelfeather>()
            .ActivateOnEnter<Priority>()
            .Raw.Update = () => _module.BlackStar?.IsTargetable == true || _module.Helper?.IsDeadOrDestroyed == true;
        SimplePhase(1, id => Timeout(id, 9999, "P2 enrage"), "P2")
            .ActivateOnEnter<ChocoBeak>()
            .ActivateOnEnter<ChocoMaelfeather>()
            .ActivateOnEnter<Priority>()
            .ActivateOnEnter<ChocoWindstorm>()
            .ActivateOnEnter<ChocoCyclone>()
            .ActivateOnEnter<ChocoBlades>()
            .ActivateOnEnter<ChocoAeroII>()
            .ActivateOnEnter<ChocoSlaughter>()
            .Raw.Update = () => _module.BlackStar?.IsDeadOrDestroyed == true || _module.Helper?.IsDeadOrDestroyed == true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13637)]
public class BlackChocobo(WorldState ws, Actor primary) : BossModule(ws, primary, new(450, 357), new ArenaBoundsSquare(20))
{
    public Actor? BlackStar { get; private set; }
    public Actor? Helper { get; private set; }

    protected override void UpdateModule()
    {
        BlackStar ??= Enemies(OID.BlackStar).FirstOrDefault();
        Helper ??= Enemies(OID.DeathWallHelper).FirstOrDefault();
    }

    public override bool DrawAllPlayers => true;
}
