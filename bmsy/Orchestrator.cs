
public class Orchestrator
{
    public static readonly Orchestrator instance = new Orchestrator();

    SimpleRest restApi = new();
    BatteryIntervention batteryManagementSystem = new();
    


    List<IInverter> configuredInverters = new();
    List<IBMS> configuredBMSs = new();
    List<IInverterInfo> cachedInfo = new();
    Dictionary<IBMS, IBMSInfo> bmsRegister = new();

    int pollinInterval = int.Parse(BMSYConfig.GetConfigByKey("PollInterval"));
    bool busy = false;
    bool pollingAllowed = true;
    Task delayTask;

    private Orchestrator()
    {
        Console.WriteLine("\t\t\t=== BMSY === ");

        configuredBMSs = DeviceFactory.GetConfiguredBMS();
        configuredInverters = DeviceFactory.GetConfiguredInverters();

        MqttPublisher.Instance.PublishOnMainTopic("Started", $"I started at {DateTime.Now.ToShortTimeString()} on {DateTime.Now.ToShortDateString()}");
    }

    public void Start()
    {
        // Connect and subscribe to events
        configuredInverters.ForEach(x => connectInverter(x));
        configuredBMSs.ForEach(x => connectBMS(x));  
        // Everyhing is hooked up and we can start polling
        StartPolling();
    }

    void connectInverter(IInverter theInverter)
    {
        theInverter.Connect();
    }

    void connectBMS(IBMS theBMS)
    {
        theBMS.BMSDataReceived += BMSDataReceived;
        theBMS.Connect();
    }

    private void BMSDataReceived(IBMS bms, BMSDataReceivedEventArgs e)
    {
        // Console.WriteLine("Orchestrator:: BMSDataReceived()");
        if (!bmsRegister.ContainsKey(bms))
            bmsRegister.Add(bms, e.BMSInfo);
        else bmsRegister[bms] = e.BMSInfo;

        MqttPublisher.Instance.PublishBMSInformation(bms, e.BMSInfo);
        batteryManagementSystem.BatteryInfoReceived(bms, e.BMSInfo);
    }

    // PUBLIC METHODS CALLED FROM REST

    internal void SetDateTimeOnInverter()
    {
        configuredInverters.ForEach(x => x.SetDateTimeOnInverter());
    }

    public void SetOutputSource(OutputSourceSelection newOutputSource, RequestSource requestSource)
    {
        configuredInverters.ForEach(x => x.SetOutputSource(newOutputSource));
    }
    public void SetChargingSource(ChargingSourceSelection newCharchingSource, RequestSource requestSource)
    {
        Log.instance.Information($"[${requestSource}] requested to SetChargingSource to {newCharchingSource.ToString()}");

        configuredInverters.ForEach(x => x.SetChargingSource(newCharchingSource));
        if (requestSource != RequestSource.BatteryManagementSystem)
            batteryManagementSystem.ChargingSourceWasChanged(newCharchingSource, requestSource);

    }
    public void SetChargingCurrent(int amps, RequestSource requestSource)
    {
        Log.instance.Information($"[${requestSource}] requested to SetChargingCurrent to {amps.ToString()}");

        configuredInverters.ForEach(x => x.SetChargingCurrent(amps));
        if (requestSource != RequestSource.BatteryManagementSystem)
            batteryManagementSystem.ChargingCurrentWasChanged(amps, requestSource);

    }
    public void SetChargingCurrentAC(int amps, RequestSource requestSource)
    {
        configuredInverters.ForEach(x => x.SetChargingCurrentAC(amps));
    }
    public double GetBatterySOC(string name)
    {
        var bmsInfo = GetBMSInfo(name);
        if (bmsInfo != null)
            return bmsInfo.SOC;
        else return 0;
    }

    private IBMSInfo? GetBMSInfo(string bmsName)
    {
        foreach (var bms in bmsRegister)
            if (bms.Key.Name == bmsName)
                return bms.Value;
        return null;
    }

    public OutputSourceSelection GetCurrentOutputSource()
    {
        foreach (var inverter in cachedInfo)
            return inverter.OutputSource;
        return OutputSourceSelection.UNKNOWN;
    }

    internal void SetPolling(bool v)
    {
        pollingAllowed = v;
        Log.instance.Information($"Polling is now { (v ? "activated" : "disabled")}");
    }

    /// VOLTAGES ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    internal void SetBatteryCutOffVoltage(double voltage, RequestSource requestSource)
    {
        configuredInverters.ForEach(x => x.SetBatteryCutOffVoltage(voltage));
    }
    internal void SetBulkVoltage(double voltage, RequestSource requestSource)
    {
        configuredInverters.ForEach(x => x.SetBulkVoltage(voltage));
    }
    internal void SetBatteryLowBackToGrid(double voltage, RequestSource requestSource)
    {
        configuredInverters.ForEach(x => x.SetBatteryLowBackToGrid(voltage));
    }

    internal IBMSInfo[] GetBMSStatuses()
    {
        return bmsRegister.Values.ToArray();
    }

    internal void SetBackToBattery(double voltage, RequestSource requestSource)
    {
        configuredInverters.ForEach(x => x.SetBackToBattery(voltage));
    }

