﻿using AutoMapper;
using SerenityHospital.Business.Constants;
using SerenityHospital.Business.Dtos.PatientRoomDtos;
using SerenityHospital.Business.Exceptions.Common;
using SerenityHospital.Business.Exceptions.Images;
using SerenityHospital.Business.Exceptions.PatientRooms;
using SerenityHospital.Business.Extensions;
using SerenityHospital.Business.ExternalServices.Interfaces;
using SerenityHospital.Business.Services.Interfaces;
using SerenityHospital.Core.Entities;
using SerenityHospital.DAL.Repositories.Interfaces;

namespace SerenityHospital.Business.Services.Implements;

public class PatientRoomService : IPatientRoomService
{
    readonly IPatientRoomRepository _repo;
    readonly IDepartmentRepository _depRepo;
    readonly IMapper _mapper;
    readonly IFileService _fileService;

    public PatientRoomService(IPatientRoomRepository repo, IMapper mapper, IDepartmentRepository depRepo, IFileService fileService)
    {
        _repo = repo;
        _mapper = mapper;
        _depRepo = depRepo;
        _fileService = fileService;
    }

    public async Task CreateAsync(PatientRoomCreateDto dto)
    {
        if (await _repo.IsExistAsync(pr => pr.Number == dto.Number)) throw new ThisRoomNumberIsAlreadyExistException();

        if (!dto.ImageFile.IsSizeValid(3)) throw new SizeNotValidException();
        if (!dto.ImageFile.IsTypeValid("image")) throw new TypeNotValidException();

        var department =await _depRepo.GetByIdAsync(dto.DepartmentId);
        if (department is null) throw new NotFoundException<Department>();

        var patientRoom = _mapper.Map<PatientRoom>(dto);
        patientRoom.ImageUrl = await _fileService.UploadAsync(dto.ImageFile, RootConstant.PatientRoomtImageRoot);

        await _repo.CreateAsync(patientRoom);
        await _repo.SaveAsync();
    }

    public async Task DeleteAsync(int id)
    {
        if (id <= 0) throw new NegativeIdException<PatientRoom>();
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) throw new NotFoundException<PatientRoom>();

        _repo.Delete(entity);
        _fileService.Delete(entity.ImageUrl);
        await _repo.SaveAsync();
    }

    public async Task<IEnumerable<PatientRoomListItemDto>> GetAllAsync(bool takeAll)
    {
        if(takeAll)
        {
            return _mapper.Map<IEnumerable<PatientRoomListItemDto>>(_repo.GetAll());
        }
        else
        {
            return _mapper.Map<IEnumerable<PatientRoomListItemDto>>(_repo.FindAll(pr => pr.IsDeleted == false));
        }
    }

    public async Task<PatientRoomDetailItemDto> GetByIdAsync(int id, bool takeAll)
    {
        if (id <= 0) throw new NegativeIdException<PatientRoom>();

        PatientRoom? entity;

        if(takeAll)
        {
            entity =await _repo.GetByIdAsync(id);
            if (entity is null) throw new NotFoundException<PatientRoom>();
        }
        else
        {
            entity = await _repo.GetSingleAsync(pr => pr.IsDeleted == false && pr.Id==id);
            if (entity is null) throw new NotFoundException<PatientRoom>();
        }

        return _mapper.Map<PatientRoomDetailItemDto>(entity);
    }

    public async Task RevertSoftDeleteAsync(int id)
    {
        if (id <= 0) throw new NegativeIdException<PatientRoom>();
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) throw new NotFoundException<PatientRoom>();

        _repo.SoftDelete(entity);
        await _repo.SaveAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        if (id <= 0) throw new NegativeIdException<PatientRoom>();
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) throw new NotFoundException<PatientRoom>();

        _repo.SoftDelete(entity);
        await _repo.SaveAsync();
    }

    public async Task UpdateAsync(int id, PatientRoomUpdateDto dto)
    {
        if (id <= 0) throw new NegativeIdException<PatientRoom>();
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) throw new NotFoundException<PatientRoom>();

        if (await _repo.IsExistAsync(pr => pr.Number == dto.Number && pr.Id != id)) throw new ThisRoomNumberIsAlreadyExistException();

        if (dto.ImageFile!=null)
        {
            _fileService.Delete(entity.ImageUrl);

            if (!dto.ImageFile.IsSizeValid(3)) throw new SizeNotValidException();
            if (!dto.ImageFile.IsTypeValid("image")) throw new TypeNotValidException();
            entity.ImageUrl = await _fileService.UploadAsync(dto.ImageFile, RootConstant.PatientRoomtImageRoot);
        }

        var department =await _depRepo.GetByIdAsync(dto.DepartmentId);
        if (department is null) throw new NotFoundException<Department>();

        entity.DepartmentId = dto.DepartmentId;

        _mapper.Map(dto, entity);
        await _repo.SaveAsync();
    }
}

