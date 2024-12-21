namespace ZoneRV.Extensions;

public static class VanInfoExtensions
{
    public static VanModel? GetModel(this string vanName)
    {
        Utils.TryGetVanName(vanName, out var vanModel, out _);
        
        return vanModel;
    }
}