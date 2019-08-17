using System;
using System.Reflection;

namespace NsqClient
{
    public static class VersionHelper
    {
        public static string Version { get; }

        static VersionHelper()
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            Version = $"{ver.Major}.{ver.Minor}.{ver.Build}";
        }
    }
}
