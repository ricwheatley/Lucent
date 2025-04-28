using Lucent.Core.Models;

namespace Lucent.Core.Repositories;

public interface IExampleRepository
{
    ExampleModel GetById(int id);
}
