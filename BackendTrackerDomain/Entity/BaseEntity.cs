﻿namespace BackendTrackerDomain.Entity;

public class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}