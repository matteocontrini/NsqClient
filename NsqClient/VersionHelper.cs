using System;
using System.Reflection;

namespace NsqClient
{
    internal static class VersionHelper
    {
        internal static string Version { get; }

        static VersionHelper()
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            Version = $"{ver.Major}.{ver.Minor}.{ver.Build}";
        }
    }
}
