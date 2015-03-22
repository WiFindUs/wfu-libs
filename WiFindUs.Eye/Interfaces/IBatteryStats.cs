
namespace WiFindUs.Eye
{
    public interface IBatteryStats
    {
        bool? Charging { get; }
        double? BatteryLevel { get; }
        bool EmptyBatteryStats { get; }
    }
}
