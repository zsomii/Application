#region

using AutoMapper;

#endregion

namespace Application.Domain.Service
{
    public class BaseService
    {
        protected readonly IMapper Mapper;

        public BaseService(IMapper mapper)
        {
            Mapper = mapper;
        }
    }
}