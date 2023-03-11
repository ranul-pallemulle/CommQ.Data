﻿using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommQ.Data
{
    public interface IDbWriter
    {
        ValueTask<int> CommandAsync(string command, Action<IDataParameterCollection> setupParameters = null, CancellationToken cancellationToken = default);
    }
}
