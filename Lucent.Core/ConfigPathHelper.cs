// File: Lucent.Core/ConfigPathHelper.cs
using System;
using System.IO;

namespace Lucent.Core
{
    public static class ConfigPathHelper
    {
        // Returns the path for appsettings.json based on the execution directory
        public static string GetSharedConfigPath()
        {
            return Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "..", "..", "..", "..", // up from bin/
                "config", "appsettings.json"));
        }
    }
}
