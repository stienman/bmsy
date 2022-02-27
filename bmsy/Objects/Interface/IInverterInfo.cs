public interface IInverterInfo
{
    double BackToBattery { get; }
    double BatteryCutOffVoltage { get; }
    double BatteryKwhDischargedLast24Hr { get; }
    double BatteryLoadInWatts { get; }
    double BatteryLowBackToGrid { get; }
    double BatteryVoltage { get; }
    double BulkVoltage { get; }
    double ChargingCurrentInAmps { get; }
    ChargingSourceSelection ChargingSource { get; }
    double FloatVoltage { get; }
    double GridChargeInKwhToBatteryLast24Hr { get; }
    double GridKwhDischargedLast24Hr { get; }
    double GridLoadInWatts { get; }
    string InverterName { get; }
    double LoadInWatts { get; }
    int LoadPercentage { get; }
    OutputSourceSelection OutputSource { get; }
    double SolarGenerationLast24hrInkWh { get; }
    SystemStatus Status { get; }
    double Temperature { get; }
    double BatteryACChargeInWatts { get; }
    double PVPowerInWatts { get; }
    double PVVoltage { get; }
}
