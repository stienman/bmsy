
using System.IO.Ports;

public class JBDRS485 : IBMS
{
    SerialPort serialPort;
    bool askingBasicInfo = false, askingCellInfo = false;
    JBDRS485Info info = new();

    byte startByte = 0xDD;
    private bool disposedValue;

    public event BMSDataReceivedEventHandler BMSDataReceived;

    protected virtual void RaiseEvent()
    {
        var handler = BMSDataReceived;
        handler?.Invoke(this, new BMSDataReceivedEventArgs() { BMSInfo = info });
    }

    public string Port { get; set; }
    public string Name {  get; set; }
    public string Type { get; set; }

    public bool Connect()
    {
        //Console.WriteLine("BMS Is connecting ....");

        try
        {
            Log.instance.Information($"JBDRS485 {Name} connecting to {Port}...");
            serialPort = new SerialPort(Port, 9600, Parity.None, 8, StopBits.One);
            serialPort.Handshake = Handshake.None;
            serialPort.DataReceived += Jbd_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;

            serialPort.Open();
            return true;
        }
        catch (Exception ex)
        {
            Log.instance.Error($"JBDRS485 {Name} error connecting: {ex.Message}");
           return false;
        }
    }

    private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        Log.instance.Error($"JBDRS485 {Name} error received on serialport {e}");
    }

    void AskBasicInfo()
    {
        //Console.WriteLine("BMS:: AskBasicInfo()");
        askingBasicInfo = true;
        askingCellInfo = false;
        var array = new byte[] { 0xdd, 0xa5, 0x03, 0x00, 0xff, 0xfd, 0x77 };
        serialPort.Write(array, 0, array.Length);
    }

    void AskCellInfo()
    {
        //Console.WriteLine("BMS:: AskCellInfo()");
        askingBasicInfo = false;
        askingCellInfo = true;
        var array = new byte[] { 0xDD, 0xA5, 0x04, 0x00, 0xFF, 0xFC, 0x77 };
        serialPort.Write(array, 0, array.Length);
    }    

    public bool GetUpdate()
    {
        //Console.WriteLine("BMS:: GetUpdate()");
        AskBasicInfo();
        return true; // eh... dunno eigenlijk 
    }

    int[] ReadFromSerialPort(int messageLength, byte startByteMessage)
    {
        //Console.WriteLine("BMS:: ReadFromSerialPort()");
        var looper = 0;
        var beginByteFound = false;
        var responseMsg = new int[messageLength];
        while (looper < messageLength)
        {
            var bitey = serialPort.ReadByte();
            if (bitey ==startByteMessage) // We zoeken een start byte
                beginByteFound = true;
            if (beginByteFound)
                responseMsg[looper++] = bitey;
        }
        return responseMsg;
    }    

    private void Jbd_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (askingBasicInfo)
            ParsebasicInfo(ReadFromSerialPort(36, startByte));
        else if (askingCellInfo)
            ParseCellInfo(ReadFromSerialPort(39, startByte));
        else Log.instance.Information($"Received some shit on the rs485 port for which I have no idea what the fuck it is --yet.");
    }

    private void ParseCellInfo(int[] responseMsg)
    {
        info.CellLow = int.MaxValue;
        info.CellHigh = int.MinValue;
        info.CellDifference = 0;
        info.CellAverage = 0;

        double cellAverage = 0;
        int numberOfCells = responseMsg[3] / 2;
        info.CellVoltages = new double[numberOfCells];

        byte offset = 4;
        for (int i = 0; i < numberOfCells; i++)
        {
            int start = (i * 2) + offset;
            int stop = start + 1;

            info.CellVoltages[i] = Math.Round( (parseNumber(responseMsg[start], responseMsg[stop])) / 1000, 4);

            cellAverage += info.CellVoltages[i];

            if (info.CellVoltages[i] > info.CellHigh)
                info.CellHigh = info.CellVoltages[i];
            if (info.CellVoltages[i] < info.CellLow)
                info.CellLow = info.CellVoltages[i];

            info.CellLow = Math.Round(info.CellLow, 4);
            info.CellHigh = Math.Round(info.CellHigh, 4);
            info.CellDifference = Math.Round(info.CellHigh - info.CellLow, 4); 
            info.CellAverage = Math.Round(cellAverage / numberOfCells, 4);
        }
        info.BMSName = Name;
        RaiseEvent();
    }

    double ParseTemp(int lowByte, int highByte)
    {
        return (parseNumber(lowByte, highByte) - 2731) / 10.00f;
    }

    double parseNumber(int high, int low)
    {
        short highbyte = (byte)high;
        short lowbyte = (byte)low;
        highbyte <<= 8; //Left shift 8 bits,

       return (highbyte | lowbyte);
    }


    private void ParsebasicInfo(int[] responseMsg)
    {
        info = new JBDRS485Info();
        info.Voltage = parseNumber(responseMsg[4], responseMsg[5]) / 100;
        //Current = intsToUint16(responseMsg[6], responseMsg[7]) / 100;
        info.Current = parseNumber(responseMsg[6], responseMsg[7]) / 100;
        info.SOC = responseMsg[23];
        info.ProtectionState = parseNumber(responseMsg[20], responseMsg[21]);
        info.Cycles = parseNumber(responseMsg[12], responseMsg[13]);

        info.Temperatures = new double[] {
          ParseTemp(responseMsg[27], responseMsg[28]),
          ParseTemp(responseMsg[29], responseMsg[30]),
          ParseTemp(responseMsg[31], responseMsg[32])
        };

        AskCellInfo();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                serialPort.DataReceived -= Jbd_DataReceived;
                serialPort.Close();
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
