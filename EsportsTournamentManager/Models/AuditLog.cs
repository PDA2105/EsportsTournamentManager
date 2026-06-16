using System;

namespace EsportsTournamentManager.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int UserId { get; set; }

        public string Action { get; set; } // "INSERT_SCORE", "UPDATE_SCHEDULE", "ROLLBACK"

        public string TableName { get; set; }

        public int RecordId { get; set; }

        public string OldDataSnapshot { get; set; } // JSON formatted string

        public string NewDataSnapshot { get; set; } // JSON formatted string

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public virtual User User { get; set; }
    }
}
