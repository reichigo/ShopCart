using AutoFixture;
using AutoFixture.AutoMoq;
using Xunit;

namespace ShopCart.Api.Tests;

public abstract class TestBase
{
    protected readonly IFixture Fixture;

    protected TestBase()
    {
        Fixture = new Fixture().Customize(new AutoMoqCustomization());
    }
}