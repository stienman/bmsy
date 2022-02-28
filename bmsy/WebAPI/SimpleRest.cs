using Newtonsoft.Json;
using Microsoft.AspNetCore.Diagnostics;

public class SimpleRest
{
    public SimpleRest()
    {
        int port = int.Parse(BMSYConfig.GetConfigByKey("ApiPort"));

        var rootPath = $"{AppDomain.CurrentDomain.BaseDirectory}wwwroot/";
        Log.instance.Information($"Serving static WebApi files from {rootPath}");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { WebRootPath = rootPath });
        builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(port));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var devCorsPolicy = "devCorsPolicy";
        builder.Services.AddCors(o => o.AddPolicy(devCorsPolicy, b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));


        var app = builder.Build();

        // Ok.. dit is een cop out maar ik kan via deze custom middleware elke exception trappen en de rest gaat blijft draaien.
        app.UseExceptionHandler(oops => oops.Run(async httppCtx => await HandleException(httppCtx)));
        
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors(devCorsPolicy);
        app.UseDefaultFiles();
        app.UseStaticFiles();
        MapMethods(app);
        app.RunAsync();
    }

    private static async Task HandleException(HttpContext context)
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        string exceptionMessge = string.Empty;
        if (exceptionHandlerPathFeature != null)
            exceptionMessge = exceptionHandlerPathFeature.Error.Message;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonConvert.SerializeObject(new { error = exceptionMessge }));
    }

    private void MapMethods(WebApplication app)
    {
        app.MapGet("/SetChargingCurrent/{current}", SetChargingCurrent);
        app.MapGet("/SetChargingCurrentAC/{current}", SetChargingCurrentAC);
        app.MapGet("/SetChargingSource/{chargingSourceSelection}", SetChargingSource);
        app.MapGet("/SetOutputSource/{outputSourceSelection}", SetOutputSource);
        app.MapGet("/SetPolling/{state}", SetPolling);
        app.MapGet("/SetPollingInterval/{state}", SetPollingInterval);
        app.MapGet("/SetFloatVoltage/{voltage}", SetFloatVoltage);
        app.MapGet("/SetBulkVoltage/{voltage}", SetBulkVoltage);
        app.MapGet("/SetBatteryCutOffVoltage/{voltage}", SetBatteryCutOffVoltage);
        app.MapGet("/SetBatteryLowBackToGrid/{voltage}", SetBatteryLowBackToGrid);
        app.MapGet("/SetBackToBattery/{voltage}", SetBackToBattery);
        app.MapGet("/SetDateTimeOnInverter", SetDateTimeOnInverter);
        app.MapGet("/GetInverterStatuses", GetInverterStatuses);
        app.MapGet("/GetBMSStatuses", GetBMSStatuses);        
        app.MapGet("/GetBatteryStatus", GetBatteryStatus);
        app.MapGet("/GetChargingCurrent", GetChargingCurrent);
        app.MapGet("/GetOutputSource", GetOutputSource);
        app.MapGet("/GetBatterySOC/{bmsName}", GetBatterySOC);
        app.MapGet("/GetBatteryVoltage", GetBatteryVoltage);
        app.MapGet("/GetCellVoltages", GetCellVoltages);

        //app.MapGet("/GetInputRegister/{inputRegisterNr}", GetInputRegister);
        //app.MapGet("/GetHoldingRegister/{holdingRegisterNr}", GetHoldingRegister);
        //app.MapGet("/GetAllHoldingRegisters", GetAllHoldingRegisters);
        //app.MapGet("/GetAllInputRegisters", GetAllInputRegisters);

    }
    string SetDateTimeOnInverter()
    {
        Orchestrator.instance.SetDateTimeOnInverter();
        return "OK";
    }

    string SetBatteryCutOffVoltage(double voltage)
    {
        Log.instance.Information($"REST: Setting SetBatteryCutOffVoltage voltage to { voltage }");
        Orchestrator.instance.SetBatteryCutOffVoltage(voltage, RequestSource.RestAPI);
        return "OK";
    }

    string SetBatteryLowBackToGrid(double voltage)
    {
        Log.instance.Information($"REST: Setting BatteryLowBackToGrid voltage to { voltage }");
        Orchestrator.instance.SetBatteryLowBackToGrid(voltage, RequestSource.RestAPI);
        return "OK";
    }
    string SetBackToBattery(double voltage)
    {
        Log.instance.Information($"REST: Setting BackToBattery voltage to { voltage }");
        Orchestrator.instance.SetBackToBattery(voltage, RequestSource.RestAPI);
        return "OK";
    }

    string SetBulkVoltage(double voltage)
    {
        Log.instance.Information($"REST: Setting Bulk voltage to { voltage }");
        Orchestrator.instance.SetBulkVoltage(voltage, RequestSource.RestAPI);
        return "OK";
    }
    string SetFloatVoltage(double voltage)
    {
        Log.instance.Information($"REST: Setting float voltage to {voltage }");
        Orchestrator.instance.SetFloatVoltage(voltage, RequestSource.RestAPI);
        return "OK";
    }

    IBMSInfo? GetBatteryStatus(string bmsName)
    {
        return Orchestrator.instance.GetBatteryStatus(bmsName);
    }

    IInverterInfo[] GetInverterStatuses()
    {
        return Orchestrator.instance.GetInverterStatuses();
    }

    IBMSInfo[] GetBMSStatuses()
    {
        return Orchestrator.instance.GetBMSStatuses();
    }


    string SetPolling(int state)
    {
        Log.instance.Information($"REST: Polling {(state==1 ? "activated": "disabled") }");
        Orchestrator.instance.SetPolling(state == 1);
        return "OK";
    }
    string SetPollingInterval(int state)
    {
        Log.instance.Information($"REST: Polling interval set to {state }");
        Orchestrator.instance.SetPollingInterval(state);
        return "OK";
    }
    string SetChargingCurrent(int current)
    {
        Log.instance.Information($"REST: Setting Charging Current to {current}");
        try
        {
            if (current < 131)
                Orchestrator.instance.SetChargingCurrent(current, RequestSource.RestAPI);
            else return $"Cannot be higher than 130";
            return $"OK";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
    string SetChargingCurrentAC(int current)
    {
        Log.instance.Information($"REST: Setting AC Charging Current to {current}");
        try
        {
            if (current < 81)
                Orchestrator.instance.SetChargingCurrentAC(current, RequestSource.RestAPI);
            else return $"Cannot be higher than 80";
            return $"OK";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }



    internal List<Tuple<string, string>> GetCellVoltages(string bmsName)
    {
        //Log.Logger.Information($"REST: GetCellVoltages info requested");
        return Orchestrator.instance.GetCellVoltages(bmsName);
    }
    double GetBatteryVoltage(string bmsName)
    {
        //Log.Logger.Information($"REST: GetBatteryVoltage requested");
        return Orchestrator.instance.GetBatteryVoltage(bmsName);
    }

    double GetBatterySOC(string bmsName)
    {
        //Log.Logger.Information($"REST: GetBatterySOC requested");
        return Orchestrator.instance.GetBatterySOC(bmsName);
    }

    string GetChargingCurrent()
    {
        //Log.Logger.Information($"REST: GetChargingCurrent requested");
        return Orchestrator.instance.GetChargingCurrent().ToString();
    }

    string GetOutputSource()
    {
        Log.instance.Information($"REST: GetOutputSource requested");
        OutputSourceSelection? selection = Orchestrator.instance.GetCurrentOutputSource();
        if (selection.HasValue)
            return selection.Value.ToString();
        else return Error();
    }
    
    string SetChargingSource(int chargingSourceSelection)
    {
        Log.instance.Information($"REST: Setting Charging Source to { ((ChargingSourceSelection)chargingSourceSelection).ToString()  } ");
        Orchestrator.instance.SetChargingSource((ChargingSourceSelection)chargingSourceSelection, RequestSource.RestAPI);
        return $"OK";
    }
    string SetOutputSource(int outputSourceSelection)
    {
        Log.instance.Information($"REST: Setting Outpput Source to {((OutputSourceSelection)outputSourceSelection).ToString()}");
        Orchestrator.instance.SetOutputSource((OutputSourceSelection)outputSourceSelection, RequestSource.RestAPI);
        return $"OK";
    }

    public string Error() { return "Error!"; }


    #region Abandoned code 
    //List<RegisterEntry> GetAllHoldingRegisters()
    //{
    //    return Orchestrator.instance.GetAllHoldingRegistryEntries();
    //}
    //List<RegisterEntry> GetAllInputRegisters()
    //{
    //    return Orchestrator.instance.GetAllInputRegistryEntries();
    //}
    //List<string> GetInputRegister(int inputRegisterNr)
    //{
    //    //Log.Logger.Information($"REST: Reading InputRegister {inputRegisterNr}");
    //    return Orchestrator.instance.GetInputRegister(inputRegisterNr);
    //}

    //List<string> GetHoldingRegister(int holdingRegisterNr)
    //{
    //    //Log.Logger.Information($"REST: Reading HoldingRegister {holdingRegisterNr}");
    //    return Orchestrator.instance.GetHoldingRegister(holdingRegisterNr);
    //}
    #endregion

}
