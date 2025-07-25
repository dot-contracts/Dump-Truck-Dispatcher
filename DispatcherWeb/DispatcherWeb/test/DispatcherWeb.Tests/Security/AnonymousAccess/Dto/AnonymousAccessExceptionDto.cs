using System;
using System.Collections.Generic;

namespace DispatcherWeb.Tests.Security.AnonymousAccess.Dto
{
    public class AnonymousAccessExceptionDto
    {
        public AnonymousAccessExceptionDto()
        {
            ExceptionsPerAssembly = new Dictionary<string, List<string>>();
        }

        private Dictionary<string, List<string>> ExceptionsPerAssembly { get; set; }

        public AnonymousAccessExceptionDto AddException(string assemblyName, string className, string methodName, string justification)
        {
            return AddExceptions(assemblyName, new List<(string className, string methodName, string justification)>
            {
                (className, methodName, justification),
            });
        }

        public AnonymousAccessExceptionDto AddExceptions(string assemblyName, List<(string className, string methodName, string justification)> values)
        {
            if (!ExceptionsPerAssembly.ContainsKey(assemblyName))
            {
                ExceptionsPerAssembly.Add(assemblyName, new List<string>());
            }

            foreach (var (className, methodName, justification) in values)
            {
                if (string.IsNullOrEmpty(justification))
                {
                    throw new ArgumentNullException(nameof(values), $"Justification has to be specified for {className}.{methodName} exception");
                }
                ExceptionsPerAssembly[assemblyName].Add($"{className}.{methodName}");
            }

            return this;
        }

        public void Filter(string assemblyName, List<string> classAndMethodNames)
        {
            if (ExceptionsPerAssembly.ContainsKey(assemblyName))
            {
                foreach (var exception in ExceptionsPerAssembly[assemblyName])
                {
                    if (exception.EndsWith(".*"))
                    {
                        classAndMethodNames.RemoveAll(x => x.StartsWith(exception[..^1]));
                    }
                    else
                    {
                        classAndMethodNames.RemoveAll(x => x == exception);
                    }
                }
            }
        }

    }
}
