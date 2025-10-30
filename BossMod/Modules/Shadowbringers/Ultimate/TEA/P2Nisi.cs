namespace BossMod.Shadowbringers.Ultimate.TEA;

class P2Nisi : BossComponent
{
    public enum Nisi { None, Alpha, Beta, Gamma, Delta }

    public int ShowPassHint; // show hints for Nth pass
    public int NumActiveNisi { get; private set; }
    private int _numNisiApplications;
    private readonly int[] _partners = Utils.MakeArray(PartyState.MaxPartySize, -1);
    private readonly Nisi[] _current = new Nisi[PartyState.MaxPartySize];
    private readonly Nisi[] _judgments = new Nisi[PartyState.MaxPartySize];

    public P2Nisi(BossModule module) : base(module)
    {
        int[] firstMembersOfGroup = [-1, -1, -1, -1];
        foreach (var p in Service.Config.Get<TEAConfig>().P2NisiPairs.Resolve(Raid))
        {
            ref var partner = ref firstMembersOfGroup[p.group];
            if (partner < 0)
            {
                partner = p.slot;
            }
            else
            {
                _partners[p.slot] = partner;
                _partners[partner] = p.slot;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (PassPartnerSlot(slot) >= 0)
        {
            hints.Add("Pass nisi!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = Raid[PassPartnerSlot(pcSlot)];
        if (partner != null)
        {
            Arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var nisi = NisiForSID((SID)status.ID);
        if (nisi != Nisi.None && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            if (_current[slot] != nisi) // sometimes same nisi is reapplied, which is weird...
                ++_numNisiApplications;
            if (_current[slot] == Nisi.None) // sometimes same nisi is reapplied, which is weird... - also i guess nisi could change in a single frame...
                ++NumActiveNisi;
            _current[slot] = nisi;
        }

        var judgment = (SID)status.ID switch
        {
            SID.FinalJudgmentNisiAlpha => Nisi.Alpha,
            SID.FinalJudgmentNisiBeta => Nisi.Beta,
            SID.FinalJudgmentNisiGamma => Nisi.Gamma,
            SID.FinalJudgmentNisiDelta => Nisi.Delta,
            _ => Nisi.None
        };
        if (judgment != Nisi.None && Raid.TryFindSlot(actor.InstanceID, out var judgmentSlot))
        {
            _judgments[judgmentSlot] = judgment;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var nisi = NisiForSID((SID)status.ID);
        if (nisi != Nisi.None && Raid.TryFindSlot(actor.InstanceID, out var slot) && nisi == _current[slot])
        {
            _current[slot] = Nisi.None;
            --NumActiveNisi;
        }
    }

    private Nisi NisiForSID(SID sid) => sid switch
    {
        SID.FinalDecreeNisiAlpha => Nisi.Alpha,
        SID.FinalDecreeNisiBeta => Nisi.Beta,
        SID.FinalDecreeNisiGamma => Nisi.Gamma,
        SID.FinalDecreeNisiDelta => Nisi.Delta,
        _ => Nisi.None
    };

    private int PassPartnerSlot(int slot)
    {
        if (_numNisiApplications < 4)
            return -1; // initial nisi not applied yet
        if (_numNisiApplications >= 4 + 4 * ShowPassHint)
            return -1; // expected nisi passes are done

        var partner = _partners[slot]; // by default use assigned partner (first two passes before judgments)
        if (_judgments[slot] != Nisi.None)
        {
            if (_current[slot] == Nisi.None)
            {
                // we need to grab correct nisi to match our judgment
                partner = Array.IndexOf(_current, _judgments[slot]);
            }
            else
            {
                // we need to pass nisi to whoever has correct judgment and no nisi
                partner = Array.IndexOf(_judgments, _current[slot]);
                if (partner >= 0 && _current[partner] != Nisi.None && partner < _judgments.Length - 1)
                    partner = Array.IndexOf(_judgments, _current[slot], partner + 1);
            }
        }

        if (partner < 0)
            return -1; // partner not assigned correctly
        if (_current[slot] != Nisi.None && _current[partner] != Nisi.None)
            return -1; // both partners have nisi already
        return partner;
    }
}
