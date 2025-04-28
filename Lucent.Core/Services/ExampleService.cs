using Lucent.Core.Models;
using Lucent.Core.Repositories;

namespace Lucent.Core.Services;

public class ExampleService
{
    private readonly IExampleRepository _repository;

    public ExampleService(IExampleRepository repository)
    {
        _repository = repository;
    }

    public ExampleModel GetExampleById(int id)
    {
        var example = _repository.GetById(id);

        if (example is null)
            throw new ArgumentException($"No Example found with Id {id}");

        return example;
    }
}
