namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class BreathOfDecay : Components.SimpleAOEs
{
    public BreathOfDecay(BossModule module) : base(module, (uint)AID.BreathOfDecay, new AOEShapeRect(40f, 4f), 2)
    {
        MaxDangerColor = 1;
    }
}

sealed class AeroIII(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AeroIII, 8f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDInvertedCircle(c.Origin, 4f), act);
            }
        }
    }
}

sealed class ProwlingGale(BossModule module) : Components.CastTowers(module, (uint)AID.ProwlingGale, 2f)
{
    private BitMask forbidden;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (forbidden.NumSetBits() < 4 && source.OID == (uint)OID.WolfOfWind1 && tether.ID is (uint)TetherID.WindsOfDecayGood or (uint)TetherID.WindsOfDecayBad)
        {
            forbidden.Set(Raid.FindSlot(tether.Target));
            if (forbidden.NumSetBits() == 4)
            {
                var towers = CollectionsMarshal.AsSpan(Towers);
                var len = towers.Length;
                for (var i = 0; i < len; ++i)
                {
                    ref var t = ref towers[i];
                    t.ForbiddenSoakers = forbidden;
                }
            }
        }
    }
}

sealed class Gust(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Gust, (uint)AID.Gust, 5f, 5.1f);
sealed class WindsOfDecayTether(BossModule module) : Components.StretchTetherDuo(module, 16f, 5.7f)
{
    private readonly AeroIII _kb = module.FindComponent<AeroIII>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.NumCasts == 1)
        {
            if (IsBaitTarget(actor) && _kb.IsImmune(slot, CurrentBaits.Ref(0).Activation))
            {
                base.AddAIHints(slot, actor, assignment, hints);
            }
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_kb.NumCasts == 1)
        {
            if (IsBaitTarget(actor) && _kb.IsImmune(slot, CurrentBaits.Ref(0).Activation))
            {
                base.AddHints(slot, actor, hints);
            }
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

sealed class WindsOfDecayBait(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Raidwide)
{
    private static readonly AOEShapeCone cone = new(40f, 15f.Degrees());
    private readonly AeroIII _kb = module.FindComponent<AeroIII>()!;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (CurrentBaits.Count < 4 && source.OID == (uint)OID.WolfOfWind1 && tether.ID is (uint)TetherID.WindsOfDecayGood or (uint)TetherID.WindsOfDecayBad)
        {
            if (WorldState.Actors.Find(tether.Target) is Actor t)
            {
                CurrentBaits.Add(new(source, t, cone, WorldState.FutureTime(7.1d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WindsOfDecay)
        {
            ++NumCasts;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_kb.NumCasts == 2)
            return;
        var baits = ActiveBaitsOn(pc);
        if (baits.Count != 0)
        {
            ref var bait = ref baits.Ref(0);
            if (_kb.IsImmune(pcSlot, bait.Activation))
            {
                return;
            }
            var center = Arena.Center;
            Arena.ZoneConeOutline(center, 0f, 4f, Angle.FromDirection(center - bait.Source.Position), 20f.Degrees(), Colors.Safe);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.NumCasts == 1)
        {
            var baits = ActiveBaitsOn(actor);
            if (baits.Count != 0)
            {
                ref var bait = ref baits.Ref(0);
                if (_kb.IsImmune(slot, bait.Activation))
                {
                    return;
                }
                var center = Arena.Center;
                hints.AddForbiddenZone(new SDInvertedCone(center, 4f, Angle.FromDirection(center - bait.Source.Position), 20f.Degrees()), bait.Activation);
            }
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_kb.NumCasts == 1)
        {
            if (IsBaitTarget(actor))
            {
                if (!_kb.IsImmune(slot, CurrentBaits.Ref(0).Activation))
                {
                    hints.Add("Wait in marked spot for knockback!");
                }
            }
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

sealed class TrackingTremors(BossModule module) : Components.StackWithIcon(module, (uint)IconID.TrackingTremors, (uint)AID.TrackingTremors, 6f, 6f, 8, 8, 8);
