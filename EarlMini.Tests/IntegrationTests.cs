using System;
using EarlMini.Core;
using NUnit.Framework;

namespace EarlMini.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        [TestCase( "https://www.google.com" )]
        public void Can_Minify_Url(string originalUrl)
        {
            string miniUrl = EarlMiniProvider.MinifyUrl(originalUrl);

            Assert.IsNotNullOrEmpty(miniUrl);
            Assert.IsTrue(miniUrl.Length == 22);
        }
    }
}
