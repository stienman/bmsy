public interface IBMSInfo
{
    string BMSName { get; set; }    
    double CellAverage { get; set; }
    double CellDifference { get; set; }
    double CellHigh { get; set; }
    double CellLow { get; set; }
    double[] CellVoltages { get; set; }
    double Current { get; set; }
    double Cycles { get; set; }
    double ProtectionState { get; set; }
    double SOC { get; set; }
    double[] Temperatures { get; set; }
    double Voltage { get; set; }
}
