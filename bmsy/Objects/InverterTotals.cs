public class InverterTotals : IInverterInfo
{
    public double BackToBattery {get;set; }

    public double BatteryCutOffVoltage {get;set; }

    public double BatteryKwhDischargedLast24Hr {get;set; }

    public double BatteryLoadInWatts {get;set; }

    public double BatteryLowBackToGrid {get;set; }

    public double BatteryVoltage {get;set; }

    public double BulkVoltage {get;set; }

    public double ChargingCurrentInAmps {get;set; }

    public ChargingSourceSelection ChargingSource {get;set; }

    public double FloatVoltage {get;set; }

    public double GridChargeInKwhToBatteryLast24Hr {get;set; }

    public double GridKwhDischargedLast24Hr {get;set; }

    public double GridLoadInWatts {get;set; }

    public string InverterName {get;set; }

    public double LoadInWatts {get;set; }

    public int LoadPercentage {get;set; }

    public OutputSourceSelection OutputSource {get;set; }

    public double SolarGenerationLast24hrInkWh {get;set; }

    public SystemStatus Status {get;set; }

    public double Temperature {get;set; }

    public double BatteryACChargeInWatts {get;set; }

    public double PVPowerInWatts {get;set; }

    public double PVVoltage {get;set; }
}