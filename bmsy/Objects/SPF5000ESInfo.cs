[Serializable()]
public class SPF5000ESInfo : IInverterInfo
{
    /// <summary>
    /// Output active power (high)    0.1W
    /// </summary>
    public int REG_OP_Watt { get; set; }
    /// <summary>
    /// Output apparent power (high) 0.1VA  
    /// </summary>
    public int REG_OP_VA { get; set; }
    /// <summary>
    /// AC charge watt (high)
    /// </summary>
    public int REG_ACChr_Watt { get; set; }
    /// <summary>
    /// AC charge apparent power 0.1VA
    /// </summary>
    public int REG_ACChr_VA { get; set; }
    /// <summary>
    /// AC input watt (high)    0.1W
    /// </summary>
    public int REG_AC_InWatt { get; set; }
    /// <summary>
    /// AC input apparent power 0.1VA
    /// </summary>
    private int REG_AC_InVA { get; set; }
    /// <summary>
    /// PV Energy today 0.1kWh
    /// </summary>
    public int REG_Epv1_today { get; set; }
    /// <summary>
    /// PV Energy total    0.1kWh
    /// </summary>
    public int REG_Epv1_total { get; set; }
    /// <summary>
    /// AC charge Energy today    0.1kWh
    /// </summary>
    public int REG_Eac_chrToday { get; set; }
    /// <summary>
    /// AC charge Energy total    0.1kW
    /// </summary>
    public int REG_Eac_chrTotal { get; set; }
    /// <summary>
    /// Bat discharge Energy today    0.1kW
    /// </summary>
    public int REG_Ebat_dischrToday { get; set; }
    /// <summary>
    /// Bat discharge Energy total    0.1kW
    /// </summary>
    public int REG_Ebat_dischrTotal { get; set; }
    /// <summary>
    /// AC discharge Energy today    0.1kW
    /// </summary>
    public int REG_Eac_dischrToday { get; set; }
    /// <summary>
    /// AC discharge Energy total    0.1kW
    /// </summary>
    public int REG_Eac_dischrTotal { get; set; }
    /// <summary>
    /// AC discharge watt (high)    0.1W 
    /// </summary>
    public int REG_AC_DisChrWatt { get; set; }
    /// <summary>
    /// AC discharge apparent power  0.1VA
    /// </summary>
    public int REG_AC_DisChrVA { get; set; }
    /// <summary>
    /// Bat discharge watt (high)    0.1W
    /// </summary>
    public int REG_Bat_DisChrWatt { get; set; }
    /// <summary>
    /// Bat discharge apparent power 0.1VA
    /// </summary>
    public int REG_Bat_DisChrVA { get; set; }
    /// <summary>
    /// Bat watt (high) (signed int 32)
    ///Positive:Battery
    ///Discharge Power;
    ///Negative: Battery
    ///Charge Power;
    ///0.1W
    /// </summary>
    /// 
    public int REG_Bat_Watt { get; set; }
    public double REG_Vpv1 { get; set; }
    public int REG_FloatChargeCurr { get; set; }
    public double REG_Ppv1 { get; set; }


    // IInverter .. 
    public double Temperature { get; set; }
    public int LoadPercentage { get; set; }
    public double BatteryVoltage { get; set; }
    public double BatteryCutOffVoltage { get; set; }
    public double BulkVoltage { get; set; }
    public double FloatVoltage { get; set; }
    public double BatteryLowBackToGrid { get; set; }
    public double BackToBattery { get; set; }
    public string InverterName { get; set; } = String.Empty;
    public SystemStatus Status { get; set; }
    public OutputSourceSelection OutputSource { get; set; }
    public ChargingSourceSelection ChargingSource { get; set; }
    public double BatteryLoadInWatts => REG_Bat_Watt;// { get { return REG_Bat_Watt; } }
    public double ChargingCurrentInAmps => REG_FloatChargeCurr; 
    public double SolarGenerationLast24hrInkWh =>  REG_Epv1_today; 
    public double GridChargeInKwhToBatteryLast24Hr => REG_Eac_chrToday; 
    public double BatteryKwhDischargedLast24Hr => REG_Ebat_dischrToday; 
    public double GridKwhDischargedLast24Hr => REG_Eac_dischrToday; 
    public double GridLoadInWatts => REG_AC_InWatt; 
    public double LoadInWatts => REG_OP_Watt; 
    public double BatteryACChargeInWatts => REG_ACChr_Watt; 
    public double PVPowerInWatts => REG_Ppv1; 
    public double PVVoltage => REG_Vpv1; 
}
