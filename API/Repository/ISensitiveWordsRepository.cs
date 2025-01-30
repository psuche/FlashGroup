using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Repository
{
    public interface ISensitiveWordsRepository
    {
        Task<IEnumerable<string>> GetAllAsync();
        Task<string> GetByIdAsync(int id);
        Task<int> CreateAsync(string word);
        Task<int> UpdateAsync(int id, string word);
        Task<int> DeleteAsync(int id);
        Task<bool> TestConnectionAsync();

        event EventHandler DataChanged;
    }
}
