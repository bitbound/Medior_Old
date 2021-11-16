﻿namespace Medior.Core.Shared.Services
{
    public interface IChrono
    {
        DateTimeOffset Now { get; }
    }

    public class Chrono : IChrono
    {
        public DateTimeOffset Now => DateTimeOffset.Now;
    }
}
