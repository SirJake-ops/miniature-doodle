namespace BackendTracker.Graphql.GraphqlTypes;

public class UserSearchInput
{
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
}