namespace BossMod.Stormblood.Hunt.RankS.Gamma;

public enum OID : uint
{
    Boss = 0x1AB2, // R=8.4
};

public enum AID : uint
{
    AutoAttack = 7351, // Boss->player, no cast, single-target
    MagitekCannon = 7912, // Boss->player, no cast, single-target
    DiffractiveLaser = 7914, // Boss->location, 2,0s cast, range 5 circle
    MagitekFlamehook = 7913, // Boss->self, 1,5s cast, range 30+R circle, raidwide + pyretic
    LimitCut = 7916, // Boss->self, 2,0s cast, single-target, applies Haste to self
    Launcher = 7915, // Boss->self, 1,5s cast, range 30+R circle, raidwide, does %HP dmg (10%, 20%, 30%, or 50%)
};

public enum SID : uint
{
    Pyretic = 960, // Boss->player, extra=0x0
    Haste = 8, // Boss->Boss, extra=0x0
};

class DiffractiveLaser : Components.LocationTargetedAOEs
{
    public DiffractiveLaser() : base(ActionID.MakeSpell(AID.DiffractiveLaser), 5) { }
}

class MagitekFlamehook : Components.RaidwideCast
{
    public MagitekFlamehook() : base(ActionID.MakeSpell(AID.MagitekFlamehook), "Raidwide + Pyretic") { }
}

class Launcher : Components.RaidwideCast
{
    public Launcher() : base(ActionID.MakeSpell(AID.Launcher), "Raidwide (%HP based)") { }
}

class MagitekFlamehookPyretic : BossComponent
{ //Note: boss is lvl 70, so this pyretic can probably be ignored at lvl 90, but we assume the player is also around lvl 70
    private BitMask _pyretic;
    public bool Pyretic { get; private set; }
    private bool casting;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MagitekFlamehook)
            casting = true;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MagitekFlamehook)
            casting = false;
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            _pyretic.Set(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            _pyretic.Clear(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_pyretic[slot] != Pyretic)
            hints.Add("Pyretic on you! STOP everything!");
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (casting)
            hints.Add("Applies Pyretic - STOP everything until it runs out!");
    }
}

class GammaStates : StateMachineBuilder
{
    public GammaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<MagitekFlamehook>()
            .ActivateOnEnter<Launcher>()
            .ActivateOnEnter<MagitekFlamehookPyretic>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 5985)]
public class Gamma : SimpleBossModule
{
    public Gamma(WorldState ws, Actor primary) : base(ws, primary) { }
}
