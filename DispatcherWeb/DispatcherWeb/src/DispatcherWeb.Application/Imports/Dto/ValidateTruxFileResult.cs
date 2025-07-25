using System.Collections.Generic;

namespace DispatcherWeb.Imports.Dto
{
    public class ValidateTruxFileResult
    {
        public bool IsValid { get; set; }
        public List<int> DuplicateShiftAssignments { get; set; } = new List<int>();
        public List<int> DuplicateShiftAssignmentsInFile { get; set; } = new List<int>();
        public int TotalRecordCount { get; internal set; }
    }
}
