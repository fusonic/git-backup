using System.Threading.Tasks;
using Fusonic.GitBackup.Models;

namespace Fusonic.GitBackup.Services
{
    interface IBackupStrategy
    {
        Task Backup(Repository repository);
        Task Cleanup();
    }
}