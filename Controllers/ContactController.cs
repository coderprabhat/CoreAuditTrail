using CoreAuditTrail.Data;
using CoreAuditTrail.Models;
using CoreAuditTrail.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Numerics;

namespace CoreAuditTrail.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin, Manager, User")]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenHelper _jwtTokenHelper;

        public ContactController(AppDbContext context, JwtTokenHelper jwtTokenHelper)
        {
            _context = context;
            _jwtTokenHelper = jwtTokenHelper;
        }

        // Create a new contact
        [HttpPost]
        public async Task<IActionResult> CreateContact([FromBody] Contact contact)
        {

            var userId = _jwtTokenHelper.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in the token.");
            }
            Console.WriteLine("User : " + userId);
            contact.CreatedBy = userId.Value;
            contact.CreatedOn = DateTime.UtcNow;
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return Ok(contact);
        }

        // Get all contacts
        [HttpGet]
        public async Task<IActionResult> GetContacts()
        {
            var contacts = await _context.Contacts.ToListAsync();
            return Ok(contacts);
        }

        // Get a specific contact by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContact(Guid id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound("Contact not found.");

            return Ok(contact);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateContactPartial(Guid id, [FromBody] JsonPatchDocument<Contact> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest("Invalid patch document.");
            }

            var userId = _jwtTokenHelper.GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized("User ID not found in the token.");
            }

            // Fetch the existing contact from the database
            var oldContact = await _context.Contacts.FindAsync(id);
            if (oldContact == null)
            {
                return NotFound("Contact not found.");
            }

            // Apply the patch to the existing entity
            patchDoc.ApplyTo(oldContact);



            // Update audit fields
            oldContact.UpdatedBy = userId.Value;
            oldContact.UpdatedOn = DateTime.UtcNow;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok(oldContact);
        }

        // Delete a Contact
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(Guid id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
                return NotFound("Contact not found.");

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return Ok("Contact deleted successfully.");
        }

    }
}
