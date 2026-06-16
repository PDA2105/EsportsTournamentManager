using System;
using System.Collections.Generic;

namespace EsportsTournamentManager.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string FullName { get; set; }

        public string Role { get; set; } // "Admin", "Referee", "Viewer"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
        public virtual ICollection<Match> Matches { get; set; } = new List<Match>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