    internal void SetFloatVoltage(double voltage, RequestSource requestSource)
    {
        configuredInverters.ForEach(x => x.SetFloatVoltage(voltage));
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    internal void SetPollingInterval(int state)
    {
        pollinInterval = state;
        delayTask = Task.Delay(pollinInterval);
    }
    internal IBMSInfo? GetBatteryStatus(string namwe)
    {
        return GetBMSInfo(namwe);
    }

    internal double GetBatteryVoltage(string name)
    {
        var bmsInfo = GetBMSInfo(name);
        if (bmsInfo != null)
            return bmsInfo.Voltage;
        else return 0;
    }

    internal List<Tuple<string, string>> GetCellVoltages(string name)
    {
        var results = new List<Tuple<string, string>>();
        var bmsInfo = GetBMSInfo(name);
        if (bmsInfo == null)
            return results;

        for (int i = 0; i < bmsInfo.CellVoltages.Length; i++)
            results.Add(new(i.ToString(), bmsInfo.CellVoltages[i].ToString()));
        return results;
    }

    internal double GetChargingCurrent()
    {
        foreach (var inverter in cachedInfo)
            return inverter.ChargingCurrentInAmps; // All inverters are supposed to use the same settings, so the first one is aa correct as the second one.
        return -1;
    }

    private async void StartPolling()
    {
        while (true)
        {
            delayTask = Task.Delay(pollinInterval);
            if (pollingAllowed)
            {
                GatherInverterInfoAndPublish();
                GatherBMSInfo();
                EfficiencyTracker.Instance.PublishEfficiency(cachedInfo, bmsRegister.Values.ToList());
                Log.instance.Pulse(); // This is just a visualization for the console window
            }
            else
            {
                Log.instance.Information($"Polling disabled.");
            }
            await delayTask;
        }
    }


    void GatherBMSInfo()
    {
        foreach (var bms in configuredBMSs)
            bms.GetUpdate();
    }

    void GatherInverterInfoAndPublish()
    {
        try
        {
            if (busy)
                return;

            busy = true;
            // We gaan het cachen
            List<IInverterInfo> tmp = new();
            foreach (var inverter in configuredInverters)
                tmp.Add(inverter.GetUpdate());
            // Send it off
            MqttPublisher.Instance.PublishInverterInformation(tmp);
            cachedInfo = tmp;
        }
        catch(TimeoutException)
        {
            // Resolve the timeout exception.
            ReconnectInverters();
        }
        catch (Exception ex)
        {
            Log.instance.Error(ex.ToString());
        }
        finally
        {
            busy = false;
        }
    }

    private void ReconnectInverters()
    {
        Log.instance.Warning("Reconnecting the inverters because there are timeouts.");
        pollingAllowed = false;
        configuredInverters.ForEach(inverter => inverter.Dispose());
        configuredInverters.Clear();

        Thread.Sleep(10000); // Magic number to sleep.

        configuredInverters = DeviceFactory.GetConfiguredInverters();
        configuredInverters.ForEach(x => x.Connect());

        pollingAllowed = true;
    }



    public IInverterInfo[] GetInverterStatuses()
    {
        if (cachedInfo == null)
            cachedInfo = new List<IInverterInfo>();
        return cachedInfo.ToArray();
    }


    #region Abandoned or no longer needed

    //public List<RegisterEntry> GetAllHoldingRegistryEntries()
    //{
    //    var results = new List<RegisterEntry>();
    //    //foreach (var inv in inverters)
    //    //{
    //    var inv = configuredInverters[0];
    //    for (int i = 0; i < 162; i++)
    //    {
    //        var res = inv.GetHoldingRegisterValueAtAddress(i);
    //        results.Add(new() { Address = i.ToString(), Contents = res, InverterName = inv.Name });
    //        Thread.Sleep(10);
    //    }
    //    //}

    //    return results;
    //}

    //internal List<RegisterEntry> GetAllInputRegistryEntries()
    //{
    //    var results = new List<RegisterEntry>();
    //    //foreach (var inv in inverters)
    //    //{
    //    var inv = configuredInverters[0];
    //    for (int i = 0; i < 381; i++)
    //    {
    //        var res = inv.GetInputRegisterValueAtAddress(i);
    //        results.Add(new() { Address = i.ToString(), Contents = res, InverterName = inv.Name });
    //        Thread.Sleep(10);
    //    }
    //    //}

    //    return results;
    //}

    //internal List<string> GetInputRegister(int inputRegisterNr)
    //{
    //    var list = new List<string>();
    //    foreach (var inverter in configuredInverters)
    //        list.Add($"Inverter {inverter.Name}, Input Register at address {inputRegisterNr} : {inverter.GetInputRegisterValueAtAddress(inputRegisterNr)}");
    //    return list;
    //}

    //internal List<string> GetHoldingRegister(int holdingRegisterNr)
    //{
    //    var list = new List<string>();
    //    foreach (var inverter in configuredInverters)
    //        list.Add($"Inverter {inverter.Name}, Holding Register at address {holdingRegisterNr} : {inverter.GetHoldingRegisterValueAtAddress(holdingRegisterNr)}");
    //    return list;
    //}


    #endregion
}
