using DispatcherWeb.Auditing;
using Shouldly;
using Xunit;

namespace DispatcherWeb.Tests.Auditing
{
    public class NamespaceStripper_Tests : AppTestBase
    {
        private readonly INamespaceStripper _namespaceStripper;

        public NamespaceStripper_Tests()
        {
            _namespaceStripper = Resolve<INamespaceStripper>();
        }

        [Fact]
        public void Should_Stripe_Namespace()
        {
            var controllerName = _namespaceStripper.StripNameSpace("DispatcherWeb.Web.Controllers.HomeController");
            controllerName.ShouldBe("HomeController");
        }

        [Theory]
        [InlineData("DispatcherWeb.Auditing.GenericEntityService`1[[DispatcherWeb.Storage.BinaryObject, DispatcherWeb.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null]]", "GenericEntityService<BinaryObject>")]
        [InlineData("CompanyName.ProductName.Services.Base.EntityService`6[[CompanyName.ProductName.Entity.Book, CompanyName.ProductName.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],[CompanyName.ProductName.Services.Dto.Book.CreateInput, N...", "EntityService<Book, CreateInput>")]
        [InlineData("DispatcherWeb.Auditing.XEntityService`1[DispatcherWeb.Auditing.AService`5[[DispatcherWeb.Storage.BinaryObject, DispatcherWeb.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],[DispatcherWeb.Storage.TestObject, DispatcherWeb.Core, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null],]]", "XEntityService<AService<BinaryObject, TestObject>>")]
        public void Should_Stripe_Generic_Namespace(string serviceName, string result)
        {
            var genericServiceName = _namespaceStripper.StripNameSpace(serviceName);
            genericServiceName.ShouldBe(result);
        }
    }
}
