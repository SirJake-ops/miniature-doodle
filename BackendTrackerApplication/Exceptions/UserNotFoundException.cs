using FluentValidation.Results;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BackendTrackerApplication.Exceptions;

[Serializable]
public class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid requestSubmitterId, IDictionary<string, string[]> errors) : base("User not found.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public UserNotFoundException(IEnumerable<ValidationFailure> failures, IDictionary<string, string[]> errors)
    {
        Errors = failures.GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public UserNotFoundException(string message, IDictionary<string, string[]> errors): base (message)
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; set; }
}