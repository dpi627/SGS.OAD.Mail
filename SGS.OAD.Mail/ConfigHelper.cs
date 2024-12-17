using System.ComponentModel;
using System.Reflection;
using System.Xml;

namespace SGS.OAD.Mail //namespace 要配合組態檔 config.xml
{
    public static class ConfigHelper
    {
        private static readonly Lazy<Dictionary<string, string>> _config = new Lazy<Dictionary<string, string>>(() =>
        {
            return LoadConfig();
        });

        /// <summary>
        /// Get string value from config
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key) => GetValue<string>(key);

        /// <summary>
        /// Get value from config, and convert to T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static T GetValue<T>(string key)
        {
            if (_config.Value.TryGetValue(key, out var value))
            {
                try
                {
                    if (typeof(T).IsEnum)
                    {
                        return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
                    }
                    else if (typeof(T) == typeof(Guid))
                    {
                        return (T)(object)Guid.Parse(value);
                    }
                    else if (typeof(T) == typeof(TimeSpan))
                    {
                        return (T)(object)TimeSpan.Parse(value);
                    }
                    else if (typeof(T) == typeof(Uri))
                    {
                        return (T)(object)new Uri(value);
                    }
                    else
                    {
                        var converter = TypeDescriptor.GetConverter(typeof(T));
                        if (converter != null && converter.CanConvertFrom(typeof(string)))
                        {
                            return (T)converter.ConvertFromInvariantString(value);
                        }
                        else
                        {
                            return (T)Convert.ChangeType(value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Can't convert '{value}' to '{typeof(T).Name}'。", ex);
                }
            }
            else
            {
                throw new KeyNotFoundException($"Can't found '{key}'。");
            }
        }

        /// <summary>
        /// Get value from config, and convert to T, if failed, return default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetValue<T>(string key, T defaultValue) where T : struct
        {
            if (Enum.TryParse(GetValue(key), true, out T value))
                return value;
            else
            {
                Console.WriteLine($"Can't parse {key}, keep default setting: {defaultValue}");
                return defaultValue;
            }
        }


        /// <summary>
        /// Load config from embedded resource, convert to dictionary
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private static Dictionary<string, string> LoadConfig()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // 注意 namespace 要配合組態檔 config.xml
            var resourceName = $"{typeof(ConfigHelper).Namespace}.config.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
                }

                var config = new Dictionary<string, string>();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);

                XmlNode root = xmlDoc.DocumentElement;
                foreach (XmlNode node in root.ChildNodes)
                {
                    config[node.Name] = node.InnerText;
                }

                return config;
            }
        }
    }
}
