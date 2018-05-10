# Repro Project for EntityFramework.Core Issue

When using `EntityFramework.Core.Sqlite 2.1.0-rc1-final`, filtering on an `enum` type in a projection
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
