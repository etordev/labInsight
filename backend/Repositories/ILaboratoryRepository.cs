using LabInsight.Api.Entities;

namespace LabInsight.Api.Repositories;

public interface ILaboratoryRepository : IRepository<Laboratory>
{
    Task<IReadOnlyList<Laboratory>> ListOrderedByNameAsync(bool isDeleted, CancellationToken cancellationToken);
}
