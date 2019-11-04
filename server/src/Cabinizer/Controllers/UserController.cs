﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Cabinizer.Data;
using Cabinizer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cabinizer.Controllers
{
    [Route("users")]
    public class UserController : ApiController
    {
        public UserController(CabinizerContext context)
        {
            Context = context;
            MapToModel = x => new UserModel
            {
                Id = x.Id,
                Email = x.Email,
                FullName = x.FullName,
                GivenName = x.GivenName,
                PictureUrl = x.PictureUrl,
                FamilyName = x.FamilyName,
                PhoneNumber = x.PhoneNumber,
                OrganizationUnitPath = x.OrganizationUnitPath,
            };
        }

        private CabinizerContext Context { get; }

        private Expression<Func<User, UserModel>> MapToModel { get; }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUserById([FromRoute] string id, CancellationToken cancellationToken)
        {
            if (string.Equals(id, "me", StringComparison.OrdinalIgnoreCase))
            {
                // Add a quick way to retrieve the current user information.
                return RedirectToAction(nameof(GetUserById), new { id = User.Id });
            }

            var user = await Context.Users
                .Select(MapToModel)
                .FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetAllUsers([FromQuery] UserQueryModel model, CancellationToken cancellationToken)
        {
            var query = Context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(model.OrgUnitPath))
            {
                query = query.Where(x => x.OrganizationUnitPath.StartsWith(model.OrgUnitPath));
            }

            var users = await query
                .OrderBy(x => x.FamilyName)
                .ThenBy(x => x.GivenName)
                .Select(MapToModel)
                .ToListAsync(cancellationToken);

            return Ok(users);
        }
    }
}