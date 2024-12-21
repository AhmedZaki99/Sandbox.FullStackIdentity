﻿using Sandbox.FullStackIdentity.Tests.Shared;

namespace Sandbox.FullStackIdentity.Tests.HostApi;


[CollectionDefinition(Name)]
public class DatabaseDependentCollection : ICollectionFixture<DbDataSourceFactory>
{
    public const string Name = "DatabaseDependent";
}
