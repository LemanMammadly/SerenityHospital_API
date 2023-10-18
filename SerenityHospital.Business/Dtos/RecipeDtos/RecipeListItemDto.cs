﻿using SerenityHospital.Business.Dtos.AppoinmentDtos;
using SerenityHospital.Business.Dtos.DoctorDtos;
using SerenityHospital.Business.Dtos.PatientDtos;
using SerenityHospital.Core.Entities;

namespace SerenityHospital.Business.Dtos.RecipeDtos;

public record RecipeListItemDto
{
    public int Id { get; set; }
    public AppoinmentInfoDto Appoinment { get; set; }
    public DoctorInfoDto Doctor { get; set; }
    public PatientInfoDto Patient { get; set; }
    public string RecipeDesc { get; set; }
    public bool IsDeleted { get; set; }
}


