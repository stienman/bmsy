public interface IInverter : IDisposable
{
    IInverterInfo GetUpdate();

    static IInverter FinishInit(IInverter? target, string port, string name)
    {
        Log.instance.Information($"Initialising inverter {name}");
        if( target == null)
            throw new ArgumentNullException($"Cannot instantiate the inverter named '${name}', the type was not found.");
        target.Name = name;
        target.Port = port;
        return target;
    }

    bool Connect();
    int GetHoldingRegisterValueAtAddress(int address);
    int[] GetHoldingRegisterValueAtAddressWithLength(int address, int length);
    int GetInputRegisterValueAtAddress(int address);
    int[] GetInputRegisterValueAtAddressWithLength(int address, int length);
    bool SetBackToBattery(double value);
    bool SetBatteryCutOffVoltage(double value);
    bool SetBatteryLowBackToGrid(double value);
    bool SetBulkVoltage(double value);
    bool SetChargingCurrent(int amps);
    bool SetChargingCurrentAC(int amps);
    bool SetChargingSource(ChargingSourceSelection newCharchingSource);
    void SetDateTimeOnInverter();
    bool SetFloatVoltage(double value);
    bool SetOutputSource(OutputSourceSelection newOutputSource);
    string Name { get; set; }
    string Port { get; set; }
    string Type { get; }
}
