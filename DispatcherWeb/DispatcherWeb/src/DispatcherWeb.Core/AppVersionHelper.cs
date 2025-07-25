using System;
using System.IO;
using Abp.Reflection.Extensions;

namespace DispatcherWeb
{
    /// <summary>
    /// Central point for application version.
    /// </summary>
    public class AppVersionHelper
    {
        /// <summary>
        /// Gets current version of the application.
        /// It's also shown in the web page.
        /// </summary>
        public static string Version => _version ??= typeof(AppVersionHelper).GetAssembly().GetName().Version?.ToString();

        private static string _version;

        /// <summary>
        /// Gets release (last build) date of the application.
        /// It's shown in the web page.
        /// </summary>
        public static DateTime ReleaseDate => _releaseDate ??= new FileInfo(typeof(AppVersionHelper).GetAssembly().Location).LastWriteTime;

        private static DateTime? _releaseDate;
    }
}
