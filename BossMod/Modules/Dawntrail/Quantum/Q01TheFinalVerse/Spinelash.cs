namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class Spinelash(BossModule module) : Components.GenericWildCharge(module, 4, null, 60)
{
    private Actor? _target;

    public BitMask IntactWindows = BitMask.Build(0, 1, 2);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Target && Raid.TryFindSlot(actor, out var slot))
        {
            _target = actor;
            Activation = WorldState.FutureTime(10.5f);
            PlayerRoles[slot] = actor.Role == Role.Tank ? PlayerRole.Target : PlayerRole.TargetNotFirst;
            foreach (var (s, p) in Raid.WithSlot().Exclude(actor))
                PlayerRoles[s] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;

            // TODO: refactor GenericWildCharge so this isn't necessary :(
            Source = new Actor(1, 0, 819, 0, "fake actor", 0, ActorType.Enemy, Class.ACN, 1, Module.PrimaryActor.PosRot with { X = actor.Position.X });
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (PlayerRoles[pcSlot] is PlayerRole.Target or PlayerRole.TargetNotFirst)
        {
            var col = AimedAtWindow(pc.Position) ? ArenaColor.Safe : ArenaColor.Danger;
            foreach (var bit in IntactWindows.SetBits())
                Arena.AddLine(new WPos(WindowCenterX(bit) + 7, -284), new WPos(WindowCenterX(bit) - 7, -284), col, 2);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (PlayerRoles[slot] is PlayerRole.Target or PlayerRole.TargetNotFirst)
            hints.Add("Aim for window!", !AimedAtWindow(actor.Position));
    }

    private float WindowCenterX(int windowBit) => -614 + windowBit * 14;

    private bool AimedAtWindow(WPos pos)
    {
        foreach (var win in IntactWindows.SetBits())
            if (MathF.Abs(pos.X - WindowCenterX(win)) < 7)
                return true;

        return false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Spinelash)
        {
            NumCasts++;
            Source = null;
            _target = null;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }

    public override void Update()
    {
        if (_target != null && Source != null)
            Source.PosRot.X = _target.Position.X;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var window = index - 0x18;
        if (window is >= 0 and <= 2 && state == 0x00020001)
            IntactWindows.Clear(window);
    }
}

class Vodoriga(BossModule module) : Components.Adds(module, (uint)OID.VodorigaMinion, forbidDots: true);
class TerrorEye(BossModule module) : Components.StandardAOEs(module, AID.TerrorEye, 6);
