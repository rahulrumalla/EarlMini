using System.Web.Http;
using EarlMini.Core;

namespace EarlMini.Api.Controllers
{
    public class HomeController : ApiController
    {
        [HttpGet]
        public IHttpActionResult Index()
        {
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult Index(string url)
        {
            string miniUrl = Request.RequestUri.AbsoluteUri;

            if ( string.IsNullOrWhiteSpace( miniUrl ) )
                return NotFound();

            string originalUrl = EarlMiniProvider.ExpandUrl( miniUrl );

            if ( string.IsNullOrWhiteSpace( miniUrl ) )
                return NotFound();

            return Redirect( originalUrl );
        }
    }
}
