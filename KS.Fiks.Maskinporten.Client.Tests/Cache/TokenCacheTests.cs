using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Ks.Fiks.Maskinporten.Client.Tests.Cache
{
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
            var sut = _fixture.CreateSut();

            var expectedValue = "a value";

            var actualValue = await sut.GetToken("key", () => Task.FromResult(expectedValue), (x) => TimeSpan.FromSeconds(100));

            actualValue.Should().Be(expectedValue);
        }

        [Fact]
        public async Task ReturnsValueFromCacheInSecondCallIfWithinTokenTimeLimit()
        {
            var sut = _fixture.CreateSut();

            var expectedValue = "a value";

            var firstValue = await sut.GetToken("key", () => Task.FromResult(expectedValue),
                (x) => TimeSpan.FromMinutes(10));
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var secondValue = await sut.GetToken("key", () => Task.FromResult("should not get"),
                (x) => TimeSpan.FromMinutes(10));

            secondValue.Should().Be(expectedValue);
        }

        [Fact]
        public async Task ReturnsNewValueIfCallIsOutsideTokenTimeLimit()
        {
            var sut = _fixture.CreateSut();

            var expectedValue = "a value";

            var firstValue = await sut.GetToken("key", () => Task.FromResult("should not get"),
                (x) => TimeSpan.FromMilliseconds(10));
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var secondValue = await sut.GetToken("key", () => Task.FromResult(expectedValue),
                (x) => TimeSpan.FromMilliseconds(10));

            secondValue.Should().Be(expectedValue);
        }
    }
}