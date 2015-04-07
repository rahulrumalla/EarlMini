using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarlMini.Core.Data
{
    public interface IRepository
    {
        bool SaveMiniUrl(string originalUrl, string fragment, string miniUrl);

        string GetOriginalUrl(string fragment);

        string GetMiniUrl(string url);

        int GetSqlBinaryCheckSum(string fragment);
    }
}
