using BusinessSuite.Domain.Entities;
using FluentAssertions;

namespace BusinessSuite.UnitTests.Domain;

public class CustomerTests
{
    [Fact]
    public void Create_Should_Normalize_Email_And_Trim_Fields()
    {
        var customer = Customer.Create(
            "  Acme Ltd  ",
            "  Billing@Acme.CO.UK  ",
            "  +44 20 1234  ",
            "  Acme  ");

        customer.Name.Should().Be("Acme Ltd");
        customer.Email.Should().Be("billing@acme.co.uk");
        customer.Phone.Should().Be("+44 20 1234");
        customer.CompanyName.Should().Be("Acme");
        customer.IsActive.Should().BeTrue();
        customer.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Update_Should_Refresh_UpdatedAtUtc()
    {
        var customer = Customer.Create("Old Name", "old@example.com");
        var before = customer.UpdatedAtUtc;

        customer.Update("New Name", "new@example.com", null, "New Co", false);

        customer.Name.Should().Be("New Name");
        customer.Email.Should().Be("new@example.com");
        customer.IsActive.Should().BeFalse();
        customer.UpdatedAtUtc.Should().NotBeNull();
        customer.UpdatedAtUtc.Should().NotBe(before);
    }
}
