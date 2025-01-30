using Dapper;
using System.Data;

namespace API.Repository
{
    public class SensitiveWordsRepository : ISensitiveWordsRepository
    {

        private readonly IDbConnection _dbConnection;

        public event EventHandler DataChanged;

        public SensitiveWordsRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<string>> GetAllAsync()
        {
            var sql = "SELECT Word FROM SensitiveWords";
            return await _dbConnection.QueryAsync<string>(sql);
        }

        public async Task<string> GetByIdAsync(int id)
        {
            var sql = "SELECT Word FROM SensitiveWords WHERE Id = @Id";
            return await _dbConnection.QueryFirstOrDefaultAsync<string>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(string word)
        {
            var sql = "INSERT INTO SensitiveWords (Word) OUTPUT INSERTED.ID VALUES (@Word)";

            //var fdsdf = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Word = word });
            var result = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Word = word });
            OnDataChanged();
            return result;
            //return await _dbConnection.ExecuteAsync(sql, new { Word = word });
        }

        public async Task<int> UpdateAsync(int id, string word)
        {
            var sql = "UPDATE SensitiveWords SET Word = @Word WHERE Id = @Id";
            var result = await _dbConnection.ExecuteAsync(sql, new { Id = id, Word = word });
            OnDataChanged();
            return result;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var sql = "DELETE FROM SensitiveWords WHERE Id = @Id";
            var result = await _dbConnection.ExecuteAsync(sql, new { Id = id });
            OnDataChanged();
            return result;
        }

        protected virtual void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                _dbConnection.Open();
                _dbConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                var brrrruuuuh = ex;
                return false;
            }
        }
    }
}
