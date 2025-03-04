using System.Threading.Tasks;

namespace EduAIAPI.Services
{
    public interface ICohereService
    {
        Task<string> GetResponseAsync(string prompt);
    }
}