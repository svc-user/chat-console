using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace chat_console
{
    internal class SettingsHelper
    {
        internal static void ShowHelp()
        {
            Console.WriteLine(
"""
                Welcome to the chat-console!! :)

                Only a few commands are built in and all start with a forward-slash (/).
                All other messages are sent directly to OpenAI for a response.

                The built-in commands are:
                * /get <filter>
                    Show all settings matching filter. Filter is a case-insensitive string used to filter settings. 
                    The filter * shows all settings.

                * /set <setting> <value>
                    Sets a setting value. If successful the updated setting will be shown. The setting key is 
                    case-insensitive, but must match the full setting name.

                * /reset <setting>
                    Remove the value of a setting. The setting key is  case-insensitive, but must match the 
                    full setting name.

                * /clear
                    Clear the chat buffer.

                * /help
                    This message.

                * /quit or /exit
                    Close the console window.                
""");
        }


        internal static void GetSettings(Settings settings, string key)
        {
            key = key == "*" ? string.Empty : key;

            var props = settings.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name == "RequestParams") continue;
                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                if (prop.Name.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    var val = prop.GetValue(settings);
                    Console.WriteLine("system".PadRight(settings.LongestName) + $" : {prop.PropertyType.Name} {prop.Name} = '{val}'");
                }
            }
            Console.WriteLine("----");
            var chatReqProps = settings.RequestParams.GetType().GetProperties();
            foreach (var prop in chatReqProps)
            {
                if (prop.Name == "Messages") continue;
                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                if (prop.Name.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    string propertyType = prop.PropertyType.Name;
                    if (prop.PropertyType.GetGenericArguments().Length > 0)
                    {
                        propertyType = prop.PropertyType.GetGenericArguments()[0].Name;
                    }

                    var val = prop.GetValue(settings.RequestParams);
                    Console.WriteLine("system".PadRight(settings.LongestName) + $" : {propertyType} {prop.Name} = '{val}'");
                }
            }


        }

        internal static async Task SetSetting(Settings settings, string key, string value)
        {
            var props = settings.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name == "RequestParams") continue;
                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                if (prop.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    var (ut, _) = GetUnderlyingType(prop.PropertyType);
                    prop.SetValue(settings, Convert.ChangeType(value, ut));
                    await settings.SaveAsync();
                    GetSettings(settings, key);
                }
            }


            var chatReqProps = settings.RequestParams.GetType().GetProperties();
            foreach (var prop in chatReqProps)
            {
                if (prop.Name == "Messages") continue;
                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                if (prop.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    var (ut, _) = GetUnderlyingType(prop.PropertyType);
                    prop.SetValue(settings.RequestParams, Convert.ChangeType(value, ut));

                    await settings.SaveAsync();
                    GetSettings(settings, key);
                }
            }
        }

        internal static async Task ResetSetting(Settings settings, string key)
        {
            var props = settings.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name == "RequestParams") continue;
                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                if (prop.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    var (_, nullable) = GetUnderlyingType(prop.PropertyType);
                    prop.SetValue(settings, nullable ? null : default);

                    await settings.SaveAsync();
                    GetSettings(settings, key);
                }
            }


            var chatReqProps = settings.RequestParams.GetType().GetProperties();
            foreach (var prop in chatReqProps)
            {
                if (prop.Name == "Messages") continue;
                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

                if (prop.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    var (_, nullable) = GetUnderlyingType(prop.PropertyType);
                    prop.SetValue(settings.RequestParams, nullable ? null : default);

                    await settings.SaveAsync();
                    GetSettings(settings, key);
                }
            }
        }

        private static (Type, bool) GetUnderlyingType(Type type)
        {
            if (type.GetGenericArguments().Length > 0)
            {
                return (type.GetGenericArguments()[0], true);
            }

            return (type, false);
        }
    }
}
