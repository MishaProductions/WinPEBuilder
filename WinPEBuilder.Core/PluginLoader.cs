using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WinPEBuilder.Core
{
    public static class PluginLoader
    {
        public static IPlugin[] GetPlugins()
        {
            var x = new List<IPlugin>();
            foreach (var item in Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFrom(item);
                    foreach (var type in asm.ExportedTypes)
                    {
                        if (type.GetInterface(nameof(IPlugin)) != null)
                        {
                            try
                            {
                                var plugin = (IPlugin?)Activator.CreateInstance(type);
                                if (plugin == null)
                                {
                                    throw new Exception("Activator.CreateInstance return null");
                                }
                                x.Add(plugin);
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Loading plugin {item} failed: {ex}");
                }
            }
            return x.ToArray();
        }
    }
}
