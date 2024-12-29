namespace ZoneRV.Extensions;

public static class VanInfoExtensions
{
    public static VanModel? GetModel(this string vanName)
    {
        Utils.TryGetVanName(vanName, out var vanModel, out _);
        
        return vanModel;
    }
    
    public static ProductionLine GetProductionLine(this VanModel model)
    {
        if (model is VanModel.Zpp or VanModel.Zsp or VanModel.Zspf or VanModel.Zssf or VanModel.Zss)
            return ProductionLine.Gen2;

        if (model is VanModel.Exp)
            return ProductionLine.Expo;

        throw new NotImplementedException($"{Enum.GetName(model)} has not been assigned a production line.");
    }
}