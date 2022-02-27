
try
{
    Orchestrator.instance.Start();
    new CancellationTokenSource().Token.WaitHandle.WaitOne(); 
}
catch (Exception ex)
{
    Log.instance.Error($"Exception trapped in Main Startup Routine! {ex.ToString()}");
}


 