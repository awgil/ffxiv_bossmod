namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P1FiendishRage(BossModule module) : Components.CastCounter(module, AID.FiendishRage)
{
    private BitMask _targets;

    private const float _range = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.Any())
        {
            int numClips = Raid.WithSlot(true).IncludedInMask(_targets).InRadius(actor.Position, _range).Count();
            if (Module.PrimaryActor.TargetID == actor.InstanceID)
            {
                if (numClips > 0)
                {
                    hints.Add("GTFO from marked players!");
                }
            }
            else if (numClips != 1)
            {
                hints.Add("Stack with single group!");
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var target in Raid.WithSlot(true).IncludedInMask(_targets))
            Arena.AddCircle(target.Item2.Position, _range, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.FiendishRage)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }
}
