using Lucent.Core;

namespace Lucent.Core;

public interface IExampleRepository
{
    ExampleModel GetById(int id);
}
