
using EasyModbus;
using System.Runtime.Serialization;



public class SPF5000ES : IInverter
{
    public string Name { get; set; }
    public string Port { get; set; }
    public string Type { get { return "SPF5000ES"; } }

    const int REG_BULK_VOLTAGE = 35, REG_PV_WATTS = 03, REG_FLOAT_VOLTAGE = 36, REG_BACK_TO_BATTERY = 95, REG_LOAD_IN_WATTS = 09, BATT_CUTTOFF_VOLTAGE = 94;
    ModbusClient modbusClient;
    private bool disposedValue;

    public bool Connect()
    {
        try
        {
            modbusClient = new ModbusClient(Port);
            modbusClient.UnitIdentifier = 1;
            modbusClient.Baudrate = 9600;
            modbusClient.Parity = System.IO.Ports.Parity.None;
            modbusClient.StopBits = System.IO.Ports.StopBits.One;
            modbusClient.ConnectionTimeout = 500;
            modbusClient.ConnectedChanged += ModbusClient_ConnectedChanged;
            modbusClient.Connect();

            Log.instance.Information($"SPF5000ES {Name} connected to {Port}");
            return true;
        }
        catch (Exception ex)
        {
            Log.instance.Error($"SPF5000ES {Name} could not connect to {Port} : {ex.Message}");
            return false;
        }
    }

    private void ModbusClient_ConnectedChanged(object sender)
    {
        Log.instance.Information($"Connection State Changed on SPF5000ES {Name}, {sender.GetType()}");
    }
    public void SetDateTimeOnInverter()
    {
        WriteToHoldingRegister(45, DateTime.Now.Year);
        Thread.Sleep(100);
        WriteToHoldingRegister(46, DateTime.Now.Month);
        Thread.Sleep(100);
        WriteToHoldingRegister(47, DateTime.Now.Day);
        Thread.Sleep(100);
        WriteToHoldingRegister(48, DateTime.Now.Hour);
        Thread.Sleep(100);
        WriteToHoldingRegister(49, DateTime.Now.Minute);
        Thread.Sleep(100);
        WriteToHoldingRegister(50, DateTime.Now.Second);
    }
    bool WriteToHoldingRegister(int address, int value)
    {
        try
        {
            Log.instance.Information($"Writing value {(int)value} to Holding register address {address} on {Name}");
            //modbusClient.Connect();
            modbusClient.WriteSingleRegister(address, value);
            // modbusClient.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
            Log.instance.Error(ex.ToString());
            return false;
        }
    }
    public bool SetOutputSource(OutputSourceSelection newOutputSource)
    {
        Log.instance.Information($"Writing value {(int)newOutputSource} to register 1 on {Name}");
        return WriteToHoldingRegister(1, (int)newOutputSource);
    }
    public bool SetChargingSource(ChargingSourceSelection newCharchingSource)
    {
        Log.instance.Information($"Writing value {(int)newCharchingSource} to register 2 on {Name}");
        return WriteToHoldingRegister(2, (int)newCharchingSource);
    }
    public bool SetChargingCurrent(int amps)
    {
        Log.instance.Information($"Writing value {amps} to register 34 on {Name}");
        return WriteToHoldingRegister(34, amps);

    }
    public bool SetChargingCurrentAC(int amps)
    {
        Log.instance.Information($"Writing value {amps} to register 38 on {Name}");
        return WriteToHoldingRegister(38, amps);
    }

    public bool SetBatteryCutOffVoltage(double value)
    {
        Log.instance.Information($"Writing value {value} to register {BATT_CUTTOFF_VOLTAGE} on {Name}");
        return WriteToHoldingRegister(BATT_CUTTOFF_VOLTAGE, (int)(value * 10));
    }
    public bool SetBulkVoltage(double value)
    {
        Log.instance.Information($"Writing value {value} to register 35 on {Name}");
        return WriteToHoldingRegister(REG_BULK_VOLTAGE, (int)(value * 10));
    }
    public bool SetFloatVoltage(double value)
    {
        Log.instance.Information($"Writing value {value} to register 36 on {Name}");
        return WriteToHoldingRegister(REG_FLOAT_VOLTAGE, (int)(value * 10));
    }
    public bool SetBatteryLowBackToGrid(double value)
    {
        Log.instance.Information($"Writing value {value} to register 37 on {Name}");
        return WriteToHoldingRegister(37, (int)(value * 10));
    }
    public bool SetBackToBattery(double value)
    {
        Log.instance.Information($"Writing value {value} to register 95 on {Name}");
        return WriteToHoldingRegister(REG_BACK_TO_BATTERY, (int)(value * 10));
    }

    int ReadHighLowInputRegister(int registerNumber, double multiplier)
    {
        return (int)(ModbusClient.ConvertRegistersToInt(modbusClient.ReadInputRegisters(registerNumber, 2), ModbusClient.RegisterOrder.HighLow) * multiplier);
    }
    int ReadHighLowHoldingRegister(int registerNumber, double multiplier)
    {
        return (int)(ModbusClient.ConvertRegistersToInt(modbusClient.ReadHoldingRegisters(registerNumber, 2), ModbusClient.RegisterOrder.HighLow) * multiplier);
    }


