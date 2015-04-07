using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UrlMini.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        [TestCase( "https://www.google.com" )]
        public void Can_Minify_Url(string originalUrl)
        {
            Uri miniUrl = EarlMiniProvider.MinifyUrl(originalUrl);

            Assert.IsNotNull(miniUrl);
            Assert.IsNotNullOrEmpty(miniUrl.AbsoluteUri);
            Assert.IsTrue(miniUrl.AbsoluteUri.Length == 22);
        }
    }
}
