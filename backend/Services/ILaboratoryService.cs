using LabInsight.Api.DTOs;

namespace LabInsight.Api.Services;

public interface ILaboratoryService
{
    Task<IReadOnlyList<LaboratoryDto>> GetLaboratoriesAsync(bool isDeleted, CancellationToken cancellationToken);
}
