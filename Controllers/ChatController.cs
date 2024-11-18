using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using PhoneBook.Services;
using PhoneBook.Data;
using PhoneBook.Models;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly OpenAiService _openAiService;
    private readonly PhoneBookContext _context;

    public ChatController(OpenAiService openAiService, PhoneBookContext context)
    {
        _openAiService = openAiService;
        _context = context;
    }

    [HttpGet("welcome")]
    public IActionResult Welcome()
    {
        return Ok("Hello! I'm your Phone Book assistant. I can add, find, edit, and delete contacts. Just tell me what you need!");
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskLLM([FromBody] string prompt)
    {
        if (string.IsNullOrEmpty(prompt))
        {
            return BadRequest("Prompt cannot be empty.");
        }

        var structuredPrompt = $"{prompt}\nPlease respond with: Name: [name], Phone: [phone] if adding, editing, deleting and finding a contact, or 'No Phone' if not adding, editing, deleting and finding a contact.";
        Console.WriteLine($"Structured Prompt Sent: {structuredPrompt}");
        
        var response = await _openAiService.GetResponseFromLLM(structuredPrompt);
        Console.WriteLine($"LLM Response: {response}");

        if (prompt.ToLower().Contains("add"))
        {
            return await AddContact(response);
        }
        else if (prompt.ToLower().Contains("find") || prompt.ToLower().Contains("phone number for"))
        {
            return FindContact(prompt);
        }
        else if (prompt.ToLower().Contains("delete"))
        {
            return DeleteContact(prompt);
        }
        else if (prompt.ToLower().Contains("edit") || prompt.ToLower().Contains("update") || prompt.ToLower().Contains("replace"))
        {
            return await EditContact(response);
        }else if (prompt.ToLower().Contains("show all") || prompt.ToLower().Contains("see") || prompt.ToLower().Contains("show"))
        {
            return await GetAllContacts();
        }

        return BadRequest("Invalid action or command format.");
    }


    private async Task<IActionResult> AddContact(string response)
    {
        var (name, phone) = ExtractNameAndPhone(response);
        Console.WriteLine($"Extracted Name: {name}, Extracted Phone: {phone}");

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
        {
            return BadRequest("Failed to add contact. Ensure both name and phone number are provided.");
        }

        
        var existingContact = _context.Contacts.FirstOrDefault(c => c.PhoneNumber == phone);
        if (existingContact != null)
        {
            Console.WriteLine("Duplicate phone number found.");
            return Ok("There is already someone with the same phone number.");
        }

        var contact = new Contact { Name = name, PhoneNumber = phone };
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();
        Console.WriteLine($"Contact {name} added with phone number {phone}.");
        return Ok($"Contact {name} added with phone number {phone}.");
    }

    
    private IActionResult FindContact(string prompt)
    {
        var nameToFind = ExtractNameFromQuery(prompt);
        Console.WriteLine($"Searching for contact with name: {nameToFind}");

        var contact = _context.Contacts
                              .AsEnumerable()
                              .FirstOrDefault(c => c.Name.Equals(nameToFind, StringComparison.OrdinalIgnoreCase));
        
        if (contact != null)
        {
            Console.WriteLine($"Contact found: {contact.Name} with phone number: {contact.PhoneNumber}");
            return Ok($"The phone number for {contact.Name} is {contact.PhoneNumber}.");
        }
        
        Console.WriteLine($"Contact {nameToFind} was not found.");
        return NotFound($"Contact {nameToFind} was not found.");
    }

    
    private IActionResult DeleteContact(string prompt)
    {
        
        var nameToDelete = ExtractNameFromDeletePrompt(prompt);
        Console.WriteLine($"Attempting to delete contact with extracted name: '{nameToDelete}'");

        if (string.IsNullOrEmpty(nameToDelete) || nameToDelete == "Unknown Name")
        {
            Console.WriteLine("Name could not be extracted from the prompt.");
            return BadRequest("Could not determine the name to delete. Please specify a valid name.");
        }

        
        var contact = _context.Contacts
            .AsEnumerable()
            .FirstOrDefault(c => c.Name.Equals(nameToDelete, StringComparison.OrdinalIgnoreCase));

        if (contact != null)
        {
            _context.Contacts.Remove(contact);
            _context.SaveChanges();
            Console.WriteLine($"Contact '{nameToDelete}' was successfully deleted.");
            return Ok($"Contact {nameToDelete} was deleted.");
        }
    
        Console.WriteLine($"Contact '{nameToDelete}' was not found in the database.");
        return NotFound($"Contact {nameToDelete} was not found.");
    }


    private string ExtractNameFromDeletePrompt(string query)
    {
        
        var nameRegex = new Regex(@"(?:delete|remove)\s+(?:the contact for\s*)?([A-Za-z\s]+)", RegexOptions.IgnoreCase);
        var nameMatch = nameRegex.Match(query);

        
        var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "Unknown Name";
        Console.WriteLine($"Extracted name for deletion from query: '{name}'");
        return name;
    }

    
    private async Task<IActionResult> EditContact(string response)
    {
        
        var (name, phone) = ExtractNameAndPhone(response);
        Console.WriteLine($"Editing Contact - Name: {name}, New Phone: {phone}");

        
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone) || name == "Unknown Name" || phone == "No Phone")
        {
            Console.WriteLine("Failed to edit contact. Ensure both name and new phone number are provided.");
            return BadRequest("Failed to edit contact. Ensure both name and new phone number are provided.");
        }

        
        var contact = _context.Contacts
            .AsEnumerable() 
            .FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        
        if (contact != null)
        {
            
            var duplicatePhoneContact = _context.Contacts
                .AsEnumerable()
                .FirstOrDefault(c => c.PhoneNumber == phone && c.Name != contact.Name);

            if (duplicatePhoneContact != null)
            {
                Console.WriteLine("Duplicate phone number found.");
                return BadRequest("There is already someone with the same phone number.");
            }

            
            contact.PhoneNumber = phone;
            await _context.SaveChangesAsync();

            Console.WriteLine($"Contact '{name}' updated with new phone number {phone}.");
            return Ok($"Contact {name} updated with new phone number {phone}.");
        }
    
        Console.WriteLine($"Contact '{name}' was not found.");
        return NotFound($"Contact {name} was not found.");
    }
    private async Task<IActionResult> GetAllContacts()
    {
        var contacts = _context.Contacts.ToList();

        if (contacts.Count == 0)
        {
            return NotFound("No contacts found in the database.");
        }
        
        var contactList = contacts.Select(c => new 
        {
            name = c.Name,
            phoneNumber = c.PhoneNumber
        }).ToList();

        return Ok(contactList);
    }

    
    private (string name, string phone) ExtractNameAndPhone(string response)
    {
        var nameRegex = new Regex(@"Name:\s*([A-Za-z\s]+)", RegexOptions.IgnoreCase);
        var phoneRegex = new Regex(@"Phone:\s*([\d\-]+)", RegexOptions.IgnoreCase);

        var nameMatch = nameRegex.Match(response);
        var phoneMatch = phoneRegex.Match(response);

        var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "Unknown Name";
        var phone = phoneMatch.Success ? phoneMatch.Groups[1].Value.Trim() : "No Phone";

        
        name = name.Replace("\n", "").Replace("\r", "").Trim();
        phone = phone.Replace("\n", "").Replace("\r", "").Trim();

        Console.WriteLine($"Extracted - Name: {name}, Phone: {phone}");
        return (name, phone);
    }

    
    private string ExtractNameFromQuery(string query)
    {
        var nameRegex = new Regex(@"(?:phone number for|find|delete|edit|update|the contact for)\s+([A-Za-z\s]+)", RegexOptions.IgnoreCase);
        var nameMatch = nameRegex.Match(query);

        var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim().TrimEnd('.') : "Unknown Name";

        Console.WriteLine($"Extracted name from query: {name}");
        return name;
    }
}
