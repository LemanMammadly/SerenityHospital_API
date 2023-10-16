﻿using SerenityHospital.Business.Dtos.DoctorDtos;
using SerenityHospital.Business.Dtos.PatientDtos;
using SerenityHospital.Core.Enums;

namespace SerenityHospital.Business.Dtos.AppoinmentDtos;

public record AppoinmentListItemDto
{
    public int Id { get; set; }
    public DoctorInfoDto Doctor { get; set; }
    public PatientInfoDto Patient { get; set; }
    public string ProblemDesc { get; set; }
    public DateTime AppoinmentDate { get; set; }
    public int Duration { get; set; }
    public AppoinmentStatus Status { get; set; }
    public decimal AppoinmentMoney { get; set; }
    public bool IsDeleted { get; set; }
}

