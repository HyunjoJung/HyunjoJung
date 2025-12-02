---
title: CardMaker - Generating Business Cards Without a Database
description: Building a stateless business card generator with Excel import, PowerPoint export, and QR code vCards using Blazor Server.
date: 2024-11-20
tags:
  - Blazor
  - OpenXML
  - QR Codes
  - C#
category: Backend
featured: true
image: /images/cardmaker.jpg
---

# CardMaker: Stateless Business Card Generation

## The Challenge

A PR agency needed to generate hundreds of business cards for employees, with requirements:

- Import employee data from Excel
- Generate PowerPoint slides with QR codes
- Preserve custom PowerPoint template formatting
- **No database** - security and privacy concerns
- Fast turnaround (seconds, not minutes)

## Solution: Stateless Architecture

```csharp
[Route("/generate")]
public async Task<IActionResult> GenerateCards(IFormFile excelFile, IFormFile templateFile)
{
    // 1. Parse Excel in-memory
    var employees = await _importService.ParseExcelAsync(excelFile.OpenReadStream());

    // 2. Generate PowerPoint in-memory
    var pptBytes = await _cardGenerator.GenerateAsync(
        employees,
        templateFile.OpenReadStream()
    );

    // 3. Return file and discard everything
    return File(pptBytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation");
}
```

No session state, no file storage, no database writes. Just pure transformation.

## Technical Deep Dive

### 1. Excel Import with OpenXML

Reading structured data from Excel templates:

```csharp
public async Task<List<EmployeeData>> ParseExcelAsync(Stream excelStream)
{
    using var document = SpreadsheetDocument.Open(excelStream, false);
    var workbookPart = document.WorkbookPart;
    var sheet = workbookPart.WorkshookPart.Sheets.GetFirstChild<Sheet>();

    // Find header row (flexible positioning)
    var headerRow = FindHeaderRow(sheetData, expectedHeaders);

    // Parse data rows
    var employees = new List<EmployeeData>();
    foreach (var row in sheetData.Elements<Row>().Skip(headerRow + 1))
    {
        employees.Add(ParseRow(row, headerMapping));
    }

    return employees;
}
```

### 2. QR Code Generation

vCard format embedded in QR codes:

```csharp
public byte[] GenerateVCardQR(EmployeeData employee)
{
    var vCard = $@"BEGIN:VCARD
VERSION:3.0
FN:{employee.Name}
ORG:{employee.Company}
TITLE:{employee.Position}
TEL:{employee.Phone}
EMAIL:{employee.Email}
END:VCARD";

    using var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode(vCard, QRCodeGenerator.ECCLevel.Q);
    var qrCode = new PngByteQRCode(qrCodeData);
    return qrCode.GetGraphic(20);
}
```

### 3. PowerPoint Slide Generation

Merging data into PowerPoint templates while preserving formatting:

```csharp
public async Task<byte[]> GenerateCardsAsync(List<EmployeeData> employees, Stream templateStream)
{
    using var memoryStream = new MemoryStream();
    templateStream.CopyTo(memoryStream);

    using var presentation = PresentationDocument.Open(memoryStream, true);
    var slideMasterPart = presentation.PresentationPart.SlideMasterParts.First();
    var slideTemplate = slideMasterPart.Slide;

    foreach (var employee in employees)
    {
        // Clone template slide
        var newSlide = CloneSlide(slideTemplate);

        // Replace placeholders
        ReplacePlaceholder(newSlide, "{name}", employee.Name);
        ReplacePlaceholder(newSlide, "{position}", employee.Position);
        ReplacePlaceholder(newSlide, "{email}", employee.Email);

        // Insert QR code image
        var qrBytes = GenerateVCardQR(employee);
        InsertImage(newSlide, "{qrcode}", qrBytes);

        presentation.PresentationPart.Presentation.AppendChild(newSlide);
    }

    presentation.Save();
    return memoryStream.ToArray();
}
```

## Security & Performance

### Security-First Design

```csharp
// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1)
        }));
});

// Content Security Policy
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'");
    await next();
});
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(false);
        return allocated < 500_000_000
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Degraded();
    });

app.MapHealthChecks("/health");
```

## Deployment

Docker Compose for simple deployment:

```yaml
version: '3.8'
services:
  cardmaker:
    image: hyunjojung/cardmaker:latest
    ports:
      - "5049:5049"
    environment:
      - ASPNETCORE_URLS=http://+:5049
    restart: unless-stopped
```

Cloudflare Tunnel for HTTPS:

```bash
cloudflared tunnel create cardmaker
cloudflared tunnel route dns cardmaker cardmaker.hyunjo.uk
```

## Results

**Before**:
- Manual PowerPoint editing: 2-3 hours for 50 cards
- Error-prone (typos, formatting inconsistencies)
- No QR codes

**After**:
- Automated generation: <10 seconds for 50 cards
- Zero errors (validated at import)
- vCard QR codes for all cards

## Live Demo

Try it yourself: [cardmaker.hyunjo.uk](https://cardmaker.hyunjo.uk)

Source code: [GitHub](https://github.com/HyunjoJung/Business-Card-Automation-System)

## Key Takeaways

1. **Stateless architectures** eliminate entire classes of security and scaling issues
2. **OpenXML SDK** provides complete control over Office file formats
3. **Blazor Server** excels at form-heavy, server-rendered apps
4. **Docker + Cloudflare Tunnel** = simple, secure deployment

---

**Building something similar?** Feel free to reach out on [GitHub](https://github.com/HyunjoJung)!
