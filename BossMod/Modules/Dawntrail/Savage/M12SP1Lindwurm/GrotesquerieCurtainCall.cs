namespace BossMod.Dawntrail.Savage.M12SLindwurm;

// TODO: show goal zone for RottingFlesh players
sealed class GrotesquerieCurtainCall(BossModule module) : Components.UniformStackSpread(module, default, 6f)
{
    private const float ChainBreakDistance = 20f;
    private const float ChainGroupUpWindow = 4f;

    private readonly bool[] _hasBurstingGrotesquerie = new bool[8];
    private readonly bool[] _hasRottingFlesh = new bool[8];
    private readonly bool[] _hasBondsAlpha = new bool[8];
    private readonly bool[] _hasUnbreakableAlpha = new bool[8];
    private readonly DateTime[] _rottingFleshExpiry = new DateTime[8];
    private DateTime _chainActivationTime;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0 || slot >= 8)
            return;

        if (status.ID == (uint)SID.BurstingGrotesquerie)
        {
            _hasBurstingGrotesquerie[slot] = true;
            AddSpread(actor, status.ExpireAt);
        }
        else if (status.ID == (uint)SID.RottingFlesh)
        {
            _hasRottingFlesh[slot] = true;
            _rottingFleshExpiry[slot] = status.ExpireAt;
        }
        else if (status.ID == (uint)SID.BondsOfFleshAlpha)
        {
            _hasBondsAlpha[slot] = true;
            // Set chain activation time when first bond is detected
            if (_chainActivationTime == default)
                _chainActivationTime = status.ExpireAt;
        }
        else if (status.ID == (uint)SID.UnbreakableFlesh0)
        {
            _hasUnbreakableAlpha[slot] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0 || slot >= 8)
            return;

        if (status.ID == (uint)SID.BurstingGrotesquerie)
        {
            _hasBurstingGrotesquerie[slot] = false;
            Spreads.RemoveAll(s => s.Target == actor);
        }
        else if (status.ID == (uint)SID.RottingFlesh)
        {
            _hasRottingFlesh[slot] = false;
            _rottingFleshExpiry[slot] = default;
        }
        else if (status.ID == (uint)SID.BondsOfFleshAlpha)
        {
            _hasBondsAlpha[slot] = false;
            // Reset chain activation time when all bonds are gone
            var anyBondsRemaining = false;
            for (var i = 0; i < 8; ++i)
            {
                if (i != slot && _hasBondsAlpha[i])
                {
                    anyBondsRemaining = true;
                    break;
                }
            }
            if (!anyBondsRemaining)
                _chainActivationTime = default;
        }
        else if (status.ID == (uint)SID.UnbreakableFlesh0)
        {
            _hasUnbreakableAlpha[slot] = false;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        // Draw center grouping indicator before chains activate
        if (_chainActivationTime != default && !_hasUnbreakableAlpha[pcSlot])
        {
            var timeUntilChains = (_chainActivationTime - WorldState.CurrentTime).TotalSeconds;
            if (timeUntilChains > 0 && timeUntilChains <= ChainGroupUpWindow)
            {
                Arena.ZoneCircleOutline(Module.Center, 2f, Colors.Safe);
            }
        }

        if (_hasUnbreakableAlpha[pcSlot])
        {
            DrawChainBreakIndicator(pcSlot, pc);
        }
    }

    private void DrawChainBreakIndicator(int pcSlot, Actor pc)
    {
        var isSupport = pc.Class.IsSupport();

        for (var i = 0; i < 8; ++i)
        {
            if (i == pcSlot || !_hasUnbreakableAlpha[i])
                continue;

            var partner = Raid[i];
            if (partner == null)
                continue;

            var partnerIsSupport = partner.Class.IsSupport();
            if (isSupport == partnerIsSupport)
                continue;

            Arena.AddLine(pc.Position, partner.Position, Colors.Danger);
            var dirToPartner = (partner.Position - pc.Position).Normalized();
            var breakPos = pc.Position - dirToPartner * ChainBreakDistance;
            breakPos = Arena.ClampToBounds(breakPos);

            var isInPosition = pc.Position.InCircle(breakPos, 2f);
            Arena.ZoneCircleOutline(breakPos, 1f, isInPosition ? Colors.Danger : Colors.Safe);
            break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_hasBurstingGrotesquerie[slot])
            hints.Add("Spread and avoid cone!", false);
        else if (_hasRottingFlesh[slot])
            hints.Add("Spread and get hit by cone!", false);

        if (_hasUnbreakableAlpha[slot])
            hints.Add("Break chains!", false);
    }
}
