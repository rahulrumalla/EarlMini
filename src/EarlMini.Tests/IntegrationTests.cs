using System;
using EarlMini.Core;
using NUnit.Framework;

namespace EarlMini.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private int ExpectedLenthOfUnsecureMiniUrl
        {
            get
            {
                return String.Format( EarlMiniProvider.UnSecureMiniUrlTemplate, EarlMiniProvider.HostName, string.Empty ).Length + EarlMiniProvider.FragmentLength;
            }
        }

        private int ExpectedLenthOfSecureMiniUrl
        {
            get
            {
                return String.Format( EarlMiniProvider.SecureMiniUrlTemplate, EarlMiniProvider.HostName, string.Empty ).Length + EarlMiniProvider.FragmentLength;
            }
        }

        [Test]
        [TestCase( "https://www.google.com" )]
        [Ignore("Integration")]
        public void Can_Minify_Url(string originalUrl)
        {
            string miniUrl = EarlMiniProvider.MinifyUrl(originalUrl);

            Assert.IsNotNullOrEmpty(miniUrl);
            Assert.IsTrue( miniUrl.Length == ExpectedLenthOfUnsecureMiniUrl );
        }
    }
}
