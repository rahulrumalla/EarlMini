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
