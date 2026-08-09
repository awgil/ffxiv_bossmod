namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class TyrannysGraspAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TyrannysGraspAOE, new AOEShapeRect(20f, 20f));

sealed class TyrannysGraspTowers(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TyrannysGraspTower1 or (uint)AID.TyrannysGraspTower2)
        {
            var party = Module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            BitMask nontanks = default;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                if (p.Item2.Role != Role.Tank)
                {
                    nontanks[p.Item1] = true;
                }
            }
            Towers.Add(new(spell.LocXZ, 4f, 1, 1, nontanks, Module.CastFinishAt(spell), caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TyrannysGraspTower1 or (uint)AID.TyrannysGraspTower2)
        {
            ++NumCasts;

            var lenT = Towers.Count;
            var id = caster.InstanceID;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < lenT; ++i)
            {
                if (towers[i].ActorID == id)
                {
                    Towers.RemoveAt(i);
                    break;
                }
            }

            var party = Raid.WithSlot(false, false, true);
            var lenP = party.Length;
            BitMask forbidden = default;
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var targ = ref targets[i];
                for (var j = 0; j < lenP; ++j)
                {
                    ref readonly var p = ref party[j];
                    if (targ.ID == p.Item2.InstanceID)
                    {
                        forbidden[p.Item1] = true;
                        break;
                    }
                }
            }

            lenT = towers.Length;
            for (var i = 0; i < lenT; ++i)
            {
                ref var t = ref towers[i];
                t.ForbiddenSoakers |= forbidden;
            }
        }
    }
}
