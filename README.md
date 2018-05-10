# Repro Project for EntityFramework.Core Issue

When using `Microsoft.EntityFramework.Core.Sqlite 2.1.0-rc1-final`, filtering on an `enum` type in a projection
throws an `InvalidCastException` if the filter criteria is not inlined:

```csharp
// Throws System.InvalidCastException :
// Invalid cast from 'EFCoreProjectionInvalidCastRepro.EmailTemplateTypeDto'
// to 'EFCoreProjectionInvalidCastRepro.EmailTemplateType'.
[Fact]
public async Task CanFilterProjectionWithCapturedVariable()
{
    var templateType = EmailTemplateTypeDto.PasswordResetRequest;
    var template = await Context
        .EmailTemplates
        .Select(t => new EmailTemplateDto {Id = t.Id, TemplateType = (EmailTemplateTypeDto) t.TemplateType})
        .Where(t => t.TemplateType == templateType)
        .FirstOrDefaultAsync();
    Assert.NotNull(template);
}

// This works with an inline filter
[Fact]
public async Task CanFilterProjectionWithInlineVariable()
{
    var template = await Context
        .EmailTemplates
        .Select(t => new EmailTemplateDto {Id = t.Id, TemplateType = (EmailTemplateTypeDto) t.TemplateType})
        .Where(t => t.TemplateType == EmailTemplateTypeDto.PasswordResetRequest)
        .FirstOrDefaultAsync();
    Assert.NotNull(template);
}
```

The exception being thrown:

```
System.InvalidCastException: 'Invalid cast from 'EFCoreProjectionInvalidCastRepro.EmailTemplateTypeDto' to 'EFCoreProjectionInvalidCastRepro.EmailTemplateType'.'

at System.Convert.DefaultToType(IConvertible value, Type targetType, IFormatProvider provider)
at Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter`2.Sanitize[T](Object value)
at Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter`2.<>c__DisplayClass3_0`2.<SanitizeConverter>b__0(Object v)
at Microsoft.EntityFrameworkCore.Storage.RelationalTypeMapping.CreateParameter(DbCommand command, String name, Object value, Nullable`1 nullable)
at Microsoft.EntityFrameworkCore.Storage.Internal.TypeMappedRelationalParameter.AddDbParameter(DbCommand command, Object value)
at Microsoft.EntityFrameworkCore.Storage.Internal.RelationalParameterBase.AddDbParameter(DbCommand command, IReadOnlyDictionary`2 parameterValues)
at Microsoft.EntityFrameworkCore.Storage.Internal.RelationalCommand.CreateCommand(IRelationalConnection connection, IReadOnlyDictionary`2 parameterValues)
at Microsoft.EntityFrameworkCore.Storage.Internal.RelationalCommand.ExecuteAsync(IRelationalConnection connection, DbCommandMethod executeMethod, IReadOnlyDictionary`2 parameterValues, CancellationToken cancellationToken)
at Microsoft.EntityFrameworkCore.Query.Internal.AsyncQueryingEnumerable`1.AsyncEnumerator.BufferlessMoveNext(DbContext _, Boolean buffer, CancellationToken cancellationToken)
at Microsoft.EntityFrameworkCore.Query.Internal.AsyncQueryingEnumerable`1.AsyncEnumerator.MoveNext(CancellationToken cancellationToken)
at System.Linq.AsyncEnumerable.FirstOrDefault_[TSource](IAsyncEnumerable`1 source, CancellationToken cancellationToken)
at Microsoft.EntityFrameworkCore.Query.Internal.AsyncLinqOperatorProvider.TaskResultAsyncEnumerable`1.Enumerator.MoveNext(CancellationToken cancellationToken)
at Microsoft.EntityFrameworkCore.Query.Internal.AsyncLinqOperatorProvider.ExceptionInterceptor`1.EnumeratorExceptionInterceptor.MoveNext(CancellationToken cancellationToken)
at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.ExecuteSingletonAsyncQuery[TResult](QueryContext queryContext, Func`2 compiledQuery, IDiagnosticsLogger`1 logger, Type contextType)
at EFCoreProjectionInvalidCastRepro.ProjectionTests.CanFilterProjectionWithCapturedVariable() in D:\Visual Studio Projects\EFCoreProjectionInvalidCastRepro\EFCoreProjectionInvalidCastRepro\ProjectionTests.cs:line 37
```
