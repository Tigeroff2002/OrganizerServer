﻿namespace Logic;

public sealed class RootConfiguration
{
    public required string RootPassword { get; set; }

    public required int MinimalAccountAgeDays { get; set; }
}
