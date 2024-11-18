using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Models;
using PhoneBook.Services;
using System.Threading.Tasks;

namespace PhoneBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ContactService _contactService;
        private readonly OpenAiService _openAiService;

        public ContactController(ContactService contactService, OpenAiService openAiService)
        {
            _contactService = contactService;
            _openAiService = openAiService;
        }

        [HttpPost("command")]
        public async Task<IActionResult> ProcessCommand([FromBody] string command)
        {
            
            var response = await _openAiService.GetResponseFromLLM(command);
            
            if (response.Contains("add"))
            {
                var name = ExtractName(response);
                var phone = ExtractPhoneNumber(response);

                if (name != null && phone != null)
                {
                    await _contactService.AddContactAsync(new Contact { Name = name, PhoneNumber = phone });
                    return Ok($"Added {name} with phone number {phone}.");
                }
                return BadRequest("Could not parse name or phone number.");
            }
            else if (response.Contains("retrieve"))
            {
                var name = ExtractName(response);
                var contact = await _contactService.GetContactByNameAsync(name);
                return contact != null ? Ok(contact) : NotFound($"Contact {name} not found.");
            }
            else if (response.Contains("delete"))
            {
                var name = ExtractName(response);
                var contact = await _contactService.GetContactByNameAsync(name);
                if (contact != null)
                {
                    await _contactService.DeleteContactAsync(contact.Id);
                    return Ok($"Deleted {name}.");
                }
                return NotFound($"Contact {name} not found.");
            }

            return BadRequest("Command not recognized.");
        }


        private string ExtractName(string text)
        {
            var namePattern = new Regex(@"Name:\s*([A-Za-z\s]+)", RegexOptions.IgnoreCase);
            var match = namePattern.Match(text);
            return match.Success ? match.Groups[1].Value.Trim() : "Unknown Name";
            
        }

        private string ExtractPhoneNumber(string text)
        {
            var phonePattern = new Regex(@"Phone:\s*([\d\-]+)", RegexOptions.IgnoreCase);
            var match = phonePattern.Match(text);
            return match.Success ? match.Groups[1].Value.Trim() : "No Phone";
        }
    }
}
