namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

abstract class MalformedReincarnation(BossModule module, uint aid) : Components.CastCounter(module, aid);
sealed class NMalformedReincarnation(BossModule module) : MalformedReincarnation(module, (uint)AID.NMalformedReincarnationAOE);
sealed class SMalformedReincarnation(BossModule module) : MalformedReincarnation(module, (uint)AID.SMalformedReincarnationAOE);

// TODO: initial hints (depending on strat?) + specific towers
sealed class MalformedPrayer2(BossModule module) : Components.GenericTowers(module)
{
    private BitMask _blueTowers;
    private BitMatrix _playerBlue; // [i] = blue debuffs for slot i; 0 = bait, 1/2/3 = soaks
    private bool _baitsDone;

    private const float TowerRadius = 4f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_baitsDone && (Towers.Any(t => t.Position.InCircle(actor.Position, TowerRadius * 2f)) || Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, TowerRadius * 2f).Any()))
            hints.Add("Bait away from other towers!");
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!_baitsDone)
            foreach (var p in Raid.WithoutSlot(false, true, true))
                Arena.ZoneCircleOutline(p.Position, TowerRadius, Colors.Danger);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.OrangeTower1 or (uint)OID.BlueTower1)
        {
            AddTower(actor.Position, actor.OID == (uint)OID.BlueTower1);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            // orange (anim/circle, see RousingReincarnation) blue
            // 17/3A                   18/3B | 1F/27                   20/28
            //       13/36       14/37       |       1B/23       1C/24
            //             -----             |             -----
            //       15/38       16/39       |       1D/25       1E/26
            // 19/3C                   1A/3D | 21/29                   22/2A
            var (offset, blue) = index switch
            {
                0x13 => (new WDir(-5f, -5f), false),
                0x14 => (new WDir(+5f, -5f), false),
                0x15 => (new WDir(-5f, +5f), false),
                0x16 => (new WDir(+5f, +5f), false),
                0x17 => (new WDir(-15f, -15f), false),
                0x18 => (new WDir(+15f, -15f), false),
                0x19 => (new WDir(-15f, +15f), false),
                0x1A => (new WDir(+15f, +15f), false),
                0x1B => (new WDir(-5f, -5f), true),
                0x1C => (new WDir(+5f, -5f), true),
                0x1D => (new WDir(-5f, +5f), true),
                0x1E => (new WDir(+5f, +5f), true),
                0x1F => (new WDir(-15f, -15f), true),
                0x20 => (new WDir(+15f, -15f), true),
                0x21 => (new WDir(-15f, +15f), true),
                0x22 => (new WDir(+15f, +15f), true),
                _ => (new WDir(), false)
            };
            if (offset != default)
            {
                AddTower(Arena.Center + offset, blue);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var blueSlot = status.ID switch
        {
            (uint)SID.OdderPrayer => 0,
            (uint)SID.OdderIncarnation1 => 1,
            (uint)SID.OdderIncarnation2 => 2,
            (uint)SID.OdderIncarnation3 => 3,
            _ => -1
        };
        if (blueSlot >= 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _playerBlue[slot, blueSlot] = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.SquirrellyPrayer or (uint)SID.OdderPrayer)
        {
            _baitsDone = true;
            EnableNextTowers();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NBurstOrange or (uint)AID.NBurstBlue or (uint)AID.SBurstOrange or (uint)AID.SBurstBlue)
        {
            ++NumCasts;
            var index = Towers.FindIndex(t => t.Position.AlmostEqual(caster.Position, 1f));
            if (index >= 0)
                Towers.RemoveAt(index);
            else
                ReportError($"Failed to find at {caster.Position}");
            if ((NumCasts & 3) == 0)
                EnableNextTowers();
        }
    }

    private void AddTower(WPos position, bool blue)
    {
        if (blue)
            _blueTowers.Set(Towers.Count);
        Towers.Add(new(position, TowerRadius, 0, 0, new(0xF)));
    }

    private void EnableNextTowers()
    {
        var blueSlot = NumCasts / 4 + 1;
        BitMask forbiddenOrange = default;
        foreach (var (slot, _) in Raid.WithSlot(true, true, true))
            if (_playerBlue[slot, blueSlot])
                forbiddenOrange.Set(slot);
        var forbiddenBlue = forbiddenOrange ^ new BitMask(0xF);
        for (int i = 0, limit = Math.Min(4, Towers.Count); i < limit; ++i)
        {
            Towers.AsSpan()[i].ForbiddenSoakers = _blueTowers[i + NumCasts] ? forbiddenBlue : forbiddenOrange;
        }
    }
}

abstract class FlickeringFlame(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(46f, 2.5f), 8);
sealed class NFlickeringFlame(BossModule module) : FlickeringFlame(module, (uint)AID.NFireSpreadCross);
sealed class SFlickeringFlame(BossModule module) : FlickeringFlame(module, (uint)AID.SFireSpreadCross);
