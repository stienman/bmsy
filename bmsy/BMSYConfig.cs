public static class BMSYConfig
{    public static string GetConfigByKey(string key)
    {
        var setting = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).Build()[key];
        Log.instance.Information($"[CONFIG] Setting \"{key}\" returned \"{setting}\"");
        return setting;
    }

    public static List<T> GetSection<T>(string sectionName)
    {
        IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).Build();
        List<T> list = new();
        config.GetSection(sectionName).Bind(list);
        return list;
    }
}
