// Guids.cs
// MUST match guids.h
using System;

namespace Anonymous.AStylish
{
    static class GuidList
    {
        public const string guidAStylishPkgString = "746d6355-eecf-4fb4-8336-25412940d69f";
        public const string guidAStylishCmdSetString = "ef5bc2f0-aeb2-4a11-8b83-0679dd905942";

        public static readonly Guid guidAStylishCmdSet = new Guid(guidAStylishCmdSetString);
    };
}