namespace BossMod.Dawntrail.Foray.ForkedTower.FTM3Necrophobia;

public enum OID : uint
{
    Boss = 0x4BE5, // R5.001, x1
    Helper = 0x233C, // R0.500, x25, Helper type
    SeveringHead = 0x4BE6, // R1.410, x8
    Necrophobia = 0x4BE9, // R1.000, x1
}

public enum AID : uint
{
    AutoAttack = 47451, // Boss->player, no cast, single-target
    HailOfHellflaresCast = 47452, // Boss->self, 5.0s cast, single-target
    HailOfHellflaresFirst = 47453, // Helper->self, no cast, ???
    HailOfHellflaresVisual = 48956, // Helper->self, no cast, single-target
    HailOfHellflaresRest = 48957, // Helper->self, no cast, ???
    DeathWall = 47454, // 4BE9->self, no cast, range 24-30 donut
    BossJump = 47450, // Boss->location, no cast, single-target
    AncientFireIIIBoss = 47455, // Boss->self, 5.0s cast, range 18 circle
    AncientFireIIIHead = 47468, // 4BE6->self, 5.5s cast, range 18 circle
    SeveredFireIII = 47465, // Boss->self, 5.5s cast, range 18 circle
    AncientBlizzardIIIBoss = 47456, // Boss->self, 5.0s cast, range 45 width 15 cross
    AncientBlizzardIIIHead = 47469, // 4BE6->self, 5.5s cast, range 45 width 15 cross
    SeveredBlizzardIII = 47466, // Boss->self, 5.5s cast, range 45 width 15 cross
    CorpseMangler = 47459, // Boss->player, 5.0s cast, single-target
    Capitation = 47460, // Boss->self, no cast, single-target
    HeadTeleport1 = 47462, // 4BE6->location, no cast, single-target
    HeadTeleport2 = 47464, // 4BE6->location, no cast, single-target
    HeadTeleport3 = 47472, // 4BE6->location, no cast, single-target
    DeathShroud = 47461, // Boss->self, 7.0s cast, single-target
    HeadsRollCast = 47463, // Boss->self, 3.0s cast, single-target
    HeadsRollInstant = 47474, // Boss->self, no cast, single-target
    VacuumWave = 47473, // Boss->self, 4.0s cast, range 30 180-degree cone
    DeathlyRay = 47475, // 4BE6->self, 5.0s cast, range 30 width 6 rect
    DarkCurrentCast = 47476, // Boss->self, 4.2+1.3s cast, single-target
    DarkCurrentFirst = 47477, // Helper->self, 5.5s cast, range 60 width 10 rect
    DarkCurrentRest = 47478, // Helper->self, 1.0s cast, range 10 width 60 rect
    SeveredDarkCurrentCast = 47479, // Boss->self, 4.2+1.3s cast, single-target
    SeveredThunderIIICast = 47467, // Boss->self, 4.7+0.8s cast, single-target
    AncientThunderIIIBossCast = 47457, // Boss->self, 4.2+0.8s cast, single-target
    AncientThunderIIIHeadCast = 47470, // 4BE6->self, 4.7+0.8s cast, single-target
    AncientThunderIII1 = 47458, // Helper->self, 5.0s cast, range 60 45-degree cone
    AncientThunderIII2 = 47471, // Helper->self, 5.5s cast, range 60 45-degree cone
    SeveredThunderIII = 50357, // Helper->self, 5.5s cast, range 60 45-degree cone
}

public enum SID : uint
{
    Unk2552 = 2552, // none->Boss, extra=0x45B/0x45A/0x45C
    Unk4956 = 4956, // none->4BE6, extra=0x2C4
    Unk3558 = 3558, // none->4BE6, extra=0x47C/0x47D/0x47E
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
}

public enum TetherID : uint
{
    Fire = 400, // 4BE6->Boss
    Ice = 401, // 4BE6->Boss
    Thunder = 402, // 4BE6->Boss
}

class HailOfHellflares(BossModule module) : Components.RaidwideCastDelay(module, AID.HailOfHellflaresCast, AID.HailOfHellflaresFirst, 0.8f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HailOfHellflaresFirst or AID.HailOfHellflaresRest)
        {
            NumCasts++;
            if (NumCasts >= 10)
            {
                NumCasts = 0;
                Activation = default;
            }
        }
    }
}

class AncientBlizzardIII(BossModule module) : Components.GroupedAOEs(module, [AID.AncientBlizzardIIIBoss, AID.AncientBlizzardIIIHead, AID.SeveredBlizzardIII], new AOEShapeCross(45, 7.5f));
class AncientFireIII(BossModule module) : Components.GroupedAOEs(module, [AID.AncientFireIIIBoss, AID.AncientFireIIIHead, AID.SeveredFireIII], new AOEShapeCircle(18));
class CorpseMangler(BossModule module) : Components.SingleTargetCast(module, AID.CorpseMangler);
class VacuumWave(BossModule module) : Components.StandardAOEs(module, AID.VacuumWave, new AOEShapeCone(30, 90.Degrees()));
class DeathlyRay(BossModule module) : Components.StandardAOEs(module, AID.DeathlyRay, new AOEShapeRect(30, 3));
class AncientThunderIII(BossModule module) : Components.GroupedAOEs(module, [AID.AncientThunderIII1, AID.AncientThunderIII2, AID.SeveredThunderIII], new AOEShapeCone(60, 22.5f.Degrees()));

class DarkCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(60, 5))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DarkCurrentFirst)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = spell.Rotation.ToDirection().OrthoR() * 10,
                Rotation = spell.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 3,
                MaxShownExplosions = 2
            });
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = spell.Rotation.ToDirection().OrthoL() * 10,
                Rotation = spell.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 3,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkCurrentFirst)
        {
            foreach (var l in Lines)
                AdvanceLine(l, l.Next);
        }

        if ((AID)spell.Action.ID == AID.DarkCurrentRest)
        {
            if (Lines.MaxBy(l => l.ExplosionsLeft) is { } l)
                AdvanceLine(l, l.Next);
            Lines.RemoveAll(l => l.ExplosionsLeft <= 0);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (c, t, r) in ImminentAOEs())
            hints.AddForbiddenZone(Shape, c, r, t);

        if (Lines is [{ ExplosionsLeft: 3 } l1, ..])
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(l1.Next, l1.Rotation, 60, 0, 7.5f), l1.NextExplosion.AddSeconds(l1.TimeToMove));
    }
}

class FT13NecrophobiaStates : StateMachineBuilder
{
    public FT13NecrophobiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HailOfHellflares>()
            .ActivateOnEnter<AncientBlizzardIII>()
            .ActivateOnEnter<AncientFireIII>()
            .ActivateOnEnter<CorpseMangler>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<DeathlyRay>()
            .ActivateOnEnter<AncientThunderIII>()
            .ActivateOnEnter<DarkCurrent>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14503)]
public class FTM3Necrophobia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 800), new ArenaBoundsCircle(24))
{
    public override bool DrawAllPlayers => true;
}

