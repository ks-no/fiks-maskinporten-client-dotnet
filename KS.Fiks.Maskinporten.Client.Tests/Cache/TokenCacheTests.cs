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

            var expectedValue = _fixture.GetRandomToken();

            var actualValue = await sut.GetToken("key", () => Task.FromResult(expectedValue));

            actualValue.Should().Be(expectedValue);
        }

        [Fact]
        public async Task ReturnsValueFromCacheInSecondCallIfWithinTokenTimeLimit()
        {
            var sut = _fixture.CreateSut();

            var expectedValue = _fixture.GetRandomToken(10);
            var otherValue = _fixture.GetRandomToken(10);


            var firstValue = await sut.GetToken("key", () => Task.FromResult(expectedValue));
            await Task.Delay(TimeSpan.FromMilliseconds(100));
            var secondValue = await sut.GetToken("key", () => Task.FromResult(otherValue));

            secondValue.Should().Be(expectedValue);
        }

        [Fact]
        public async Task ReturnsNewValueIfCallIsOutsideTokenTimeLimit()
        {
            var sut = _fixture.CreateSut();

            var expectedValue = _fixture.GetRandomToken(1);
            var otherValue = _fixture.GetRandomToken(1);

            var firstValue = await sut.GetToken("key", () => Task.FromResult(otherValue));
            await Task.Delay(TimeSpan.FromMilliseconds(1500));
            var secondValue = await sut.GetToken("key", () => Task.FromResult(expectedValue));

            secondValue.Should().Be(expectedValue);
        }
    }
}