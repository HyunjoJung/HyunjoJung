---
title: CardMaker - 데이터베이스 없이 명함 생성하기
description: Blazor Server를 사용해 Excel 가져오기, PowerPoint 내보내기, QR 코드 vCard가 포함된 무상태 명함 생성기 개발
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

# CardMaker: 무상태 명함 생성

## 과제

PR 에이전시에서 직원들을 위한 수백 장의 명함을 생성해야 했습니다. 요구사항은 다음과 같습니다:

- Excel에서 직원 데이터 가져오기
- QR 코드가 포함된 PowerPoint 슬라이드 생성
- 사용자 정의 PowerPoint 템플릿 서식 유지
- **데이터베이스 없음** - 보안 및 프라이버시 우려
- 빠른 처리 시간 (분이 아닌 초 단위)

## 솔루션: 무상태 아키텍처

```csharp
[Route("/generate")]
public async Task<IActionResult> GenerateCards(IFormFile excelFile, IFormFile templateFile)
{
    // 1. Excel을 메모리에서 파싱
    var employees = await _importService.ParseExcelAsync(excelFile.OpenReadStream());

    // 2. PowerPoint를 메모리에서 생성
    var pptBytes = await _cardGenerator.GenerateAsync(
        employees,
        templateFile.OpenReadStream()
    );

    // 3. 파일 반환 후 모든 것을 폐기
    return File(pptBytes, "application/vnd.openxmlformats-officedocument.presentationml.presentation");
}
```

세션 상태도, 파일 저장소도, 데이터베이스 쓰기도 없습니다. 순수한 변환만 수행합니다.

## 기술 심층 분석

### 1. OpenXML을 사용한 Excel 가져오기

Excel 템플릿에서 구조화된 데이터 읽기:

```csharp
public async Task<List<EmployeeData>> ParseExcelAsync(Stream excelStream)
{
    using var document = SpreadsheetDocument.Open(excelStream, false);
    var workbookPart = document.WorkbookPart;
    var sheet = workbookPart.WorkshookPart.Sheets.GetFirstChild<Sheet>();

    // 헤더 행 찾기 (유연한 위치 지정)
    var headerRow = FindHeaderRow(sheetData, expectedHeaders);

    // 데이터 행 파싱
    var employees = new List<EmployeeData>();
    foreach (var row in sheetData.Elements<Row>().Skip(headerRow + 1))
    {
        employees.Add(ParseRow(row, headerMapping));
    }

    return employees;
}
```

### 2. QR 코드 생성

QR 코드에 포함된 vCard 형식:

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

### 3. PowerPoint 슬라이드 생성

서식을 유지하면서 PowerPoint 템플릿에 데이터 병합:

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
        // 템플릿 슬라이드 복제
        var newSlide = CloneSlide(slideTemplate);

        // 플레이스홀더 교체
        ReplacePlaceholder(newSlide, "{name}", employee.Name);
        ReplacePlaceholder(newSlide, "{position}", employee.Position);
        ReplacePlaceholder(newSlide, "{email}", employee.Email);

        // QR 코드 이미지 삽입
        var qrBytes = GenerateVCardQR(employee);
        InsertImage(newSlide, "{qrcode}", qrBytes);

        presentation.PresentationPart.Presentation.AppendChild(newSlide);
    }

    presentation.Save();
    return memoryStream.ToArray();
}
```

## 보안 및 성능

### 보안 우선 설계

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

### 헬스 체크

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

## 배포

간단한 배포를 위한 Docker Compose:

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

HTTPS를 위한 Cloudflare Tunnel:

```bash
cloudflared tunnel create cardmaker
cloudflared tunnel route dns cardmaker cardmaker.hyunjo.uk
```

## 결과

**이전**:
- 수동 PowerPoint 편집: 50장 기준 2-3시간 소요
- 오류 발생 위험 (오타, 서식 불일치)
- QR 코드 없음

**이후**:
- 자동 생성: 50장 기준 10초 미만
- 오류 제로 (가져오기 시 검증)
- 모든 카드에 vCard QR 코드

## 라이브 데모

직접 사용해보기: [cardmaker.hyunjo.uk](https://cardmaker.hyunjo.uk)

소스 코드: [GitHub](https://github.com/HyunjoJung/Business-Card-Automation-System)

## 주요 교훈

1. **무상태 아키텍처**는 보안 및 확장 문제의 전체 범주를 제거합니다
2. **OpenXML SDK**는 Office 파일 형식에 대한 완전한 제어를 제공합니다
3. **Blazor Server**는 폼 중심의 서버 렌더링 앱에서 뛰어납니다
4. **Docker + Cloudflare Tunnel** = 간단하고 안전한 배포

---

**비슷한 것을 만드시나요?** [GitHub](https://github.com/HyunjoJung)에서 연락해주세요!
