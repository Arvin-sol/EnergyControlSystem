using Microsoft.Extensions.Configuration;


namespace Common.Utilities;
public class ConfigurationManager
{
    public static IConfiguration Configuration { private get; set; }

    public static string GetConnectionString(string index) => Configuration.GetConnectionString(index);

    public static string GetValue(string index) => GetValue<string>(index);

    public static T GetValue<T>(string key) => Configuration.GetValue<T>(key);
}

