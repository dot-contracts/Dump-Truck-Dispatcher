using Microsoft.Data.SqlClient;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.General
{
    public class ConnectionString_Tests
    {
        [Fact]
        public void SqlConnectionStringBuilder_Test()
        {
            var csb = new SqlConnectionStringBuilder("Server=localhost; Database=DispatcherWeb; Trusted_Connection=True;");
            csb["Database"].ShouldBe("DispatcherWeb");
        }
    }
}
