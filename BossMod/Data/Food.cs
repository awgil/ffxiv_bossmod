namespace BossMod;

public enum PotionType : uint
{
    None,
    Strength = 1,
    Dexterity = 2,
    // vitality = 3, useless
    Intelligence = 4,
    Mind = 5,
    // piety = 6, useless
    // etc...
}

public static class Food
{
    public static readonly PotionType[] PotionTypesByFoodId = CollectPotionTypes();

    public static PotionType GetPotionType(ushort statusParam)
    {
        if (statusParam >= 10000) // hq
            statusParam -= 10000;

        return PotionTypesByFoodId.BoundSafeAt(statusParam);
    }

    private static PotionType[] CollectPotionTypes()
    {
        var itemFoodSheet = Service.LuminaSheet<Lumina.Excel.Sheets.ItemFood>();
        if (itemFoodSheet == null)
        {
            Service.Log("[AD] Unable to load food sheet, potions will not behave correctly!");
            return [];
        }

        var foodCount = itemFoodSheet.Count;

        var types = new PotionType[foodCount];
        for (var i = 0; i < foodCount; i++)
        {
            var primaryBaseParam = itemFoodSheet[(uint)i].Params[0].BaseParam.RowId;
            if (primaryBaseParam is 1 or 2 or 4 or 5)
                types[i] = (PotionType)primaryBaseParam;
        }

        return types;
    }
}
