#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.YmirPiece;

public enum OID : uint
{
    Boss = 0x4C93, // R4.000, x1
    _Gen_SahaginPiece = 0x4C95, // R2.000, x1
    Helper = 0x233C, // R0.500, x5, Helper type
    _Gen_YmirShell = 0x4C94, // R2.000, x1 (spawn during fight), Part type
}

public enum AID : uint
{
    _Spell_Water = 48626, // 4C95->player, no cast, single-target
    _Spell_ParalyzingSpikes = 50532, // 4C95->self, 3.0s cast, single-target
    _Weaponskill_HeadSnatch = 48477, // Boss->self, no cast, range 7 120?-degree cone
    _Spell_WaterII = 48483, // Helper->location, 3.0s cast, range 6 circle
    _Spell_WaterII1 = 48482, // 4C95->self, 3.0s cast, single-target
    _Weaponskill_BlanketThunder = 48479, // Boss->self, 5.0s cast, range 40 circle
    _Weaponskill_Tsunami = 48480, // 4C95->self, 8.0s cast, single-target
    _Weaponskill_Tsunami1 = 48481, // Helper->self, 8.0s cast, range 60 width 60 rect
    _Ability_ = 48484, // 4C95->location, no cast, single-target
    _Ability_1 = 48476, // Boss->location, no cast, single-target
    _Spell_Dreadwash = 48485, // 4C95->self, 8.0s cast, range 30 circle
}

public enum SID : uint
{
    _Gen_ParalyzingSpikes = 5434, // none->4C95, extra=0x64
    _Gen_Paralysis = 5388, // 4C95->player, extra=0x0
    _Gen_VulnerabilityDown = 2198, // none->Boss, extra=0x0
}

public enum TetherID : uint
{
    _Gen_Tether_chn_thunder1f = 6, // 4C95->Boss
}

class VulnerabilityDown(BossModule module) : Components.InvincibleStatus(module, (uint)SID._Gen_VulnerabilityDown, "Attack the shell!");
class YmirShell(BossModule module) : Components.Adds(module, (uint)OID._Gen_YmirShell);
class SahaginPiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_SahaginPiece)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == (uint)OID._Gen_SahaginPiece))
        {
            target.Priority = 0;
            target.TankDistance = 30;

            // it is real spikes, but we don't want to make it annoying to manually use PB/TR on sahagin since he can be bursted down before he uses tsunami
            // TODO: it should be possible to express this some other way
            if (target.Actor.FindStatus(SID._Gen_ParalyzingSpikes) != null)
                target.Priority = AIHints.Enemy.PriorityInvincible;
        }
    }
}

class HeadSnatch(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.FindStatus(SID._Gen_VulnerabilityDown) != null)
            Arena.AddCone(Module.PrimaryActor.Position, 7, Module.PrimaryActor.Rotation, 60.Degrees(), ArenaColor.Danger);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.FindStatus(SID._Gen_VulnerabilityDown) != null)
            hints.AddForbiddenZone(ShapeDistance.Cone(Module.PrimaryActor.Position, 7, Module.PrimaryActor.Rotation, 90.Degrees()), DateTime.MaxValue);
    }
}

class WaterII(BossModule module) : Components.StandardAOEs(module, AID._Spell_WaterII, 6);

class BlanketThunder(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_BlanketThunder);
class Tsunami(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_Tsunami1, 35, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters is [{ CastInfo: { } ci }, ..])
        {
            var rot = (ci.Rotation + 180.Degrees()).ToDirection();
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center + rot * 15, Arena.Center + rot * 20, 20), Module.CastFinishAt(ci));
        }
    }
}
class Dreadwash(BossModule module) : Components.CastInterruptHint(module, AID._Spell_Dreadwash);

class YmirPieceStates : StateMachineBuilder
{
    public YmirPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VulnerabilityDown>()
            .ActivateOnEnter<YmirShell>()
            .ActivateOnEnter<SahaginPiece>()
            .ActivateOnEnter<HeadSnatch>()
            .ActivateOnEnter<WaterII>()
            .ActivateOnEnter<BlanketThunder>()
            .ActivateOnEnter<Tsunami>()
            .ActivateOnEnter<Dreadwash>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && ((YmirPiece)module).Sahagin is { IsDeadOrDestroyed: true };
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14569)]
public class YmirPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20))
{
    public Actor? Sahagin { get; private set; }

    protected override void UpdateModule()
    {
        Sahagin ??= Enemies(OID._Gen_SahaginPiece).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actor(Sahagin, ArenaColor.Enemy);
    }
}

