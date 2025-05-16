namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class MalformedReincarnation(BossModule module, AID aid) : Components.CastCounter(module, aid);
class NMalformedReincarnation(BossModule module) : MalformedReincarnation(module, AID.NMalformedReincarnationAOE);
class SMalformedReincarnation(BossModule module) : MalformedReincarnation(module, AID.SMalformedReincarnationAOE);

// TODO: initial hints (depending on strat?) + specific towers
class MalformedPrayer2(BossModule module) : Components.GenericTowers(module)
{
    private BitMask _blueTowers;
    private BitMatrix _playerBlue; // [i] = blue debuffs for slot i; 0 = bait, 1/2/3 = soaks
    private bool _baitsDone;

    private const float TowerRadius = 4;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_baitsDone && (Towers.Any(t => t.Position.InCircle(actor.Position, TowerRadius * 2)) || Raid.WithoutSlot().InRadiusExcluding(actor, TowerRadius * 2).Any()))
            hints.Add("Bait away from other towers!");
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!_baitsDone)
            foreach (var p in Raid.WithoutSlot())
                Arena.AddCircle(p.Position, TowerRadius, ArenaColor.Danger);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.OrangeTower1 or OID.BlueTower1)
        {
            AddTower(actor.Position, (OID)actor.OID == OID.BlueTower1);
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            // orange (anim/circle, see RousingReincarnation) blue
            // 17/3A                   18/3B | 1F/27                   20/28
            //       13/36       14/37       |       1B/23       1C/24
            //             -----             |             -----
            //       15/38       16/39       |       1D/25       1E/26
            // 19/3C                   1A/3D | 21/29                   22/2A
            var (offset, blue) = index switch
            {
                19 => (new WDir(-5, -5), false),
                20 => (new WDir(+5, -5), false),
                21 => (new WDir(-5, +5), false),
                22 => (new WDir(+5, +5), false),
                23 => (new WDir(-15, -15), false),
                24 => (new WDir(+15, -15), false),
                25 => (new WDir(-15, +15), false),
                26 => (new WDir(+15, +15), false),
                27 => (new WDir(-5, -5), true),
                28 => (new WDir(+5, -5), true),
                29 => (new WDir(-5, +5), true),
                30 => (new WDir(+5, +5), true),
                31 => (new WDir(-15, -15), true),
                32 => (new WDir(+15, -15), true),
                33 => (new WDir(-15, +15), true),
                34 => (new WDir(+15, +15), true),
                _ => (new WDir(), false)
            };
            if (offset != default)
            {
                AddTower(Module.Center + offset, blue);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var blueSlot = (SID)status.ID switch
        {
            SID.OdderPrayer => 0,
            SID.OdderIncarnation1 => 1,
            SID.OdderIncarnation2 => 2,
            SID.OdderIncarnation3 => 3,
            _ => -1
        };
        if (blueSlot >= 0 && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _playerBlue[slot, blueSlot] = true;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.SquirrellyPrayer or SID.OdderPrayer)
        {
            _baitsDone = true;
            EnableNextTowers();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBurstOrange or AID.NBurstBlue or AID.SBurstOrange or AID.SBurstBlue)
        {
            ++NumCasts;
            var index = Towers.FindIndex(t => t.Position.AlmostEqual(caster.Position, 1));
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
        BitMask forbiddenOrange = new();
        foreach (var (slot, _) in Raid.WithSlot(true))
            if (_playerBlue[slot, blueSlot])
                forbiddenOrange.Set(slot);
        var forbiddenBlue = forbiddenOrange ^ new BitMask(0xF);
        for (int i = 0, limit = Math.Min(4, Towers.Count); i < limit; ++i)
        {
            Towers.AsSpan()[i].ForbiddenSoakers = _blueTowers[i + NumCasts] ? forbiddenBlue : forbiddenOrange;
        }
    }
}

class FlickeringFlame(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeRect(46, 2.5f), 8);
class NFlickeringFlame(BossModule module) : FlickeringFlame(module, AID.NFireSpreadCross);
class SFlickeringFlame(BossModule module) : FlickeringFlame(module, AID.SFireSpreadCross);
