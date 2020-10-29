using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phonebook.Api.Models;
using Phonebook.Domain.ApplicationServices;
using Phonebook.Domain.ApplicationServices.Commands;
using Phonebook.Domain.ApplicationServices.Queries;
using Phonebook.Domain.Model.ValueObjects;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult<IEnumerable<ContactDetailsModel>>> Get()
        {
            try
            {
                var phonebookContacts = await _getPhonebookContactsQuery.Execute(GetUserId());

                return Ok(phonebookContacts
                    .Select(x => new ContactDetailsModel(x.Id, x.ContactName, x.ContactPhoneNumber.Value)));
            }
            catch (UserPhonebookNotFoundException)
            {
                return BadRequest();
            }
        }

        [HttpPost("contacts")]
        public async Task<ActionResult> Post([FromBody]CreateNewContactModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _createNewContactCommand.Execute(
                            new CreateNewContactDto(
                                GetUserId(),
                                model.ContactFullName,
                                new PhoneNumber(model.ContactPhoneNumber)));

                    return Ok();
                }
                catch (UserPhonebookNotFoundException)
                {
                    return BadRequest();
                }
            }

            return BadRequest(ModelState);
        }

        private int GetUserId()
        {
            return int.Parse(User.Claims.First(i => i.Type == "UserId").Value);
        }
    }
}
