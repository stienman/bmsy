public class DeviceFactory
{
    public static List<IInverter> GetConfiguredInverters()
    {
        return BMSYConfig.GetSection<InverterConfigurationSetting>("Inverters").ConvertAll(config => 
            IInverter.FinishInit(FindAndCreateInstance<IInverter>(config.Type), config.Port, config.Name));
    }
    public static List<IBMS> GetConfiguredBMS()
    {
        return BMSYConfig.GetSection<InverterConfigurationSetting>("BMS").ConvertAll(config => 
            IBMS.FinishInit(FindAndCreateInstance<IBMS>(config.Type), config.Port, config.Name));
    }
    private static T? FindAndCreateInstance<T>(string baseClassName)
    {
        var target = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(alltypes => alltypes.GetTypes())
            .Where(matchingInterface => typeof(T).IsAssignableFrom(matchingInterface))
            .Where(neo => neo.Name == baseClassName).FirstOrDefault();
        if (target == null) // well shoot.
            return default(T?); // return null but written like so, because T!
        return (T?)Activator.CreateInstance(target);
    }
}

