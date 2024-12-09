namespace Personals.Common.Contracts.LookupTypes;

public record LookupTypeSearchResponse(Guid Id, string Code, string Name)
{
    public override string ToString() => Name;
}