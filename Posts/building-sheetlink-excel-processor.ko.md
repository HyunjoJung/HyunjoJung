---
title: SheetLink 개발기 - 프라이버시 우선 Excel 링크 처리기
description: Blazor Server와 OpenXML을 사용해 데이터베이스 없이 메모리 내에서 모든 처리를 수행하는 무상태 Excel 링크 추출기 개발 과정
date: 2024-11-15
tags:
  - Blazor
  - ASP.NET Core
  - OpenXML
  - Docker
category: Backend
featured: true
image: /images/sheetlink.jpg
---

# SheetLink 개발기: 프라이버시 우선 Excel 링크 처리기

## 문제 인식

하이퍼링크가 포함된 Excel 파일을 작업하는 것은 흔한 일이지만, 링크를 추출하거나 제목 + URL 열을 클릭 가능한 링크로 병합하는 작업은 수동 작업이나 데스크톱 소프트웨어가 필요합니다. 저는 다음과 같은 웹 기반 솔루션을 만들고 싶었습니다:

- **프라이버시 중심**: 서버 저장소 없이 모든 처리를 메모리 내에서 수행
- **빠른 속도**: 데이터베이스 오버헤드 없이 즉각적인 결과 제공
- **무료**: 회원가입이나 결제 불필요
- **자체 호스팅**: 배포에 대한 완전한 제어권

## 기술 스택

**Blazor Server**와 **ASP.NET Core 10**을 선택한 이유:

```csharp
// 간단하고 무상태한 아키텍처
public class LinkExtractorService
{
    public async Task<List<HyperlinkData>> ExtractLinksAsync(Stream fileStream)
    {
        // 모든 처리가 메모리에서 발생
        // 데이터베이스 쓰기 없음, 파일 저장 없음
        using var document = SpreadsheetDocument.Open(fileStream, false);
        // ... 추출 로직
    }
}
```

### Blazor Server를 선택한 이유

- **서버 사이드 렌더링** - SEO와 초기 로드에 유리
- **SignalR을 통한 실시간 업데이트** (이 앱에서는 불필요하지만)
- **모든 곳에서 C#** - 언어 간 컨텍스트 전환 없음
- **기본적으로 무상태** - 프라이버시 중심 앱에 완벽

## 아키텍처 하이라이트

### 1. Excel 처리를 위한 DocumentFormat.OpenXML

Microsoft의 공식 OpenXML SDK는 Excel 파일 내부에 대한 낮은 수준의 액세스를 제공합니다:

```csharp
var workbookPart = document.WorkbookPart;
var worksheetPart = workbookPart.WorksheetParts.First();
var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

foreach (var row in sheetData.Elements<Row>())
{
    foreach (var cell in row.Elements<Cell>())
    {
        // relationship에서 하이퍼링크 추출
        var hyperlinkRelationship = worksheetPart
            .HyperlinkRelationships
            .FirstOrDefault(r => r.Id == cell.InnerText);
    }
}
```

### 2. 메모리 내 처리

영구 저장이 없다는 것은 프라이버시 우려가 없다는 의미입니다:

- 파일 업로드가 메모리로 직접 스트리밍됨
- 동기식으로 처리 (대부분의 파일은 1초 미만)
- 결과가 반환되고 즉시 삭제됨
- 10MB 파일 크기 제한으로 메모리 문제 방지

### 3. Docker 배포

멀티 스테이지 빌드로 간단하고 재현 가능한 배포:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExcelLinkExtractorWeb.dll"]
```

## CI/CD 파이프라인

GitHub Actions → Docker Hub → 자체 호스팅 Ubuntu:

```yaml
- name: Build and push Docker image
  run: |
    docker build -t hyunjojung/sheetlink:latest .
    docker push hyunjojung/sheetlink:latest
```

Cloudflare Tunnel이 포트 노출 없이 HTTPS를 제공:

```bash
cloudflared tunnel create sheetlink
cloudflared tunnel route dns sheetlink sheetlink.hyunjo.uk
```

## 성능 지표

최적화 후 Lighthouse 점수:

- **Performance**: 98
- **Accessibility**: 100
- **Best Practices**: 100
- **SEO**: 100

### 적용된 최적화

1. **정적 자산 캐싱** (7일 캐시 헤더)
2. GET 요청에 대한 **응답 캐싱**
3. 로드 밸런서용 `/health`에서 **헬스 체크**
4. `/metrics`에서 **Prometheus 메트릭**

## 배운 점

### 1. 무상태는 아름답다

데이터베이스가 없다는 것은:
- 마이그레이션 없음
- 백업 전략 불필요
- 데이터 유출 위험 없음
- 무한 수평 확장 가능

### 2. OpenXML은 강력하지만 복잡하다

학습 곡선은 가파르지만 그만한 가치가 있습니다:
- Excel 파일 조작에 대한 완전한 제어
- 서식 보존 (병합 기능에 중요)
- 외부 의존성이나 라이선스 없음

### 3. 간단한 사용 사례에는 Blazor Server

복잡한 상호작용이 없는 무상태 앱의 경우 Blazor Server가 완벽합니다:
- 간단한 배포 모델
- 빠른 초기 로드 (서버 렌더링)
- C# 풀스택 개발

## 직접 사용해보기

라이브 데모: [sheetlink.hyunjo.uk](https://sheetlink.hyunjo.uk)

소스 코드: [GitHub](https://github.com/HyunjoJung/ExcelLinkExtractor)

## 다음 계획

수요가 있다면 추가할 기능:
- CSV 내보내기 옵션
- 여러 파일에 대한 일괄 처리
- 프로그래밍 방식 액세스를 위한 API 엔드포인트

---

**질문이나 피드백이 있으신가요?** [GitHub](https://github.com/HyunjoJung)에서 찾아주세요
