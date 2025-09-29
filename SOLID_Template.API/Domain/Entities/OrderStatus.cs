namespace SOLID_Template.Domain.Entities;

/// <summary>
/// Enum that represents the possible statuses of an order
/// </summary>
public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Approved = 3,
    Rejected = 4,
    Canceled = 5,
    Completed = 6
}