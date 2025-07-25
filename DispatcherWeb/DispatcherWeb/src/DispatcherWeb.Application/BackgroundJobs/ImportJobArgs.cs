using System;
using Abp;
using DispatcherWeb.Imports;

namespace DispatcherWeb.BackgroundJobs
{
    [Serializable]
    public class ImportJobArgs
    {
        public UserIdentifier RequestorUser { get; set; }
        public string File { get; set; }
        public FieldMapItem[] FieldMap { get; set; }
        public ImportType ImportType { get; set; }
        public bool JacobusEnergy { get; set; }
    }
}
