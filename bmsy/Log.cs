#nullable enable

public class Log
{
    private static object semaphore = new object();
    public static readonly Log instance = new Log();

    public void Information(string msg)
    {
        writeLine(LogToFile($"[{DateTime.Now.ToLongTimeString()}] (info)   {msg}"), ConsoleColor.Green);
    }
    public void Warning(string msg)
    {
        writeLine(LogToFile($"[{DateTime.Now.ToLongTimeString()}] (warning) {msg}"), ConsoleColor.Yellow);
    }
    public void Error(string msg)
    {
        writeLine(LogToFile($"[{DateTime.Now.ToLongTimeString()}] (error)   {msg}"), ConsoleColor.Red);
        Orchestrator.instance.SendToMQTT("ERROR", msg);
    }

    private string LogToFile(string msg)
    {
        File.AppendAllText($"{AppDomain.CurrentDomain.BaseDirectory}/{DateTime.Now.ToString("dd_MM_yyyy")}.log", $"{msg}{Environment.NewLine}");
        return msg;
    }

    void writeLine(string message, ConsoleColor blinkin)
    {
        lock (semaphore)
        {
            Console.ForegroundColor = blinkin;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    int color = 0;
    
    public void Pulse()
    {        
        char outchar = (char)0x2588;
        lock (semaphore)
        {
            color = color > 14 ? 0 : color + 1;
            Console.ForegroundColor = (ConsoleColor)color;
            Console.Write("*");
            Console.ResetColor();
        }
    }
}