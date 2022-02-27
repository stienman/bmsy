public interface IBMS : IDisposable
{
    static IBMS FinishInit(IBMS? target, string port, string name)
    {
        Log.instance.Information($"Initialising bms {name}");

        if (target == null)
            throw new ArgumentNullException($"Cannot instantiate the BMS named ${name}, the type was not found.");
        target.Name = name;
        target.Port = port;
        return target;
    }
    bool Connect();
    bool GetUpdate();
    event BMSDataReceivedEventHandler BMSDataReceived;
    string Name { get; set; }
    string Port { get; set; }
    string Type { get; }
}
public delegate void BMSDataReceivedEventHandler(IBMS bms, BMSDataReceivedEventArgs e);

public class BMSDataReceivedEventArgs : EventArgs
{
    public IBMSInfo BMSInfo { get; set; }
}