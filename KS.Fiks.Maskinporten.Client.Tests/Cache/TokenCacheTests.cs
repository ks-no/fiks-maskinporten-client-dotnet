using System;
using System.Threading.Tasks;
using KS.Fiks.Maskinporten.Client;
using Shouldly;
using Xunit;

namespace Ks.Fiks.Maskinporten.Client.Tests.Cache;

public class TokenCacheTests
{
    private TokenCacheFixture _fixture;

    public TokenCacheTests()
    {
        _fixture = new TokenCacheFixture();
    }

    [Fact]
    public async Task ReturnsValueFromGetterAtFirstCall()
    {
        var sut = TokenCacheFixture.CreateSut();

        var expectedValue = _fixture.GetRandomToken();

        var actualValue = await sut.GetToken(new TokenRequest { Scopes = "key"}, () => Task.FromResult(expectedValue)).ConfigureAwait(false);

        actualValue.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task ReturnsValueFromCacheInSecondCallIfWithinTokenTimeLimit()
    {
        var sut = TokenCacheFixture.CreateSut();

        var expectedValue = _fixture.GetRandomToken(10);
        var otherValue = _fixture.GetRandomToken(10);

        var firstValue = await sut.GetToken(new TokenRequest { Scopes = "key"}, () => Task.FromResult(expectedValue)).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
        var secondValue = await sut.GetToken(new TokenRequest { Scopes = "key"}, () => Task.FromResult(otherValue)).ConfigureAwait(false);

        secondValue.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task ReturnsNewValueIfCallIsOutsideTokenTimeLimit()
    {
        var sut = TokenCacheFixture.CreateSut();

        var expectedValue = _fixture.GetRandomToken(1);
        var otherValue = _fixture.GetRandomToken(1);

        var firstValue = await sut.GetToken(new TokenRequest { Scopes = "key"}, () => Task.FromResult(otherValue)).ConfigureAwait(false);
        await Task.Delay(TimeSpan.FromMilliseconds(1500)).ConfigureAwait(false);
        var secondValue = await sut.GetToken(new TokenRequest { Scopes = "key"}, () => Task.FromResult(expectedValue)).ConfigureAwait(false);

        secondValue.ShouldBe(expectedValue);
    }
}