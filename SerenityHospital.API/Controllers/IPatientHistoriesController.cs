﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SerenityHospital.Business.Services.Interfaces;
using SerenityHospital.Core.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SerenityHospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IPatientHistoriesController : ControllerBase
    {
        readonly IPatientHistoryService _service;

        public IPatientHistoriesController(IPatientHistoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _service.GetAllAsync());
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }
    }
}

