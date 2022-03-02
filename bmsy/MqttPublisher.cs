#nullable enable
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;


public class MqttPublisher
{

    public static readonly MqttPublisher Instance = new MqttPublisher();

    string MQTTMAINTOPIC = "BMSY", MQTTTOTALSTOPIC = "Totals";
    IManagedMqttClient client;
    List<Tuple<string, string>> backLog = new List<Tuple<string, string>>();

    private MqttPublisher()
    {
        string host = BMSYConfig.GetConfigByKey("MQTTServerHost");
        if (host != null && host != String.Empty)
        {
            int port = int.Parse(BMSYConfig.GetConfigByKey("MQTTServerPort"));
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder().WithClientId("BMSY").WithTcpServer(host, port);
            ManagedMqttClientOptions options = new ManagedMqttClientOptionsBuilder().WithAutoReconnectDelay(TimeSpan.FromSeconds(60)).WithClientOptions(builder.Build()).Build();

            client = new MqttFactory().CreateManagedMqttClient();
            client.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            client.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            client.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);
            client.StartAsync(options).GetAwaiter().GetResult();            
        }
    }

    internal void Publish(string topic, double? msg)
    {
        if (msg != null && msg.HasValue)
            Publish(topic, Math.Round(msg.Value, 2).ToString());
    }

    internal void Publish(string topic, int? msg)
    {
        if (msg != null && msg.HasValue)
            Publish(topic, msg.Value.ToString());
        else Log.instance.Warning($"No value in msg for MQTT topic {topic}");
    }

    public void Publish(string topic, string msg)
    {
        if (client != null && client.IsConnected)
            client.PublishAsync(topic, msg);
        else
            lock (backLog)
                backLog.Add(new Tuple<string, string>(topic, msg));
    }

    internal void PublishOnMainTopic(string subTopic, string msg)
    {
        Publish($"{MQTTMAINTOPIC}/{subTopic}", msg);
    }

    public void OnConnected(MqttClientConnectedEventArgs obj)
    {
        Log.instance.Information("Successfully connected to MQTT Server.");
        if (backLog.Count > 0)
            foreach (var pair in backLog)
                Publish(pair.Item1, pair.Item2);
    }

    public void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
    {
        Log.instance.Warning("Couldn't connect to MQTT broker.");
    }
    public void OnDisconnected(MqttClientDisconnectedEventArgs obj)
    {
        Log.instance.Information("Successfully disconnected from MQTT Server.");
    }
    internal void PublishInverterInformation(List<IInverterInfo> cachedInfo)
    {
        double totalLoadInWatts = 0,
            totalGridLoadInWatts = 0,
            totalPvWatts = 0,
            batteryLoadWatts = 0,
            totalGridChargeInKwhToBatteryLast24Hr = 0,
            totalBatteryKwhDischargedLast24Hr = 0,
            totalGridKwhDischargedLast24Hr = 0,
            totalAcCharge = 0,
            totalSolarGenerationLast24hrInkWh = 0;

        foreach (var inverter in cachedInfo)
        {
            
            totalLoadInWatts += inverter.LoadInWatts;
            batteryLoadWatts += inverter.BatteryLoadInWatts;
            totalAcCharge += inverter.BatteryACChargeInWatts;
            totalPvWatts += inverter.PVPowerInWatts;
            totalGridLoadInWatts += inverter.GridLoadInWatts;
            totalSolarGenerationLast24hrInkWh += inverter.SolarGenerationLast24hrInkWh;
            totalGridChargeInKwhToBatteryLast24Hr += inverter.GridChargeInKwhToBatteryLast24Hr;
            totalGridKwhDischargedLast24Hr += inverter.GridKwhDischargedLast24Hr;
            totalBatteryKwhDischargedLast24Hr += inverter.BatteryKwhDischargedLast24Hr;

            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/BatteryLoadInWatts", inverter.BatteryLoadInWatts);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/BatteryLowBackToGrid", inverter.BatteryLowBackToGrid);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/BatteryVoltage", inverter.BatteryVoltage);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/BulkVoltage", inverter.BulkVoltage);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/ChargingCurrentInAmps", inverter.ChargingCurrentInAmps);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/SolarGenerationLast24hrInkWh", inverter.SolarGenerationLast24hrInkWh);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/GridChargeInKwhToBatteryLast24Hr", inverter.GridChargeInKwhToBatteryLast24Hr);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/BatteryKwhDischargedLast24Hr", inverter.BatteryKwhDischargedLast24Hr);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/GridKwhDischargedLast24Hr", inverter.GridKwhDischargedLast24Hr);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/FloatVoltage", inverter.FloatVoltage);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/GridLoadInWatts", inverter.GridLoadInWatts);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/LoadInWatts", inverter.LoadInWatts);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/LoadPercentage", inverter.LoadPercentage);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/ChargingSource", inverter.ChargingSource.ToString());
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/OutputSource", inverter.OutputSource.ToString());
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/Status", inverter.Status.ToString());
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/Temperature", inverter.Temperature);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/PVAmps", (inverter.PVPowerInWatts / inverter.PVVoltage));
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/PVWatts", inverter.PVPowerInWatts);
            Publish($"{MQTTMAINTOPIC}/{inverter.InverterName}/PVVoltage", inverter.PVVoltage);
        }

        // Toon de richting van de watts omgekeerd, - is eigenlijk charging en + is discharging
        batteryLoadWatts = batteryLoadWatts / -1;

        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/PVWatts", Math.Round( totalPvWatts));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/BatteryWatts", Math.Round(batteryLoadWatts));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/LoadInWatts", Math.Round(totalLoadInWatts));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/GridLoadInWatts", Math.Round(totalGridLoadInWatts));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/GridChargeToBatteryLast24HrInkWh", Math.Round(totalGridChargeInKwhToBatteryLast24Hr,2));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/GridDischargedLast24HrInkWh", Math.Round(totalGridKwhDischargedLast24Hr,2));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/BatteryDischargedLast24HrInkWh", Math.Round(totalBatteryKwhDischargedLast24Hr,2));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/SolarGenerationLast24hrInkWh", Math.Round(totalSolarGenerationLast24hrInkWh,2));
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/TotalACCharge", Math.Round(totalAcCharge, 2));
    }

    internal void PublishBMSInformation(IBMS bms, IBMSInfo bmsInfo)
    {
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/Voltage", bmsInfo.Voltage);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/Current", bmsInfo.Current);

        double totalWatts = Math.Round(bmsInfo.Current * bmsInfo.Voltage, 0);

        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/Watts", totalWatts);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/SOC", bmsInfo.SOC);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/ProtectionState", bmsInfo.ProtectionState);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/Cycles", bmsInfo.Cycles);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/CellLow", bmsInfo.CellLow);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/CellHigh", bmsInfo.CellHigh);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/CellAverage", bmsInfo.CellAverage);
        Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/CellDifference", bmsInfo.CellDifference);

        for (int i = 0; i < bmsInfo.Temperatures.Length; i++)
            Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/TEMPERATURES/Temp{i + 1}", bmsInfo.Temperatures[i]);

        for (int i = 0; i < bmsInfo.CellVoltages.Length; i++)
            Publish($"{MQTTMAINTOPIC}/BMS/{bms.Name}/CELLS/Cell{i}", bmsInfo.CellVoltages[i]);
    }


    internal void PublishOnTotalsTopic(string topic, string message)
    {
        Publish($"{MQTTMAINTOPIC}/{MQTTTOTALSTOPIC}/{topic}", message);
    }
}