    public IInverterInfo GetUpdate()
    {
        SPF5000ESInfo info = new() { InverterName = Name };

        if (!modbusClient.Connected)
        {
            modbusClient.Connect();
            Thread.Sleep(100);
        }

        info.REG_OP_Watt = ReadHighLowInputRegister(REG_LOAD_IN_WATTS, .1);
        info.REG_OP_VA = ReadHighLowInputRegister(11, .1);
        info.REG_ACChr_Watt = ReadHighLowInputRegister(13, .1);
        info.REG_ACChr_VA = ReadHighLowInputRegister(15, .1);
        info.REG_AC_InWatt = ReadHighLowInputRegister(36, .1);
        //info.REG_AC_InVA = ReadHighLowInputRegister(38, .1);
        info.REG_Epv1_today = ReadHighLowInputRegister(48, .1);
        info.REG_Epv1_total = ReadHighLowInputRegister(50, .1);
        info.REG_Eac_chrToday = ReadHighLowInputRegister(56, .1);
        info.REG_Eac_chrTotal = ReadHighLowInputRegister(58, .1);
        info.REG_Ebat_dischrToday = ReadHighLowInputRegister(60, .1);
        info.REG_Ebat_dischrTotal = ReadHighLowInputRegister(62, .1);
        info.REG_Eac_dischrToday = ReadHighLowInputRegister(64, .1);
        info.REG_Eac_dischrTotal = ReadHighLowInputRegister(66, .1);
        info.REG_AC_DisChrWatt = ReadHighLowInputRegister(69, .1);
        info.REG_AC_DisChrVA = ReadHighLowInputRegister(71, .1);
        info.REG_Bat_DisChrWatt = ReadHighLowInputRegister(73, .1);
        info.REG_Bat_DisChrVA = ReadHighLowInputRegister(75, .1);
        info.REG_Bat_Watt = ReadHighLowInputRegister(77, .1);
        info.REG_Ppv1 = ReadHighLowInputRegister(REG_PV_WATTS, .1);
        info.REG_Vpv1 = GetDoubleOrReturn(GetInputRegisterValueAtAddress(1), .1, -1);
        info.REG_FloatChargeCurr = GetIntOrReturn(GetHoldingRegisterValueAtAddress(38), 1, 2);

        info.BatteryVoltage = GetDoubleOrReturn(GetInputRegisterValueAtAddress(17), .01, 2, -1);
        info.BatteryCutOffVoltage = GetDoubleOrReturn(GetHoldingRegisterValueAtAddress(BATT_CUTTOFF_VOLTAGE), .1, 2, -1);
        info.Temperature = GetDoubleOrReturn(GetInputRegisterValueAtAddress(25), .1, 2, -1);
        info.LoadPercentage = GetIntOrReturn(GetInputRegisterValueAtAddress(27), .1, -1);
        info.BulkVoltage = Math.Round(GetDoubleOrReturn(GetHoldingRegisterValueAtAddress(REG_BULK_VOLTAGE), .1, 1), 1);
        info.FloatVoltage = Math.Round(GetDoubleOrReturn(GetHoldingRegisterValueAtAddress(REG_FLOAT_VOLTAGE), .1, 1), 1);
        info.BatteryLowBackToGrid = Math.Round(GetDoubleOrReturn(GetHoldingRegisterValueAtAddress(37), .1, 1), 1);
        info.BackToBattery = Math.Round(GetDoubleOrReturn(GetHoldingRegisterValueAtAddress(REG_BACK_TO_BATTERY), .1, 1), 1);

        info.OutputSource = (OutputSourceSelection)(GetHoldingRegisterValueAtAddress(1));
        info.ChargingSource = (ChargingSourceSelection)(GetHoldingRegisterValueAtAddress(2));
        info.Status = (SystemStatus)((GetInputRegisterValueAtAddress(0)));

        return info;
    }

    int GetIntOrReturn(int result, double multiplier, int returnIfFails)
    {
        return (int)(result * multiplier);
    }
    double GetDoubleOrReturn(int result, double multiplier, int decimals, double returnIfFials)
    {
        return Math.Round(GetDoubleOrReturn(result, multiplier, returnIfFials), decimals);
    }
    double GetDoubleOrReturn(int result, double multiplier, double returnIfFials)
    {
        return result * multiplier;
    }
    public int GetInputRegisterValueAtAddress(int address)
    {
        return GetInputRegisterValueAtAddressWithLength(address, 1)[0];
    }
    public int[] GetInputRegisterValueAtAddressWithLength(int address, int length)
    {
        try
        {
            return modbusClient.ReadInputRegisters(address, length);
        }
        catch (Exception ex)
        {
            Log.instance.Warning(ex.Message);
            return new int[1] { -1 };
        }
    }
    public int GetHoldingRegisterValueAtAddress(int address)
    {
        return GetHoldingRegisterValueAtAddressWithLength(address, 1)[0];
    }
    public int[] GetHoldingRegisterValueAtAddressWithLength(int address, int length)
    {
        try
        {
            return modbusClient.ReadHoldingRegisters(address, length);
        }
        catch (Exception ex)
        {
            Log.instance.Error($"Error reading HoldingRegister number {address} on {Name}: {ex.Message}");
            return new int[1] { -1 };
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                modbusClient.ConnectedChanged -= ModbusClient_ConnectedChanged;
                modbusClient.Disconnect();
                modbusClient = null;
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}