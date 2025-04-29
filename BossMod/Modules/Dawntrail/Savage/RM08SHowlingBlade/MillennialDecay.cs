namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class MillennialDecay(BossModule module) : Components.RaidwideCast(module, AID.MillennialDecay);

class BreathOfDecay(BossModule module) : Components.StandardAOEs(module, AID.BreathOfDecay, new AOEShapeRect(40, 4))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            yield return aoe with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 };
            if (++i > 2)
                break;
        }
    }
}

class Gust(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Gust, AID.Gust, 5, 5.1f);

class AeroIII(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AeroIII, 8);

class ProwlingGale(BossModule module) : Components.CastTowers(module, AID.ProwlingGale1, 2, maxSoakers: 1)
{
    private BitMask Tethers;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.WolfOfWindDecay && (TetherID)tether.ID is TetherID.Danger or TetherID.Generic)
            UpdateMask(Raid.FindSlot(tether.Target));
    }

    private void UpdateMask(int slot = -1)
    {
        Tethers.Set(slot);

        for (var i = 0; i < Towers.Count; i++)
            Towers.Ref(i).ForbiddenSoakers = Tethers;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            UpdateMask();
    }
}

class WindsOfDecay : Components.GenericBaitAway
{
    private DateTime Activation;

    public WindsOfDecay(BossModule module) : base(module, AID.WindsOfDecay)
    {
        EnableHints = false;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.WolfOfWindDecay && (TetherID)tether.ID is TetherID.Danger or TetherID.Generic && !CurrentBaits.Any(b => b.Target.InstanceID == tether.Target))
        {
            if (Activation == default)
                Activation = WorldState.FutureTime(7.2f);

            CurrentBaits.Add(new(source, WorldState.Actors.Find(tether.Target)!, new AOEShapeCone(40, 15.Degrees()), Activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class WindsOfDecayTether(BossModule module) : Components.CastCounter(module, AID.WindsOfDecay)
{
    private DateTime Activation;

    private readonly Dictionary<Actor, (ulong Target, bool Stretched)> Tethers = [];

    public bool EnableHints;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.WolfOfWindDecay && (TetherID)tether.ID is TetherID.Danger or TetherID.Generic)
        {
            if (Activation == default)
                Activation = WorldState.FutureTime(7.2f);

            Tethers[source] = (tether.Target, (TetherID)tether.ID == TetherID.Generic);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Tethers.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            foreach (var (_, to) in Tethers)
                if (to.Target == actor.InstanceID)
                    hints.Add("Stretch tether!", !to.Stretched);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (from, to) in Tethers)
            if (to.Target == pc.InstanceID)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(from.Position, pc.Position, 0xFF000000, 2);
                Arena.AddLine(from.Position, pc.Position, to.Stretched ? ArenaColor.Safe : ArenaColor.Danger);
            }
    }
}
