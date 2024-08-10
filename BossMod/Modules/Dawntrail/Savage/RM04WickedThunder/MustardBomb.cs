namespace BossMod.Dawntrail.Savage.RM04WickedThunder;

class MustardBomb(BossModule module) : Components.UniformStackSpread(module, 0, 6, alwaysShowSpreads: true)
{
    public enum Mechanic { None, Tethers, Nisi, Done }

    public Mechanic CurMechanic;
    private Actor? _tetherTarget;
    private BitMask _bombTargets;
    private BitMask _forbidden;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        switch (CurMechanic)
        {
            case Mechanic.Tethers:
                var wantTether = actor.Role == Role.Tank;
                var hasTether = _bombTargets[slot];
                if (wantTether != hasTether)
                    hints.Add(wantTether ? "Grab tether!" : "Pass tether!");
                break;
            case Mechanic.Nisi:
                var wantNisi = !_forbidden[slot];
                var hasNisi = _bombTargets[slot];
                if (wantNisi != hasNisi)
                    hints.Add(wantNisi ? "Grab nisi!" : "Pass nisi!");
                break;
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => CurMechanic switch
    {
        Mechanic.Tethers => PlayerPriority.Normal,
        Mechanic.Nisi => _bombTargets[playerSlot] || !_forbidden[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal,
        _ => PlayerPriority.Irrelevant
    };

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurMechanic == Mechanic.Tethers && _tetherTarget != null)
            foreach (var (_, p) in Raid.WithSlot().IncludedInMask(_bombTargets))
                Arena.AddLine(_tetherTarget.Position, p.Position, ArenaColor.Danger);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.MustardBomb)
        {
            _bombTargets.Set(Raid.FindSlot(source.InstanceID));
            if (CurMechanic == Mechanic.None)
            {
                CurMechanic = Mechanic.Tethers;
                _tetherTarget = WorldState.Actors.Find(tether.Target);
                AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(8.8f));
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.MustardBomb)
            _bombTargets.Clear(Raid.FindSlot(source.InstanceID));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MustardBomb)
        {
            _bombTargets.Set(Raid.FindSlot(actor.InstanceID));
            AddSpread(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MustardBomb)
        {
            _bombTargets.Clear(Raid.FindSlot(actor.InstanceID));
            Spreads.RemoveAll(s => s.Target == actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MustardBombFirst:
            case AID.KindlingCauldron:
                CurMechanic = Mechanic.Nisi;
                Spreads.Clear();
                _forbidden.Set(Raid.FindSlot(spell.MainTargetID));
                break;
            case AID.MustardBombSecond:
                CurMechanic = Mechanic.Done;
                Spreads.Clear();
                break;
        }
    }
}
