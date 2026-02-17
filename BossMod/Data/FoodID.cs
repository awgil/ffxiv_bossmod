using Lumina.Excel;

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

public class FoodID
{
    public readonly PotionType[] PotionTypesByFoodId;

    public FoodID(ExcelSheet<Lumina.Excel.Sheets.ItemFood> itemFoodSheet)
    {
        var foodCount = itemFoodSheet.Count;

        PotionTypesByFoodId = new PotionType[foodCount];
        for (var i = 0; i < foodCount; i++)
        {
            var primaryBaseParam = itemFoodSheet[(uint)i].Params[0].BaseParam.RowId;
            if (primaryBaseParam is 1 or 2 or 4 or 5)
                PotionTypesByFoodId[i] = (PotionType)primaryBaseParam;
        }
    }

    public PotionType GetPotionType(ushort statusParam)
    {
        if (statusParam >= 10000) // hq
            statusParam -= 10000;

        return PotionTypesByFoodId.BoundSafeAt(statusParam);
    }
}
