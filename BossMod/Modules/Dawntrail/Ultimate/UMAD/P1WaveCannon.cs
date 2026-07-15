namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1PulseWave(BossModule module) : Components.Knockback(module, AID.PulseWave, true)
{
    public static readonly WPos Origin = new(100, 65);
    public const float Distance = 13f;

    public DateTime Activation { get; private set; }
    public BitMask Targets;

    bool _blizzardStarted;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GravenImage)
        {
            Targets.Set(Raid.FindSlot(tether.Target));
            Activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Targets.Clear(Raid.FindSlot(spell.MainTargetID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _blizzardStarted |= (AID)spell.Action.ID is AID.BlizzardIIIBlowout1 or AID.BlizzardIIIBlowout2;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Targets[slot])
            yield return new(Origin, Distance, Activation);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Targets[slot] || _blizzardStarted)
            return;

        // TODO tweak zone size. we want the player to move north if they get tethered, since blizzard zones are only visible for ~2.5s before kb activates
        // max melee is 9.5y circle around arena center
        hints.AddForbiddenZone(ShapeContains.InvertedRect(new(100, 80), new(100, 93), 40), Activation);
    }
}

class P1BlizzardIIIBlowout(BossModule module) : Components.GroupedAOEs(module, [AID.BlizzardIIIBlowout1, AID.BlizzardIIIBlowout2], new AOEShapeCone(40, 45.Degrees()))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Module.FindComponent<P1PulseWave>() is { } pw)
            return base.ActiveAOEs(slot, actor).Select(a => a with { Risky = !pw.Targets[slot] });

        return base.ActiveAOEs(slot, actor);
    }
}
class P1ThrummingThunderIII(BossModule module) : Components.GroupedAOEs(module, [AID.ThrummingThunderIII1, AID.ThrummingThunderIII2], new AOEShapeRect(40, 5));


class P1WaveCannon : Components.UntelegraphedBait
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    public P1WaveCannon(BossModule module) : base(module, AID.WaveCannon)
    {
        CurrentBaits.Add(new(P1PulseWave.Origin, Raid.WithSlot().Mask(), new AOEShapeRect(100, 3), WorldState.FutureTime(4.3f), 8));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;

        var activation = CurrentBaits[0].Activation;
        var myOrder = _config.P1WaveCannonConga[assignment];

        if (activation < WorldState.FutureTime(1) || myOrder < 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            return;
        }

        var dest = P1PulseWave.Origin + new WDir(0, 38).Rotate(((myOrder - 3.5f) * 8).Degrees());
        hints.AddForbiddenZone(ShapeContains.InvertedCircle(dest, 1), activation);
    }
}

class P1Explosion(BossModule module) : Components.CastTowers(module, AID.ExplosionP1, 4)
{
    readonly DateTime[] _vuln = new DateTime[8];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MagicVulnerabilityUp && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _vuln[slot] = status.ExpireAt;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && Towers.Count == 4)
        {
            Towers.SortBy(t => t.Position.X);

            var itower = 0;
            foreach (var (slot, group) in Service.Config.Get<UMADConfig>().P1WaveCannonConga.Resolve(Raid).OrderBy(s => s.group))
            {
                if (_vuln[slot] < Towers[itower].Activation)
                {
                    Towers.Ref(itower++).ForbiddenSoakers = ~BitMask.Build(slot);
                    if (itower >= 4)
                        break;
                }
            }

            if (itower == 0)
            {
                var vulnAll = BitMask.Build([.. Enumerable.Range(0, _vuln.Length).Where(i => _vuln[i] != default)]);
                for (var i = 0; i < Towers.Count; i++)
                    Towers.Ref(i).ForbiddenSoakers = vulnAll;
            }
        }
    }
}
