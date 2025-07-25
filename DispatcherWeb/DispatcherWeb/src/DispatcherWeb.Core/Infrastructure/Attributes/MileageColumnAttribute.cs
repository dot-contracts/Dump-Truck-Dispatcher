using System.ComponentModel.DataAnnotations.Schema;

namespace DispatcherWeb.Infrastructure.Attributes
{
    public class MileageColumnAttribute : ColumnAttribute
    {
        public MileageColumnAttribute()
        {
            TypeName = "decimal(19, 1)";
        }

    }
}
