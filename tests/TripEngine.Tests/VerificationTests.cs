using System;
using Xunit;
using TripEngine.Core;

namespace TripEngine.Tests
{
    public class VerificationTests
    {
        private readonly ScalarTripcodeBackend _backend = new ScalarTripcodeBackend();

        [Theory]
        [InlineData("12345678", "WBRXcNtpf.")]
        [InlineData("testkey", "VyFhpBGgO2")]
        [InlineData("abcdefgh", "/Pbzx9FKd2")]
        public void TestDESGroundTruthTripcodes(string key, string expectedTripcode)
        {
            string actualTripcode = _backend.GenerateTripcode(key, isSha1: false);
            Assert.Equal(expectedTripcode, actualTripcode);
        }

        [Theory]
        [InlineData("123456789012", "jZk8zfYo4m4X")]
        [InlineData("abcdefghijkl", "60YIzr/P1N.B")]
        public void TestSHA1GroundTruthTripcodes(string key, string expectedTripcode)
        {
            string actualTripcode = _backend.GenerateTripcode(key, isSha1: true);
            Assert.Equal(expectedTripcode, actualTripcode);
        }

        [Fact]
        public void TestShiftJISAndSpecialCharacters()
        {
            string sjisKey = "テスト";
            string tripcodeDes = _backend.GenerateTripcode(sjisKey, isSha1: false);
            Assert.NotNull(tripcodeDes);
            Assert.Equal(10, tripcodeDes.Length);

            string tripcodeSha1 = _backend.GenerateTripcode(sjisKey, isSha1: true);
            Assert.NotNull(tripcodeSha1);
            Assert.Equal(12, tripcodeSha1.Length);
        }
    }
}
