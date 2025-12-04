using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Sender))]
        public int SenderId { get; set; }
        public Customer? Sender { get; set; }

        [Required]
        [ForeignKey(nameof(Receiver))]
        public int ReceiverId { get; set; }
        public Customer? Receiver { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = null!;

        public DateTime SentAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        public bool IsRead { get; set; } = false;
    }
}
