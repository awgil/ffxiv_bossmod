namespace BossMod.Heavensward.Dungeon.D05GreatGubalLibrary.D052Byblos;

public enum OID : uint
{
    Boss = 0xE83, // R3.000, x?
    Page64 = 0x10B7, // R1.200, x?
    WhaleOil = 0x10C4, // R2.000, x?
    TomeWind = 0x10C5, // R1.000, x?
    Helper = 0x233C, // x3
}
public enum AID : uint
{
    Attack = 870, // 105A/105B/104F/104E/1051/1060/1069/105F/1053/1054/1068->player, no cast, single-target
    PageTear = 4159, // E83->self, no cast, range 5+R ?-degree cone
    HeadDown = 4163, // E83->player, 4.0s cast, width 8 rect charge
    BoneShaker = 4164, // E83->self, no cast, range 50+R circle
    Bibliocide = 4167, // 10C4->self, no cast, range 3 circle
    GaleCut = 4158, // E83->self, 3.0s cast, single-target
    TailSmash = 4165, // E83->self, 2.5s cast, range 9+R 90-degree cone

    DeathRay = 5058, // 10B7->self, 4.0s cast, range 23+R width 3 rect
}
public enum TetherID : uint
{
    WhaleOilTether = 3,
}

class PageTear(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PageTear), new AOEShapeCone(5f + 3f, 45.Degrees()));
class HeadDown(BossModule module) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(AID.HeadDown), 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaitsNotOn(actor))
            hints.AddForbiddenZone(b.Shape, BaitOrigin(b), b.Rotation, b.Activation);
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(new AOEShapeDonut(3f + 2.6f, 23f), BaitOrigin(b), b.Rotation, b.Activation);
    }
}
class BoneShaker(BossModule module) : Components.RaidwideInstant(module, ActionID.MakeSpell(AID.BoneShaker), 5.1f);
class Bibliocide(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(3f), (uint)TetherID.WhaleOilTether, ActionID.MakeSpell(AID.Bibliocide))
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (DrawTethers)
        {
            foreach (var b in ActiveBaits)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                Arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
                Arena.AddCircle(Module.Center, 3f, ArenaColor.Safe);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        {
            foreach (var b in ActiveBaits)
            {
                if (b.Target == actor)
                {
                    hints.Add("Bait Tether through Boss!");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        {
            foreach (var b in ActiveBaits)
            {
                if (b.Target == actor)
                {
                    hints.AddForbiddenZone(new AOEShapeDonut(3f, 23f), Module.Center);
                }
                else
                {
                    hints.AddForbiddenZone(new AOEShapeCircle(3f), b.Source.Position, b.Rotation, b.Activation);
                }
            }
        }
    }
};
class GaleCut(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.GaleCut));
class TailSmash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailSmash), new AOEShapeCone(9f + 3f, 45.Degrees()));
class DeathRay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DeathRay), new AOEShapeRect(23f + 3f, 1.5f));
class TomeWind(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> TomeWinds => Module.Enemies(OID.TomeWind).Where(e => !e.IsDead);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in TomeWinds)
            Arena.AddCircleFilled(b.Position, 3, ArenaColor.AOE);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in TomeWinds)
            hints.AddForbiddenZone(new AOEShapeCircle(3), b.Position);
    }
}
class AddsModule(BossModule module) : Components.Adds(module, (uint)OID.Page64);
class D052ByblosStates : StateMachineBuilder
{
    public D052ByblosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PageTear>()
            .ActivateOnEnter<HeadDown>()
            .ActivateOnEnter<BoneShaker>()
            .ActivateOnEnter<Bibliocide>()
            .ActivateOnEnter<GaleCut>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<DeathRay>()
            .ActivateOnEnter<TomeWind>()
            .ActivateOnEnter<AddsModule>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 31, NameID = 3925)]
public class D052Byblos(WorldState ws, Actor primary) : BossModule(ws, primary, new(177.8f, 27.1f), new ArenaBoundsCircle(23));
