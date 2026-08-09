namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class AuraBurstHoly(BossModule module) : BossComponent(module)
{
    private bool? isHoly;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.HolyVisual or (uint)AID.AuraBurstVisual)
        {
            isHoly = id == (uint)AID.HolyVisual;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (isHoly is bool holy)
        {
            Arena.Actors(Module.Enemies(holy ? (uint)OID.LanceEmpowermentConduit : (uint)OID.AxeEmpowermentConduit), Colors.Object);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (isHoly is bool holy)
        {
            hints.PrioritizeTargetsByOID(holy ? (uint)OID.LanceEmpowermentConduit : (uint)OID.AxeEmpowermentConduit, 1);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (isHoly is bool holy)
        {
            var conduits = Module.Enemies(holy ? (uint)OID.LanceEmpowermentConduit : (uint)OID.AxeEmpowermentConduit);
            var count = conduits.Count;
            for (var i = 0; i < count; ++i)
            {
                var c = conduits[i];
                if (c.HPMP.CurHP > 1u)
                {
                    hints.Add($"Destroy the {c.Name}s, tanks infront!");
                    return;
                }
            }
        }
    }
}

sealed class ArcaneReaction(BossModule module) : Components.GenericBaitAway(module)
{
    private Actor? conduit;
    private readonly AOEShapeRect rect = new(55f, 3f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.UniversalEmpowermentConduit)
        {
            conduit = actor;
        }
    }

    public override void Update()
    {
        if (conduit != null)
        {
            CurrentBaits.Clear();

            var players = new List<Actor>(48);
            // note: this is problematic because player culling messes this up and leads to incorrect results (eg not finding the actual bait target)
            // in any frame players can be removed or added to the object table and will likely never contain all 48 players at the same time
            // caching removed players is also pointless since the player position no longer gets updated when this happens
            foreach (var a in Module.WorldState.Actors.Actors.Values)
            {
                if (a.OID == default && !a.IsDead)
                {
                    players.Add(a);
                }
            }
            var count = players.Count;
            Actor? closest = null;
            var minDistSq = float.MaxValue;
            var pos = conduit.Position;

            for (var i = 0; i < count; ++i)
            {
                var actor = players[i];
                var distSq = (actor.Position - pos).LengthSq();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = actor;
                }
            }
            if (closest != null)
            {
                CurrentBaits.Add(new(conduit, closest, rect));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count != 0)
        {
            base.AddHints(slot, actor, hints);
            var bait = CurrentBaits[0];
            var isPhantomBerserker = actor.FindStatus((uint)Data.PhantomSID.PhantomBerserker) != null;
            if (!isPhantomBerserker && bait.Target == actor)
            {
                hints.Add($"GTFO from {conduit!.Name}!");
            }
            else if (isPhantomBerserker && bait.Target != actor && bait.Target.FindStatus((uint)Data.PhantomSID.PhantomBerserker) == null)
            {
                hints.Add($"Get closer to {conduit!.Name}!!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (conduit != null && pc.FindStatus((uint)Data.PhantomSID.PhantomBerserker) != null)
        {
            Arena.Actor(conduit, Colors.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArcaneReaction)
        {
            conduit = null;
            CurrentBaits.Clear();
        }
    }
}

sealed class ArcaneRecoil(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> targets = [with(6)];
    private List<Actor> conduits = [with(3)];
    private bool? isHoly;
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id is (uint)AID.HolyVisual or (uint)AID.AuraBurstVisual)
        {
            isHoly = id == (uint)AID.HolyVisual;
            conduits = Module.Enemies(isHoly == true ? (uint)OID.LanceEmpowermentConduit : (uint)OID.AxeEmpowermentConduit);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (isHoly is bool holy && actor.OID is var id) // double checking actors since cast start and actor creation can happen in arbitrary order in the network packages
        {
            if (holy && id != (uint)OID.LanceEmpowermentConduit || !holy && id != (uint)OID.AxeEmpowermentConduit)
            {
                return;
            }
            var count = conduits.Count;
            for (var i = 0; i < count; ++i)
            {
                if (conduits[i] == actor)
                {
                    return;
                }
            }
            conduits.Add(actor);
        }
    }

    public override void Update()
    {
        if (isHoly == null)
        {
            return;
        }
        targets.Clear();

        var countConduits = conduits.Count;

        for (var i = 0; i < countConduits; ++i)
        {
            var c = conduits[i];
            var pos = c.Position;
            List<(Actor actor, float distSq)> distances = [with(48)];
            // note: this is problematic because player culling messes this up and leads to incorrect results (eg not finding the actual bait target)
            // in any frame players can be removed or added to the object table and will likely never contain all 48 players at the same time
            // caching removed players is also pointless since the player position no longer gets updated when this happens
            foreach (var a in Module.WorldState.Actors.Actors.Values)
            {
                if (a.OID == default && !a.IsDead)
                {
                    distances.Add((a, (a.Position - pos).LengthSq()));
                }
            }
            var countDistances = distances.Count;
            var counttargets = Math.Min(2, countDistances);
            for (var j = 0; j < counttargets; ++j)
            {
                var selIdx = j;
                for (var k = j + 1; k < countDistances; ++k)
                {
                    if (distances[k].distSq < distances[selIdx].distSq)
                        selIdx = k;
                }

                if (selIdx != j)
                {
                    (distances[selIdx], distances[j]) = (distances[j], distances[selIdx]);
                }
                targets.Add(distances[j].actor);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ArcaneRecoil)
        {
            ++NumCasts;

            var count = conduits.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                if (conduits[i].Position.AlmostEqual(pos, 1f))
                {
                    conduits.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(targets, Colors.Vulnerable);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = targets.Count;
        if (count == 0 || actor.IsDead)
            return;
        if (actor.Role == Role.Tank)
        {
            var isTarget = false;
            var anyNonTank = false;
            for (var i = 0; i < count; ++i)
            {
                var t = targets[i];
                if (t == actor)
                {
                    isTarget = true;
                }
                else if (t.Role != Role.Tank)
                {
                    anyNonTank = true;
                }
            }

            hints.Add($"Bait a {conduits[0].Name}", !isTarget && anyNonTank);
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                if (targets[i] == actor)
                {
                    hints.Add("GTFO from conduit!");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = targets.Count;
        if (count != 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var mask = new BitMask();
            for (var i = 0; i < count; ++i)
            {
                if (Raid.FindSlot(targets[i].InstanceID) is var slot2 && slot2 >= 0)
                {
                    mask[slot2] = true;
                }
            }
            hints.AddPredictedDamage(mask, default, AIHints.PredictedDamageType.Tankbuster);
        }
    }
}

sealed class AuraBurstHolyRaidwide(BossModule module) : Components.RaidwideCastsDelay(module, [(uint)AID.AuraBurstVisual, (uint)AID.HolyVisual], [(uint)AID.AuraBurst, (uint)AID.Holy], 1d)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Activation == default)
        {
            return;
        }

        if ((Activation - WorldState.CurrentTime).TotalSeconds < 6d)
        {
            hints.Add(Hint);
        }
    }
}
