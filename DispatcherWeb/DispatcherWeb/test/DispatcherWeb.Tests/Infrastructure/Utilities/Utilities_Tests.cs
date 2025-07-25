using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Infrastructure.Utilities
{
    public class Utilities_Tests
    {
        [Fact]
        public void Test_FormatAddress_should_return_properly_formatted_address()
        {
            // Arrange
            const string address = "Palm blv.";
            const string city = "Los Angeles";
            const string state = "CA";
            const string zipCode = "90210";
            const string countryCode = "USA";

            // Act
            var result1 = DispatcherWeb.Utilities.FormatAddress(address, city, state, zipCode, countryCode);
            var result2 = DispatcherWeb.Utilities.FormatAddress("", city, state, zipCode, countryCode);
            var result3 = DispatcherWeb.Utilities.FormatAddress(address, "", state, zipCode, countryCode);

            // Assert
            result1.ShouldBe($"{address}, {city}, {state}, {zipCode}, {countryCode}");
            result2.ShouldBe($"{city}, {state}, {zipCode}, {countryCode}");
            result3.ShouldBe($"{address}, {state}, {zipCode}, {countryCode}");
        }
    }
}
