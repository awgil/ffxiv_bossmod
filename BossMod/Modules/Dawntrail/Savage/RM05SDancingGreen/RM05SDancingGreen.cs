namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

public enum OID : uint
{
    Boss = 0x47B9,
    Helper = 0x233C,
    Frogtourage = 0x47BA, // R3.142, x8
    Spotlight = 0x47BB, // R1.000, x8
}

public enum SID : uint
{
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_Bleeding = 2088, // Helper->player, extra=0x0
    _Gen_NeedlePoised = 4459, // Boss->Boss, extra=0x0
    _Gen_BurnBabyBurn = 4461, // none->player, extra=0x0
    _Gen_ = 2056, // Frogtourage->Spotlight/Frogtourage, extra=0x37D/0x386/0xE1
    _Gen_1 = 4515, // none->player, extra=0x0
    _Gen_Spotlightless = 4472, // none->player, extra=0x0
    _Gen_InTheSpotlight = 4471, // none->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_DirectionalDisregard = 3808, // none->Boss, extra=0x0
    _Gen_WavelengthA = 4462, // none->player, extra=0x0
    _Gen_WavelengthB = 4463, // none->player, extra=0x0
    _Gen_DamageDown = 2911, // Helper/Boss/Frogtourage->player, extra=0x0
    _Gen_PerfectGroove = 4464, // none->player, extra=0x0
    _Gen_SustainedDamage = 2935, // Helper->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_FrogtourageFan = 3998, // Helper->player, extra=0x0
}

class Wavelength(BossModule module) : BossComponent(module)
{
    enum Letter { None, A, B }
    struct PlayerState(Letter assignment, DateTime activation, int slot, BitMask players = default, int order = 0)
    {
        public Letter Assignment = assignment;
        public DateTime Activation = activation;
        public int Slot = slot;
        public BitMask Players = players;
        public int Order = order;
    }

    private PlayerState[] PlayerStates = Utils.MakeArray(8, new PlayerState());

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        if ((SID)status.ID == SID._Gen_WavelengthA)
            PlayerStates[slot] = new(Letter.A, status.ExpireAt, slot);
        if ((SID)status.ID == SID._Gen_WavelengthB)
            PlayerStates[slot] = new(Letter.B, status.ExpireAt, slot);

        if (PlayerStates.All(p => p.Assignment != Letter.None))
        {
            var ordered = PlayerStates.Select(st => new PlayerState(st.Assignment, st.Activation, st.Slot, st.Players, PlayerStates.Count(p1 => p1.Assignment == st.Assignment && p1.Activation < st.Activation) + 1));
            var masked = ordered.Select(st =>
            {
                var mask = new BitMask();
                foreach (var o in ordered.Where(o => o.Order == st.Order))
                    mask.Set(o.Slot);
                return st with { Players = mask };
            });
            PlayerStates = [.. masked];
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID._Gen_WavelengthA or SID._Gen_WavelengthB)
            PlayerStates[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (NextOrder == 0)
            return PlayerPriority.Normal;

        var ps = PlayerStates[playerSlot];
        if (ps.Order == NextOrder)
            return ps.Players[pcSlot] ? PlayerPriority.Interesting : PlayerPriority.Danger;

        return PlayerPriority.Normal;
    }

    private int NextOrder => PlayerStates.Select(p => p.Order).Where(x => x > 0).DefaultIfEmpty(0).Min();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var ps = PlayerStates[slot];
        if (ps.Order > 0)
            hints.Add($"Order: {ps.Order}", false);
    }
}

class DancingGreenStates : StateMachineBuilder
{
    public DancingGreenStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<Wavelength>();
    }

    //private void XXX(uint id, float delay)
}

#if DEBUG
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1020, NameID = 13778)]
public class DancingGreen(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
#endif
