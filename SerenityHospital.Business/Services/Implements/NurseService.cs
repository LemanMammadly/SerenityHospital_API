﻿using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SerenityHospital.Business.Constants;
using SerenityHospital.Business.Dtos.DepartmentDtos;
using SerenityHospital.Business.Dtos.NurseDtos;
using SerenityHospital.Business.Dtos.RoleDtos;
using SerenityHospital.Business.Dtos.TokenDtos;
using SerenityHospital.Business.Exceptions.Common;
using SerenityHospital.Business.Exceptions.Images;
using SerenityHospital.Business.Exceptions.Roles;
using SerenityHospital.Business.Exceptions.Tokens;
using SerenityHospital.Business.Extensions;
using SerenityHospital.Business.ExternalServices.Interfaces;
using SerenityHospital.Business.Services.Interfaces;
using SerenityHospital.Core.Entities;
using SerenityHospital.Core.Enums;
using SerenityHospital.DAL.Repositories.Interfaces;

namespace SerenityHospital.Business.Services.Implements;

public class NurseService : INurseService
{
    readonly UserManager<Nurse> _userManager;
    readonly UserManager<AppUser> _appUserManager;
    readonly RoleManager<IdentityRole> _roleManager;
    readonly IDepartmentRepository _departmentRepository;
    readonly IMapper _mapper;
    readonly IFileService _fileService;
    readonly ITokenService _tokenService;

    public NurseService(UserManager<Nurse> userManager, UserManager<AppUser> appUserManager, IDepartmentRepository departmentRepository, IMapper mapper, IFileService fileService, ITokenService tokenService, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _appUserManager = appUserManager;
        _departmentRepository = departmentRepository;
        _mapper = mapper;
        _fileService = fileService;
        _tokenService = tokenService;
        _roleManager = roleManager;
    }

    public async Task AddRole(AddRoleDto dto)
    {
        if (!await _roleManager.RoleExistsAsync(dto.roleName)) throw new NotFoundException<IdentityRole>();

        var user = await _userManager.FindByNameAsync(dto.userName);
        if (user == null) throw new AppUserNotFoundException<Nurse>();

        var result = await _userManager.AddToRoleAsync(user, dto.roleName);

        if(!result.Succeeded)
        {
            string a = " ";
            foreach (var err in result.Errors)
            {
                a += err.Description + " ";
            }
            throw new RoleCreatedFailedException(a);
        }
    }

    public async Task CreateAsync(NurseCreateDto dto)
    {
        if(dto.ImageFile != null)
        {
            if (!dto.ImageFile.IsSizeValid(3)) throw new SizeNotValidException();
            if (!dto.ImageFile.IsTypeValid("image")) throw new TypeNotValidException();
        }

        if (await _userManager.Users.AnyAsync(u => u.UserName == dto.UserName || u.Email == dto.Email)) throw new AppUserIsAlreadyExistException<Nurse>();
        if (await _appUserManager.Users.AnyAsync(a => a.UserName == dto.UserName || a.Email == dto.Email)) throw new AppUserIsAlreadyExistException<Nurse>();

        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId);
        if (department is null) throw new NotFoundException<Department>();

        var nurse = _mapper.Map<Nurse>(dto);

        nurse.DepartmentId = department.Id;
        nurse.Status = WorkStatus.Active;

        if(dto.ImageFile != null)
        {
            nurse.ImageUrl = await _fileService.UploadAsync(dto.ImageFile, RootConstant.NurseImageRoot);
        }

        var result = await _userManager.CreateAsync(nurse,dto.Password);
        if(!result.Succeeded)
        {
            string a = " ";
            foreach (var err in result.Errors)
            {
                a += err.Description + " ";
            }
            throw new RegisterFailedException<Nurse>();
        }
    }

    public async Task<ICollection<NurseListItemDto>> GetAllAsync(bool takeAll)
    {
        ICollection<NurseListItemDto> nurses = new List<NurseListItemDto>();

        if(takeAll)
        {
            foreach (var user in await _userManager.Users.Include(u=>u.Department).ToListAsync())
            {
                var userDto = new NurseListItemDto()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Age = user.Age,
                    UserName = user.UserName,
                    Email = user.Email,
                    ImageUrl = user.ImageUrl,
                    StartWork = user.StartWork,
                    EndWork = user.EndWork,
                    IsDeleted = user.IsDeleted,
                    Department = _mapper.Map<DepartmentInfoDto>(user.Department),
                    Roles = await _userManager.GetRolesAsync(user)
                };
                nurses.Add(userDto);
            }
            return nurses;
        }
        else {
            foreach (var user in await _userManager.Users.Include(u=>u.Department).Where(u=>u.IsDeleted==false).ToListAsync())
            {
                var userDto = new NurseListItemDto()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Age = user.Age,
                    UserName = user.UserName,
                    Email = user.Email,
                    ImageUrl = user.ImageUrl,
                    StartWork = user.StartWork,
                    EndWork = user.EndWork,
                    IsDeleted = user.IsDeleted,
                    Department = _mapper.Map<DepartmentInfoDto>(user.Department),
                    Roles=await _userManager.GetRolesAsync(user)
                };
                nurses.Add(userDto);
            }
            return nurses;
        }
    }

    public async Task<TokenResponseDto> LoginAsync(NurseLoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.UserName);
        if (user is null) throw new LoginFailedException<Nurse>("Username or password is wrong");

        var result = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!result) throw new LoginFailedException<Nurse>("Username or password is wrong");

        return _tokenService.CreateNurseToken(user);
    }

    public async Task<TokenResponseDto> LoginWithRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) throw new ArgumentIsNullException();
        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshToken == refreshToken);
        if (user is null) throw new NotFoundException<Nurse>();
        if (user.RefreshTokenExpiresDate < DateTime.UtcNow.AddHours(4)) throw new RefreshTokenExpiresIsOldException();
        return _tokenService.CreateNurseToken(user);
    }

    public async Task RemoveRole(RemoveRoleDto dto)
    {
        if (!await _roleManager.RoleExistsAsync(dto.roleName)) throw new NotFoundException<IdentityRole>();

        var user = await _userManager.FindByNameAsync(dto.userName);
        if (user is null) throw new NotFoundException<Nurse>();

        var result = await _userManager.RemoveFromRoleAsync(user, dto.roleName);

        if(!result.Succeeded)
        {
            string a = " ";
            foreach (var err in result.Errors)
            {
                a += err.Description + " ";
            }
            throw new RoleRemoveFailedException();
        }
    }
}
