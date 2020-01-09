#region

using System.Collections.Generic;
using System.Reflection.Emit;
using Application.Data.Dto.Log;
using Application.Data.Entity.Log;
using AutoMapper;

#endregion

namespace Application.Infrastructure
{
    public sealed class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Log, LogDto>();
        }
    }

}