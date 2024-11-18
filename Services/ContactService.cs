using PhoneBook.Data;
using PhoneBook.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhoneBook.Services
{
    public class ContactService
    {
        private readonly PhoneBookContext _context;

        public ContactService(PhoneBookContext context)
        {
            _context = context;
        }

        public async Task<List<Contact>> GetAllContactsAsync()
        {
            return await _context.Contacts.ToListAsync();
        }

        public async Task<Contact?> GetContactByNameAsync(string name)
        {
            return await _context.Contacts.FirstOrDefaultAsync(c => c.Name == name);
        }

        public async Task AddContactAsync(Contact contact)
        {
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateContactAsync(Contact contact)
        {
            _context.Contacts.Update(contact);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
        }
    }
}