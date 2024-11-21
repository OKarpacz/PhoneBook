using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PhoneBook.Data;
using PhoneBook.Models;

namespace PhoneBook.Services
{
    public class ChatService
    {
        private readonly OpenAiService _openAiService;
        private readonly PhoneBookContext _context;

        public ChatService(OpenAiService openAiService, PhoneBookContext context)
        {
            _openAiService = openAiService;
            _context = context;
        }

        public async Task<IActionResult> ProcessPrompt(string prompt)
        {
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
            }
            else if (prompt.ToLower().Contains("show all") || prompt.ToLower().Contains("see") || prompt.ToLower().Contains("show"))
            {
                return await GetAllContacts();
            }

            return new BadRequestObjectResult("Invalid action or command format.");
        }

        private async Task<IActionResult> AddContact(string response)
        {
            var (name, phone) = ExtractNameAndPhone(response);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone))
            {
                return new BadRequestObjectResult("Failed to add contact. Ensure both name and phone number are provided.");
            }

            var existingContact = _context.Contacts.FirstOrDefault(c => c.PhoneNumber == phone);
            if (existingContact != null)
            {
                return new OkObjectResult("There is already someone with the same phone number.");
            }

            var contact = new Contact { Name = name, PhoneNumber = phone };
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return new OkObjectResult($"Contact {name} added with phone number {phone}.");
        }

        private IActionResult FindContact(string prompt)
        {
            var nameToFind = ExtractNameFromQuery(prompt);

            var contact = _context.Contacts
                                  .AsEnumerable()
                                  .FirstOrDefault(c => c.Name.Equals(nameToFind, StringComparison.OrdinalIgnoreCase));

            if (contact != null)
            {
                return new OkObjectResult($"The phone number for {contact.Name} is {contact.PhoneNumber}.");
            }

            return new NotFoundObjectResult($"Contact {nameToFind} was not found.");
        }

        private IActionResult DeleteContact(string prompt)
        {
            var nameToDelete = ExtractNameFromDeletePrompt(prompt);

            if (string.IsNullOrEmpty(nameToDelete) || nameToDelete == "Unknown Name")
            {
                return new BadRequestObjectResult("Could not determine the name to delete. Please specify a valid name.");
            }

            var contact = _context.Contacts
                .AsEnumerable()
                .FirstOrDefault(c => c.Name.Equals(nameToDelete, StringComparison.OrdinalIgnoreCase));

            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                _context.SaveChanges();
                return new OkObjectResult($"Contact {nameToDelete} was deleted.");
            }

            return new NotFoundObjectResult($"Contact {nameToDelete} was not found.");
        }

        private async Task<IActionResult> EditContact(string response)
        {
            var (name, phone) = ExtractNameAndPhone(response);

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phone) || name == "Unknown Name" || phone == "No Phone")
            {
                return new BadRequestObjectResult("Failed to edit contact. Ensure both name and new phone number are provided.");
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
                    return new BadRequestObjectResult("There is already someone with the same phone number.");
                }

                contact.PhoneNumber = phone;
                await _context.SaveChangesAsync();

                return new OkObjectResult($"Contact {name} updated with new phone number {phone}.");
            }

            return new NotFoundObjectResult($"Contact {name} was not found.");
        }

        private async Task<IActionResult> GetAllContacts()
        {
            var contacts = _context.Contacts.ToList();

            if (contacts.Count == 0)
            {
                return new NotFoundObjectResult("No contacts found in the database.");
            }

            var contactList = contacts.Select(c => new
            {
                name = c.Name,
                phoneNumber = c.PhoneNumber
            }).ToList();

            return new OkObjectResult(contactList);
        }

        private (string name, string phone) ExtractNameAndPhone(string response)
        {
            var nameRegex = new Regex(@"Name:\s*([A-Za-z\s]+)", RegexOptions.IgnoreCase);
            var phoneRegex = new Regex(@"Phone:\s*([\d\-]+)", RegexOptions.IgnoreCase);

            var nameMatch = nameRegex.Match(response);
            var phoneMatch = phoneRegex.Match(response);

            var name = nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "Unknown Name";
            var phone = phoneMatch.Success ? phoneMatch.Groups[1].Value.Trim() : "No Phone";

            return (name, phone);
        }

        private string ExtractNameFromQuery(string query)
        {
            var nameRegex = new Regex(@"(?:phone number for|find|delete|edit|update|the contact for)\s+([A-Za-z\s]+)", RegexOptions.IgnoreCase);
            var nameMatch = nameRegex.Match(query);

            return nameMatch.Success ? nameMatch.Groups[1].Value.Trim().TrimEnd('.') : "Unknown Name";
        }

        private string ExtractNameFromDeletePrompt(string query)
        {
            var nameRegex = new Regex(@"(?:delete|remove)\s+(?:the contact for\s*)?([A-Za-z\s]+)", RegexOptions.IgnoreCase);
            var nameMatch = nameRegex.Match(query);

            return nameMatch.Success ? nameMatch.Groups[1].Value.Trim() : "Unknown Name";
        }
    }
}
