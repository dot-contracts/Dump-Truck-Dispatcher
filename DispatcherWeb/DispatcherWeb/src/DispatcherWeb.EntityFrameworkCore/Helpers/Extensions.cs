using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Helpers
{
    internal static class Extensions
    {
        /// <summary>
        /// Read a SQL script that is embedded as a resource named after the migration class.
        /// </summary>
        /// <param name="TMigrationSQLScriptType">The migration type the SQL file script is attached to.</param>
        /// <param name="upOrDownScaleFilePrefix">Optional parameter providing a strategy to distinguish between UP or DOWN SQL scripts.</param>
        /// <returns>The content of the SQL file.</returns>
        public static string ReadSql<MigrationType>(this MigrationType migrationType, string upOrDownScaleFilePrefix = "")
            where MigrationType : Migration
        {
            var type = typeof(MigrationType);
            var customAttribute = type.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(MigrationAttribute));
            var migrationAttributeName = customAttribute.ConstructorArguments[0].Value.ToString();

            if (!string.IsNullOrEmpty(upOrDownScaleFilePrefix))
            {
                migrationAttributeName += upOrDownScaleFilePrefix;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var sqlFile = assembly.GetManifestResourceNames().FirstOrDefault(scriptFile => scriptFile.Contains(migrationAttributeName));
            if (string.IsNullOrEmpty(sqlFile))
            {
                return string.Empty;
            }

            using var stream = assembly.GetManifestResourceStream(sqlFile);
            using StreamReader reader = new(stream);
            var sqlScript = reader.ReadToEnd();

            return sqlScript;
        }
    }
}
