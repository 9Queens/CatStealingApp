﻿namespace CatApp.Shared
{
    public class ScheduledJobResult
    {
        public bool Success { get; set; }
        public Guid JobId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
