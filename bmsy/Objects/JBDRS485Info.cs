#nullable enable
[Serializable()]
public class JBDRS485Info : IBMSInfo
{
    public string BMSName { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
    public double SOC { get; set; }
    public double ProtectionState { get; set; }
    public double Cycles { get; set; }
    public double[] Temperatures { get; set; }
    public double[] CellVoltages { get; set; }
    public double CellHigh { get; set; }
    public double CellLow { get; set; }
    public double CellDifference { get; set; }
    public double CellAverage { get; set; }
}