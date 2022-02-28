public class BatteryIntervention
{

    // This class needs to be finalized, there is a ton of logic that could go here to keep the battery within parameters, charge it when power is cheap etc.
    // The trick is to not interfere with other processes like actions taken in nodered via the rest api.
    // It should really just be a backstop


    Dictionary<int, DateTime> SOCHistory = new Dictionary<int, DateTime>();
    List<BMSTask> activeTasks = new();
    int emergencyStartSOC, emergencyStopSOC, ignoreSOC = int.MinValue;

    public BatteryIntervention()
    {
        emergencyStartSOC = int.Parse(BMSYConfig.GetConfigByKey("BatteryIntervention:EmergencyStartSOC"));
        emergencyStopSOC = int.Parse(BMSYConfig.GetConfigByKey("BatteryIntervention:EmergencyStopSOC"));
    }


    public void BatteryInfoReceived(IBMS bms, IBMSInfo info)
    {
        RegisterSOC((int)info.SOC);
        if (info.SOC == ignoreSOC)
            return; // We are not taking any actions on this SOC because it was delibratly set to be ignored 

        ignoreSOC = int.MinValue; // we're in an SOC that is different to the one we need to ignore, reset the ignore SOC.

        if (info.SOC < emergencyStartSOC && !ActionIsActive(TaskInfo.EmergencyCharge))
        {
            Log.instance.Information($"[BatteryIntervention] The SOC is {info.SOC}%, now initiating a emergency charge to {emergencyStartSOC}%");
            activeTasks.Add(new BMSTask(TaskInfo.EmergencyCharge));
            Orchestrator.instance.SetChargingCurrent(10, RequestSource.BatteryManagementSystem);
            Orchestrator.instance.SetChargingSource(ChargingSourceSelection.PVAndUtility, RequestSource.BatteryManagementSystem);
        }
        if (info.SOC == emergencyStopSOC && ActionIsActive(TaskInfo.EmergencyCharge))
        {
            Log.instance.Information($"[BatteryIntervention] The SOC is {emergencyStartSOC}%, now setting charging source to PVOnly");
            // ok, there is an action active and we now reached the target.
            RemoveAction(TaskInfo.EmergencyCharge);
            // We're here and no changes have been made to the charging, otherwise we wouldn't be here.
            // Disable charging from grid which is the last ditch resort to keep the battery operational.
            Orchestrator.instance.SetChargingSource(ChargingSourceSelection.PVOnly, RequestSource.BatteryManagementSystem);
        }
    }

    public void ChargingCurrentWasChanged(int amps, RequestSource requestSource)
    {
        // manual intervention or other logic has made it so that we can no longer operate automaticly
        if (activeTasks.Any())
        {
            Log.instance.Information("[BatteryIntervention] ChargingCurrent was changed, cancelling all pending actions");
            foreach (var task in activeTasks)
                RemoveAction(task.TaskInfo);
        }

        ignoreSOC = GetLastRecordedSOC();
        activeTasks = new();
    }

    public void ChargingSourceWasChanged(ChargingSourceSelection newCharchingSource, RequestSource requestSource)
    {

        // manual intervention or other logic has made it so that we can no longer operate automaticly
        if (activeTasks.Any())
        {
            Log.instance.Information("[BatteryIntervention] ChargingSource was changed, cancelling all pending actions");
            foreach (var task in activeTasks)
                RemoveAction(task.TaskInfo);
        }

        ignoreSOC = GetLastRecordedSOC();
        activeTasks = new();
    }


    /// HELPER FUNCTIONS

    double LowTarrifHoursLeft(DateTime now)
    {
        var nextStart = new DateTime(now.Year, now.Month, now.Day, 21, 00, 00);
        var nextEnd = new DateTime(now.Year, now.Month, now.Day + 1, 6, 00, 00);
        if (nextEnd.DayOfWeek == DayOfWeek.Saturday || nextEnd.DayOfWeek == DayOfWeek.Sunday)
            nextEnd = GetNextWeekday(now, DayOfWeek.Monday, 6, 0);
        if (nextStart > now)
            return (now - nextStart).TotalHours;
        else return (nextEnd - now).TotalHours;
    }

    static DateTime GetNextWeekday(DateTime start, DayOfWeek day, int hour, int minutes)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7; // (... + 7) % 7 zorgt er voor dat we in de range van [0, 6] terecht komen
        start = start.AddDays(daysToAdd);
        return new DateTime(start.Year, start.Month, start.Day, hour, minutes, 0);
    }
    void RemoveAction(TaskInfo action)
    {
        var result = activeTasks.Where<BMSTask>(t => t.TaskInfo == action).FirstOrDefault();
        if (result != null)
        {
            Log.instance.Information($"Cancelling {result.TaskInfo} task which was started on {result.StartTime.ToShortTimeString()}.");
            activeTasks.Remove(result);
        }
    }
    bool ActionIsActive(TaskInfo actionToFind)
    {
        return activeTasks.Where<BMSTask>(a => a.TaskInfo == actionToFind).Any();
    }
    void RegisterSOC(int sOC)
    {
        if (SOCHistory.ContainsKey(sOC)) 
            SOCHistory[sOC] = DateTime.Now;
        else SOCHistory.Add(sOC, DateTime.Now);
    }
    private int GetLastRecordedSOC()
    {
        return SOCHistory.MaxBy(kvp => kvp.Value).Key;
    }
}    

