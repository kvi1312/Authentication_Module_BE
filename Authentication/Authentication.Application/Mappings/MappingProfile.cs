using Authentication.Application.Commands;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Request;
using Authentication.Domain.Entities;
using AutoMapper;

namespace Authentication.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.Ignore())
            .ForMember(dest => dest.UserType, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<Role, RoleDto>();

        // Request to Command mappings
        CreateMap<LoginRequest, LoginCommand>();

        CreateMap<RefreshTokenRequest, RefreshTokenCommand>();

        CreateMap<LogoutRequest, LogoutCommand>();

        CreateMap<RegisterRequest, RegisterCommand>();

        // Additional mappings for complex scenarios
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserRoles.FirstOrDefault().Role.UserType));
    }
}