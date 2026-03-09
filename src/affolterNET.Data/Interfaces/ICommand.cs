using affolterNET.Data.Result;

namespace affolterNET.Data.Interfaces
{
    /// <summary>
    /// Represents a database command (insert, update, delete) that returns the number of affected rows.
    /// Extends <see cref="IQuery{T}"/> with <c>int</c> as the result type.
    /// </summary>
    public interface ICommand : IQuery<int>
    {
    }
}
