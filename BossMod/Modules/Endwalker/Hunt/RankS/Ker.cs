namespace BossMod.Endwalker.Hunt.RankS.Ker;

public enum OID : uint
{
    Boss = 0x35CF, // R8.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    MinaxGlare = 27635, // Boss->self, 6.0s cast, range 40 circle
    Heliovoid = 27636, // Boss->self, 6.0s cast, range 12 circle
    AncientBlizzard = 27637, // Boss->self, 6.0s cast, range 8-40 donut
    AncientBlizzard2 = 27652, // Boss->self, 6.0s cast, range 8-40 donut
    WhispersManifest3 = 27659, // Boss->self, 6.0s cast, range 8-40 donut (remembered skill from Whispered Incantation)
    ForeInterment = 27641, // Boss->self, 6.0s cast, range 40 180-degree cone
    RearInterment = 27642, // Boss->self, 6.0s cast, range 40 180-degree cone
    RightInterment = 27643, // Boss->self, 6.0s cast, range 40 180-degree cone
    LeftInterment = 27644, // Boss->self, 6.0s cast, range 40 180-degree cone
    WhisperedIncantation = 27645, // Boss->self, 5.0s cast, single-target, applies status to boss, remembers next skill
    EternalDamnation = 27647, // Boss->self, 6.0s cast, range 40 circle gaze, applies doom
    EternalDamnation2 = 27640, // Boss->self, 6.0s cast, range 40 circle gaze, applies doom
    WhispersManifest4 = 27654, // Boss->self, 6.0s cast, range 40 circle gaze, applies doom
    AncientFlare = 27704, // Boss->self, 6.0s cast, range 40 circle, applies pyretic
    AncientFlare2 = 27638, // Boss->self, 6.0s cast, range 40 circle, applies pyretic
    WhispersManifest = 27706, // Boss->self, 6.0s cast, range 40 circle, applies pyretc (remembered skill from Whispered Incantation)
    AncientHoly = 27646, // Boss->self, 6.0s cast, range 40 circle, circle with dmg fall off, harmless after ca. range 20 (it is hard to say because people accumulate vuln stacks which skews the damage fall off with distance from source)
    AncientHoly2 = 27639, // Boss->self, 6.0s cast, range 40 circle, circle with dmg fall off, harmless after around range 20
    WhispersManifest2 = 27653, // Boss->self, 6.0s cast, range 40 circle, circle with dmg fall off, harmless after around range 20
    MirroredIncantation = 27927, // Boss->self, 3.0s cast, single-target, mirrors the next 3 interments
    MirroredIncantation2 = 27928, // Boss->self, 3.0s cast, single-target, mirrors the next 4 interments
    MirroredRightInterment = 27663, // Boss->self, 6.0s cast, range 40 180-degree cone
    MirroredLeftInterment = 27664, // Boss->self, 6.0s cast, range 40 180-degree cone
    MirroredForeInterment = 27661, // Boss->self, 6.0s cast, range 40 180-degree cone
    MirroredRearInterment = 27662, // Boss->self, 6.0s cast, range 40 180-degree cone
    //unknown = 25698, // Boss->player, no cast, single-target, no idea what this is for, gets very rarely used, my 6min replay from pull to death doesn't have it for instance

}
public enum SID : uint
{
    TemporaryMisdirection = 1422, // Boss->player, extra=0x2D0
    WhisperedIncantation = 2846, // Boss->Boss, extra=0x0
    MirroredIncantation = 2848, // Boss->Boss, extra=0x3/0x2/0x1/0x4
    Pyretic = 960, // Boss->player, extra=0x0
    Doom = 1970, // Boss->player
    WhispersManifest = 2847, // Boss->Boss, extra=0x0
}

