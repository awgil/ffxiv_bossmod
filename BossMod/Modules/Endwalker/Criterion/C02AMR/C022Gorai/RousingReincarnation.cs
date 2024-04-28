namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class RousingReincarnation(BossModule module, AID aid) : Components.CastCounter(module, ActionID.MakeSpell(aid));
class NRousingReincarnation(BossModule module) : RousingReincarnation(module, AID.NRousingReincarnationAOE);
class SRousingReincarnation(BossModule module) : RousingReincarnation(module, AID.SRousingReincarnationAOE);

// note on towers: indices are 0-7 CW from N, even (cardinal) are blue, odd (intercardinal) are orange
class MalformedPrayer1(BossModule module) : Components.GenericTowers(module)
{
    public int[] OrangeSoakOrder = [-1, -1, -1, -1]; // blue is inferred as (x+2)%4
    private readonly List<int> _towerOrder = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var order = (SID)status.ID switch
        {
            SID.RodentialRebirth1 => 0,
            SID.RodentialRebirth2 => 1,
            SID.RodentialRebirth3 => 2,
            SID.RodentialRebirth4 => 3,
            _ => -1,
        };
        if (order >= 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            OrangeSoakOrder[slot] = order;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBurstOrange or AID.NBurstBlue or AID.SBurstOrange or AID.SBurstBlue)
        {
            ++NumCasts;
            if ((NumCasts & 1) == 0)
                UpdateTowers();
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            // 03-0A are anims, 0B-12 are tower circles
            // anims states are 00020001 (appear) and 00080004 (fall start)
            // circles states are 00020001 (appear), 00200010 (enter), 00400001 (exit), 00080004 (disappear)
            // ==> towers 0B = 180, 0C = -90, 0D = +90, 0E = 0, 0F = -135, 10 = 135, 11 = -45, 12 = 45
            // ==> icons 03 = -135, 04 = 135, 05 = -45, 06 = 45, 07 = 180, 08 = -90, 09 = 90, 0A = 0
            // blue are always card, yellow intercard
            var towerIndex = index switch
            {
                3 => 7,
                4 => 1,
                5 => 5,
                6 => 3,
                7 => 0,
                8 => 6,
                9 => 2,
                10 => 4,
                _ => -1
            };
            if (towerIndex >= 0)
            {
                _towerOrder.Add(towerIndex);
                if (_towerOrder.Count == 2)
                    UpdateTowers();
            }
        }
    }

    private void UpdateTowers()
    {
        Towers.Clear();
        int towerOrder = NumCasts / 2;
        var orangeSoaker = Array.IndexOf(OrangeSoakOrder, towerOrder);
        var blueSoaker = Array.IndexOf(OrangeSoakOrder, (towerOrder + 2) & 3);
        foreach (var index in _towerOrder.Skip(NumCasts).Take(2))
        {
            BitMask forbidden = new(0xf);
            var soakerSlot = (index & 1) != 0 ? orangeSoaker : blueSoaker;
            forbidden.Clear(soakerSlot);
            Towers.Add(new(Module.Center + 11 * (180.Degrees() - index * 45.Degrees()).ToDirection(), 4, forbiddenSoakers: forbidden));
        }
    }
}

class PointedPurgation : Components.BaitAwayTethers
{
    private BitMask _oddSoakers; // players with 1/3 debuff

    public PointedPurgation(BossModule module) : base(module, new AOEShapeCone(60, 22.5f.Degrees()), (uint)TetherID.PointedPurgation)
    {
        var malformedPlayer = module.FindComponent<MalformedPrayer1>();
        foreach (var (index, _) in Raid.WithSlot(true))
        {
            var soakOrder = malformedPlayer?.OrangeSoakOrder[index] ?? -1;
            if (soakOrder is 0 or 2)
                _oddSoakers.Set(index);
        }
        ForbiddenPlayers = _oddSoakers;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _oddSoakers[playerSlot] != _oddSoakers[pcSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NPointedPurgationAOE or AID.SPointedPurgationAOE)
        {
            ++NumCasts;
            ForbiddenPlayers = (NumCasts & 2) != 0 ? ~_oddSoakers : _oddSoakers;
        }
    }
}
