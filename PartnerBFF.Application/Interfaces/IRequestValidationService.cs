using System.ComponentModel.DataAnnotations;

namespace PartnerBFF.Application.Interfaces
{
    public interface IRequestValidationService<T>
    {
        void Validate(T request);
    }
}
