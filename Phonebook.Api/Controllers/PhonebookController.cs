using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phonebook.Api.Models;
using Phonebook.Domain.ApplicationServices.Commands;
using Phonebook.Domain.ApplicationServices.Queries;
using Phonebook.Domain.Model.ValueObjects;
using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PhoneBook.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("[controller]")]
    public class PhonebookController : ControllerBase
    {
        private readonly GetPhonebookContactsQuery _getPhonebookContactsQuery;
        private readonly CreateNewContactCommand _createNewContactCommand;

        public PhonebookController(
            GetPhonebookContactsQuery getPhonebookContactsQuery,
            CreateNewContactCommand createNewContactCommand)
        {
            _getPhonebookContactsQuery = getPhonebookContactsQuery;
            _createNewContactCommand = createNewContactCommand;
        }

        [HttpGet]
        public async Task<ActionResult<GetUserPhonebookResponseData>> Get()
        {
            var queryResult = await _getPhonebookContactsQuery.Execute(GetUserId());

            return Ok(new GetUserPhonebookResponseData(queryResult.Results.Select(x =>
                new GetUserPhonebookResponseData.Result(x.Id, x.ContactName, x.ContactPhoneNumber.Value))));
        }

        [HttpPost("contacts")]
        public async Task<ActionResult> Post([FromBody]PostNewContactRequestData requestData)
        {
            if (ModelState.IsValid)
            {
                await _createNewContactCommand.Execute(
                        new CreateNewContactCommand.Request(
                            GetUserId(),
                            requestData.ContactFullName!,
                            new PhoneNumber(requestData.ContactPhoneNumber!)));

                return Ok();
            }

            return BadRequest(ModelState);
        }

        private Guid GetUserId()
        {
            if (Guid.TryParse(User.Claims.FirstOrDefault(i => i.Type == "UserId")?.Value, out var userId))
            {
                return userId;
            }

            throw new AuthenticationException("UserId claim not found");
        }
    }
}
