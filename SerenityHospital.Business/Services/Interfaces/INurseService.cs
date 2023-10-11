﻿using SerenityHospital.Business.Dtos.NurseDtos;
using SerenityHospital.Business.Dtos.RoleDtos;
using SerenityHospital.Business.Dtos.TokenDtos;

namespace SerenityHospital.Business.Services.Interfaces;

public interface INurseService
{
    Task CreateAsync(NurseCreateDto dto);
    Task<TokenResponseDto> LoginAsync(NurseLoginDto dto);
    Task<TokenResponseDto> LoginWithRefreshTokenAsync(string refreshToken);
    Task<ICollection<NurseListItemDto>> GetAllAsync(bool takeAll);
    Task AddRole(AddRoleDto dto);
    Task RemoveRole(RemoveRoleDto dto);
}