class MinaxGlare(BossModule module) : Components.CastHint(module, AID.MinaxGlare, "Applies temporary misdirection");
class Heliovoid(BossModule module) : Components.StandardAOEs(module, AID.Heliovoid, new AOEShapeCircle(12));
class AncientBlizzard(BossModule module) : Components.StandardAOEs(module, AID.AncientBlizzard, new AOEShapeDonut(8, 40));
class AncientBlizzard2(BossModule module) : Components.StandardAOEs(module, AID.AncientBlizzard2, new AOEShapeDonut(8, 40));
class AncientBlizzardWhispersManifest(BossModule module) : Components.StandardAOEs(module, AID.WhispersManifest3, new AOEShapeDonut(8, 40));
class ForeInterment(BossModule module) : Components.StandardAOEs(module, AID.ForeInterment, new AOEShapeCone(40, 90.Degrees()));
class RearInterment(BossModule module) : Components.StandardAOEs(module, AID.RearInterment, new AOEShapeCone(40, 90.Degrees()));
class RightInterment(BossModule module) : Components.StandardAOEs(module, AID.RightInterment, new AOEShapeCone(40, 90.Degrees()));
class LeftInterment(BossModule module) : Components.StandardAOEs(module, AID.LeftInterment, new AOEShapeCone(40, 90.Degrees()));
class MirroredForeInterment(BossModule module) : Components.StandardAOEs(module, AID.MirroredForeInterment, new AOEShapeCone(40, 90.Degrees()));
class MirroredRearInterment(BossModule module) : Components.StandardAOEs(module, AID.MirroredRearInterment, new AOEShapeCone(40, 90.Degrees()));
class MirroredRightInterment(BossModule module) : Components.StandardAOEs(module, AID.MirroredRightInterment, new AOEShapeCone(40, 90.Degrees()));
class MirroredLeftInterment(BossModule module) : Components.StandardAOEs(module, AID.MirroredLeftInterment, new AOEShapeCone(40, 90.Degrees()));
class EternalDamnation(BossModule module) : Components.CastGaze(module, AID.EternalDamnation);
class EternalDamnationWhispersManifest(BossModule module) : Components.CastGaze(module, AID.WhispersManifest4);
class EternalDamnation2(BossModule module) : Components.CastGaze(module, AID.EternalDamnation2);
class WhisperedIncantation(BossModule module) : Components.CastHint(module, AID.WhisperedIncantation, "Remembers the next skill and uses it again when casting Whispers Manifest");

class MirroredIncantation(BossModule module) : BossComponent(module)
{
    private int Mirrorstacks;
    public enum Types { None, Mirroredx3, Mirroredx4 }
    public Types Type { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MirroredIncantation)
            Type = Types.Mirroredx3;
        if ((AID)spell.Action.ID == AID.MirroredIncantation2)
            Type = Types.Mirroredx4;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MirroredIncantation or AID.MirroredIncantation2)
            Type = Types.None;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor)
            switch ((SID)status.ID)
            {
                case SID.MirroredIncantation:
                    var stacks = status.Extra switch
                    {
                        0x1 => 1,
                        0x2 => 2,
                        0x3 => 3,
                        0x4 => 4,
                        _ => 0
                    };
                    Mirrorstacks = stacks;
                    break;
            }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor)
        {
            if ((SID)status.ID == SID.MirroredIncantation)
                Mirrorstacks = 0;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Mirrorstacks > 0)
            hints.Add($"Mirrored interments left: {Mirrorstacks}!");
        if (Type == Types.Mirroredx3)
            hints.Add("The next three interments will be mirrored!");
        if (Type == Types.Mirroredx4)
            hints.Add("The next four interments will be mirrored!");
    }
}

class AncientFlare(BossModule module) : BossComponent(module)
{
    private BitMask _pyretic;
    public bool Pyretic { get; private set; }
    private bool casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AncientFlare or AID.AncientFlare2 or AID.WhispersManifest)
            casting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AncientFlare or AID.AncientFlare2 or AID.WhispersManifest)
            casting = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            _pyretic.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyretic)
            _pyretic.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_pyretic[slot] != Pyretic)
            hints.Add("Pyretic on you! STOP everything!");
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (casting)
            hints.Add("Applies Pyretic - STOP everything until it runs out!");
    }
}

class AncientHoly(BossModule module) : Components.StandardAOEs(module, AID.AncientHoly, new AOEShapeCircle(20));
class AncientHoly2(BossModule module) : Components.StandardAOEs(module, AID.AncientHoly2, new AOEShapeCircle(20));
class AncientHolyWhispersManifest(BossModule module) : Components.StandardAOEs(module, AID.WhispersManifest2, new AOEShapeCircle(20));

// TODO: wicked swipe, check if there are even more skills missing

class KerStates : StateMachineBuilder
{
    public KerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MinaxGlare>()
            .ActivateOnEnter<Heliovoid>()
            .ActivateOnEnter<AncientBlizzard>()
            .ActivateOnEnter<AncientBlizzard2>()
            .ActivateOnEnter<AncientBlizzardWhispersManifest>()
            .ActivateOnEnter<ForeInterment>()
            .ActivateOnEnter<RearInterment>()
            .ActivateOnEnter<RightInterment>()
            .ActivateOnEnter<LeftInterment>()
            .ActivateOnEnter<MirroredForeInterment>()
            .ActivateOnEnter<MirroredRearInterment>()
            .ActivateOnEnter<MirroredRightInterment>()
            .ActivateOnEnter<MirroredLeftInterment>()
            .ActivateOnEnter<MirroredIncantation>()
            .ActivateOnEnter<EternalDamnation>()
            .ActivateOnEnter<EternalDamnation2>()
            .ActivateOnEnter<EternalDamnationWhispersManifest>()
            .ActivateOnEnter<AncientFlare>()
            .ActivateOnEnter<AncientHoly>()
            .ActivateOnEnter<AncientHoly2>()
            .ActivateOnEnter<AncientHolyWhispersManifest>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.SS, NameID = 10615)]
public class Ker(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
