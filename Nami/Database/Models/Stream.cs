using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nami.Database.Models
{
    [Table("streams")]
    public class Stream : IEquatable<Stream>
    {
        public const int NameLimit = 32;
        public const int UrlLimit = 128;

        [ForeignKey("Stream")]
        [Column("gid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long GuildIdDb { get; set; }
       
        [NotMapped] public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("member_id"), Required, MaxLength(NameLimit)]
        public ulong? MemberID { get; set; } = default!;

        [Column("joined"), Required, MaxLength(UrlLimit)]
        public int Joined { get; set; } = default!;


        public virtual GuildConfig GuildConfig { get; set; } = null!;


        public bool Equals(Stream? other) => other is { } && this.GuildId == other.GuildId && this.MemberID == other.MemberID;
        public override bool Equals(object? obj) => this.Equals(obj as Stream);
        public override int GetHashCode() => HashCode.Combine(this.GuildId, this.MemberID);
    }
}
