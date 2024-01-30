using LinqToDB.Mapping;

namespace Vint.Core.Database.Models;

[Table("Invites")]
public class Invite {
    [PrimaryKey, Identity] public long Id { get; set; }
    [Column] public string Code { get; init; } = null!;
    [Column] public ushort RemainingUses { get; set; }

    public override string ToString() => $"Invite '{Code}', {RemainingUses} uses remaining (Id: {Id})";
}