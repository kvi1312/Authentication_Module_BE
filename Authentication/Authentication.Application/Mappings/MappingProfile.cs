using Authentication.Application.Commands;
using Authentication.Application.Dtos;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Extensions;
using Authentication.Application.Queries;
using Authentication.Domain.Entities;
using AutoMapper;

namespace Authentication.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.Name).ToRoleTypes()))
            .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.UserRoles.Any() ? src.UserRoles.First().Role.UserType : Domain.Enums.UserType.EndUser));

        CreateMap<Role, RoleDto>();

        CreateMap<LoginRequest, LoginCommand>();
        CreateMap<RefreshTokenRequest, RefreshTokenCommand>();
        CreateMap<LogoutRequest, LogoutCommand>();
        CreateMap<RegisterRequest, RegisterCommand>();

        CreateMap<AddUserRoleRequest, AddUserRoleCommand>();
        CreateMap<RemoveUserRoleRequest, RemoveUserRoleCommand>();
        CreateMap<UpdateUserProfileRequest, UpdateUserProfileCommand>();
        CreateMap<GetUsersRequest, GetUsersQuery>();
    }
}