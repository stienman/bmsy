
public class EfficiencyTracker
{
    public static readonly EfficiencyTracker Instance = new EfficiencyTracker();  
    // Helper class to store values and give return the average.
    private class EfficiencyItem
    {
        bool looped = false;
        int max = 5, counter = 0;
        double[] values;

        public EfficiencyItem(int maxNumberOfValues)
        {
            max = maxNumberOfValues;
            values = new double[max];
        }

        public void Store(double val)
        {
            if (counter == max)
            {
                looped = true;
                counter = 0;
            }


            values[counter++] = val;
        }
        public double GetAverage(int precision)
        {
            int end = looped ? max : counter;
            double tmp = 0;
            for (int i = 0; i < end; i++)
                tmp += values[i];
            return Math.Round(tmp / end, precision);
        }

        public double GetAverage()
        {
            return GetAverage(2);
        }
    }

    private EfficiencyTracker()
    {
        int maxNumber = 5; // This is to get some sort of smoothing but it could very well just be the actual value calculated by setting it to 1
        pvtodc = new EfficiencyItem(maxNumber);
        actodc = new EfficiencyItem(maxNumber);
        dcvtoac = new EfficiencyItem(maxNumber);
    }

    EfficiencyItem pvtodc, actodc, dcvtoac;

    public void PublishEfficiency(List<IInverterInfo> inverters, List<IBMSInfo> bmss)
    {
        // I'll need to further clkean this up and verify if it proces correct values

        double totalWattsOnBMS = 0, batteryACChargeInWatts = 0, totalLoadInWatts = 0, totalBatteryWatts = 0, loss = 0;
        foreach (var bms in bmss)
            totalWattsOnBMS += (bms.Voltage * bms.Current);

        bool atLeastOneInPVChageAndDischarge = inverters.Where(t => t.Status == SystemStatus.PV_Charge_and_Discharge).ToList().Count > 0;
        bool allInvertersAreDischarging = inverters.Where(t => t.Status == SystemStatus.Discharge).ToList().Count == inverters.Count;
        bool allInvertersAreChargingFromAC = inverters.Where(t=> t.ChargingSource == ChargingSourceSelection.PVAndUtility).ToList().Count == inverters.Count;


        foreach (var inverter in inverters)
        {
            totalBatteryWatts += inverter.BatteryLoadInWatts;
            batteryACChargeInWatts += inverter.BatteryACChargeInWatts;
            totalLoadInWatts += inverter.LoadInWatts;
        }

        if (atLeastOneInPVChageAndDischarge)
        {
            // Als 1 van de inverters in PV_Charge_and_Discharge staat dan kunnen we dit berekenen.
            loss = (totalBatteryWatts / -1) - totalWattsOnBMS; // totalBatteryWatts is negatief.
            if (loss > 0)
            {  // There has to be a loss.
                var PVtoDCPctLoss = Math.Round(((loss / (totalBatteryWatts / -1)) * 100), 2);
                pvtodc.Store(PVtoDCPctLoss);
                MqttPublisher.Instance.PublishOnTotalsTopic("PVtoDCPctLoss", pvtodc.GetAverage().ToString());

                Console.WriteLine($"PV ->DC Loss :{totalBatteryWatts} - {totalWattsOnBMS} = {loss} watt");
            }
        }
        else
        {
            MqttPublisher.Instance.PublishOnTotalsTopic("PVtoDCPctLoss", "0");
        }

        if (allInvertersAreChargingFromAC)
        {
            // AC ->DC Loss
            loss = batteryACChargeInWatts - totalWattsOnBMS;
            if (loss > 0 && batteryACChargeInWatts > 1)
            { // There has to be a loss and we have to be actually charging or it's not going to make sense.
                var ACtoDCPctLoss = Math.Round(((loss / batteryACChargeInWatts) * 100), 2);
                actodc.Store(ACtoDCPctLoss);
                MqttPublisher.Instance.PublishOnTotalsTopic("ACtoDCPctLoss", actodc.GetAverage().ToString());
                // Console.WriteLine($"AC ->DC Loss : {loss} watt");
            }
        }
        else
        {
            MqttPublisher.Instance.PublishOnTotalsTopic("ACtoDCPctLoss", "0");
        }

        if (allInvertersAreDischarging)
        {
            // DC ->AC Loss 
            totalWattsOnBMS = totalWattsOnBMS / -1;
            loss = totalWattsOnBMS - totalLoadInWatts;
            if (loss > 0)
            { // There has to be a loss.
                var DCtoACPctLoss = Math.Round(((loss / totalWattsOnBMS) * 100), 2);
                dcvtoac.Store(DCtoACPctLoss);
                MqttPublisher.Instance.PublishOnTotalsTopic("DCtoACPctLoss", dcvtoac.GetAverage().ToString());
                // Console.WriteLine($"DC ->AC Loss : {loss} watt");
            }
        }
        else
        {
            MqttPublisher.Instance.PublishOnTotalsTopic("DCtoACPctLoss", "0");
        }
    }
}